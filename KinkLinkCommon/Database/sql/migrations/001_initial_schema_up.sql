-- use PSQL extension for encrypting passwords (lower risk)
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- TODO: Add cryptography module to postgres migrations for security
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
    discord_id BIGINT NOT NULL UNIQUE,
    secret_key_hash VARCHAR(64),
    verified BOOLEAN DEFAULT false,
    banned BOOLEAN DEFAULT false,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- This is used for separate character profiles.
-- Each user may have many profiles with a unique identifier
-- Each profile contains: name, description, picture, and foreign key to user.id
CREATE TABLE IF NOT EXISTS Profiles (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL REFERENCES Users(id) ON DELETE CASCADE,
    UID VARCHAR(10) UNIQUE NOT NULL,
    chat_role VARCHAR(20) DEFAULT '',
    alias VARCHAR(20) DEFAULT '',
    title VARCHAR(20) DEFAULT 'Kinkster',
    description TEXT DEFAULT '',
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

-- This is the pairs table used to link UUIDs
-- The pair permissions can expire to facilitate temporary pairings
-- Each individual permission is just a boolean value for simplicity
CREATE TABLE IF NOT EXISTS Pairs (
    id INTEGER NOT NULL REFERENCES Profiles(id) ON DELETE CASCADE,
    pair_id INTEGER NOT NULL REFERENCES Profiles(id) ON DELETE CASCADE,
    PRIMARY KEY (id, pair_id),
    expires TIMESTAMP,

    toggle_timer_locks BOOLEAN DEFAULT false,
    toggle_permanent_locks BOOLEAN DEFAULT false,

    toggle_garbler BOOLEAN DEFAULT false,
    lock_garbler BOOLEAN DEFAULT false,
    toggle_channels BOOLEAN DEFAULT false,
    lock_channels BOOLEAN DEFAULT false,
    
    apply_gag BOOLEAN DEFAULT false,
    lock_gag BOOLEAN DEFAULT false,
    unlock_gag BOOLEAN DEFAULT false,
    remove_gag BOOLEAN DEFAULT false,

    apply_wardrobe BOOLEAN DEFAULT false,
    lock_wardrobe BOOLEAN DEFAULT false,
    unlock_wardrobe BOOLEAN DEFAULT false,
    remove_wardrobe BOOLEAN DEFAULT false,

    apply_moodles BOOLEAN DEFAULT false,
    lock_moodles BOOLEAN DEFAULT false,
    unlock_moodles BOOLEAN DEFAULT false,
    remove_moodles BOOLEAN DEFAULT false
);
