-- name: InsertTestUser :one
-- Inserts a test user into the Users table
INSERT INTO Users (discord_id, secret_key_hash, verified, banned)
VALUES ($1, $2, COALESCE($3, false)::boolean, COALESCE($4, false)::boolean)
RETURNING id;

-- name: InsertTestProfile :one
-- Inserts a test profile into the Profiles table
INSERT INTO Profiles (user_id, uid, chat_role, alias, title, description)
VALUES ($1, $2, COALESCE($3, '')::varchar, COALESCE($4, '')::varchar, COALESCE($5, 'Kinkster')::varchar, COALESCE($6, '')::varchar)
RETURNING id;

-- name: InsertTestPair :one
-- Inserts a test pair relationship between two profiles
INSERT INTO Pairs (id, pair_id, priority, gags, wardrobe, moodles)
VALUES ($1, $2, COALESCE($3, 0)::integer, COALESCE($4, 0)::integer, COALESCE($5, 0)::integer, COALESCE($6, 0)::integer)
RETURNING *;

-- name: InsertTestPairWithPermissions :one
-- Inserts a test pair with specific permission values
INSERT INTO Pairs (id, pair_id, expires, priority, gags, wardrobe, moodles)
VALUES ($1, $2, $3, $4, $5, $6, $7)
RETURNING *;

-- name: DeleteTestUserByDiscordId :one
-- Deletes a user and all related profiles and pairs by discord_id
DELETE FROM Users WHERE Users.discord_id = $1
RETURNING *;

-- name: DeleteTestProfileByUid :one
-- Deletes a profile by UID
DELETE FROM Profiles WHERE uid = $1
RETURNING *;

-- name: DeleteTestPair :one
-- Deletes a pair relationship between two profile IDs
DELETE FROM Pairs WHERE (id = $1 AND pair_id = $2) OR (id = $2 AND pair_id = $1)
RETURNING *;

-- name: GetProfileIdByUid :one
-- Gets the profile ID from a UID
SELECT id FROM Profiles WHERE uid = $1;

-- name: GetUserIdByDiscordId :one
-- Gets the user ID from a discord_id
SELECT id FROM Users WHERE discord_id = $1;

-- name: TableExists :one
-- Checks if a table exists in the database
SELECT EXISTS (
    SELECT 1 FROM information_schema.tables 
    WHERE table_schema = 'public' 
    AND table_name = $1::varchar
)::boolean as exists;

-- name: ColumnExists :one
-- Checks if a column exists in a table
SELECT EXISTS (
    SELECT 1 FROM information_schema.columns 
    WHERE table_schema = 'public' 
    AND table_name = $1::varchar
    AND column_name = $2::varchar
)::boolean as exists;

-- name: TruncateTables :exec
-- Truncates all tables in correct order for test isolation
TRUNCATE TABLE Pairs, Profiles, Users RESTART IDENTITY CASCADE;
