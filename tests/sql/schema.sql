IF DB_ID('NotificationPreferences') IS NULL
BEGIN
    CREATE DATABASE [NotificationPreferences];
END
GO

USE [NotificationPreferences];
GO

IF OBJECT_ID('dbo.NotificationTopics', 'U') IS NOT NULL DROP TABLE [dbo].[NotificationTopics];
IF OBJECT_ID('dbo.NotificationCategories', 'U') IS NOT NULL DROP TABLE [dbo].[NotificationCategories];
GO

CREATE TABLE [dbo].[NotificationCategories] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [Code] VARCHAR(50) NOT NULL,
    [DisplayName] NVARCHAR(100) NOT NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_NotificationCategories_IsActive] DEFAULT (1),
    [CreatedUtc] DATETIME2(7) NOT NULL CONSTRAINT [DF_NotificationCategories_CreatedUtc] DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT [PK_NotificationCategories] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [UQ_NotificationCategories_Code] UNIQUE NONCLUSTERED ([Code] ASC)
);

CREATE TABLE [dbo].[NotificationTopics] (
    [Id] INT IDENTITY(1,1) NOT NULL,
    [CategoryId] INT NOT NULL,
    [Code] VARCHAR(100) NOT NULL,
    [DisplayName] NVARCHAR(150) NOT NULL,
    [Description] NVARCHAR(250) NULL,
    [IsActive] BIT NOT NULL CONSTRAINT [DF_NotificationTopics_IsActive] DEFAULT (1),
    [CreatedUtc] DATETIME2(7) NOT NULL CONSTRAINT [DF_NotificationTopics_CreatedUtc] DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT [PK_NotificationTopics] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_NotificationTopics_NotificationCategories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[NotificationCategories] ([Id]),
    CONSTRAINT [UQ_NotificationTopics_CategoryId_Code] UNIQUE NONCLUSTERED ([CategoryId] ASC, [Code] ASC)
);
GO

SET IDENTITY_INSERT [dbo].[NotificationCategories] ON;
INSERT INTO [dbo].[NotificationCategories] ([Id], [Code], [DisplayName], [IsActive]) VALUES
    (1, 'ACCOUNT',   N'Account & Security', 1),
    (2, 'ORDERS',    N'Orders & Payments',  1),
    (3, 'MARKETING', N'Marketing',          0),
    (4, 'EMPTY',     N'Empty Category',     1);
SET IDENTITY_INSERT [dbo].[NotificationCategories] OFF;

SET IDENTITY_INSERT [dbo].[NotificationTopics] ON;
INSERT INTO [dbo].[NotificationTopics] ([Id], [CategoryId], [Code], [DisplayName], [Description], [IsActive]) VALUES
    (1,  1, 'SECURITY_ALERTS',  N'Sign-in alerts',        N'Get notified about new sign-ins to your account.', 1),
    (2,  1, 'PASSWORD_CHANGES', N'Password changes',      NULL,                                               1),
    (3,  1, 'RETIRED_TOPIC',    N'Retired topic',         N'Inactive topic that must not appear.',             0),
    (4,  2, 'ORDER_STATUS',     N'Order status updates',  N'Shipping and delivery updates.',                    1),
    (5,  2, 'PAYMENT_FAILED',   N'Payment failures',      N'When a payment cannot be processed.',               1),
    (6,  3, 'PROMOTIONS',       N'Promotions',            N'Category is inactive so this must not appear.',      1),
    (7,  4, 'ALL_OFF',          N'Everything off',        N'Category has no active topics.',                     0);
SET IDENTITY_INSERT [dbo].[NotificationTopics] OFF;
GO

SELECT COUNT(*) AS CategoryCount FROM [dbo].[NotificationCategories];
SELECT COUNT(*) AS TopicCount FROM [dbo].[NotificationTopics];
