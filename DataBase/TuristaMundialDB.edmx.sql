
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/08/2023 00:13:04
-- Generated from EDMX file: D:\repos\Juego\TuristaServerGame\DataBase\TuristaMundialDB.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [BDTurista];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_FriendRequest_PlayerSet1ID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FriendRequest] DROP CONSTRAINT [FK_FriendRequest_PlayerSet1ID];
GO
IF OBJECT_ID(N'[dbo].[FK_FriendRequest_PlayerSet2ID]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FriendRequest] DROP CONSTRAINT [FK_FriendRequest_PlayerSet2ID];
GO
IF OBJECT_ID(N'[dbo].[FK_friendship_player1]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[friendship] DROP CONSTRAINT [FK_friendship_player1];
GO
IF OBJECT_ID(N'[dbo].[FK_friendship_player2]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[friendship] DROP CONSTRAINT [FK_friendship_player2];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FriendRequest]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FriendRequest];
GO
IF OBJECT_ID(N'[dbo].[friendship]', 'U') IS NOT NULL
    DROP TABLE [dbo].[friendship];
GO
IF OBJECT_ID(N'[dbo].[PlayerSet]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PlayerSet];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'PlayerSet'
CREATE TABLE [dbo].[PlayerSet] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Nickname] nvarchar(max)  NOT NULL,
    [eMail] nvarchar(max)  NOT NULL,
    [Password] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'friendship'
CREATE TABLE [dbo].[friendship] (
    [id] int IDENTITY(1,1) NOT NULL,
    [player1_id] int  NULL,
    [player2_id] int  NULL
);
GO

-- Creating table 'FriendRequest'
CREATE TABLE [dbo].[FriendRequest] (
    [IDRequest] int IDENTITY(1,1) NOT NULL,
    [PlayerSet1ID] int  NULL,
    [PlayerSet2ID] int  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'PlayerSet'
ALTER TABLE [dbo].[PlayerSet]
ADD CONSTRAINT [PK_PlayerSet]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [id] in table 'friendship'
ALTER TABLE [dbo].[friendship]
ADD CONSTRAINT [PK_friendship]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [IDRequest] in table 'FriendRequest'
ALTER TABLE [dbo].[FriendRequest]
ADD CONSTRAINT [PK_FriendRequest]
    PRIMARY KEY CLUSTERED ([IDRequest] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [player1_id] in table 'friendship'
ALTER TABLE [dbo].[friendship]
ADD CONSTRAINT [FK_friendship_player1]
    FOREIGN KEY ([player1_id])
    REFERENCES [dbo].[PlayerSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_friendship_player1'
CREATE INDEX [IX_FK_friendship_player1]
ON [dbo].[friendship]
    ([player1_id]);
GO

-- Creating foreign key on [player2_id] in table 'friendship'
ALTER TABLE [dbo].[friendship]
ADD CONSTRAINT [FK_friendship_player2]
    FOREIGN KEY ([player2_id])
    REFERENCES [dbo].[PlayerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_friendship_player2'
CREATE INDEX [IX_FK_friendship_player2]
ON [dbo].[friendship]
    ([player2_id]);
GO

-- Creating foreign key on [PlayerSet1ID] in table 'FriendRequest'
ALTER TABLE [dbo].[FriendRequest]
ADD CONSTRAINT [FK_FriendRequest_PlayerSet1ID]
    FOREIGN KEY ([PlayerSet1ID])
    REFERENCES [dbo].[PlayerSet]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FriendRequest_PlayerSet1ID'
CREATE INDEX [IX_FK_FriendRequest_PlayerSet1ID]
ON [dbo].[FriendRequest]
    ([PlayerSet1ID]);
GO

-- Creating foreign key on [PlayerSet2ID] in table 'FriendRequest'
ALTER TABLE [dbo].[FriendRequest]
ADD CONSTRAINT [FK_FriendRequest_PlayerSet2ID]
    FOREIGN KEY ([PlayerSet2ID])
    REFERENCES [dbo].[PlayerSet]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_FriendRequest_PlayerSet2ID'
CREATE INDEX [IX_FK_FriendRequest_PlayerSet2ID]
ON [dbo].[FriendRequest]
    ([PlayerSet2ID]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------