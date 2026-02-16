DROP TABLE IF EXISTS Pairs;
-- This is the pairs table used to link UUIDs
-- The pair permissions can expire to facilitate temporary pairings
-- Each individual permission is just a boolean value for simplicity
CREATE TABLE IF NOT EXISTS Pairs (
    id SERIAL NOT NULL REFERENCES Profiles(id) ON DELETE CASCADE,
    pair_id INTEGER NOT NULL REFERENCES Profiles(id) ON DELETE CASCADE,
    PRIMARY KEY (id, pair_id),
    expires TIMESTAMP,
    priority INTEGER NOT NULL DEFAULT 0,
    controls_perm bool NOT NULL DEFAULT FALSE,
    controls_config bool NOT NULL DEFAULT FALSE,
    disable_safeword bool NOT NULL DEFAULT FALSE,
    gags INTEGER NOT NULL DEFAULT 0,
    wardrobe INTEGER NOT NULL DEFAULT 0,
    moodles INTEGER NOT NULL DEFAULT 0
);

CREATE TABLE IF NOT EXISTS ProfileConfig (
    id SERIAL NOT NULL REFERENCES Profiles(id) ON DELETE CASCADE,
    enable_glamours BOOL NOT NULL DEFAULT false,
    enable_garbler BOOL NOT NULL DEFAULT false,
    enable_garbler_channels BOOL NOT NULL DEFAULT false,
    enable_moodles BOOL NOT NULL DEFAULT false
);
