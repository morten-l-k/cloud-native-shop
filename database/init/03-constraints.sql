-- Add foreign key constraints AFTER data is loaded
-- This is much faster than loading with constraints in place

\echo ''
\echo '================================================'
\echo 'Adding foreign key constraints...'
\echo '================================================'

-- Products (olist_products_dataset.csv) -> product_category_translation
-- Note: Some products reference categories not in translation table (pc_gamer, portateis_cozinha_e_preparadores_de_alimentos)
-- Using NOT VALID to allow existing orphaned data while validating new data
\echo 'Adding product category foreign key (NOT VALID for existing data)...'
ALTER TABLE products 
    ADD CONSTRAINT fk_product_category 
    FOREIGN KEY (product_category_name) 
    REFERENCES product_category_translation(product_category_name)
    ON DELETE SET NULL
    NOT VALID;

-- Orders (olist_orders_dataset.csv) -> customers (olist_customers_dataset.csv)
\echo 'Adding order customer foreign key...'
ALTER TABLE orders 
    ADD CONSTRAINT fk_order_customer 
    FOREIGN KEY (customer_id) 
    REFERENCES customers(customer_id)
    ON DELETE SET NULL;

-- Order items (olist_order_items_dataset.csv) -> orders (olist_orders_dataset.csv)
\echo 'Adding order_items order foreign key...'
ALTER TABLE order_items 
    ADD CONSTRAINT fk_order_item_order 
    FOREIGN KEY (order_id) 
    REFERENCES orders(order_id)
    ON DELETE CASCADE;

-- Order items -> products
\echo 'Adding order_items product foreign key...'
ALTER TABLE order_items 
    ADD CONSTRAINT fk_order_item_product 
    FOREIGN KEY (product_id) 
    REFERENCES products(product_id)
    ON DELETE SET NULL;

-- Order items -> sellers
\echo 'Adding order_items seller foreign key...'
ALTER TABLE order_items 
    ADD CONSTRAINT fk_order_item_seller 
    FOREIGN KEY (seller_id) 
    REFERENCES sellers(seller_id)
    ON DELETE SET NULL;

-- Order payments -> orders
\echo 'Adding order_payments order foreign key...'
ALTER TABLE order_payments 
    ADD CONSTRAINT fk_payment_order 
    FOREIGN KEY (order_id) 
    REFERENCES orders(order_id)
    ON DELETE CASCADE;

-- Order reviews -> orders
\echo 'Adding order_reviews order foreign key...'
ALTER TABLE order_reviews 
    ADD CONSTRAINT fk_review_order 
    FOREIGN KEY (order_id) 
    REFERENCES orders(order_id)
    ON DELETE CASCADE;

\echo ''
\echo 'Foreign key constraints added successfully!'
