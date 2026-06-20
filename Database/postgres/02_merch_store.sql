-- Database: merch_store
-- Player saves and current supermarket state.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS player_profiles (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    profile_name    TEXT NOT NULL UNIQUE,
    money           INTEGER NOT NULL DEFAULT 300 CHECK (money >= 0),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS store_saves (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    profile_id      UUID NOT NULL REFERENCES player_profiles(id) ON DELETE CASCADE,
    save_name       TEXT NOT NULL DEFAULT 'Autosave',
    scene_name      TEXT NOT NULL DEFAULT 'Game',
    player_position JSONB NOT NULL DEFAULT '{"x":0,"y":0,"z":0}',
    player_rotation JSONB NOT NULL DEFAULT '{"x":0,"y":0,"z":0}',
    saved_at        TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS placed_equipment (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    save_id         UUID NOT NULL REFERENCES store_saves(id) ON DELETE CASCADE,
    equipment_code  TEXT NOT NULL,
    position        JSONB NOT NULL,
    rotation        JSONB NOT NULL,
    scale           JSONB NOT NULL DEFAULT '{"x":1,"y":1,"z":1}',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS shelf_inventory (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    save_id         UUID NOT NULL REFERENCES store_saves(id) ON DELETE CASCADE,
    shelf_code      TEXT NOT NULL,
    placement_index INTEGER NOT NULL CHECK (placement_index >= 0),
    product_code    TEXT NOT NULL,
    quantity        INTEGER NOT NULL DEFAULT 1 CHECK (quantity >= 0),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    UNIQUE (save_id, shelf_code, placement_index)
);

CREATE TABLE IF NOT EXISTS delivery_zone_items (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    save_id         UUID NOT NULL REFERENCES store_saves(id) ON DELETE CASCADE,
    product_code    TEXT NOT NULL,
    quantity        INTEGER NOT NULL DEFAULT 1 CHECK (quantity >= 0),
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

INSERT INTO player_profiles (profile_name, money)
VALUES ('Default', 300)
ON CONFLICT (profile_name) DO NOTHING;

CREATE INDEX IF NOT EXISTS idx_store_saves_profile ON store_saves (profile_id, saved_at DESC);
CREATE INDEX IF NOT EXISTS idx_inventory_save ON shelf_inventory (save_id, product_code);
CREATE INDEX IF NOT EXISTS idx_equipment_save ON placed_equipment (save_id, equipment_code);
