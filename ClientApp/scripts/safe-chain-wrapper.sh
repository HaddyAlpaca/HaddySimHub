#!/usr/bin/env bash
set -euo pipefail

SAFE_CHAIN_VERSION="1.5.12"
SAFE_CHAIN_BASE="${HOME}/.safe-chain"
SAFE_CHAIN_BIN="${SAFE_CHAIN_BASE}/bin/safe-chain"
SHIMS_DIR="${SAFE_CHAIN_BASE}/shims"

SCRIPTS_DIR="$(cd "$(dirname "$0")" && pwd)"
CHECKSUMS_FILE="${SCRIPTS_DIR}/safe-chain-checksums.txt"

# Install Safe Chain if not present
if [ ! -x "${SAFE_CHAIN_BIN}" ]; then
  echo "[safe-chain] Not found — installing v${SAFE_CHAIN_VERSION}..."

  TMP_INSTALLER="/tmp/install-safe-chain.sh"
  INSTALLER_URL="https://github.com/AikidoSec/safe-chain/releases/download/${SAFE_CHAIN_VERSION}/install-safe-chain.sh"

  if command -v curl &>/dev/null; then
    curl -fsSL "$INSTALLER_URL" -o "$TMP_INSTALLER"
  elif command -v wget &>/dev/null; then
    wget -q "$INSTALLER_URL" -O "$TMP_INSTALLER"
  else
    echo "[safe-chain] ERROR: Neither curl nor wget found." >&2
    exit 1
  fi

  EXPECTED_HASH=$(grep "install-safe-chain.sh" "$CHECKSUMS_FILE" | grep -v "^#" | awk '{print $1}')
  if command -v sha256sum &>/dev/null; then
    ACTUAL_HASH=$(sha256sum "$TMP_INSTALLER" | awk '{print $1}')
  elif command -v shasum &>/dev/null; then
    ACTUAL_HASH=$(shasum -a 256 "$TMP_INSTALLER" | awk '{print $1}')
  else
    echo "[safe-chain] ERROR: No sha256sum or shasum available." >&2
    exit 1
  fi

  if [ "$ACTUAL_HASH" != "$EXPECTED_HASH" ]; then
    echo "[safe-chain] ERROR: Checksum mismatch!" >&2
    echo "  Expected: $EXPECTED_HASH" >&2
    echo "  Actual:   $ACTUAL_HASH" >&2
    rm -f "$TMP_INSTALLER"
    exit 1
  fi
  echo "[safe-chain] Installer checksum verified"

  sh "$TMP_INSTALLER" --ci
  rm -f "$TMP_INSTALLER"
fi

# Ensure Safe Chain shims are in PATH for this process
if [[ ":$PATH:" != *":${SHIMS_DIR}:"* ]]; then
  export PATH="${SHIMS_DIR}:${SAFE_CHAIN_BASE}/bin:${PATH}"
fi

exec "$@"
