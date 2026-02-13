-- Analytical views for common queries
-- These views make it easier to query business metrics

\echo ''
\echo '================================================'
\echo 'Creating analytical views...'
\echo '================================================'

-- Customer order statistics
\echo 'Creating customer_order_stats view...'
CREATE VIEW customer_order_stats AS
SELECT 
    c.customer_id,
    c.customer_unique_id,
    c.customer_city,
    c.customer_state,
    COUNT(DISTINCT o.order_id) AS total_orders,
    SUM(oi.price) AS total_spent,
    AVG(oi.price) AS avg_order_value,
    MAX(o.order_purchase_timestamp) AS last_order_date,
    MIN(o.order_purchase_timestamp) AS first_order_date
FROM customers c
LEFT JOIN orders o ON c.customer_id = o.customer_id
LEFT JOIN order_items oi ON o.order_id = oi.order_id
GROUP BY c.customer_id, c.customer_unique_id, c.customer_city, c.customer_state;

-- Product performance
\echo 'Creating product_performance view...'
CREATE VIEW product_performance AS
SELECT 
    p.product_id,
    p.product_category_name,
    pct.product_category_name_english,
    COUNT(DISTINCT oi.order_id) AS times_ordered,
    SUM(oi.price) AS total_revenue,
    AVG(oi.price) AS avg_price,
    AVG(orv.review_score) AS avg_review_score,
    COUNT(DISTINCT orv.review_id) AS review_count
FROM products p
LEFT JOIN order_items oi ON p.product_id = oi.product_id
LEFT JOIN orders o ON oi.order_id = o.order_id
LEFT JOIN order_reviews orv ON o.order_id = orv.order_id
LEFT JOIN product_category_translation pct ON p.product_category_name = pct.product_category_name
GROUP BY p.product_id, p.product_category_name, pct.product_category_name_english;

-- Seller performance
\echo 'Creating seller_performance view...'
CREATE VIEW seller_performance AS
SELECT 
    s.seller_id,
    s.seller_city,
    s.seller_state,
    COUNT(DISTINCT oi.order_id) AS total_orders,
    COUNT(DISTINCT oi.product_id) AS unique_products_sold,
    SUM(oi.price) AS total_revenue,
    AVG(oi.price) AS avg_item_price,
    SUM(oi.freight_value) AS total_freight_collected
FROM sellers s
LEFT JOIN order_items oi ON s.seller_id = oi.seller_id
GROUP BY s.seller_id, s.seller_city, s.seller_state;

-- Order fulfillment metrics
\echo 'Creating order_fulfillment_metrics view...'
CREATE VIEW order_fulfillment_metrics AS
SELECT 
    o.order_id,
    o.order_status,
    o.order_purchase_timestamp,
    o.order_delivered_customer_date,
    o.order_estimated_delivery_date,
    EXTRACT(DAY FROM (o.order_delivered_customer_date - o.order_purchase_timestamp)) AS actual_delivery_days,
    EXTRACT(DAY FROM (o.order_estimated_delivery_date - o.order_purchase_timestamp)) AS estimated_delivery_days,
    CASE 
        WHEN o.order_delivered_customer_date <= o.order_estimated_delivery_date THEN 'On Time'
        WHEN o.order_delivered_customer_date > o.order_estimated_delivery_date THEN 'Late'
        ELSE 'Pending'
    END AS delivery_status,
    c.customer_state,
    COUNT(oi.order_item_id) AS item_count,
    SUM(oi.price) AS order_value
FROM orders o
JOIN customers c ON o.customer_id = c.customer_id
LEFT JOIN order_items oi ON o.order_id = oi.order_id
GROUP BY o.order_id, o.order_status, o.order_purchase_timestamp, 
         o.order_delivered_customer_date, o.order_estimated_delivery_date, c.customer_state;

-- Category performance
\echo 'Creating category_performance view...'
CREATE VIEW category_performance AS
SELECT 
    pct.product_category_name_english AS category,
    COUNT(DISTINCT p.product_id) AS product_count,
    COUNT(DISTINCT oi.order_id) AS order_count,
    SUM(oi.price) AS total_revenue,
    AVG(oi.price) AS avg_price,
    AVG(orv.review_score) AS avg_review_score
FROM product_category_translation pct
LEFT JOIN products p ON pct.product_category_name = p.product_category_name
LEFT JOIN order_items oi ON p.product_id = oi.product_id
LEFT JOIN orders o ON oi.order_id = o.order_id
LEFT JOIN order_reviews orv ON o.order_id = orv.order_id
GROUP BY pct.product_category_name_english;

-- Monthly sales trends
\echo 'Creating monthly_sales_trends view...'
CREATE VIEW monthly_sales_trends AS
SELECT 
    TO_CHAR(o.order_purchase_timestamp, 'YYYY-MM') AS year_month,
    COUNT(DISTINCT o.order_id) AS order_count,
    COUNT(DISTINCT o.customer_id) AS unique_customers,
    SUM(oi.price) AS total_revenue,
    AVG(oi.price) AS avg_order_value,
    SUM(oi.freight_value) AS total_freight
FROM orders o
LEFT JOIN order_items oi ON o.order_id = oi.order_id
WHERE o.order_purchase_timestamp IS NOT NULL
GROUP BY TO_CHAR(o.order_purchase_timestamp, 'YYYY-MM')
ORDER BY year_month;

\echo ''
\echo 'Analytical views created successfully!'
