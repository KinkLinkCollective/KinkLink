-- name: RegisterNewUser :one
-- If it doesn't exist, register a new user.
-- TODO: Ensure that the secret is hashed by postgres
INSERT INTO Users (discord_id, secret_key)
VALUES ($1, $2)
ON CONFLICT (discord_id) DO NOTHING
RETURNING *;

-- name: RegenerateSecretKey :exec
-- Allows users to get a new seret key from the service.
-- TODO: Ensure that the secret is hashed by postgres
UPDATE Users
SET secret_key=$2
WHERE discord_id=$1;

-- name: DeleteUserAccount :one
-- Deletes a user account and cascades to all data referencing the user.
DELETE FROM Users
WHERE discord_id = $1
RETURNING *;

-- name: SelectUserbyDiscordId :one
SELECT * FROM Users
WHERE discord_id = $1;

-- name: UserExists :one
SELECT EXISTS(
    SELECT 1 FROM Users 
    WHERE discord_id = $1
);
