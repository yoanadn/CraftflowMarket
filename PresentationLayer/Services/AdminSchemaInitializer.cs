using DataLayer;
using Microsoft.EntityFrameworkCore;

namespace PresentationLayer.Services;

public static class AdminSchemaInitializer
{
    public static async Task EnsureAsync(CraftflowDbContext context)
    {
        const string sql = """
            IF OBJECT_ID(N'[BanRecords]', N'U') IS NULL
            BEGIN
                CREATE TABLE [BanRecords](
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [UserId] INT NOT NULL,
                    [BannedUntil] DATETIME2 NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [ModifiedOn] DATETIME2 NULL
                );
            END;

            IF OBJECT_ID(N'[Reports]', N'U') IS NULL
            BEGIN
                CREATE TABLE [Reports](
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [TargetType] INT NOT NULL,
                    [TargetId] INT NOT NULL,
                    [Status] INT NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [ModifiedOn] DATETIME2 NULL
                );
            END;

            IF OBJECT_ID(N'[ModerationActions]', N'U') IS NULL
            BEGIN
                CREATE TABLE [ModerationActions](
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [ReportId] INT NOT NULL,
                    [Action] NVARCHAR(MAX) NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [ModifiedOn] DATETIME2 NULL
                );
            END;

            IF OBJECT_ID(N'[SystemSettings]', N'U') IS NULL
            BEGIN
                CREATE TABLE [SystemSettings](
                    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                    [Key] NVARCHAR(MAX) NOT NULL,
                    [Value] NVARCHAR(MAX) NOT NULL,
                    [CreatedOn] DATETIME2 NOT NULL,
                    [ModifiedOn] DATETIME2 NULL
                );
            END;
            """;

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
