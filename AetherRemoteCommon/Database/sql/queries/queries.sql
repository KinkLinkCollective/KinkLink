-- name: GetFriendCodeBySecret :one
-- Retrieves a friend code by secret for initial connection
SELECT FriendCode FROM Accounts WHERE Secret = sqlc.arg('secret');

-- name: GetAccountByFriendCode :one
-- Retrieves full account by friend code
SELECT DiscordId, FriendCode, Secret, Admin, CreatedAt, UpdatedAt
FROM Accounts WHERE FriendCode = sqlc.arg('friendCode') LIMIT 1;

-- name: GetAccountsByDiscordId :many
-- Retrieves all accounts for a discord user (supports multiple UIDs)
SELECT FriendCode, Secret, Admin, CreatedAt, UpdatedAt
FROM Accounts WHERE DiscordId = sqlc.arg('discordId');

-- name: CreateAccount :exec
-- Creates a new account
INSERT INTO Accounts (DiscordId, FriendCode, Secret, Admin)
VALUES (sqlc.arg('discordId'), sqlc.arg('friendCode'), sqlc.arg('secret'), FALSE)
ON CONFLICT (FriendCode) DO NOTHING;

-- name: AccountExistsAfterCreate :one
-- Checks if account was created (returns FriendCode if exists, null if just created)
SELECT FriendCode FROM Accounts WHERE DiscordId = sqlc.arg('discordId') AND FriendCode = sqlc.arg('friendCode') LIMIT 1;

-- name: UpdateFriendCode :execrows
-- Updates a friend code for an existing account
UPDATE Accounts
SET FriendCode = sqlc.arg('newFriendCode'), UpdatedAt = CURRENT_TIMESTAMP
WHERE DiscordId = sqlc.arg('discordId') AND FriendCode = sqlc.arg('oldFriendCode');

-- name: DeleteAccount :execrows
-- Deletes an account and all associated permissions
DELETE FROM Accounts WHERE DiscordId = sqlc.arg('discordId') AND FriendCode = sqlc.arg('friendCode');

-- name: CheckFriendCodeExists :one
-- Checks if a friend code exists
SELECT 1 AS Exists FROM Accounts WHERE FriendCode = sqlc.arg('friendCode') LIMIT 1;

-- name: CreatePermissions :execrows
-- Creates permissions between two users if target exists, returns affected rows
INSERT INTO Permissions (FriendCode, TargetFriendCode, PrimaryPermissions, SpeakPermissions, ElevatedPermissions)
SELECT sqlc.arg('friendCode'), sqlc.arg('targetFriendCode'), 0, 0, 0
WHERE EXISTS (SELECT 1 FROM Accounts WHERE FriendCode = sqlc.arg('targetFriendCode'))
ON CONFLICT DO NOTHING;

-- name: UpdatePermissions :execrows
-- Updates permissions for a relationship
UPDATE Permissions
SET PrimaryPermissions = sqlc.arg('primaryPermissions'),
    SpeakPermissions = sqlc.arg('speakPermissions'),
    ElevatedPermissions = sqlc.arg('elevatedPermissions'),
    UpdatedAt = CURRENT_TIMESTAMP
WHERE FriendCode = sqlc.arg('friendCode') AND TargetFriendCode = sqlc.arg('targetFriendCode');

-- name: GetPermissions :one
-- Gets permissions for a single relationship
SELECT PrimaryPermissions, SpeakPermissions, ElevatedPermissions
FROM Permissions
WHERE FriendCode = sqlc.arg('friendCode') AND TargetFriendCode = sqlc.arg('targetFriendCode')
LIMIT 1;

-- name: GetAllPermissions :many
-- Gets all permissions for a user (both directions)
SELECT
    p.TargetFriendCode,
    p.PrimaryPermissions AS PrimaryPermissionsTo, p.SpeakPermissions AS SpeakPermissionsTo, p.ElevatedPermissions AS ElevatedPermissionsTo,
    r.PrimaryPermissions AS PrimaryPermissionsFrom, r.SpeakPermissions AS SpeakPermissionsFrom, r.ElevatedPermissions AS ElevatedPermissionsFrom
FROM Permissions AS p
LEFT JOIN Permissions AS r ON r.FriendCode = p.TargetFriendCode AND r.TargetFriendCode = p.FriendCode
WHERE p.FriendCode = sqlc.arg('friendCode');

-- name: DeletePermissions :execrows
-- Deletes a permissions relationship
DELETE FROM Permissions WHERE FriendCode = sqlc.arg('friendCode') AND TargetFriendCode = sqlc.arg('targetFriendCode');

-- name: DeleteAllPermissionsForFriendCode :execrows
-- Deletes all permissions where this friend code is either party
DELETE FROM Permissions WHERE FriendCode = sqlc.arg('friendCode') OR TargetFriendCode = sqlc.arg('friendCode');
