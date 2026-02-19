#!/bin/bash
set -e

# ==========================================
# Sherpa-ONNX Model Setup Script (Updated)
# ==========================================

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

# 1. Define Models
# ------------------------
BASE_URL="https://github.com/k2-fsa/sherpa-onnx/releases/download/tts-models"

# Format: "directory_name|archive_name|extra_files_needed"
MODELS=(
    "matcha-icefall-zh-en|matcha-icefall-zh-en.tar.bz2|vocoder"
    "vits-zh-aishell3|vits-zh-aishell3.tar.bz2|"
    "vits-icefall-zh-aishell3|vits-icefall-zh-aishell3.tar.bz2|"
    # Kokoro v1.0 (Multi-lang: Chinese & English)
    "kokoro-multi-lang-v1_0|kokoro-multi-lang-v1_0.tar.bz2|"
)

# 2. Prepare Directories
# ----------------------
MODEL_ROOT="volumes/models"
TEMP_DIR="./tmp_download"
mkdir -p "$MODEL_ROOT"
mkdir -p "$TEMP_DIR"

echo "[INFO] Checking model files..."

# 3. Process Models
# --------------------------------
for entry in "${MODELS[@]}"; do
    IFS='|' read -r DIR_NAME ARCHIVE_NAME EXTRA <<< "$entry"
    MODEL_DIR="$MODEL_ROOT/$DIR_NAME"
    
    if [ ! -d "$MODEL_DIR" ] || [ -z "$(ls -A "$MODEL_DIR")" ]; then
        echo "[INFO] Model $DIR_NAME not found or empty. Downloading..."
        
        ARCHIVE_PATH="$TEMP_DIR/$ARCHIVE_NAME"
        MODEL_URL="$BASE_URL/$ARCHIVE_NAME"
        
        echo "  - Downloading $ARCHIVE_NAME..."
        wget -q --show-progress -O "$ARCHIVE_PATH" "$MODEL_URL"
        
        echo "  - Extracting to $MODEL_DIR..."
        mkdir -p "$MODEL_DIR"
        tar -xvf "$ARCHIVE_PATH" -C "$TEMP_DIR" 1>/dev/null
        
        # Move extracted files to target dir
        if [ -d "$TEMP_DIR/$DIR_NAME" ]; then
            cp -r "$TEMP_DIR/$DIR_NAME/"* "$MODEL_DIR/"
            rm -rf "$TEMP_DIR/$DIR_NAME"
        else
            cp -r "$TEMP_DIR/"* "$MODEL_DIR/" 2>/dev/null || true
        fi
        
        rm -f "$ARCHIVE_PATH"
        
        # 4. Handle Extra Files (like Vocoder for Matcha)
        if [[ "$EXTRA" == *"vocoder"* ]]; then
            VOCODER_FILE="$MODEL_DIR/vocos-16khz-univ.onnx"
            if [ ! -f "$VOCODER_FILE" ]; then
                echo "  - Downloading vocoder for $DIR_NAME..."
                VOCODER_URL="https://github.com/k2-fsa/sherpa-onnx/releases/download/vocoder-models/vocos-16khz-univ.onnx"
                wget -q --show-progress -O "$VOCODER_FILE" "$VOCODER_URL"
            fi
        fi

        echo "[OK] $DIR_NAME setup complete."
    else
        echo "[OK] $DIR_NAME already exists."
    fi
done

# Cleanup temp
rm -rf "$TEMP_DIR"
echo "[DONE] All model files ready."
