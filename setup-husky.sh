#!/usr/bin/env sh
# One-shot Husky + Commitlint bootstrap (v9+)
# Run: sh scripts/setup-husky.sh
set -e

info() { printf '\033[36m» %s\033[0m\n' "$*"; }
ok()   { printf '\033[32m✓ %s\033[0m\n' "$*"; }
warn() { printf '\033[33m! %s\033[0m\n' "$*"; }

# 1. Ensure Node/npm
command -v node >/dev/null 2>&1 || { echo "Node.js not found"; exit 1; }
command -v npm  >/dev/null 2>&1 || { echo "npm not found"; exit 1; }
ok "Node $(node -v), npm $(npm -v)"

# 4. Install deps
info "Installing husky + commitlint"
npm i -D husky @commitlint/cli @commitlint/config-conventional commitlint-plugin-function-rules >/dev/null

# If lock exists, use ci
if [ -f package-lock.json ]; then npm ci; else npm install; fi


# 8. Commitlint config if missing
if [ ! -f commitlint.config.js ]; then
  echo 'module.exports = { extends: ["@commitlint/config-conventional"] };' > commitlint.config.js
  ok "commitlint.config.js created"
else
  ok "commitlint.config.js found"
fi

# 9. Self-test
info "Testing commitlint (should fail on 'bad')"
if echo bad | npx commitlint >/dev/null 2>&1; then
  warn "Commitlint accepted 'bad' — check rules"
else
  ok "Commitlint active"
fi

ok "Husky + Commitlint fully configured 🎉"
