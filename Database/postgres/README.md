# PostgreSQL — базы данных проекта

Три отдельные базы разделяют **каталог**, **состояние магазина** и **продажи**.

| База | SQL-файл | Назначение |
|------|----------|------------|
| `merch_catalog` | [01_merch_catalog.sql](./01_merch_catalog.sql) | Товары, оборудование, правила цен |
| `merch_store` | [02_merch_store.sql](./02_merch_store.sql) | Профиль, сохранения, полки, доставка |
| `merch_sales` | [03_merch_sales.sql](./03_merch_sales.sql) | Смены, чеки, события NPC, кошелёк |

Полное описание с ER-диаграммой: [docs/DATABASE.md](../../docs/DATABASE.md)

## Установка

Из корня проекта в PowerShell:

```powershell
.\Database\postgres\Install-PostgresDatabases.ps1 -User postgres
```

Если `psql` не в PATH:

```powershell
.\Database\postgres\Install-PostgresDatabases.ps1 `
  -User postgres `
  -PsqlPath "C:\Program Files\PostgreSQL\17\bin\psql.exe"
```

Скрипт создаёт базы, применяет SQL и вставляет стартовые данные (товары, оборудование, профиль `Default` с 300 ₽).

## Таблицы по базам

### merch_catalog

- `products` — SKU, цены, `prefab_key` для Unity
- `equipment` — полки, кассы (`equipment_type`)
- `price_rules` — наценки и акции

### merch_store

- `player_profiles` — имя и баланс
- `store_saves` — позиция игрока (`JSONB`)
- `placed_equipment` — установленное оборудование
- `shelf_inventory` — выкладка по слотам
- `delivery_zone_items` — коробки в зоне доставки

### merch_sales

- `sale_sessions` — игровая смена
- `sales` — строки чека (`total_price` вычисляемый)
- `customer_events` — лог поведения NPC
- `wallet_events` — покупки, продажи, отладка
- `daily_sales_summary` — VIEW для отчётов

## Примеры запросов

```sql
-- Выручка за сегодня (merch_sales)
SELECT product_name, SUM(total_price) AS revenue
FROM sales WHERE sold_at >= CURRENT_DATE
GROUP BY product_name ORDER BY revenue DESC;

-- Активный каталог (merch_catalog)
SELECT code, display_name, sale_price FROM products WHERE is_active;
```

## Связь с Unity

Схема подготовлена для демонстрации информационной системы. Прямые credentials в билд не включаются — подключение через backend или пакет Npgsql на этапе интеграции.

Игровые аналоги в рантайме:

| Unity | БД |
|-------|-----|
| `ComputerTerminalUI` | `products`, `equipment` |
| `PlayerWallet` | `player_profiles`, `wallet_events` |
| `ShelfPlacementPoint` | `shelf_inventory` |
| `CashierStationInteractable` | `sales` |
| `CustomerNpc` | `customer_events` |
