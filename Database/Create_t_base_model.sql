IF OBJECT_ID(N'dbo.t_base_model', N'U') IS NOT NULL
    DROP TABLE dbo.t_base_model;
GO

CREATE TABLE dbo.t_base_model (
    id nvarchar(36) NOT NULL DEFAULT NEWID(),
    factoryCode nvarchar(200) NOT NULL,
    model nvarchar(200) NOT NULL,
    modelDesc nvarchar(200) NULL,
    color nvarchar(200) NULL,
    imeiCount int NULL DEFAULT 2,
    boxNumber int NULL DEFAULT 10,
    note nvarchar(200) NULL,
    createDate datetime NULL,
    createUser nvarchar(200) NULL,
    modifyDate datetime NULL,
    modifyUser nvarchar(200) NULL,
    CONSTRAINT PK_t_base_model PRIMARY KEY (factoryCode, model)
);
GO

-- Sample data for demonstration
INSERT INTO dbo.t_base_model (factoryCode, model, modelDesc, color, imeiCount, boxNumber, note, createDate, createUser)
VALUES
    (N'mobile', N'X100', N'Smartphone X100', N'Black', 2, 10, N'Launch batch', GETDATE(), N'exam_user'),
    (N'mobile', N'X200', N'Smartphone X200', N'White', 2, 10, NULL, GETDATE(), N'exam_user'),
    (N'tablet', N'T100', N'Tablet T100', N'Gray', 2, 10, NULL, GETDATE(), N'exam_user');
GO

PRINT 'dbo.t_base_model created.';
GO
