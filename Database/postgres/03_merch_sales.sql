-- Database: merch_sales
-- Sales, scans, customer flow, and economy events for balancing.

CREATE EXTENSION IF NOT EXISTS pgcrypto;

CREATE TABLE IF NOT EXISTS sale_sessions (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    profile_name    TEXT NOT NULL DEFAULT 'Default',
    started_at      TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    ended_at        TIMESTAMPTZ,
    starting_money  INTEGER NOT NULL DEFAULT 300,
    ending_money    INTEGER
);

CREATE TABLE IF NOT EXISTS sales (
    id              UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    session_id      UUID REFERENCES sale_sessions(id) ON DELETE SET NULL,
    product_code    TEXT NOT NULL,
    product_name    TEXT NOT NULL,
    unit_price      INTEGER NOT NULL DEFAULT 15 CHECK (unit_price >= 0),
    quantity        INTEGER NOT NULL DEFAULT 1 CHECK (quantity > 0),
    total_price     INTEGER GENERATED ALWAYS AS (unit_price * quantity) STORED,
    sold_at         TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS customer_events (
    id              BIGSERIAL PRIMARY KEY,
    session_id      UUID REFERENCES sale_sessions(id) ON DELETE SET NULL,
    event_type      TEXT NOT NULL CHECK (event_type IN ('spawned', 'entered', 'picked_product', 'queued', 'paid', 'left_no_product', 'left_after_sale')),
    product_code    TEXT,
    details         JSONB NOT NULL DEFAULT '{}',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS wallet_events (
    id              BIGSERIAL PRIMARY KEY,
    session_id      UUID REFERENCES sale_sessions(id) ON DELETE SET NULL,
    event_type      TEXT NOT NULL CHECK (event_type IN ('initial', 'buy_product', 'buy_equipment', 'scan_sale', 'refund', 'debug')),
    amount_delta    INTEGER NOT NULL,
    balance_after   INTEGER NOT NULL CHECK (balance_after >= 0),
    details         JSONB NOT NULL DEFAULT '{}',
    created_at      TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE OR REPLACE VIEW daily_sales_summary AS
SELECT
    DATE_TRUNC('day', sold_at) AS day,
    product_code,
    product_name,
    SUM(quantity) AS units_sold,
    SUM(total_price) AS revenue
FROM sales
GROUP BY DATE_TRUNC('day', sold_at), product_code, product_name;

CREATE INDEX IF NOT EXISTS idx_sales_session ON sales (session_id, sold_at DESC);
CREATE INDEX IF NOT EXISTS idx_customer_events_session ON customer_events (session_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_wallet_events_session ON wallet_events (session_id, created_at DESC);
