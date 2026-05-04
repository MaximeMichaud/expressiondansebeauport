#!/bin/sh
set -e

# Fix ownership on mounted volume root dirs only (not recursive - files
# created by the app will already be owned by UID 1654)
chown 1654:1654 /app/logs /app/backups /app/wwwroot/uploads /home/app/.aspnet/DataProtection-Keys 2>/dev/null || true

if [ -d /app/seed-uploads ]; then
  find /app/seed-uploads -type f | while IFS= read -r source; do
    relative_path=${source#/app/seed-uploads/}
    destination="/app/wwwroot/uploads/$relative_path"

    if [ ! -f "$destination" ]; then
      mkdir -p "$(dirname "$destination")"
      cp "$source" "$destination"
      chown 1654:1654 "$destination" 2>/dev/null || true
    fi
  done
fi

# Drop to non-root user and exec the app
exec su-exec 1654:1654 "$@"
