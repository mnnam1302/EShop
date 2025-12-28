
-----------------------------------
-- Schema Tenancy
----------------------------------- 
DO $$ DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') 
	LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
	
	FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'hangfire') 
	LOOP
        EXECUTE 'DROP TABLE IF EXISTS hangfire.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
END $$;

-----------------------------------
-- Schema Authorization
----------------------------------- 
DO $$ DECLARE
    r RECORD;
BEGIN
    FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'public') 
	LOOP
        EXECUTE 'DROP TABLE IF EXISTS public.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
	
	FOR r IN (SELECT tablename FROM pg_tables WHERE schemaname = 'hangfire') 
	LOOP
        EXECUTE 'DROP TABLE IF EXISTS hangfire.' || quote_ident(r.tablename) || ' CASCADE';
    END LOOP;
END $$;