
CREATE TABLE IF NOT EXISTS wardrobe (
    id UUID UNIQUE PRIMARY KEY,
    profile_id INTEGER NOT NULL REFERENCES Profiles(id) ON DELETE CASCADE,
    name VARCHAR(50),
    type VARCHAR(20) NOT NULL, -- 'item' or 'set' or 'modsettings'
    description VARCHAR(240),
    slot INTEGER, -- Ignored if 'set' or 'modsettings' maps to GlamourerEquipmentSlot
    relationship_priority INTEGER DEFAULT 0,
    data JSONB NOT NULL, -- This is the data as serialized. 'item' and 'moditem' are deserialized as `WardrobeItem` while `set' is deserialized as `WardrobeSet`
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),

    CONSTRAINT valid_type CHECK (type IN ('item', 'set', 'moditem'))
);

CREATE TABLE IF NOT EXISTS activewardrobe (
    id bigserial PRIMARY KEY,
    profile_id INTEGER NOT NULL REFERENCES Profiles(id) ON DELETE CASCADE,
    glamourerset JSONB,
    head JSONB,
    body JSONB,
    hand JSONB,
    legs JSONB,
    feet JSONB,
    earring JSONB,
    neck JSONB,
    bracelet JSONB,
    lring JSONB,
    rring JSONB,
    moditems JSONB
);
