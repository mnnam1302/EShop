# =============================================================================
# MongoDB initialization — PRODUCTION ONLY.
#
# Shell scripts in /docker-entrypoint-initdb.d/ run as root inside the
# container, so they can read Docker secret files directly from /run/secrets/.
# No environment variables needed — password never exposed in process list
# or docker inspect output.
#
# Authentication happens natively against each database — no authSource needed.
# =============================================================================
#!/bin/bash
set -euo pipefail

CATALOG_PASSWORD=$(cat /run/secrets/mongodb_catalog_password)

mongosh --username "$MONGO_INITDB_ROOT_USERNAME" \
        --password "$(cat /run/secrets/mongodb_password)" \
        --authenticationDatabase admin \
        --eval "
            db = db.getSiblingDB('eshop-catalog');
            db.createUser({
                user: 'catalog',
                pwd: '${CATALOG_PASSWORD}',
                roles: [{ role: 'readWrite', db: 'eshop-catalog' }]
            });
        "
