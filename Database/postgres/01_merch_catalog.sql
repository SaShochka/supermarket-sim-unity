-- Database: merch_catalog
-- Static and editable catalog data used by the in-game computer shop.

CREATE TABLE IF NOT EXISTS products (
    id              BIGSERIAL PRIMARY KEY,
    code            TEXT NOT NULL UNIQUE,
    display_name    TEXT NOT NULL,
    category        TEXT NOT NULL DEFAULT 'food',
    base_price      INTEGER NOT NULL CHECK (base_price >= 0),
    sale_price      INTEGER NOT NULL DEFAULT 15 CHECK (sale_price >= 0),
    prefab_key      TEXT,
    icon_key        TEXT,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS equipment (
    id              BIGSERIAL PRIMARY KEY,
    code            TEXT NOT NULL UNIQUE,
    display_name    TEXT NOT NULL,
    equipment_type  TEXT NOT NULL CHECK (equipment_type IN ('shelf', 'cash_register', 'decor', 'other')),
    price           INTEGER NOT NULL CHECK (price >= 0),
    prefab_key      TEXT,
    is_placeable    BOOLEAN NOT NULL DEFAULT TRUE,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE,
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS price_rules (
    id              BIGSERIAL PRIMARY KEY,
    code            TEXT NOT NULL UNIQUE,
    target_type     TEXT NOT NULL CHECK (target_type IN ('product', 'equipment', 'global')),
    target_code     TEXT,
    multiplier      NUMERIC(8, 3) NOT NULL DEFAULT 1.0 CHECK (multiplier >= 0),
    flat_delta      INTEGER NOT NULL DEFAULT 0,
    starts_at       TIMESTAMPTZ,
    ends_at         TIMESTAMPTZ,
    is_active       BOOLEAN NOT NULL DEFAULT TRUE
);

INSERT INTO products (code, display_name, category, base_price, sale_price, prefab_key, icon_key)
VALUES
    ('chips', 'Чипсы', 'food', 25, 15, 'chips', 'chips'),
    ('bread', 'Хлеб', 'food', 18, 15, 'bread', 'bread'),
    ('cereal', 'Хлопья', 'food', 35, 15, 'cereal', 'cereal'),
    ('chocolate_bar', 'Шоколадный батончик', 'food', 20, 15, 'chocolate_bar', 'chocolate_bar'),
    ('ice_cream', 'Мороженое', 'food', 30, 15, 'ice_cream', 'ice_cream'),
    ('drink', 'Напиток', 'drink', 22, 15, 'drink', 'drink')
ON CONFLICT (code) DO NOTHING;

INSERT INTO equipment (code, display_name, equipment_type, price, prefab_key)
VALUES
    ('shelf_basic', 'Полка', 'shelf', 90, 'shelf_basic'),
    ('cash_register_basic', 'Касса', 'cash_register', 160, 'cash_register_basic')
ON CONFLICT (code) DO NOTHING;

CREATE INDEX IF NOT EXISTS idx_products_active ON products (is_active, category);
CREATE INDEX IF NOT EXISTS idx_equipment_active ON equipment (is_active, equipment_type);
