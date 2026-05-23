-- PostgreSQL initialisation script
-- Runs once when the postgres container is first created.
-- Add new service users/databases here as services are onboarded.

DO
$do$
DECLARE
   userpassword varchar[];
   users varchar[] := array[['authorization','authorization-password-dev'],
                            ['tenancy','tenancy-password-dev'],
                            ['catalog','catalog-password-dev'],
                            ['inventory','inventory-password-dev'],
                            ['order','order-password-dev'],
                           	['finance','finance-password-dev']];
BEGIN
   FOREACH userpassword SLICE 1 IN ARRAY users
   LOOP
      IF NOT EXISTS (
          SELECT FROM pg_catalog.pg_user p
          WHERE usename = userpassword[1]) THEN
          EXECUTE 'CREATE USER '|| userpassword[1] ||' PASSWORD '|| quote_literal(userpassword[2]);
      END IF;
   END LOOP;
END $do$;

-- Tenancy database

CREATE DATABASE eshop_tenancy;
GRANT ALL PRIVILEGES ON DATABASE eshop_tenancy TO tenancy;
\c eshop_tenancy
CREATE EXTENSION IF NOT EXISTS citext;
GRANT ALL PRIVILEGES ON SCHEMA public TO tenancy;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO tenancy;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO tenancy;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO tenancy;

-- Authorization database

CREATE DATABASE eshop_authorization;
GRANT ALL PRIVILEGES ON DATABASE eshop_authorization TO authorization;
\c eshop_authorization
CREATE EXTENSION IF NOT EXISTS citext;
GRANT ALL PRIVILEGES ON SCHEMA public TO authorization;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO authorization;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO authorization;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO authorization;

-- Catalog database

CREATE DATABASE eshop_catalog;
GRANT ALL PRIVILEGES ON DATABASE eshop_catalog TO catalog;
\c eshop_catalog
CREATE EXTENSION IF NOT EXISTS citext;
GRANT ALL PRIVILEGES ON SCHEMA public TO catalog;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO catalog;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO catalog;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO catalog;

-- Inventory database

CREATE DATABASE eshop_inventory;
GRANT ALL PRIVILEGES ON DATABASE eshop_inventory TO inventory;
\c eshop_inventory
CREATE EXTENSION IF NOT EXISTS citext;
GRANT ALL PRIVILEGES ON SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO inventory;

-- Order database

CREATE DATABASE eshop_order;
GRANT ALL PRIVILEGES ON DATABASE eshop_order TO inventory;
\c eshop_order
CREATE EXTENSION IF NOT EXISTS citext;
GRANT ALL PRIVILEGES ON SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO inventory;

-- Finance database

CREATE DATABASE eshop_finance;
GRANT ALL PRIVILEGES ON DATABASE eshop_finance TO inventory;
\c eshop_finance
CREATE EXTENSION IF NOT EXISTS citext;
GRANT ALL PRIVILEGES ON SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO inventory;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO inventory;
