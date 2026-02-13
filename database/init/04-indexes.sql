-- Create indexes for query performance
-- Indexes are added AFTER data loading and constraints

\echo ''
\echo '================================================'
\echo 'Creating indexes for query optimization...'
\echo '================================================'

-- Customer indexes
\echo 'Creating customer indexes...'
CREATE INDEX idx_customer_unique_id ON customers(customer_unique_id);
CREATE INDEX idx_customer_zip ON customers(customer_zip_code_prefix);
CREATE INDEX idx_customer_state ON customers(customer_state);

-- Geolocation indexes
\echo 'Creating geolocation indexes...'
CREATE INDEX idx_geo_zip ON geolocation(geolocation_zip_code_prefix);
CREATE INDEX idx_geo_state ON geolocation(geolocation_state);

-- Product indexes
\echo 'Creating product indexes...'
CREATE INDEX idx_product_category ON products(product_category_name);

-- Seller indexes
\echo 'Creating seller indexes...'
CREATE INDEX idx_seller_zip ON sellers(seller_zip_code_prefix);
CREATE INDEX idx_seller_state ON sellers(seller_state);

-- Order indexes
\echo 'Creating order indexes...'
CREATE INDEX idx_order_customer ON orders(customer_id);
CREATE INDEX idx_order_status ON orders(order_status);
CREATE INDEX idx_order_purchase_timestamp ON orders(order_purchase_timestamp);
CREATE INDEX idx_order_delivered_date ON orders(order_delivered_customer_date);

-- Order items indexes
\echo 'Creating order_items indexes...'
CREATE INDEX idx_order_item_order ON order_items(order_id);
CREATE INDEX idx_order_item_product ON order_items(product_id);
CREATE INDEX idx_order_item_seller ON order_items(seller_id);

-- Order payments indexes
\echo 'Creating order_payments indexes...'
CREATE INDEX idx_payment_order ON order_payments(order_id);
CREATE INDEX idx_payment_type ON order_payments(payment_type);

-- Order reviews indexes
\echo 'Creating order_reviews indexes...'
CREATE INDEX idx_review_order ON order_reviews(order_id);
CREATE INDEX idx_review_score ON order_reviews(review_score);
CREATE INDEX idx_review_creation_date ON order_reviews(review_creation_date);

\echo ''
\echo 'All indexes created successfully!'
