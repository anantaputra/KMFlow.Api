using KMFlow.Application.DTOs.Knowledges;
using KMFlow.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Security.Claims;
using System.Text;

namespace KMFlow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class KnowledgeController : Controller
{
    private readonly IKnowledgeService _knowledgeService;
    private readonly IWebHostEnvironment _environment;

    public KnowledgeController(IKnowledgeService knowledgeService, IWebHostEnvironment environment)
    {
        _knowledgeService = knowledgeService;
        _environment = environment;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var response = await _knowledgeService.GetAllKnowledgeAsync();
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.GetKnowledgeStatsAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? query, [FromQuery] string? department)
    {
        var response = await _knowledgeService.SearchKnowledgeAsync(query, department);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("pending-review")]
    public async Task<IActionResult> GetAllPendingReview()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.GetAllPendingReviewKnowledgeAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("in-review")]
    public async Task<IActionResult> GetAllInReview()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.GetAllInReviewKnowledgeAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("rejected")]
    public async Task<IActionResult> GetAllRejected()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.GetAllRejectKnowledgeAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("approved")]
    public async Task<IActionResult> GetAllApproved()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.GetAllApproveKnowledgeAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("published")]
    public async Task<IActionResult> GetAllPublished()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.GetAllPublishKnowledgeAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("recently-added")]
    public async Task<IActionResult> GetRecentlyAdded()
    {
        var response = await _knowledgeService.GetRecentlyAddedKnowledgeAsync();
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyKnowledge()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.GetKnowledgeByUserAsync(userId);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var response = await _knowledgeService.GetDetailKnowledgeAsync(id);
        if (!response.Status)
            return NotFound(response);

        return Ok(response);
    }

    [HttpGet("{id:guid}/download")]
    public async Task<IActionResult> Download(Guid id)
    {
        var response = await _knowledgeService.GetDetailKnowledgeAsync(id);
        if (!response.Status || response.Data == null)
            return NotFound(response);

        var physicalPath = GetPhysicalPathFromPublicPath(response.Data.FilePath);
        if (string.IsNullOrWhiteSpace(physicalPath) || !System.IO.File.Exists(physicalPath))
            return NotFound(new BaseResponse(false, "File tidak ditemukan"));

        var extension = GetExtensionFromPublicPath(response.Data.FilePath);
        var downloadName = BuildDownloadFileName(response.Data.FileName, extension);
        var contentType = string.Equals(extension, ".pdf", StringComparison.OrdinalIgnoreCase)
            ? "application/pdf"
            : "application/octet-stream";

        return PhysicalFile(physicalPath, contentType, downloadName, enableRangeProcessing: true);
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] AddKnowledgeWithFileRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.AddKnowledgeAsync(userId, request);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateDraft(Guid id, [FromForm] UpdateDraftKnowledgeRequestDto request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.UpdateDraftKnowledgeAsync(userId, id, request);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("{id:guid}/submit")]
    public async Task<IActionResult> SubmitDraft(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.SubmitDraftKnowledgeAsync(userId, id);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpPost("{id:guid}/review")]
    public async Task<IActionResult> Review(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.ReviewKnowledgeAsync(userId, id);
        if (!response.Status)
        {
            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.ApproveKnowledgeAsync(userId, id);
        if (!response.Status)
        {
            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<IActionResult> Publish(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.PublishKnowledgeAsync(userId, id);
        if (!response.Status)
        {
            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.RejectKnowledgeAsync(userId, id);
        if (!response.Status)
        {
            if (response.Message.Contains("tidak ditemukan", StringComparison.OrdinalIgnoreCase))
                return NotFound(response);

            return BadRequest(response);
        }

        return Ok(response);
    }

    [HttpGet("statuses")]
    public async Task<IActionResult> GetStatuses()
    {
        var response = await _knowledgeService.GetKnowledgeStatusesAsync();
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteDraft(Guid id)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
            return Unauthorized(new BaseResponse(false, "Invalid user token"));

        var response = await _knowledgeService.DeleteDraftKnowledgeAsync(userId, id);
        if (!response.Status)
            return BadRequest(response);

        return Ok(response);
    }

    private bool IsPublicKmAssetPath(string publicPath)
    {
        return !string.IsNullOrWhiteSpace(publicPath) &&
               publicPath.StartsWith("/km-assets/", StringComparison.OrdinalIgnoreCase);
    }

    private string GetPhysicalPathFromPublicPath(string publicPath)
    {
        if (!IsPublicKmAssetPath(publicPath))
            return string.Empty;

        var pathWithoutQuery = publicPath.Split('?', '#')[0];
        var relativePath = pathWithoutQuery.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var webRoot = _environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        return Path.Combine(webRoot, relativePath);
    }

    private static string GetExtensionFromPublicPath(string publicPath)
    {
        if (string.IsNullOrWhiteSpace(publicPath))
            return ".pdf";

        var pathWithoutQuery = publicPath.Split('?', '#')[0];
        var ext = Path.GetExtension(pathWithoutQuery);
        return string.IsNullOrWhiteSpace(ext) ? ".pdf" : ext;
    }

    private static string BuildDownloadFileName(string fileNameFromDb, string extensionFromStoredFile)
    {
        var baseName = Path.GetFileNameWithoutExtension((fileNameFromDb ?? string.Empty).Trim());
        if (string.IsNullOrWhiteSpace(baseName))
            baseName = "knowledge";

        baseName = SanitizeFileName(baseName);
        var extension = string.IsNullOrWhiteSpace(extensionFromStoredFile) ? ".pdf" : extensionFromStoredFile.Trim();
        if (!extension.StartsWith(".", StringComparison.Ordinal))
            extension = "." + extension;

        return baseName + extension;
    }

    private static string SanitizeFileName(string fileName)
    {
        var value = (fileName ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(value))
            return "knowledge";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (char.IsControl(ch) || Array.IndexOf(invalidChars, ch) >= 0)
            {
                sb.Append('_');
                continue;
            }

            sb.Append(ch);
        }

        var result = sb.ToString().Trim();
        return string.IsNullOrWhiteSpace(result) ? "knowledge" : result;
    }
}
