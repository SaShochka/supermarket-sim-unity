-- Run this script with psql against the default "postgres" database.
-- It creates separate databases for game catalog data, player saves, and sales analytics.

SELECT 'CREATE DATABASE merch_catalog WITH ENCODING = ''UTF8'' TEMPLATE = template0'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'merch_catalog')\gexec

SELECT 'CREATE DATABASE merch_store WITH ENCODING = ''UTF8'' TEMPLATE = template0'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'merch_store')\gexec

SELECT 'CREATE DATABASE merch_sales WITH ENCODING = ''UTF8'' TEMPLATE = template0'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'merch_sales')\gexec
