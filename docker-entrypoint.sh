#!/bin/sh
set -e

# Fix ownership on mounted volumes (Docker creates them as root)
chown -R 1654:1654 /app/logs /app/backups /app/wwwroot/uploads 2>/dev/null || true

# Drop to non-root user and exec the app
exec su-exec 1654:1654 "$@"
