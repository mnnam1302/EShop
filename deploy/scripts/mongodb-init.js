// =============================================================================
// MongoDB initialization — LOCAL DEVELOPMENT ONLY.
//
// Creates a dedicated user for each application database.
// Authentication happens natively against each database — no authSource needed.
//
// Run automatically on first container start via /docker-entrypoint-initdb.d/
// =============================================================================

db = db.getSiblingDB('eshop-catalog');

db.createUser({
    user: 'catalog',
    pwd: 'catalog-dev',
    roles: [
        { role: 'readWrite', db: 'eshop-catalog' }
    ]
});
