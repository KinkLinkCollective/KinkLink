-- Database: AetherRemote
-- This schema is used by sqlc for code generation

CREATE TABLE IF NOT EXISTS Accounts (
    DiscordId BIGINT NOT NULL,
    FriendCode VARCHAR(12) NOT NULL PRIMARY KEY,
    Secret VARCHAR(64) NOT NULL UNIQUE,
    Admin BOOLEAN NOT NULL DEFAULT FALSE,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS Permissions (
    FriendCode VARCHAR(12) NOT NULL,
    TargetFriendCode VARCHAR(12) NOT NULL,
    PrimaryPermissions INTEGER NOT NULL DEFAULT 0,
    SpeakPermissions INTEGER NOT NULL DEFAULT 0,
    ElevatedPermissions INTEGER NOT NULL DEFAULT 0,
    CreatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (FriendCode, TargetFriendCode),
    FOREIGN KEY (FriendCode) REFERENCES Accounts(FriendCode) ON DELETE CASCADE,
    FOREIGN KEY (TargetFriendCode) REFERENCES Accounts(FriendCode) ON DELETE CASCADE
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_permissions_friendcode ON Permissions(FriendCode);
CREATE INDEX IF NOT EXISTS idx_permissions_target ON Permissions(TargetFriendCode);
CREATE INDEX IF NOT EXISTS idx_accounts_discord ON Accounts(DiscordId);
CREATE INDEX IF NOT EXISTS idx_accounts_secret ON Accounts(Secret);
