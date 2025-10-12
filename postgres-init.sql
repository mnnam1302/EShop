DO
$do$
DECLARE
   userpassword varchar[];
   users varchar[] := array[['authorization','authorization-password-dev'],
							['users','users-password-dev'], 
                            ['tenancy','tenancy-password-dev'],
                            ['configuration','configuration-password-dev'],
							['catalog','catalog-password-dev']];
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

CREATE DATABASE eshop_authorization;

GRANT ALL PRIVILEGES ON DATABASE eshop_authorization TO authorization;
\c eshop_authorization
CREATE EXTENSION IF NOT EXISTS citext;
\c eshop_authorization
GRANT ALL PRIVILEGES ON SCHEMA public TO authorization;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO authorization;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO authorization;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO authorization;


CREATE DATABASE eshop_users;

GRANT ALL PRIVILEGES ON DATABASE eshop_users TO users;
\c eshop_users
CREATE EXTENSION IF NOT EXISTS citext;
\c eshop_users
GRANT ALL PRIVILEGES ON SCHEMA public TO users;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO users;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO users;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO users;


CREATE DATABASE eshop_tenancy;

GRANT ALL PRIVILEGES ON DATABASE eshop_tenancy TO tenancy;
\c eshop_tenancy
CREATE EXTENSION IF NOT EXISTS citext;
\c eshop_tenancy
GRANT ALL PRIVILEGES ON SCHEMA public TO tenancy;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO tenancy;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO tenancy;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO tenancy;

CREATE DATABASE eshop_configuration;

GRANT ALL PRIVILEGES ON DATABASE eshop_configuration TO configuration;
\c eshop_configuration
CREATE EXTENSION IF NOT EXISTS citext;
\c eshop_configuration
GRANT ALL PRIVILEGES ON SCHEMA public TO configuration;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO configuration;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO configuration;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO configuration;


CREATE DATABASE eshop_catalog;

GRANT ALL PRIVILEGES ON DATABASE eshop_catalog TO catalog;
\c eshop_catalog
CREATE EXTENSION IF NOT EXISTS citext;
\c eshop_catalog
GRANT ALL PRIVILEGES ON SCHEMA public TO catalog;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO catalog;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO catalog;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO catalog;
