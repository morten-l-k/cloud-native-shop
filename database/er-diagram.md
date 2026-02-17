# E-commerce Database ER Diagram

## Crow's Foot Notation Guide

- `||` = **Exactly one** (must exist)
- `|o` = **Zero or one** (optional, max one)
- `}|` = **One or many** (at least one)
- `}o` = **Zero or many** (optional, unlimited)

**Example:** `customers ||--o{ orders` means "One customer can have zero or many orders, and each order belongs to exactly one customer"

## Database Schema

```mermaid
erDiagram
    customers ||--o{ orders : "places"
    orders ||--o{ order_items : "contains"
    orders ||--o{ order_payments : "has"
    orders ||--o{ order_reviews : "receives"
    products ||--o{ order_items : "included in"
    sellers ||--o{ order_items : "sells"
    product_category_translation ||--o{ products : "categorizes"
    
    customers {
        varchar customer_id PK
        varchar customer_unique_id
        varchar customer_zip_code_prefix
        varchar customer_city
        varchar customer_state
    }
    
    orders {
        varchar order_id PK
        varchar customer_id FK
        varchar order_status
        timestamp order_purchase_timestamp
        timestamp order_approved_at
        timestamp order_delivered_carrier_date
        timestamp order_delivered_customer_date
        timestamp order_estimated_delivery_date
    }
    
    order_items {
        serial order_item_id PK
        varchar order_id FK
        int order_item_number
        varchar product_id FK
        varchar seller_id FK
        timestamp shipping_limit_date
        decimal price
        decimal freight_value
    }
    
    order_payments {
        serial payment_id PK
        varchar order_id FK
        int payment_sequential
        varchar payment_type
        int payment_installments
        decimal payment_value
    }
    
    order_reviews {
        varchar review_id PK
        varchar order_id FK
        int review_score
        text review_comment_title
        text review_comment_message
        timestamp review_creation_date
        timestamp review_answer_timestamp
    }
    
    products {
        varchar product_id PK
        varchar product_category_name FK
        int product_name_length
        int product_description_length
        int product_photos_qty
        int product_weight_g
        int product_length_cm
        int product_height_cm
        int product_width_cm
    }
    
    sellers {
        varchar seller_id PK
        varchar seller_zip_code_prefix
        varchar seller_city
        varchar seller_state
    }
    
    product_category_translation {
        varchar product_category_name PK
        varchar product_category_name_english
    }
    
    geolocation {
        serial geolocation_id PK
        varchar geolocation_zip_code_prefix
        decimal geolocation_lat
        decimal geolocation_lng
        varchar geolocation_city
        varchar geolocation_state
    }
```

## Relationships

- **customers** → **orders**: One customer can place zero or many orders
- **orders** → **order_items**: Each order contains zero or many items
- **orders** → **order_payments**: Each order has zero or many payment records
- **orders** → **order_reviews**: Each order receives zero or many reviews
- **products** → **order_items**: Each product appears in zero or many order items
- **sellers** → **order_items**: Each seller fulfills zero or many order items
- **product_category_translation** → **products**: Each category contains zero or many products
- **geolocation**: Used for geographic lookups via zip code prefix (no direct foreign key)
