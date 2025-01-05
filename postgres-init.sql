DO
$do$
DECLARE
   userpassword varchar[];
   users varchar[] := array[['users','users-password-dev']];
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


CREATE DATABASE eshop_users;

GRANT ALL PRIVILEGES ON DATABASE eshop_users TO users;
\c eshop_users
CREATE EXTENSION IF NOT EXISTS citext;
\c eshop_users
GRANT ALL PRIVILEGES ON SCHEMA public TO users;
GRANT ALL PRIVILEGES ON ALL TABLES IN SCHEMA public TO users;
GRANT ALL PRIVILEGES ON ALL SEQUENCES IN SCHEMA public TO users;
GRANT ALL PRIVILEGES ON ALL FUNCTIONS IN SCHEMA public TO users;

drop database eshop_users;