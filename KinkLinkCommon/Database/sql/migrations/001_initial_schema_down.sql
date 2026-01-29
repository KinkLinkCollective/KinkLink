-- The scheme down exist primarily for local testing `DbUp` only provides a roll forward functionality by default
-- TL;DR: These are maintained out of habit and for simplicity in testing.
DROP TABLE IF EXISTS Pairs;
DROP TABLE IF EXISTS Profiles;
DROP TABLE IF EXISTS Users;
DROP TABLE IF EXISTS Admin;
-- Drop the schema versions
DROP TABLE IF EXISTS SchemaVersions;
