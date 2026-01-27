-- name: RegisterNewUser :one
-- If it doesn't exist, register a new user.
INSERT INTO Users (discord_id, secret_key)
VALUES ($1, $2)
ON CONFLICT (discord_id) DO NOTHING
RETURNING *;

-- name: RegenerateSecretKey :exec
-- Allows users to get a new seret key from the service.
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

-- name: CreateNewUIDForUser :one
-- If the provided user exists, create a new UID for them.
INSERT INTO Profiles (user_id, UID, chat_role, alias, title, description)
VALUES ($1, $2, $3, $4, $5, $6)
RETURNING *;

-- name: DeleteProfile :one
-- Should cascade on deletion for any foreign keys
DELETE FROM Profiles
WHERE UID = $1 AND user_id = $2
RETURNING *;

-- name: ProfileExists :one
SELECT EXISTS(
    SELECT 1 FROM Profiles 
    WHERE UID = $1
);

-- name: UpdateAliasForProfile :one
UPDATE Profiles 
SET alias = $1, updated_at = CURRENT_TIMESTAMP
WHERE UID = $2 AND user_id = $3
RETURNING *;

-- name: UpdateDetailsForProfile :one
UPDATE Profiles 
SET title = $1, description = $2, updated_at = CURRENT_TIMESTAMP
WHERE UID = $3 AND user_id = $4
RETURNING *;

-- name: GetAllPairsForProfile :many
-- Retreives all currently registered pairs for a Profile
-- This will return the data from both pairs 
SELECT p.*, 
       p1.UID as profile_uid,
       p2.UID as pair_uid
FROM Pairs p
JOIN Profiles p1 ON p.id = p1.id
JOIN Profiles p2 ON p.pair_id = p2.id
WHERE p1.UID = $1 OR p2.UID = $1;

-- name: ConfirmTwoWayPair :one
-- Checks if both directions of a pair exist in the database.
SELECT EXISTS(
    SELECT 1 FROM Pairs 
    WHERE Pairs.id = $1 AND Pairs.pair_id = $2
) AND EXISTS(
    SELECT 1 FROM Pairs 
    WHERE Pairs.id = $2 AND Pairs.pair_id = $1
) as TwoWayPair;

-- name: AddPair :one
-- This will add a pair without an expiry
INSERT INTO Pairs (id, pair_id)
VALUES ($1, $2)
RETURNING *;
-- name: AddTemporaryPair :one
-- This will add a pair with an expiry
INSERT INTO Pairs (id, pair_id, expires)
VALUES ($1, $2, $3)
RETURNING *;

-- name: RemovePair :one
DELETE FROM Pairs
WHERE (id = $1 AND pair_id = $2) OR (id = $2 AND pair_id = $1)
RETURNING *;

-- name: UpdatePairPermissions :one
-- Updates the users permissions granted to the pair.
-- Note: It is possible to give access to a user that is not paired back.
-- These pair permissions are considered to be _pending_ until a two way pair can be confirmed
UPDATE Pairs 
SET toggle_timer_locks=$1,
    toggle_permanent_locks=$2,

    toggle_garbler=$3,
    lock_garbler=$4,
    toggle_channels=$5,
    lock_channels=$6,
    
    apply_gag=$7,
    lock_gag=$8,
    unlock_gag=$9,
    remove_gag=$10,

    apply_wardrobe=$11,
    lock_wardrobe=$12,
    unlock_wardrobe=$13,
    remove_wardrobe=$14,

    apply_moodles=$15,
    lock_moodles=$16,
    unlock_moodles=$17,
    remove_moodles=$18
WHERE (id = $19 AND pair_id = $20)
RETURNING *;

-- name: PurgeExpiredPairs :one
DELETE FROM Pairs
WHERE expires IS NOT NULL AND expires < CURRENT_TIMESTAMP
RETURNING *;

-- name: HasExpiredPairs :one
SELECT EXISTS(
    SELECT 1 FROM Pairs 
    WHERE expires IS NOT NULL AND expires < CURRENT_TIMESTAMP
);
