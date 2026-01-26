-- Initial schema for KinkLink database
-- Admin accounts are stored in a separate table for security purposes.
-- There is no overlap in this and the other accounts with separate
-- Needs admin table to check for Admins
CREATE TABLE IF NOT EXISTS Admin (
    id SERIAL PRIMARY KEY,
    name VARCHAR(256) NOT NULL,
    password VARCHAR(256) NOT NULL,
    discord_id BIGINT NOT NULL
);

-- This contains the user accounts for kinklink
-- Verified/banned are used to track and purge accounts based on settings
CREATE TABLE IF NOT EXISTS Users (
    id SERIAL PRIMARY KEY,
    discord_id BIGINT NOT NULL,
    secret_key VARCHAR(64) NOT NULL UNIQUE,
    verified bool DEFAULT false,
    banned bool DEFAULT false,
    createdAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updatedAt TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- This is used for separate character profiles.
-- Each user may have many profiles with a unique identifier
-- Each profile contains: name, description, picture, and foreign key to user.id
CREATE TABLE IF NOT EXISTS Profiles (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    UID VARCHAR(10) NOT NULL,
    chat_role VARCHAR(20) DEFAULT '',
    alias VARCHAR(20) DEFAULT '',
    title VARCHAR(20) DEFAULT 'Kinkster',
    description TEXT
);

-- This is the pairs table used to link UUIDs
-- The pair permissions can expire to facilitate temporary pairings
-- Each individual permission is just a boolean value for simplicity
CREATE TABLE IF NOT EXISTS Pairs (
    id SERIAL NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    pair_id INTEGER NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    PRIMARY KEY (id, pair_id),
    expires TIMESTAMP,

    toggle_timer_locks BOOL DEFAULT false,
    toggle_permanent_locks BOOL DEFAULT false,

    toggle_garbler BOOL DEFAULT false,
    lock_garbler BOOL DEFAULT false,
    toggle_channels BOOL DEFAULT false,
    lock_channels BOOL DEFAULT false,
    
    apply_gag BOOL DEFAULT false,
    lock_gag BOOL DEFAULT false,
    unlock_gag BOOL DEFAULT false,
    remove_gag BOOL DEFAULT false,

    apply_wardrobe BOOL DEFAULT false,
    lock_wardrobe BOOL DEFAULT false,
    unlock_wardrobe BOOL DEFAULT false,
    remove_wardrobe BOOL DEFAULT false,

    apply_moodles BOOL DEFAULT false,
    lock_moodles BOOL DEFAULT false,
    unlock_moodles BOOL DEFAULT false,
    remove_moodles BOOL DEFAULT false
);
