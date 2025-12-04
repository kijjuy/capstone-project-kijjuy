-- First database migration
-- Dropped orders column for CCLast4

DROP TABLE IF EXISTS categories;

CREATE TABLE categories (
    category_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    category_name NVARCHAR(30) NOT NULL
);

DROP TABLE IF EXISTS products;

CREATE TABLE products (
    product_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    category_id INTEGER NOT NULL,
    name NVARCHAR(50) NOT NULL,
    price NUMERIC(6,2) NOT NULL,
    description NVARCHAR(500) NOT NULL,
    creation_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    update_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_available INTEGER NOT NULL,

    FOREIGN KEY (category_id) REFERENCES categories(category_id)
);

DROP TABLE IF EXISTS images;

CREATE TABLE images (
    image_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    product_id INTEGER NOT NULL,
    file_path NVARCHAR(100) NOT NULL,

    FOREIGN KEY (product_id) REFERENCES products(product_id)
);

DROP TABLE IF EXISTS orders;
CREATE TABLE orders (
    order_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    user_name NVARCHAR(100) NOT NULL,
    subtotal_paid NUMERIC(7,2) NOT NULL,
    tax_paid NUMERIC(6,2) NOT NULL,
    shipping_paid NUMERIC(5,2),
    total_paid NUMERIC (7,2),
    shipping_address NVARCHAR(100) NOT NULL,
    shipping_name NVARCHAR(100) NOT NULL,
    order_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    order_status CHAR(8)
);

DROP TABLE IF EXISTS order_products;
CREATE TABLE order_products (
    order_product_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    product_id INTEGER NOT NULL,
    order_id INTEGER NOT NULL,

    FOREIGN KEY(product_id) REFERENCES products(product_id),
    FOREIGN KEY(order_id) REFERENCES orders(order_id)
);


-- Not working currently
DROP TRIGGER IF EXISTS auto_update_date_products;

CREATE TRIGGER auto_update_date_products
AFTER UPDATE
on products
FOR EACH ROW
    BEGIN
	UPDATE products SET update_date = CURRENT_TIMESTAMP WHERE old.product_id = new.product_id;
    END;


