-- Verification and summary of database setup

\echo ''
\echo '================================================'
\echo '          DATABASE INITIALIZATION COMPLETE'
\echo '================================================'
\echo ''

-- Show row counts
\echo '=== TABLE ROW COUNTS ==='
SELECT 
    table_name,
    TO_CHAR(row_count, '999,999,999') AS rows,
    pg_size_pretty(table_size) AS size
FROM (
    SELECT 'product_category_translation' AS table_name, 
           COUNT(*)::bigint AS row_count,
           pg_total_relation_size('product_category_translation') AS table_size
    FROM product_category_translation
    UNION ALL
    SELECT 'customers', COUNT(*)::bigint, pg_total_relation_size('customers') FROM customers
    UNION ALL
    SELECT 'products', COUNT(*)::bigint, pg_total_relation_size('products') FROM products
    UNION ALL
    SELECT 'sellers', COUNT(*)::bigint, pg_total_relation_size('sellers') FROM sellers
    UNION ALL
    SELECT 'geolocation', COUNT(*)::bigint, pg_total_relation_size('geolocation') FROM geolocation
    UNION ALL
    SELECT 'orders', COUNT(*)::bigint, pg_total_relation_size('orders') FROM orders
    UNION ALL
    SELECT 'order_items', COUNT(*)::bigint, pg_total_relation_size('order_items') FROM order_items
    UNION ALL
    SELECT 'order_payments', COUNT(*)::bigint, pg_total_relation_size('order_payments') FROM order_payments
    UNION ALL
    SELECT 'order_reviews', COUNT(*)::bigint, pg_total_relation_size('order_reviews') FROM order_reviews
) t
ORDER BY row_count DESC;

-- Show total rows
\echo ''
\echo '=== TOTAL ROWS LOADED ==='
SELECT 
    TO_CHAR(
        (SELECT COUNT(*) FROM product_category_translation) +
        (SELECT COUNT(*) FROM customers) +
        (SELECT COUNT(*) FROM products) +
        (SELECT COUNT(*) FROM sellers) +
        (SELECT COUNT(*) FROM geolocation) +
        (SELECT COUNT(*) FROM orders) +
        (SELECT COUNT(*) FROM order_items) +
        (SELECT COUNT(*) FROM order_payments) +
        (SELECT COUNT(*) FROM order_reviews),
        '999,999,999'
    ) AS total_rows_loaded;

-- Show constraint info
\echo ''
\echo '=== FOREIGN KEY CONSTRAINTS ==='
SELECT 
    tc.table_name, 
    tc.constraint_name,
    kcu.column_name,
    ccu.table_name AS foreign_table_name,
    ccu.column_name AS foreign_column_name
FROM information_schema.table_constraints tc
JOIN information_schema.key_column_usage kcu
    ON tc.constraint_name = kcu.constraint_name
JOIN information_schema.constraint_column_usage ccu
    ON ccu.constraint_name = tc.constraint_name
WHERE tc.constraint_type = 'FOREIGN KEY'
ORDER BY tc.table_name, tc.constraint_name;

-- Show index count
\echo ''
\echo '=== INDEX SUMMARY ==='
SELECT 
    schemaname,
    COUNT(*) as index_count
FROM pg_indexes
WHERE schemaname = 'public'
GROUP BY schemaname;

-- Show views
\echo ''
\echo '=== ANALYTICAL VIEWS CREATED ==='
SELECT table_name 
FROM information_schema.views 
WHERE table_schema = 'public'
ORDER BY table_name;

-- Sample queries
\echo ''
\echo '=== SAMPLE DATA: Top 5 product categories by revenue ==='
SELECT 
    category,
    product_count,
    order_count,
    TO_CHAR(total_revenue, '999,999,999.99') AS total_revenue
FROM category_performance
WHERE total_revenue IS NOT NULL
ORDER BY total_revenue DESC
LIMIT 5;

\echo ''
\echo '=== SAMPLE DATA: Orders by status ==='
SELECT 
    order_status,
    COUNT(*) as count
FROM orders
GROUP BY order_status
ORDER BY count DESC;

\echo ''
\echo '================================================'
\echo 'Database is ready for use!'
\echo 'Connect with: docker exec -it b2c_postgres psql -U b2c_user -d b2c_db'
\echo '================================================'
\echo ''
