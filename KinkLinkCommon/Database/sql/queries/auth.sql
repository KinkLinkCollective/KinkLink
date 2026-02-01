-- name: ListUIDsForSecret :many
-- For a secret_key join on the profiles and return all the UIDs that are associated with it.
-- Uses hashed secret key for security
SELECT p.UID FROM Profiles p
JOIN Users u ON p.user_id = u.id
WHERE u.secret_key_hash = $1;

-- name: Login :one
-- Confirm whether or not a UID is valid with a secret.
-- If it exists it can be authorized
-- Uses hashed secret key for security
SELECT u.secret_key_hash, u.secret_key_salt FROM Profiles p
JOIN Users u ON p.user_id = u.id
WHERE p.UID = $1;
