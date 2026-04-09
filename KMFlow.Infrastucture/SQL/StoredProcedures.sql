CREATE OR ALTER PROCEDURE dbo.sp_Department_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.Id,
        d.DeptName AS Name,
        d.DeptSlug AS Slug
    FROM dbo.Departments d
    ORDER BY d.DeptName;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Department_GetById
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        d.Id,
        d.DeptName AS Name,
        d.DeptSlug AS Slug
    FROM dbo.Departments d
    WHERE d.Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Department_GetByName
    @Name nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        d.Id,
        d.DeptName AS Name,
        d.DeptSlug AS Slug
    FROM dbo.Departments d
    WHERE d.DeptName = @Name OR d.DeptSlug = @Name
    ORDER BY d.DeptName;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Department_Create
    @CreatedBy uniqueidentifier,
    @Name nvarchar(255),
    @Slug nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Now datetime2 = SYSUTCDATETIME();
    DECLARE @Id uniqueidentifier = NEWID();

    INSERT INTO dbo.Departments (Id, DeptName, DeptSlug, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
    VALUES (@Id, @Name, @Slug, @CreatedBy, @Now, @CreatedBy, @Now);

    SELECT
        d.Id,
        d.DeptName AS Name,
        d.DeptSlug AS Slug
    FROM dbo.Departments d
    WHERE d.Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Department_Update
    @UpdatedBy uniqueidentifier,
    @Id uniqueidentifier,
    @Name nvarchar(255),
    @Slug nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Id = @Id)
    BEGIN
        RETURN;
    END

    UPDATE dbo.Departments
    SET
        DeptName = @Name,
        DeptSlug = @Slug,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id;

    SELECT
        d.Id,
        d.DeptName AS Name,
        d.DeptSlug AS Slug
    FROM dbo.Departments d
    WHERE d.Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Department_Delete
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Id = @Id)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Department tidak ditemukan' AS nvarchar(4000)) AS Message;
        RETURN;
    END

    BEGIN TRY
        DELETE FROM dbo.Departments WHERE Id = @Id;

        SELECT
            CAST(1 AS bit) AS IsSuccess,
            CAST(N'Department berhasil dihapus' AS nvarchar(4000)) AS Message;
    END TRY
    BEGIN CATCH
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(ERROR_MESSAGE() AS nvarchar(4000)) AS Message;
    END CATCH
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Role_GetAll
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        r.Id,
        r.RoleName AS Name
    FROM dbo.Roles r
    ORDER BY r.RoleName;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetLoginByEmail
    @Email nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        u.Id,
        u.Name,
        u.Email,
        u.PasswordHash,
        u.DeptId,
        u.RoleId,
        u.IsActive,
        u.CreatedBy,
        u.CreatedAt,
        u.UpdatedBy,
        u.UpdatedAt,
        d.DeptName AS DeptName,
        d.DeptSlug AS DeptSlug,
        r.RoleName AS RoleName
    FROM dbo.Users u
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    WHERE u.Email = @Email AND u.IsActive = 1
    ORDER BY u.CreatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetLoginById
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        u.Id,
        u.Name,
        u.Email,
        u.PasswordHash,
        u.DeptId,
        u.RoleId,
        u.IsActive,
        u.CreatedBy,
        u.CreatedAt,
        u.UpdatedBy,
        u.UpdatedAt,
        d.DeptName AS DeptName,
        d.DeptSlug AS DeptSlug,
        r.RoleName AS RoleName
    FROM dbo.Users u
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    WHERE u.Id = @Id AND u.IsActive = 1;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_Create
    @CreatedBy uniqueidentifier,
    @Id uniqueidentifier,
    @Name nvarchar(255),
    @Email nvarchar(255),
    @DeptId uniqueidentifier,
    @RoleId uniqueidentifier,
    @PasswordHash nvarchar(4000)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Email sudah terdaftar' AS nvarchar(4000)) AS ResponseMessage,
            CAST(@Id AS uniqueidentifier) AS Id,
            CAST(@Name AS nvarchar(255)) AS Name,
            CAST(@Email AS nvarchar(255)) AS Email,
            CAST(N'' AS nvarchar(255)) AS RoleName,
            CAST(N'' AS nvarchar(255)) AS DeptName,
            CAST(SYSUTCDATETIME() AS datetime2) AS CreatedAt;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Id = @DeptId)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Department tidak valid' AS nvarchar(4000)) AS ResponseMessage,
            CAST(@Id AS uniqueidentifier) AS Id,
            CAST(@Name AS nvarchar(255)) AS Name,
            CAST(@Email AS nvarchar(255)) AS Email,
            CAST(N'' AS nvarchar(255)) AS RoleName,
            CAST(N'' AS nvarchar(255)) AS DeptName,
            CAST(SYSUTCDATETIME() AS datetime2) AS CreatedAt;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Id = @RoleId)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Role tidak valid' AS nvarchar(4000)) AS ResponseMessage,
            CAST(@Id AS uniqueidentifier) AS Id,
            CAST(@Name AS nvarchar(255)) AS Name,
            CAST(@Email AS nvarchar(255)) AS Email,
            CAST(N'' AS nvarchar(255)) AS RoleName,
            CAST(N'' AS nvarchar(255)) AS DeptName,
            CAST(SYSUTCDATETIME() AS datetime2) AS CreatedAt;
        RETURN;
    END

    DECLARE @Now datetime2 = SYSUTCDATETIME();

    INSERT INTO dbo.Users (Id, Name, Email, PasswordHash, DeptId, RoleId, IsActive, CreatedBy, CreatedAt, UpdatedBy, UpdatedAt)
    VALUES (@Id, @Name, @Email, @PasswordHash, @DeptId, @RoleId, 1, @CreatedBy, @Now, @CreatedBy, @Now);

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'User berhasil dibuat' AS nvarchar(4000)) AS ResponseMessage,
        u.Id,
        u.Name,
        u.Email,
        r.RoleName AS RoleName,
        d.DeptName AS DeptName,
        u.CreatedAt
    FROM dbo.Users u
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    WHERE u.Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_Update
    @UpdatedBy uniqueidentifier,
    @Id uniqueidentifier,
    @Name nvarchar(255),
    @Email nvarchar(255),
    @DeptId uniqueidentifier,
    @RoleId uniqueidentifier,
    @PasswordHash nvarchar(4000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id AND IsActive = 1)
    BEGIN
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Users WHERE Email = @Email AND Id <> @Id)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Email sudah dipakai user lain' AS nvarchar(4000)) AS ResponseMessage,
            CAST(@Id AS uniqueidentifier) AS Id,
            CAST(@Name AS nvarchar(255)) AS Name,
            CAST(@Email AS nvarchar(255)) AS Email,
            CAST(N'' AS nvarchar(255)) AS RoleName,
            CAST(N'' AS nvarchar(255)) AS DeptName,
            CAST(SYSUTCDATETIME() AS datetime2) AS CreatedAt;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Id = @DeptId)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Department tidak valid' AS nvarchar(4000)) AS ResponseMessage,
            CAST(@Id AS uniqueidentifier) AS Id,
            CAST(@Name AS nvarchar(255)) AS Name,
            CAST(@Email AS nvarchar(255)) AS Email,
            CAST(N'' AS nvarchar(255)) AS RoleName,
            CAST(N'' AS nvarchar(255)) AS DeptName,
            CAST(SYSUTCDATETIME() AS datetime2) AS CreatedAt;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Roles WHERE Id = @RoleId)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Role tidak valid' AS nvarchar(4000)) AS ResponseMessage,
            CAST(@Id AS uniqueidentifier) AS Id,
            CAST(@Name AS nvarchar(255)) AS Name,
            CAST(@Email AS nvarchar(255)) AS Email,
            CAST(N'' AS nvarchar(255)) AS RoleName,
            CAST(N'' AS nvarchar(255)) AS DeptName,
            CAST(SYSUTCDATETIME() AS datetime2) AS CreatedAt;
        RETURN;
    END

    UPDATE dbo.Users
    SET
        Name = @Name,
        Email = @Email,
        DeptId = @DeptId,
        RoleId = @RoleId,
        PasswordHash = CASE WHEN @PasswordHash IS NULL OR LTRIM(RTRIM(@PasswordHash)) = N'' THEN PasswordHash ELSE @PasswordHash END,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @Id;

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'User berhasil diperbarui' AS nvarchar(4000)) AS ResponseMessage,
        u.Id,
        u.Name,
        u.Email,
        r.RoleName AS RoleName,
        d.DeptName AS DeptName,
        u.CreatedAt
    FROM dbo.Users u
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    WHERE u.Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_SetRole
    @UpdatedBy uniqueidentifier,
    @TargetUserId uniqueidentifier,
    @RoleName nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @TargetUserId AND IsActive = 1)
    BEGIN
        RETURN;
    END

    DECLARE @RoleId uniqueidentifier;
    SELECT TOP (1) @RoleId = r.Id FROM dbo.Roles r WHERE r.RoleName = @RoleName;

    IF @RoleId IS NULL
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Role tidak valid' AS nvarchar(4000)) AS ResponseMessage,
            CAST(@TargetUserId AS uniqueidentifier) AS Id,
            CAST(N'' AS nvarchar(255)) AS Name,
            CAST(N'' AS nvarchar(255)) AS Email,
            CAST(N'' AS nvarchar(255)) AS RoleName,
            CAST(N'' AS nvarchar(255)) AS DeptName,
            CAST(SYSUTCDATETIME() AS datetime2) AS CreatedAt;
        RETURN;
    END

    UPDATE dbo.Users
    SET
        RoleId = @RoleId,
        UpdatedBy = @UpdatedBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @TargetUserId;

    INSERT INTO dbo.Notifications (UserID, KnowledgeID, Type, Title, Message, IsRead, RelatedActionBy)
    VALUES (
        @TargetUserId,
        NULL,
        'RoleChanged',
        'Perubahan Role',
        CONCAT('Role anda telah diubah menjadi ', @RoleName),
        0,
        @UpdatedBy
    );

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Role user berhasil diubah' AS nvarchar(4000)) AS ResponseMessage,
        u.Id,
        u.Name,
        u.Email,
        r.RoleName AS RoleName,
        d.DeptName AS DeptName,
        u.CreatedAt
    FROM dbo.Users u
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    WHERE u.Id = @TargetUserId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_SoftDelete
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @Id)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'User tidak ditemukan' AS nvarchar(4000)) AS Message;
        RETURN;
    END

    BEGIN TRY
        BEGIN TRANSACTION;

        DELETE FROM dbo.Notifications WHERE UserID = @Id OR RelatedActionBy = @Id;
        DELETE FROM dbo.KnowledgeHistories WHERE UserId = @Id;
        DELETE FROM dbo.Knowledges WHERE SubmittedBy = @Id;
        DELETE FROM dbo.Users WHERE Id = @Id;

        COMMIT TRANSACTION;

        SELECT
            CAST(1 AS bit) AS IsSuccess,
            CAST(N'User berhasil dihapus' AS nvarchar(4000)) AS Message;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(ERROR_MESSAGE() AS nvarchar(4000)) AS Message;
    END CATCH
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetByRole
    @RoleName nvarchar(255)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.Id,
        u.Name,
        u.Email,
        r.RoleName AS RoleName,
        d.DeptName AS DeptName,
        u.CreatedAt
    FROM dbo.Users u
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    WHERE u.IsActive = 1 AND r.RoleName = @RoleName
    ORDER BY u.CreatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetAllActive
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.Id,
        u.Name,
        u.Email,
        r.RoleName AS RoleName,
        d.DeptName AS DeptName,
        u.CreatedAt
    FROM dbo.Users u
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    WHERE u.IsActive = 1
    ORDER BY u.CreatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_User_GetByIdActive
    @Id uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        u.Id,
        u.Name,
        u.Email,
        r.RoleName AS RoleName,
        d.DeptName AS DeptName,
        u.CreatedAt
    FROM dbo.Users u
    LEFT JOIN dbo.Roles r ON r.Id = u.RoleId
    LEFT JOIN dbo.Departments d ON d.Id = u.DeptId
    WHERE u.IsActive = 1 AND u.Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Notification_Create
    @RecipientUserId uniqueidentifier,
    @Type nvarchar(50),
    @Title nvarchar(255),
    @Message nvarchar(max),
    @KnowledgeId uniqueidentifier = NULL,
    @RelatedActionBy uniqueidentifier = NULL
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @RecipientUserId AND IsActive = 1)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Recipient user tidak ditemukan' AS nvarchar(4000)) AS ResponseMessage,
            CAST(0 AS int) AS NotificationId,
            CAST(@RecipientUserId AS uniqueidentifier) AS UserId,
            CAST(@KnowledgeId AS uniqueidentifier) AS KnowledgeId,
            CAST(@Type AS nvarchar(50)) AS Type,
            CAST(@Title AS nvarchar(255)) AS Title,
            CAST(@Message AS nvarchar(max)) AS Message,
            CAST(0 AS bit) AS IsRead,
            CAST(@RelatedActionBy AS uniqueidentifier) AS RelatedActionBy,
            CAST(GETDATE() AS datetime2) AS CreatedDate;
        RETURN;
    END

    DECLARE @InsertedId int;

    INSERT INTO dbo.Notifications (UserID, KnowledgeID, Type, Title, Message, IsRead, RelatedActionBy)
    VALUES (@RecipientUserId, @KnowledgeId, @Type, @Title, @Message, 0, @RelatedActionBy);

    SET @InsertedId = CAST(SCOPE_IDENTITY() AS int);

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Notification berhasil dibuat' AS nvarchar(4000)) AS ResponseMessage,
        n.NotificationID AS NotificationId,
        n.UserID AS UserId,
        n.KnowledgeID AS KnowledgeId,
        n.Type,
        n.Title,
        n.Message,
        n.IsRead,
        n.RelatedActionBy,
        CAST(n.CreatedDate AS datetime2) AS CreatedDate
    FROM dbo.Notifications n
    WHERE n.NotificationID = @InsertedId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Notification_GetByUser
    @UserId uniqueidentifier,
    @IsRead bit = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        n.NotificationID AS NotificationId,
        n.UserID AS UserId,
        n.KnowledgeID AS KnowledgeId,
        n.Type,
        n.Title,
        n.Message,
        n.IsRead,
        n.RelatedActionBy,
        CAST(n.CreatedDate AS datetime2) AS CreatedDate
    FROM dbo.Notifications n
    WHERE
        n.UserID = @UserId
        AND (@IsRead IS NULL OR n.IsRead = @IsRead)
    ORDER BY n.CreatedDate DESC, n.NotificationID DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Notification_GetUnreadCount
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT CAST(COUNT(1) AS int) AS Count
    FROM dbo.Notifications n
    WHERE n.UserID = @UserId AND n.IsRead = 0;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Notification_MarkAsRead
    @UserId uniqueidentifier,
    @NotificationId int
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Notifications WHERE NotificationID = @NotificationId AND UserID = @UserId)
    BEGIN
        RETURN;
    END

    UPDATE dbo.Notifications
    SET IsRead = 1
    WHERE NotificationID = @NotificationId AND UserID = @UserId;

    SELECT
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Notification berhasil ditandai dibaca' AS nvarchar(4000)) AS Message;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Notification_MarkAllAsRead
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Notifications WHERE UserID = @UserId AND IsRead = 0)
    BEGIN
        RETURN;
    END

    UPDATE dbo.Notifications
    SET IsRead = 1
    WHERE UserID = @UserId AND IsRead = 0;

    SELECT
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Notification berhasil ditandai dibaca' AS nvarchar(4000)) AS Message;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetUserDepartment
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        d.Id,
        d.DeptName AS Name,
        d.DeptSlug AS Slug
    FROM dbo.Users u
    INNER JOIN dbo.Departments d ON d.Id = u.DeptId
    WHERE u.Id = @UserId AND u.IsActive = 1;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_Create
    @SubmittedBy uniqueidentifier,
    @OwnerDeptId uniqueidentifier,
    @FileName nvarchar(255),
    @FilePath nvarchar(4000),
    @Status int
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Id = @SubmittedBy AND IsActive = 1)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'User submitter tidak valid' AS nvarchar(4000)) AS ResponseMessage,
            CAST(NULL AS uniqueidentifier) AS Id,
            CAST(NULL AS nvarchar(255)) AS FileName,
            CAST(NULL AS nvarchar(4000)) AS FilePath,
            CAST(NULL AS nvarchar(255)) AS OwnerDepartment,
            CAST(NULL AS nvarchar(255)) AS PublishedBy,
            CAST(NULL AS nvarchar(50)) AS Status,
            CAST(NULL AS datetime2) AS PublishedAt,
            CAST(NULL AS datetime2) AS UpdatedAt;
        RETURN;
    END

    IF NOT EXISTS (SELECT 1 FROM dbo.Departments WHERE Id = @OwnerDeptId)
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Department tidak valid' AS nvarchar(4000)) AS ResponseMessage,
            CAST(NULL AS uniqueidentifier) AS Id,
            CAST(NULL AS nvarchar(255)) AS FileName,
            CAST(NULL AS nvarchar(4000)) AS FilePath,
            CAST(NULL AS nvarchar(255)) AS OwnerDepartment,
            CAST(NULL AS nvarchar(255)) AS PublishedBy,
            CAST(NULL AS nvarchar(50)) AS Status,
            CAST(NULL AS datetime2) AS PublishedAt,
            CAST(NULL AS datetime2) AS UpdatedAt;
        RETURN;
    END

    DECLARE @Now datetime2 = SYSUTCDATETIME();
    DECLARE @Id uniqueidentifier = NEWID();
    DECLARE @PublishedAt datetime2 = CASE WHEN @Status = 4 THEN @Now ELSE NULL END;

    INSERT INTO dbo.Knowledges
    (
        Id,
        FileName,
        FilePath,
        OwnerDept,
        Status,
        SubmittedBy,
        SubmittedAt,
        PublishedAt,
        CreatedBy,
        CreatedAt,
        UpdatedBy,
        UpdatedAt
    )
    VALUES
    (
        @Id,
        @FileName,
        @FilePath,
        @OwnerDeptId,
        @Status,
        @SubmittedBy,
        @Now,
        @PublishedAt,
        @SubmittedBy,
        @Now,
        @SubmittedBy,
        @Now
    );

    INSERT INTO dbo.KnowledgeHistories (Id, KnowledgeId, Action, FromStatus, ToStatus, ActionBy, ActionAt)
    VALUES (NEWID(), @Id, @Status, @Status, @Status, @SubmittedBy, @Now);

    IF @Status IN (1, 2)
    BEGIN
        DECLARE @Title nvarchar(255) = N'Knowledge Submitted';
        DECLARE @Msg nvarchar(max) = CONCAT(N'Knowledge "', @FileName, N'" membutuhkan review');

        INSERT INTO dbo.Notifications (UserID, KnowledgeID, Type, Title, Message, IsRead, RelatedActionBy)
        SELECT
            u.Id,
            @Id,
            'KnowledgeSubmitted',
            @Title,
            @Msg,
            0,
            @SubmittedBy
        FROM dbo.Users u
        INNER JOIN dbo.Roles r ON r.Id = u.RoleId
        WHERE u.IsActive = 1 AND r.RoleName IN ('Admin', 'SME');
    END

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Knowledge berhasil dibuat' AS nvarchar(4000)) AS ResponseMessage,
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        CASE k.Status
            WHEN 0 THEN 'Draft'
            WHEN 1 THEN 'Pending Review'
            WHEN 2 THEN 'In Review'
            WHEN 3 THEN 'Approved'
            WHEN 4 THEN 'Published'
            WHEN 5 THEN 'Rejected'
            ELSE CAST(k.Status AS nvarchar(50))
        END AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @Id;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetAllPublished
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Published' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Status = 4
    ORDER BY k.PublishedAt DESC, k.UpdatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetStats
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        CAST((SELECT COUNT(1) FROM dbo.Knowledges WHERE Status = 4) AS int) AS TotalKnowledge,
        CAST((SELECT COUNT(1) FROM dbo.Knowledges WHERE SubmittedBy = @UserId) AS int) AS MyContribution,
        CAST((SELECT COUNT(1) FROM dbo.Knowledges WHERE Status = 1) AS int) AS PendingReview;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_Search
    @Query nvarchar(255) = NULL,
    @Department nvarchar(255) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Q nvarchar(255) = NULLIF(LTRIM(RTRIM(@Query)), N'');
    DECLARE @Dept nvarchar(255) = NULLIF(LTRIM(RTRIM(@Department)), N'');

    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Published' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE
        k.Status = 4
        AND (@Q IS NULL OR k.FileName LIKE '%' + @Q + '%')
        AND (@Dept IS NULL OR d.DeptName = @Dept OR d.DeptSlug = @Dept)
    ORDER BY k.PublishedAt DESC, k.UpdatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetPendingReviewByUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Pending Review' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Users u ON u.Id = @UserId AND u.IsActive = 1
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Status = 1 AND k.OwnerDept = u.DeptId
    ORDER BY k.SubmittedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetInReviewByUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'In Review' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Users u ON u.Id = @UserId AND u.IsActive = 1
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Status = 2 AND k.OwnerDept = u.DeptId
    ORDER BY k.UpdatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_Review
    @ActionBy uniqueidentifier,
    @KnowledgeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Knowledges WHERE Id = @KnowledgeId)
    BEGIN
        RETURN;
    END

    DECLARE @FromStatus int;
    SELECT TOP (1) @FromStatus = Status FROM dbo.Knowledges WHERE Id = @KnowledgeId;

    IF @FromStatus <> 1
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Knowledge tidak dalam status Pending' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    UPDATE dbo.Knowledges
    SET
        Status = 2,
        UpdatedBy = @ActionBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KnowledgeId;

    INSERT INTO dbo.KnowledgeHistories (Id, KnowledgeId, Action, FromStatus, ToStatus, ActionBy, ActionAt)
    VALUES (NEWID(), @KnowledgeId, 2, @FromStatus, 2, @ActionBy, SYSUTCDATETIME());

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Knowledge berhasil dipindahkan ke In Review' AS nvarchar(4000)) AS ResponseMessage,
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'In Review' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @KnowledgeId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetRejectedByUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Rejected' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Users u ON u.Id = @UserId AND u.IsActive = 1
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Status = 5 AND k.OwnerDept = u.DeptId
    ORDER BY k.UpdatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetApprovedByUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Approved' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Users u ON u.Id = @UserId AND u.IsActive = 1
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Status = 3 AND k.OwnerDept = u.DeptId
    ORDER BY k.UpdatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetPublishedByUser
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Published' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Status = 4 AND k.SubmittedBy = @UserId
    ORDER BY k.PublishedAt DESC, k.UpdatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetRecentlyAdded
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (10)
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Published' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Status = 4
    ORDER BY k.CreatedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_Approve
    @ActionBy uniqueidentifier,
    @KnowledgeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Knowledges WHERE Id = @KnowledgeId)
    BEGIN
        RETURN;
    END

    DECLARE @FromStatus int;
    DECLARE @SubmittedBy uniqueidentifier;
    DECLARE @FileName nvarchar(255);
    SELECT TOP (1)
        @FromStatus = Status,
        @SubmittedBy = SubmittedBy,
        @FileName = FileName
    FROM dbo.Knowledges
    WHERE Id = @KnowledgeId;

    IF @FromStatus NOT IN (2, 1)
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Knowledge tidak dalam status yang bisa di-approve' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    UPDATE dbo.Knowledges
    SET
        Status = 3,
        UpdatedBy = @ActionBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KnowledgeId;

    INSERT INTO dbo.KnowledgeHistories (Id, KnowledgeId, Action, FromStatus, ToStatus, ActionBy, ActionAt)
    VALUES (NEWID(), @KnowledgeId, 3, @FromStatus, 3, @ActionBy, SYSUTCDATETIME());

    INSERT INTO dbo.Notifications (UserID, KnowledgeID, Type, Title, Message, IsRead, RelatedActionBy)
    VALUES (
        @SubmittedBy,
        @KnowledgeId,
        'KnowledgeApproved',
        'Knowledge Approved',
        CONCAT('Knowledge "', @FileName, '" telah di-approve'),
        0,
        @ActionBy
    );

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Knowledge berhasil di-approve' AS nvarchar(4000)) AS ResponseMessage,
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Approved' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @KnowledgeId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_Publish
    @ActionBy uniqueidentifier,
    @KnowledgeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Knowledges WHERE Id = @KnowledgeId)
    BEGIN
        RETURN;
    END

    DECLARE @FromStatus int;
    DECLARE @SubmittedBy uniqueidentifier;
    DECLARE @FileName nvarchar(255);
    SELECT TOP (1)
        @FromStatus = Status,
        @SubmittedBy = SubmittedBy,
        @FileName = FileName
    FROM dbo.Knowledges
    WHERE Id = @KnowledgeId;

    IF @FromStatus <> 3
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Knowledge harus dalam status Approved untuk dipublish' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    UPDATE dbo.Knowledges
    SET
        Status = 4,
        PublishedAt = SYSUTCDATETIME(),
        UpdatedBy = @ActionBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KnowledgeId;

    INSERT INTO dbo.KnowledgeHistories (Id, KnowledgeId, Action, FromStatus, ToStatus, ActionBy, ActionAt)
    VALUES (NEWID(), @KnowledgeId, 4, @FromStatus, 4, @ActionBy, SYSUTCDATETIME());

    INSERT INTO dbo.Notifications (UserID, KnowledgeID, Type, Title, Message, IsRead, RelatedActionBy)
    VALUES (
        @SubmittedBy,
        @KnowledgeId,
        'KnowledgePublished',
        'Knowledge Published',
        CONCAT('Knowledge "', @FileName, '" telah dipublish'),
        0,
        @ActionBy
    );

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Knowledge berhasil dipublish' AS nvarchar(4000)) AS ResponseMessage,
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Published' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @KnowledgeId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_Reject
    @ActionBy uniqueidentifier,
    @KnowledgeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Knowledges WHERE Id = @KnowledgeId)
    BEGIN
        RETURN;
    END

    DECLARE @FromStatus int;
    DECLARE @SubmittedBy uniqueidentifier;
    DECLARE @FileName nvarchar(255);
    SELECT TOP (1)
        @FromStatus = Status,
        @SubmittedBy = SubmittedBy,
        @FileName = FileName
    FROM dbo.Knowledges
    WHERE Id = @KnowledgeId;

    IF @FromStatus NOT IN (2, 1)
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Knowledge tidak dalam status yang bisa di-reject' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    UPDATE dbo.Knowledges
    SET
        Status = 5,
        UpdatedBy = @ActionBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KnowledgeId;

    INSERT INTO dbo.KnowledgeHistories (Id, KnowledgeId, Action, FromStatus, ToStatus, ActionBy, ActionAt)
    VALUES (NEWID(), @KnowledgeId, 5, @FromStatus, 5, @ActionBy, SYSUTCDATETIME());

    INSERT INTO dbo.Notifications (UserID, KnowledgeID, Type, Title, Message, IsRead, RelatedActionBy)
    VALUES (
        @SubmittedBy,
        @KnowledgeId,
        'KnowledgeRejected',
        'Knowledge Rejected',
        CONCAT('Knowledge "', @FileName, '" telah di-reject'),
        0,
        @ActionBy
    );

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Knowledge berhasil di-reject' AS nvarchar(4000)) AS ResponseMessage,
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Rejected' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @KnowledgeId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetById
    @KnowledgeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (1)
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        CASE k.Status
            WHEN 0 THEN 'Draft'
            WHEN 1 THEN 'Pending Review'
            WHEN 2 THEN 'In Review'
            WHEN 3 THEN 'Approved'
            WHEN 4 THEN 'Published'
            WHEN 5 THEN 'Rejected'
            ELSE CAST(k.Status AS nvarchar(50))
        END AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @KnowledgeId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_GetBySubmittedBy
    @UserId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        CASE k.Status
            WHEN 0 THEN 'Draft'
            WHEN 1 THEN 'Pending Review'
            WHEN 2 THEN 'In Review'
            WHEN 3 THEN 'Approved'
            WHEN 4 THEN 'Published'
            WHEN 5 THEN 'Rejected'
            ELSE CAST(k.Status AS nvarchar(50))
        END AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.SubmittedBy = @UserId
    ORDER BY k.SubmittedAt DESC;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_UpdateDraft
    @SubmittedBy uniqueidentifier,
    @KnowledgeId uniqueidentifier,
    @OwnerDeptId uniqueidentifier,
    @FileName nvarchar(255),
    @FilePath nvarchar(4000),
    @Status int
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Knowledges WHERE Id = @KnowledgeId)
    BEGIN
        RETURN;
    END

    DECLARE @CurrentStatus int;
    SELECT TOP (1) @CurrentStatus = Status FROM dbo.Knowledges WHERE Id = @KnowledgeId;

    IF @CurrentStatus <> 0
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Hanya knowledge Draft yang bisa diupdate' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    UPDATE dbo.Knowledges
    SET
        FileName = @FileName,
        FilePath = @FilePath,
        OwnerDept = @OwnerDeptId,
        Status = 0,
        UpdatedBy = @SubmittedBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KnowledgeId AND SubmittedBy = @SubmittedBy;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Tidak diizinkan update draft' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    INSERT INTO dbo.KnowledgeHistories (Id, KnowledgeId, Action, FromStatus, ToStatus, ActionBy, ActionAt)
    VALUES (NEWID(), @KnowledgeId, 0, 0, 0, @SubmittedBy, SYSUTCDATETIME());

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Draft berhasil diupdate' AS nvarchar(4000)) AS ResponseMessage,
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Draft' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @KnowledgeId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_SubmitDraft
    @SubmittedBy uniqueidentifier,
    @KnowledgeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Knowledges WHERE Id = @KnowledgeId)
    BEGIN
        RETURN;
    END

    DECLARE @FromStatus int;
    DECLARE @FileName nvarchar(255);
    SELECT TOP (1) @FromStatus = Status, @FileName = FileName
    FROM dbo.Knowledges
    WHERE Id = @KnowledgeId;

    IF @FromStatus <> 0
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Hanya Draft yang bisa disubmit' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    UPDATE dbo.Knowledges
    SET
        Status = 1,
        UpdatedBy = @SubmittedBy,
        UpdatedAt = SYSUTCDATETIME()
    WHERE Id = @KnowledgeId AND SubmittedBy = @SubmittedBy;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT TOP (1)
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Tidak diizinkan submit draft' AS nvarchar(4000)) AS ResponseMessage,
            k.Id,
            k.FileName,
            k.FilePath,
            d.DeptName AS OwnerDepartment,
            COALESCE(pub.Name, N'') AS PublishedBy,
            CASE k.Status
                WHEN 0 THEN 'Draft'
                WHEN 1 THEN 'Pending Review'
                WHEN 2 THEN 'In Review'
                WHEN 3 THEN 'Approved'
                WHEN 4 THEN 'Published'
                WHEN 5 THEN 'Rejected'
                ELSE CAST(k.Status AS nvarchar(50))
            END AS Status,
            k.PublishedAt,
            k.UpdatedAt AS UpdatedAt
        FROM dbo.Knowledges k
        INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
        LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
        WHERE k.Id = @KnowledgeId;
        RETURN;
    END

    INSERT INTO dbo.KnowledgeHistories (Id, KnowledgeId, Action, FromStatus, ToStatus, ActionBy, ActionAt)
    VALUES (NEWID(), @KnowledgeId, 1, @FromStatus, 1, @SubmittedBy, SYSUTCDATETIME());

    DECLARE @Title nvarchar(255) = N'Knowledge Submitted';
    DECLARE @Msg nvarchar(max) = CONCAT(N'Knowledge "', @FileName, N'" membutuhkan review');

    INSERT INTO dbo.Notifications (UserID, KnowledgeID, Type, Title, Message, IsRead, RelatedActionBy)
    SELECT
        u.Id,
        @KnowledgeId,
        'KnowledgeSubmitted',
        @Title,
        @Msg,
        0,
        @SubmittedBy
    FROM dbo.Users u
    INNER JOIN dbo.Roles r ON r.Id = u.RoleId
    WHERE u.IsActive = 1 AND r.RoleName IN ('Admin', 'SME');

    SELECT TOP (1)
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Draft berhasil disubmit' AS nvarchar(4000)) AS ResponseMessage,
        k.Id,
        k.FileName,
        k.FilePath,
        d.DeptName AS OwnerDepartment,
        COALESCE(pub.Name, N'') AS PublishedBy,
        'Pending Review' AS Status,
        k.PublishedAt,
        k.UpdatedAt AS UpdatedAt
    FROM dbo.Knowledges k
    INNER JOIN dbo.Departments d ON d.Id = k.OwnerDept
    LEFT JOIN dbo.Users pub ON pub.Id = k.SubmittedBy
    WHERE k.Id = @KnowledgeId;
END

GO

CREATE OR ALTER PROCEDURE dbo.sp_Knowledge_DeleteDraft
    @SubmittedBy uniqueidentifier,
    @KnowledgeId uniqueidentifier
AS
BEGIN
    SET NOCOUNT ON;
    IF NOT EXISTS (SELECT 1 FROM dbo.Knowledges WHERE Id = @KnowledgeId)
    BEGIN
        RETURN;
    END

    DECLARE @CurrentStatus int;
    SELECT TOP (1) @CurrentStatus = Status FROM dbo.Knowledges WHERE Id = @KnowledgeId;

    IF @CurrentStatus <> 0
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Hanya Draft yang bisa dihapus' AS nvarchar(4000)) AS Message;
        RETURN;
    END

    DELETE FROM dbo.Knowledges
    WHERE Id = @KnowledgeId AND SubmittedBy = @SubmittedBy AND Status = 0;

    IF @@ROWCOUNT = 0
    BEGIN
        SELECT
            CAST(0 AS bit) AS IsSuccess,
            CAST(N'Tidak diizinkan menghapus draft' AS nvarchar(4000)) AS Message;
        RETURN;
    END

    SELECT
        CAST(1 AS bit) AS IsSuccess,
        CAST(N'Draft berhasil dihapus' AS nvarchar(4000)) AS Message;
END

GO
