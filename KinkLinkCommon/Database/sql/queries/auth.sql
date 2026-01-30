-- name: ListUIDsForSecret :many
-- For a secret_key join on the profiles and return all the UIDs that are associatd with it.
SELECT p.UID FROM Profiles p
JOIN Users u ON p.user_id = u.id
WHERE u.secret_key = $1;

-- name: Login :one
-- Confirm whether or not a UID is valid with a secret.
-- If it exists it can be authorized
SELECT 1 as valid FROM Profiles p
JOIN Users u ON p.user_id = u.id
WHERE p.UID = $1 AND u.secret_key = $2;
