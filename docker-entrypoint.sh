#!/bin/sh
set -e

# Fix ownership on mounted volume root dirs only (not recursive - files
# created by the app will already be owned by UID 1654)
chown 1654:1654 /app/logs /app/backups /app/wwwroot/uploads /home/app/.aspnet/DataProtection-Keys 2>/dev/null || true

# Drop to non-root user and exec the app
exec su-exec 1654:1654 "$@"
