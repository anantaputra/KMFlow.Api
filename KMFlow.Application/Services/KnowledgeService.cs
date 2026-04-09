using KMFlow.Application.DTOs.Knowledges;
using KMFlow.Application.Interfaces.Repositories;
using KMFlow.Application.Interfaces.Services;
using KMFlow.Domain.Enums;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace KMFlow.Application.Services;

public class KnowledgeService : IKnowledgeService
{
    private readonly IKnowledgeRepository _knowledgeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IWebHostEnvironment _environment;

    public KnowledgeService(IKnowledgeRepository knowledgeRepository, IUserRepository userRepository, IWebHostEnvironment environment)
    {
        _knowledgeRepository = knowledgeRepository;
        _userRepository = userRepository;
        _environment = environment;
    }

    public async Task<Response<KnowledgeResponseDto>> AddKnowledgeAsync(Guid submittedBy, AddKnowledgeWithFileRequestDto request)
    {
        try
        {
            if (request.Attachment == null || request.Attachment.Length == 0)
            {
                return new Response<KnowledgeResponseDto>(false, "attachment wajib diisi", null);
            }

            if (string.IsNullOrWhiteSpace(request.NamaFile))
            {
                return new Response<KnowledgeResponseDto>(false, "namaFile wajib diisi", null);
            }

            var requestedExt = NormalizeExtension(Path.GetExtension(Path.GetFileName(request.Attachment.FileName)));
            if (!string.Equals(requestedExt, ".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return new Response<KnowledgeResponseDto>(false, "attachment harus file .pdf", null);
            }

            var status = KnowledgeStatus.Draft;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<KnowledgeStatus>(request.Status.Trim(), true, out status))
                {
                    return new Response<KnowledgeResponseDto>(false, $"status '{request.Status}' tidak valid", null);
                }
            }

            var deptResponse = string.IsNullOrWhiteSpace(request.OwnerDepartment)
                ? await _knowledgeRepository.GetUserDepartmentAsync(submittedBy)
                : await _knowledgeRepository.GetDepartmentByNameAsync(request.OwnerDepartment);

            if (!deptResponse.Status || deptResponse.Data == null)
            {
                return new Response<KnowledgeResponseDto>(false, deptResponse.Message, null);
            }

            var deptSlug = SanitizePathSegment(deptResponse.Data.Slug);
            if (string.IsNullOrWhiteSpace(deptSlug))
            {
                deptSlug = "general";
            }

            var uploadsRoot = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "km-assets",
                deptSlug
            );
            Directory.CreateDirectory(uploadsRoot);

            var savedFileName = $"{Guid.NewGuid():N}.pdf";
            var physicalPath = Path.Combine(uploadsRoot, savedFileName);

            var attempt = 0;
            while (File.Exists(physicalPath) && attempt < 3)
            {
                attempt++;
                savedFileName = $"{Guid.NewGuid():N}.pdf";
                physicalPath = Path.Combine(uploadsRoot, savedFileName);
            }

            if (File.Exists(physicalPath))
            {
                return new Response<KnowledgeResponseDto>(false, "Gagal membuat nama file unik", null);
            }

            await using (var stream = new FileStream(physicalPath, FileMode.CreateNew))
            {
                await request.Attachment.CopyToAsync(stream);
            }

            var filePath = $"/km-assets/{deptSlug}/{savedFileName}";
            return await _knowledgeRepository.AddKnowledgeAsync(
                submittedBy,
                deptResponse.Data.Id,
                request.NamaFile,
                filePath,
                (int)status
            );
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error add knowledge: {ex.Message}", null);
        }
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllKnowledgeAsync()
    {
        var response = await _knowledgeRepository.GetAllKnowledgeAsync();
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<Response<KnowledgeStatsResponseDto>> GetKnowledgeStatsAsync(Guid userId)
    {
        return await _knowledgeRepository.GetKnowledgeStatsAsync(userId);
    }

    public async Task<ResponseList<KnowledgeResponseDto>> SearchKnowledgeAsync(string? query, string? department)
    {
        var response = await _knowledgeRepository.SearchKnowledgeAsync(query, department);
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                Status = k.Status,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllPendingReviewKnowledgeAsync(Guid userId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(userId);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "User tidak ditemukan"
            };
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "Hanya SME yang bisa melihat pending review knowledge"
            };
        }

        var response = await _knowledgeRepository.GetAllPendingReviewKnowledgeAsync(userId);
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                Status = k.Status,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllInReviewKnowledgeAsync(Guid userId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(userId);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "User tidak ditemukan"
            };
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "Hanya SME yang bisa melihat in review knowledge"
            };
        }

        var response = await _knowledgeRepository.GetAllInReviewKnowledgeAsync(userId);
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                Status = k.Status,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<Response<KnowledgeResponseDto>> ReviewKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(actionBy);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new Response<KnowledgeResponseDto>(false, "User tidak ditemukan", null);
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new Response<KnowledgeResponseDto>(false, "Hanya SME yang bisa review knowledge", null);
        }

        return await _knowledgeRepository.ReviewKnowledgeAsync(actionBy, knowledgeId);
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllRejectKnowledgeAsync(Guid userId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(userId);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "User tidak ditemukan"
            };
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "Hanya SME yang bisa melihat rejected knowledge"
            };
        }

        var response = await _knowledgeRepository.GetAllRejectKnowledgeAsync(userId);
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                Status = k.Status,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllApproveKnowledgeAsync(Guid userId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(userId);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "User tidak ditemukan"
            };
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new ResponseList<KnowledgeResponseDto>(new List<KnowledgeResponseDto>())
            {
                Status = false,
                Message = "Hanya SME yang bisa melihat approved knowledge"
            };
        }

        var response = await _knowledgeRepository.GetAllApproveKnowledgeAsync(userId);
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                Status = k.Status,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetAllPublishKnowledgeAsync(Guid userId)
    {
        var response = await _knowledgeRepository.GetAllPublishKnowledgeAsync(userId);
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                Status = k.Status,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetRecentlyAddedKnowledgeAsync()
    {
        var response = await _knowledgeRepository.GetRecentlyAddedKnowledgeAsync();
        if (!response.Status)
        {
            return response;
        }

        var mapped = response.Data
            .Select(k => new KnowledgeResponseDto
            {
                Id = k.Id,
                FileName = k.FileName,
                FilePath = BuildAliasedPublicFilePath(k.FilePath, k.FileName),
                OwnerDepartment = k.OwnerDepartment,
                PublishedBy = k.PublishedBy,
                Status = k.Status,
                PublishedAt = k.PublishedAt,
                UpdatedAt = k.UpdatedAt
            })
            .ToList();

        var result = new ResponseList<KnowledgeResponseDto>(mapped, response.TotalCount)
        {
            Status = response.Status,
            Message = response.Message,
            Errors = response.Errors
        };

        return result;
    }

    public async Task<Response<KnowledgeResponseDto>> ApproveKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(actionBy);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new Response<KnowledgeResponseDto>(false, "User tidak ditemukan", null);
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new Response<KnowledgeResponseDto>(false, "Hanya SME yang bisa approve knowledge", null);
        }

        return await _knowledgeRepository.ApproveKnowledgeAsync(actionBy, knowledgeId);
    }

    public async Task<Response<KnowledgeResponseDto>> PublishKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(actionBy);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new Response<KnowledgeResponseDto>(false, "User tidak ditemukan", null);
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new Response<KnowledgeResponseDto>(false, "Hanya SME yang bisa publish knowledge", null);
        }

        return await _knowledgeRepository.PublishKnowledgeAsync(actionBy, knowledgeId);
    }

    public async Task<Response<KnowledgeResponseDto>> RejectKnowledgeAsync(Guid actionBy, Guid knowledgeId)
    {
        var userResponse = await _userRepository.GetUserByIdAsync(actionBy);
        if (!userResponse.Status || userResponse.Data == null)
        {
            return new Response<KnowledgeResponseDto>(false, "User tidak ditemukan", null);
        }

        if (!string.Equals(userResponse.Data.RoleName, "SME", StringComparison.OrdinalIgnoreCase))
        {
            return new Response<KnowledgeResponseDto>(false, "Hanya SME yang bisa reject knowledge", null);
        }

        return await _knowledgeRepository.RejectKnowledgeAsync(actionBy, knowledgeId);
    }

    public async Task<ResponseList<KnowledgeResponseDto>> GetKnowledgeByUserAsync(Guid userId)
    {
        return await _knowledgeRepository.GetKnowledgeByUserAsync(userId);
    }

    public async Task<Response<KnowledgeResponseDto>> GetDetailKnowledgeAsync(Guid knowledgeId)
    {
        return await _knowledgeRepository.GetKnowledgeAsync(knowledgeId);
    }

    public async Task<Response<KnowledgeResponseDto>> UpdateDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId, UpdateDraftKnowledgeRequestDto request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NamaFile))
            {
                return new Response<KnowledgeResponseDto>(false, "namaFile wajib diisi", null);
            }

            var existingResponse = await _knowledgeRepository.GetKnowledgeAsync(knowledgeId);
            if (!existingResponse.Status || existingResponse.Data == null)
            {
                return new Response<KnowledgeResponseDto>(false, existingResponse.Message, null);
            }

            var deptResponse = string.IsNullOrWhiteSpace(request.OwnerDepartment)
                ? await _knowledgeRepository.GetUserDepartmentAsync(submittedBy)
                : await _knowledgeRepository.GetDepartmentByNameAsync(request.OwnerDepartment);

            if (!deptResponse.Status || deptResponse.Data == null)
            {
                return new Response<KnowledgeResponseDto>(false, deptResponse.Message, null);
            }

            var status = KnowledgeStatus.Draft;
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                if (!Enum.TryParse<KnowledgeStatus>(request.Status.Trim(), true, out status))
                {
                    return new Response<KnowledgeResponseDto>(false, $"status '{request.Status}' tidak valid", null);
                }
            }

            var deptSlug = SanitizePathSegment(deptResponse.Data.Slug);
            if (string.IsNullOrWhiteSpace(deptSlug))
            {
                deptSlug = "general";
            }

            var uploadsRoot = Path.Combine(
                _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"),
                "km-assets",
                deptSlug
            );
            Directory.CreateDirectory(uploadsRoot);

            var filePath = existingResponse.Data.FilePath;
            var targetFileName = Path.GetFileName(filePath);
            var targetPhysicalPath = GetPhysicalPathFromPublicPath(filePath);

            if (request.Attachment != null && request.Attachment.Length > 0)
            {
                var requestedExt = NormalizeExtension(Path.GetExtension(Path.GetFileName(request.Attachment.FileName)));
                if (!string.Equals(requestedExt, ".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return new Response<KnowledgeResponseDto>(false, "attachment harus file .pdf", null);
                }

                targetFileName = $"{Guid.NewGuid():N}.pdf";
                targetPhysicalPath = Path.Combine(uploadsRoot, targetFileName);

                var attempt = 0;
                while (File.Exists(targetPhysicalPath) && attempt < 3)
                {
                    attempt++;
                    targetFileName = $"{Guid.NewGuid():N}.pdf";
                    targetPhysicalPath = Path.Combine(uploadsRoot, targetFileName);
                }

                if (File.Exists(targetPhysicalPath))
                {
                    return new Response<KnowledgeResponseDto>(false, "Gagal membuat nama file unik", null);
                }

                await using (var stream = new FileStream(targetPhysicalPath, FileMode.CreateNew))
                {
                    await request.Attachment.CopyToAsync(stream);
                }

                TryDeletePhysicalFile(existingResponse.Data.FilePath);
                filePath = $"/km-assets/{deptSlug}/{targetFileName}";
            }
            else
            {
                if (IsPublicKmAssetPath(existingResponse.Data.FilePath))
                {
                    var desiredPublicPath = $"/km-assets/{deptSlug}/{targetFileName}";
                    if (!string.Equals(existingResponse.Data.FilePath, desiredPublicPath, StringComparison.OrdinalIgnoreCase))
                    {
                        var desiredPhysicalPath = Path.Combine(uploadsRoot, targetFileName);
                        var currentPhysicalPath = GetPhysicalPathFromPublicPath(existingResponse.Data.FilePath);

                        if (!string.IsNullOrWhiteSpace(currentPhysicalPath) && File.Exists(currentPhysicalPath))
                        {
                            if (!File.Exists(desiredPhysicalPath))
                            {
                                File.Move(currentPhysicalPath, desiredPhysicalPath);
                                filePath = desiredPublicPath;
                            }
                        }
                    }
                }
            }

            return await _knowledgeRepository.UpdateDraftKnowledgeAsync(
                submittedBy,
                knowledgeId,
                deptResponse.Data.Id,
                request.NamaFile,
                filePath,
                (int)status
            );
        }
        catch (Exception ex)
        {
            return new Response<KnowledgeResponseDto>(false, $"Error update knowledge: {ex.Message}", null);
        }
    }

    public async Task<Response<KnowledgeResponseDto>> SubmitDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId)
    {
        return await _knowledgeRepository.SubmitDraftKnowledgeAsync(submittedBy, knowledgeId);
    }

    public Task<ResponseList<string>> GetKnowledgeStatusesAsync()
    {
        var values = Enum.GetValues<KnowledgeStatus>()
            .Select(s =>
            {
                var member = typeof(KnowledgeStatus).GetMember(s.ToString()).FirstOrDefault();
                var display = member?.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
                return string.IsNullOrWhiteSpace(display?.Name) ? s.ToString() : display!.Name!;
            })
            .ToList();

        return Task.FromResult(new ResponseList<string>(values));
    }

    public async Task<BaseResponse> DeleteDraftKnowledgeAsync(Guid submittedBy, Guid knowledgeId)
    {
        var existingResponse = await _knowledgeRepository.GetKnowledgeAsync(knowledgeId);
        if (!existingResponse.Status || existingResponse.Data == null)
        {
            return new BaseResponse(false, existingResponse.Message);
        }

        var response = await _knowledgeRepository.DeleteDraftKnowledgeAsync(submittedBy, knowledgeId);
        if (response.Status)
        {
            TryDeletePhysicalFile(existingResponse.Data.FilePath);
        }

        return response;
    }

    private static string SanitizePathSegment(string value)
    {
        value = value.Trim().ToLowerInvariant();
        value = Regex.Replace(value, @"\s+", "-");
        value = Regex.Replace(value, @"[^a-z0-9\-]", "");
        value = Regex.Replace(value, @"-+", "-");
        return value.Trim('-');
    }

    private static string BuildAliasedPublicFilePath(string publicPath, string fileName)
    {
        if (string.IsNullOrWhiteSpace(publicPath) || string.IsNullOrWhiteSpace(fileName))
        {
            return publicPath;
        }

        var aliasBaseName = BuildAliasBaseName(fileName);
        if (string.IsNullOrWhiteSpace(aliasBaseName))
        {
            return publicPath;
        }

        var pathWithoutQuery = publicPath.Split('?', '#')[0];
        var lastSlash = pathWithoutQuery.LastIndexOf('/');
        if (lastSlash < 0)
        {
            return publicPath;
        }

        var directory = pathWithoutQuery.Substring(0, lastSlash + 1);
        var rawExtension = Path.GetExtension(pathWithoutQuery);
        var extension = string.IsNullOrWhiteSpace(rawExtension) ? ".pdf" : NormalizeExtension(rawExtension);

        var aliased = directory + aliasBaseName + extension;
        if (string.Equals(pathWithoutQuery, aliased, StringComparison.OrdinalIgnoreCase))
        {
            return publicPath;
        }

        var suffix = publicPath.Substring(pathWithoutQuery.Length);
        return aliased + suffix;
    }

    private static string BuildAliasBaseName(string fileName)
    {
        var value = fileName.Trim().ToLowerInvariant();
        value = Regex.Replace(value, @"\s+", "_");
        value = Regex.Replace(value, @"[^a-z0-9_]", "");
        value = Regex.Replace(value, @"_+", "_");
        return value.Trim('_');
    }

    private static string NormalizeExtension(string ext)
    {
        if (string.IsNullOrWhiteSpace(ext))
        {
            return ".bin";
        }

        if (!ext.StartsWith(".", StringComparison.Ordinal))
        {
            ext = "." + ext;
        }

        ext = ext.Trim();
        ext = Regex.Replace(ext, @"[^a-zA-Z0-9\.]", "");

        if (ext.Length > 15)
        {
            ext = ext.Substring(0, 15);
        }

        return ext.ToLowerInvariant();
    }

    private bool IsPublicKmAssetPath(string publicPath)
    {
        return !string.IsNullOrWhiteSpace(publicPath) &&
               publicPath.StartsWith("/km-assets/", StringComparison.OrdinalIgnoreCase);
    }

    private string GetPhysicalPathFromPublicPath(string publicPath)
    {
        if (!IsPublicKmAssetPath(publicPath))
        {
            return string.Empty;
        }

        var relativePath = publicPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, relativePath);
    }

    private void TryDeletePhysicalFile(string publicPath)
    {
        var physicalPath = GetPhysicalPathFromPublicPath(publicPath);
        if (string.IsNullOrWhiteSpace(physicalPath))
        {
            return;
        }

        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
        }
    }
}
