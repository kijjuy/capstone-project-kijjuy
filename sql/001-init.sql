-- First database migration

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

    FOREIGN KEY (category_id) REFERENCES "categories" (category_id)
);

DROP TABLE IF EXISTS images;

CREATE TABLE images (
    image_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    product_id INTEGER FOREIGN KEY REFERENCES "products" (product_id),
    file_path NVARCHAR(100) NOT NULL
);

DROP TABLE IF EXISTS users;
CREATE TABLE users (
    user_id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
    email TEXT NOT NULL,
    pass_hash TEXT NOT NULL,
    first_name NVARCHAR(50),
    last_name NVARCHAR(50),
    phone CHAR(10),
    address NVARCHAR(100),
    creation_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    update_date DATE NOT NULL DEFAULT CURRENT_TIMESTAMP,
);

DROP TABLE IF EXISTS orders;
CREATE TABLE orders (
    
);

DROP TABLE IF EXISTS order_products;
CREATE TABLE order_products (
    
);


-- Not working currently
DROP TIGGER IF EXISTS auto_update_date

CREATE TRIGGER auto_update_date
AFTER UPDATE
on products
FOR EACH ROW
    BEGIN
	UPDATE products SET update_date = CURRENT_TIMESTAMP WHERE old.product_id = new.product_id;
    END;


