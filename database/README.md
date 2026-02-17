# Database Initialization

This directory contains SQL scripts to initialize the PostgreSQL database with Brazilian e-commerce data (~1.56 million rows).

## Files (Executed in Order)

1. **01-schema.sql** - Creates all tables (without foreign keys for fast loading)
2. **02-load.sql** - Loads ~1.56 million rows from CSV files
3. **03-constraints.sql** - Adds foreign key constraints after data is loaded
4. **04-indexes.sql** - Creates indexes for query optimization
5. **05-views.sql** - Creates analytical views for common queries
6. **06-verify.sql** - Verifies setup and displays summary statistics

## Strategy

**Loading data BEFORE constraints** provides:
- ✅ Faster bulk loading (no FK validation during insert)
- ✅ Avoids constraint violation issues during import
- ✅ More reliable initialization process

## Automatic Initialization

When you start the database with Docker Compose, all scripts run automatically in alphabetical order:

```bash
docker compose up db
```

**Initialization time:** ~2-5 minutes (mostly loading 1M geolocation records)

## Expected Row Counts

| Table | Rows | Notes |
|-------|------|-------|
| geolocation | ~1,000,000 | Largest table |
| order_items | ~113,000 | |
| order_reviews | ~105,000 | |
| order_payments | ~104,000 | |
| customers | ~99,000 | |
| orders | ~99,000 | |
| products | ~33,000 | |
| sellers | ~3,000 | |
| product_category_translation | 72 | |
| **TOTAL** | **~1,556,000** | |

## Database Schema

```
customers ──┐
            ├──> orders ──┬──> order_items ──┬──> products ──> product_category_translation
            │             │                   └──> sellers
            │             ├──> order_payments
            │             └──> order_reviews
            └──> geolocation (via zip code)
```

See `er-diagram.md` file for more information on database model.

## Analytical Views Created

- `customer_order_stats` - Customer purchasing patterns and lifetime value
- `product_performance` - Product sales metrics and review scores
- `seller_performance` - Seller statistics and revenue
- `order_fulfillment_metrics` - Delivery performance tracking
- `category_performance` - Category-level analytics
- `monthly_sales_trends` - Time-series sales data

## Quick Start Queries

Connect to the database:
```bash
docker exec -it b2c_postgres psql -U b2c_user -d b2c_db
```

Sample queries:
```sql
-- Top 10 product categories
SELECT * FROM category_performance 
ORDER BY total_revenue DESC LIMIT 10;

-- Monthly sales trends
SELECT * FROM monthly_sales_trends 
ORDER BY year_month DESC LIMIT 12;

-- Customer lifetime value
SELECT customer_id, customer_city, total_orders, 
       ROUND(total_spent::numeric, 2) as total_spent
FROM customer_order_stats 
WHERE total_spent IS NOT NULL
ORDER BY total_spent DESC LIMIT 20;

-- Delivery performance
SELECT delivery_status, COUNT(*) as orders
FROM order_fulfillment_metrics
GROUP BY delivery_status;
```

## Manual Execution

To run the verification script manually:
```bash
docker exec -it b2c_postgres psql -U b2c_user -d b2c_db -f /docker-entrypoint-initdb.d/06-verify.sql
```

## Troubleshooting

**Check initialization logs:**
```bash
docker logs b2c_postgres
```

**Reset and reinitialize:**
```bash
docker-compose down -v  # Remove volumes
docker-compose up -d db # Recreate and reinitialize
```

**Connect and check manually:**
```bash
docker exec -it b2c_postgres psql -U b2c_user -d b2c_db
\dt  # List tables
\di  # List indexes
\dv  # List views
```

## Data Source

Brazilian E-Commerce Public Dataset by Olist (Kaggle)  
Real anonymized commercial data from 2016-2018
