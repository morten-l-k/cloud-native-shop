-- Load data from CSV files
-- Loading in dependency order (independent tables first)

\echo '================================================'
\echo 'Starting data load process...'
\echo '================================================'

-- 1. Product category translation (independent, small)
\echo ''
\echo '[1/9] Loading product_category_translation (72 rows)...'
\COPY product_category_translation FROM '/data/product_category_name_translation.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"');

-- 2. Customers (independent)
\echo '[2/9] Loading customers (~99k rows)...'
\COPY customers FROM '/data/olist_customers_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"');

-- 3. Products (independent in this phase)
\echo '[3/9] Loading products (~33k rows)...'
\COPY products FROM '/data/olist_products_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"', NULL '');

-- 4. Sellers (independent)
\echo '[4/9] Loading sellers (~3k rows)...'
\COPY sellers FROM '/data/olist_sellers_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"');

-- 5. Orders (independent in this phase)
\echo '[5/9] Loading orders (~99k rows)...'
\COPY orders FROM '/data/olist_orders_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"', NULL '');

-- 6. Geolocation (independent, LARGE - this takes 1-2 minutes)
\echo '[6/9] Loading geolocation (~1M rows - this will take 1-2 minutes)...'
\COPY geolocation(geolocation_zip_code_prefix, geolocation_lat, geolocation_lng, geolocation_city, geolocation_state) FROM '/data/olist_geolocation_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"');

-- 7. Order items (independent in this phase)
\echo '[7/9] Loading order_items (~113k rows)...'
\COPY order_items(order_id, order_item_quantity, product_id, seller_id, shipping_limit_date, price, freight_value) FROM '/data/olist_order_items_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"', NULL '');

-- 8. Order payments (independent in this phase)
\echo '[8/9] Loading order_payments (~104k rows)...'
\COPY order_payments(order_id, payment_sequential, payment_type, payment_installments, payment_value) FROM '/data/olist_order_payments_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"');

-- 9. Order reviews (independent in this phase) - skip duplicates
\echo '[9/9] Loading order_reviews (~105k rows - removing duplicates)...'

-- Load into stage table first to handle duplicates
CREATE TEMP TABLE stg_reviews (LIKE order_reviews);
\COPY stg_reviews FROM '/data/olist_order_reviews_dataset.csv' WITH (FORMAT csv, HEADER true, DELIMITER ',', QUOTE '"', NULL '');

-- Insert only first occurrence of each review_id
INSERT INTO order_reviews
SELECT DISTINCT ON (review_id) *
FROM stg_reviews
ORDER BY review_id;

DROP TABLE stg_reviews;

\echo ''
\echo '================================================'
\echo 'Data loading complete!'
\echo '================================================'
