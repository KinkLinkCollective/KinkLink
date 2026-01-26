-- name: RegisterNewUser :one
-- If it doesn't exist, register a new user.
INSERT INTO Users (discord_id, secret_key, verified, banned)
VALUES ($1, $2, $3, $4)
ON CONFLICT (discord_id) DO NOTHING
RETURNING *;

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
SET alias = $1, updatedAt = CURRENT_TIMESTAMP
WHERE UID = $2 AND user_id = $3
RETURNING *;

-- name: UpdateDetailsForProfile :one
UPDATE Profiles 
SET title = $1, description = $2, updatedAt = CURRENT_TIMESTAMP
WHERE UID = $3 AND user_id = $4
RETURNING *;
-- name: GetAllPairsForProfile :one
SELECT p.*, 
       p1.UID as profile_uid,
       p2.UID as pair_uid
FROM Pairs p
JOIN Profiles p1 ON p.id = p1.id
JOIN Profiles p2 ON p.pair_id = p2.id
WHERE p1.UID = $1 OR p2.UID = $1;

-- name: ConfirmTwoWayPair :one
-- Checks both `id`, `pair_id` and `id` and `pair_id` are found in the database.
SELECT EXISTS(
    SELECT 1 FROM Pairs p1
    JOIN Pairs p2 ON p1.id = p2.pair_id AND p1.pair_id = p2.id
    WHERE p1.id = $1 AND p1.pair_id = $2
);

-- name: AddPair :one
INSERT INTO Pairs (id, pair_id, expires, apply_gag, lock_gag, unlock_gag, remove_gag, apply_wardrobe, lock_wardrobe, unlock_wardrobe, remove_wardrobe)
VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9, $10, $11)
RETURNING *;

-- name: RemovePair :one
DELETE FROM Pairs
WHERE (id = $1 AND pair_id = $2) OR (id = $2 AND pair_id = $1)
RETURNING *;
-- name: UpdatePairPermissions :one
UPDATE Pairs 
SET apply_gag = $1,
    lock_gag = $2,
    unlock_gag = $3,
    remove_gag = $4,
    apply_wardrobe = $5,
    lock_wardrobe = $6,
    unlock_wardrobe = $7,
    remove_wardrobe = $8
WHERE (id = $9 AND pair_id = $10) OR (id = $10 AND pair_id = $9)
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
