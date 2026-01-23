-- Initial schema for KinkLink database
-- Needs admin table to check for Admins
CREATE TABLE IF NOT EXISTS Admin (
    id SERIAL PRIMARY KEY,
    DiscordId BIGINT NOT null,
    GuildId BIGINT NOT null,
);

-- This table contains the userid, their secret key 
CREATE TABLE IF NOT EXISTS Users (
    id SERIAL PRIMARY KEY,
    DiscordId BIGINT NOT NULL,
    Secret VARCHAR(64) NOT NULL UNIQUE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- This is used for separate character profiles. Each user may have many profiles
CREATE TABLE IF NOT EXISTS Profiles (

);

-- Permissions table is used 
CREATE TABLE IF NOT EXISTS PairPermissions (
    id SERIAL NOT NULL,
    TargetFriendCode VARCHAR(12) NOT NULL,
    PrimaryPermissions INTEGER NOT NULL DEFAULT 0,
    SpeakPermissions INTEGER NOT NULL DEFAULT 0,
    ElevatedPermissions INTEGER NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id, id),
    FOREIGN KEY (id) REFERENCES Accounts(FriendCode) ON DELETE CASCADE,
    FOREIGN KEY (TargetFriendCode) REFERENCES Accounts(FriendCode) ON DELETE CASCADE
);
