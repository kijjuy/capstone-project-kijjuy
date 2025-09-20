insert into categories (category_name) values('test_category');
insert into categories (category_name) values('test_category2');

insert into products (name, price, description, is_available, category_id) values('test_product', 129.99, 'description for test prouduct', TRUE, (select category_id from categories where category_name = 'test_category'));
insert into products (name, price, description, is_available, category_id) values('test_product2', 159.99, 'description for test prouduct 2', TRUE, (select category_id from categories where category_name = 'test_category'));
