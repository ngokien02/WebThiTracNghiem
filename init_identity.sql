IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [HoTen] nvarchar(max) NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);

CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DeThi] (
    [Id] int NOT NULL IDENTITY,
    [TieuDe] nvarchar(max) NOT NULL,
    [MaDe] nvarchar(max) NOT NULL,
    [GioBD] datetime2 NOT NULL,
    [GioKT] datetime2 NOT NULL,
    [SoCauHoi] int NOT NULL,
    [DiemToiDa] float NOT NULL,
    [RandomCauHoi] bit NOT NULL,
    [RandomDapAn] bit NOT NULL,
    [ShowKQ] bit NOT NULL,
    [IdGiangVien] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_DeThi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DeThi_AspNetUsers_IdGiangVien] FOREIGN KEY ([IdGiangVien]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [CauHoi] (
    [Id] int NOT NULL IDENTITY,
    [NoiDung] nvarchar(max) NOT NULL,
    [Loai] nvarchar(max) NOT NULL,
    [DeThiId] int NOT NULL,
    CONSTRAINT [PK_CauHoi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CauHoi_DeThi_DeThiId] FOREIGN KEY ([DeThiId]) REFERENCES [DeThi] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [DapAn] (
    [Id] int NOT NULL IDENTITY,
    [NoiDung] nvarchar(max) NOT NULL,
    [DungSai] bit NOT NULL,
    [CauHoiId] int NOT NULL,
    CONSTRAINT [PK_DapAn] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DapAn_CauHoi_CauHoiId] FOREIGN KEY ([CauHoiId]) REFERENCES [CauHoi] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);

CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;

CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);

CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);

CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);

CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);

CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;

CREATE INDEX [IX_CauHoi_DeThiId] ON [CauHoi] ([DeThiId]);

CREATE INDEX [IX_DapAn_CauHoiId] ON [DapAn] ([CauHoiId]);

CREATE INDEX [IX_DeThi_IdGiangVien] ON [DeThi] ([IdGiangVien]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250704125302_Init', N'9.0.6');

ALTER TABLE [DeThi] ADD [ThoiGian] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250707131818_AddTimeColumn', N'9.0.6');

CREATE TABLE [KetQua] (
    [Id] int NOT NULL IDENTITY,
    [IdSinhVien] nvarchar(450) NOT NULL,
    [DeThiId] int NOT NULL,
    [SoCauDung] int NULL,
    [Diem] float NULL,
    [GioLam] datetime2 NOT NULL,
    [GioNop] datetime2 NULL,
    [TrangThai] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_KetQua] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_KetQua_AspNetUsers_IdSinhVien] FOREIGN KEY ([IdSinhVien]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_KetQua_DeThi_DeThiId] FOREIGN KEY ([DeThiId]) REFERENCES [DeThi] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_KetQua_DeThiId] ON [KetQua] ([DeThiId]);

CREATE INDEX [IX_KetQua_IdSinhVien] ON [KetQua] ([IdSinhVien]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250712092051_AddTableKetQua', N'9.0.6');

ALTER TABLE [AspNetUsers] ADD [AvatarUrl] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [AspNetUsers] ADD [DiaChi] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [AspNetUsers] ADD [GioiTinh] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [AspNetUsers] ADD [Khoa] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [AspNetUsers] ADD [KhoaHoc] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [AspNetUsers] ADD [LopHoc] nvarchar(max) NOT NULL DEFAULT N'';

ALTER TABLE [AspNetUsers] ADD [NgaySinh] datetime2 NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250719141626_EmailRecovery', N'9.0.6');

DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'LopHoc');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [LopHoc] nvarchar(max) NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'KhoaHoc');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [KhoaHoc] nvarchar(max) NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'Khoa');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [Khoa] nvarchar(max) NULL;

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'GioiTinh');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var3 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [GioiTinh] nvarchar(max) NULL;

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'DiaChi');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var4 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [DiaChi] nvarchar(max) NULL;

DECLARE @var5 sysname;
SELECT @var5 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUsers]') AND [c].[name] = N'AvatarUrl');
IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUsers] DROP CONSTRAINT [' + @var5 + '];');
ALTER TABLE [AspNetUsers] ALTER COLUMN [AvatarUrl] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250719143626_AllowNullInfo', N'9.0.6');

ALTER TABLE [AspNetUsers] ADD [CCCD] nvarchar(max) NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250719152408_AddCCCD', N'9.0.6');

CREATE TABLE [ThongBaoAdmin] (
    [Id] int NOT NULL IDENTITY,
    [LoaiTB] nvarchar(max) NOT NULL,
    [TieuDe] nvarchar(max) NOT NULL,
    [GioTB] datetime2 NOT NULL,
    CONSTRAINT [PK_ThongBaoAdmin] PRIMARY KEY ([Id])
);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250824135234_AdminNoti', N'9.0.6');

ALTER TABLE [CauHoi] DROP CONSTRAINT [FK_CauHoi_DeThi_DeThiId];

DECLARE @var6 sysname;
SELECT @var6 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CauHoi]') AND [c].[name] = N'DeThiId');
IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [CauHoi] DROP CONSTRAINT [' + @var6 + '];');
ALTER TABLE [CauHoi] ALTER COLUMN [DeThiId] int NULL;

ALTER TABLE [CauHoi] ADD [ChuDeId] int NOT NULL DEFAULT 0;

CREATE TABLE [ChiTietDeThi] (
    [Id] int NOT NULL IDENTITY,
    [DeThiId] int NOT NULL,
    [CauHoiId] int NOT NULL,
    CONSTRAINT [PK_ChiTietDeThi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChiTietDeThi_CauHoi_CauHoiId] FOREIGN KEY ([CauHoiId]) REFERENCES [CauHoi] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChiTietDeThi_DeThi_DeThiId] FOREIGN KEY ([DeThiId]) REFERENCES [DeThi] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ChiTietKQ] (
    [Id] int NOT NULL IDENTITY,
    [KetQuaId] int NOT NULL,
    [CauHoiId] int NOT NULL,
    [DapAnId] int NULL,
    [Diem] float NOT NULL,
    CONSTRAINT [PK_ChiTietKQ] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChiTietKQ_CauHoi_CauHoiId] FOREIGN KEY ([CauHoiId]) REFERENCES [CauHoi] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChiTietKQ_DapAn_DapAnId] FOREIGN KEY ([DapAnId]) REFERENCES [DapAn] ([Id]),
    CONSTRAINT [FK_ChiTietKQ_KetQua_KetQuaId] FOREIGN KEY ([KetQuaId]) REFERENCES [KetQua] ([Id]) ON DELETE CASCADE
);

CREATE TABLE [ChuDe] (
    [Id] int NOT NULL IDENTITY,
    [TenCD] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_ChuDe] PRIMARY KEY ([Id])
);

CREATE INDEX [IX_CauHoi_ChuDeId] ON [CauHoi] ([ChuDeId]);

CREATE INDEX [IX_ChiTietDeThi_CauHoiId] ON [ChiTietDeThi] ([CauHoiId]);

CREATE INDEX [IX_ChiTietDeThi_DeThiId] ON [ChiTietDeThi] ([DeThiId]);

CREATE INDEX [IX_ChiTietKQ_CauHoiId] ON [ChiTietKQ] ([CauHoiId]);

CREATE INDEX [IX_ChiTietKQ_DapAnId] ON [ChiTietKQ] ([DapAnId]);

CREATE INDEX [IX_ChiTietKQ_KetQuaId] ON [ChiTietKQ] ([KetQuaId]);

ALTER TABLE [CauHoi] ADD CONSTRAINT [FK_CauHoi_ChuDe_ChuDeId] FOREIGN KEY ([ChuDeId]) REFERENCES [ChuDe] ([Id]) ON DELETE CASCADE;

ALTER TABLE [CauHoi] ADD CONSTRAINT [FK_CauHoi_DeThi_DeThiId] FOREIGN KEY ([DeThiId]) REFERENCES [DeThi] ([Id]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250912113050_UpdateModel', N'9.0.6');

ALTER TABLE [CauHoi] DROP CONSTRAINT [FK_CauHoi_DeThi_DeThiId];

DROP INDEX [IX_CauHoi_DeThiId] ON [CauHoi];

DECLARE @var7 sysname;
SELECT @var7 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CauHoi]') AND [c].[name] = N'DeThiId');
IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [CauHoi] DROP CONSTRAINT [' + @var7 + '];');
ALTER TABLE [CauHoi] DROP COLUMN [DeThiId];

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250912151851_UpdateModel_1', N'9.0.6');

ALTER TABLE [ChiTietKQ] DROP CONSTRAINT [FK_ChiTietKQ_DapAn_DapAnId];

DROP INDEX [IX_ChiTietKQ_DapAnId] ON [ChiTietKQ];

DECLARE @var8 sysname;
SELECT @var8 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChiTietKQ]') AND [c].[name] = N'DapAnId');
IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [ChiTietKQ] DROP CONSTRAINT [' + @var8 + '];');
ALTER TABLE [ChiTietKQ] DROP COLUMN [DapAnId];

DECLARE @var9 sysname;
SELECT @var9 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ChiTietKQ]') AND [c].[name] = N'Diem');
IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [ChiTietKQ] DROP CONSTRAINT [' + @var9 + '];');
ALTER TABLE [ChiTietKQ] ALTER COLUMN [Diem] float NULL;

CREATE TABLE [ChiTietKQ_DapAn] (
    [Id] int NOT NULL IDENTITY,
    [ChiTietKQId] int NOT NULL,
    [DapAnId] int NOT NULL,
    CONSTRAINT [PK_ChiTietKQ_DapAn] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChiTietKQ_DapAn_ChiTietKQ_ChiTietKQId] FOREIGN KEY ([ChiTietKQId]) REFERENCES [ChiTietKQ] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_ChiTietKQ_DapAn_DapAn_DapAnId] FOREIGN KEY ([DapAnId]) REFERENCES [DapAn] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_ChiTietKQ_DapAn_ChiTietKQId] ON [ChiTietKQ_DapAn] ([ChiTietKQId]);

CREATE INDEX [IX_ChiTietKQ_DapAn_DapAnId] ON [ChiTietKQ_DapAn] ([DapAnId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250917165040_updateModelKQ', N'9.0.6');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250918130025_initidentity', N'9.0.6');

COMMIT;
GO

