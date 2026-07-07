#!/usr/bin/env bash
set -euo pipefail

SAFE_CHAIN_VERSION="1.5.12"
REPO_URL="https://github.com/AikidoSec/safe-chain"
SCRIPTS_DIR="$(cd "$(dirname "$0")" && pwd)"
CHECKSUMS_FILE="${SCRIPTS_DIR}/safe-chain-checksums.txt"
TMP_INSTALLER="/tmp/install-safe-chain.sh"

usage() {
    echo "Usage: $0 [--ci]"
    echo ""
    echo "  --ci    Install Safe Chain in CI mode (shims in PATH instead of shell aliases)"
    exit 1
}

CI_MODE=false
while [[ $# -gt 0 ]]; do
    case "$1" in
        --ci) CI_MODE=true ;;
        -h|--help) usage ;;
        *) echo "Unknown option: $1"; usage ;;
    esac
    shift
done

echo "=== Aikido Safe Chain Installer (v${SAFE_CHAIN_VERSION}) ==="
echo ""

# Step 1: Download the installer script from the pinned release
echo "1. Downloading install script from GitHub release..."
INSTALLER_URL="${REPO_URL}/releases/download/${SAFE_CHAIN_VERSION}/install-safe-chain.sh"
if command -v curl &>/dev/null; then
    curl -fsSL "$INSTALLER_URL" -o "$TMP_INSTALLER"
elif command -v wget &>/dev/null; then
    wget -q "$INSTALLER_URL" -O "$TMP_INSTALLER"
else
    echo "ERROR: Neither curl nor wget found." >&2
    exit 1
fi
echo "   Downloaded to: $TMP_INSTALLER"
echo ""

# Step 2: Verify the installer script checksum
echo "2. Verifying installer script checksum..."
EXPECTED_HASH=$(grep "install-safe-chain.sh" "$CHECKSUMS_FILE" | grep -v "^#" | awk '{print $1}')
if [ -z "$EXPECTED_HASH" ]; then
    echo "ERROR: Could not find expected hash in $CHECKSUMS_FILE" >&2
    exit 1
fi

if command -v sha256sum &>/dev/null; then
    ACTUAL_HASH=$(sha256sum "$TMP_INSTALLER" | awk '{print $1}')
elif command -v shasum &>/dev/null; then
    ACTUAL_HASH=$(shasum -a 256 "$TMP_INSTALLER" | awk '{print $1}')
else
    echo "ERROR: No sha256sum or shasum available." >&2
    exit 1
fi

if [ "$ACTUAL_HASH" != "$EXPECTED_HASH" ]; then
    echo "ERROR: Checksum mismatch!" >&2
    echo "  Expected: $EXPECTED_HASH" >&2
    echo "  Actual:   $ACTUAL_HASH" >&2
    echo "  This could indicate a tampered installer. Aborting." >&2
    rm -f "$TMP_INSTALLER"
    exit 1
fi
echo "   Checksum verified: ${ACTUAL_HASH}"
echo ""

# Step 3: Run the verified installer
echo "3. Running Safe Chain installer..."
chmod +x "$TMP_INSTALLER"
if [ "$CI_MODE" = true ]; then
    sh "$TMP_INSTALLER" --ci
else
    sh "$TMP_INSTALLER"
fi
echo ""

# Step 4: Clean up
rm -f "$TMP_INSTALLER"

echo "=== Safe Chain installation complete ==="
echo ""
echo "If this is a local install, restart your terminal or run:"
echo "  source ~/.bashrc   # (or ~/.zshrc, etc.)"
