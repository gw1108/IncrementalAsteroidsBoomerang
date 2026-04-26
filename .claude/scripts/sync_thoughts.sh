#!/usr/bin/env bash
# Ensures thoughts directories exist and stages any new/modified thoughts files in git.
# Usage: sync_thoughts.sh [optional-file-path]
set -euo pipefail

REPO_ROOT="$(git -C "$(dirname "$0")" rev-parse --show-toplevel)"

mkdir -p "$REPO_ROOT/thoughts/shared/research"
mkdir -p "$REPO_ROOT/thoughts/shared/plans"
mkdir -p "$REPO_ROOT/thoughts/shared/tickets"

git -C "$REPO_ROOT" add "$REPO_ROOT/thoughts/" 2>/dev/null || true

echo "thoughts/ synced and staged."
