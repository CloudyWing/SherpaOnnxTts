# sherpa-onnx TTS Demo Server

這是本地端 TTS 技術評估期間的實驗方案。此專案轉為實驗存檔，不提供維護與支援。

## 專案緣起

一開始是想找能在本機執行有 **CPU-only** 版本的 TTS。

中間嘗試了以下選擇：

- **GPT-SoVITS**：功能很強大，但操作流程太碎、太複雜，玩了兩天就放棄。
- **AllTalk**：沒辦法在同一句話裡自然切換中英文（需手動指定語言）。
- **Kokoro**：中英混用效果差且中文帶有方言口音（雖然後來看到 sherpa-onnx 有支援 Kokoro 的雙語模型版本，但可能是我當時用的內建模型不支援或設定問題）。
- **ChatTTS**：架設失敗，不予置評。
- **sherpa-onnx**：(搭配 `matcha-icefall-zh-en` 模型) 念出來的語音聽起來較習慣，而模型本身就同時支援中英文，不會有前面選中文模型唸到英文念得很奇怪或是根本不支援的問題。

不過後來我本地 AI 改用 MoE 架構後，有多餘的 VRAM 可用 (MoE 模型萬歲)，便轉向其他 GPU 加速的 TTS。這專案只把當時 Antigravity 幫我寫的 Python 版本轉為 .NET 後，就先擱置著。

---

## 這是什麼

使用 sherpa-onnx 的本機 TTS 服務，由此專案封裝為 Web API：

- **技術棧**：ASP.NET Core 10 / C#。
- **API 介面**：提供 OpenAI 相容 API (`/v1/audio/speech`) 與簡易 GET 介面。
- **部署**：Docker 容器化部署 (支援非 root 安全執行)。
- **核心能力**：
  - 支援中英文混合合成。
  - 支援多模型動態載入 (Matcha、Kokoro、Vits)。
  - 支援 CPU 推論 (適合無 GPU 環境)。

---

## 快速開始

### 1. 準備模型

執行腳本自動下載預設模型：

```bash
bash ./setup_models.sh
```

> ⚠️ **注意**：模型檔案較大且包含 Vocoder，請確保下載過程網路穩定。

### 2. 啟動服務

使用 Docker Compose 啟動：

```bash
docker compose up -d --build
```

### 3. API 參數說明

| 參數名稱 | 說明 | 適用情境 | 範例 |
| :--- | :--- | :--- | :--- |
| **voice** | **說話者 ID 或 名稱** (內建對映表)。 | GET /tts, POST | `0`, `af_bella` |
| **sid** / **speaker_id** | **說話者 ID** (僅限數值)。 | GET /tts | `0`, `1` |

> **💡 Kokoro 模型 (v1.0) 常見對應表**：
>
> 系統已內建名稱對映，可直接在 `voice` 欄位傳入以下名稱：
>
> - **`af_bella`** (或 `shimmer`) ➡️ ID 2
> - **`af_alloy`** (或 `alloy`) ➡️ ID 0
> - **`af_aoede`** (或 `nova`) ➡️ ID 1
> - **`am_adam`** (或 `echo`) ➡️ ID 30
> - **`am_michael`** (或 `fable` / `onyx`) ➡️ ID 33
> - **其他**：`af_jessica` (3), `af_kore` (4), `af_nicole` (11), `af_sky` (21)
| **model** | **模型引擎**。決定載入哪個模型資料夾。 | GET /tts, POST | `vits-zh-aishell3` |
| **format** | **下載格式** (GET 專用)。 | GET /tts | `wav` (預設), `pcm` |

### 4. 串流模式 (Streaming) - POST /v1/audio/speech

**POST `/v1/audio/speech` 專為串流設計，不提供檔案下載功能。**

- **行為**：永遠並強制回傳 Raw PCM (f32le) 串流。
- **用途**：適用於即時語音對話機器人、低延遲應用開發。

> **ℹ️ 技術說明：為什麼強制回傳 Raw PCM？**
>
> 1. **WAV 限制**：標準 WAV 檔頭必須包含「檔案總長度」。在串流生成模式下，伺服器無法預知音訊總長，因此無法產生合法的 WAV 檔頭。
> 2. **MP3/Opus 支援**：SherpaOnnx 專注於推論，核心庫不包含複雜的 MP3/Opus 編碼器。
> 3. **目前實踐**：為了確保 Client 端收到的是「真正可即時播放」的數據流，我們選擇直接輸出 Float32 PCM，避免發送「假 WAV」導致播放器錯誤。

#### 瀏覽器測試

由於 Scalar UI 與瀏覽器原生 `<audio>` 標籤皆不支援直接播放 Raw PCM 串流，因此我們提供了專用的測試頁面來驗證此功能。
請訪問 <http://localhost:5364/stream-test.html>

### 5. 使用介面

開啟瀏覽器訪問 `http://localhost:5364/scalar/v1` 即可查看 Scalar API 文件與測試頁面。

---

## API 參考

服務預設監聽 `5364` 埠。

### 1. 健康檢查與模型列表

`GET /health`
回傳服務狀態、預設語者與已載入的模型列表。

### 2. OpenAI 相容介面

`POST /v1/audio/speech`

- `input`: 要合成的文字。
- `voice`: 模型名稱或 ID (對應 `./volumes/models/` 下的資料夾名稱)。
- `speed`: 語速 (預設 1.0)。
- `response_format`: `pcm`。

**範例：**

```bash
curl -X POST "http://127.0.0.1:5364/v1/audio/speech" \
  -H "Content-Type: application/json" \
  -d '{"input":"你好，很高興見到你 (Hello, nice to meet you)","voice":"default","speed":1.0}' \
  -o output.wav
```

### 3. 簡易 GET 介面

`GET /tts?text=...&model=...&voice=...`
適合瀏覽器直接測試。

---

## 開發指南

### 系統需求

- .NET 10.0 SDK
- Docker Desktop

### 專案結構

```text
.
├── src/                 # C# 原始碼
│   └── SherpaOnnxTts/   # ASP.NET Core Web API 專案
├── tests/               # 測試專案
│   └── SherpaOnnxTts.Tests/ # NUnit 測試
├── volumes/             # 存放模型檔案 (gitignored)
├── compose.yml          # Docker Compose 設定
├── Dockerfile           # 多階段建置 Dockerfile (Non-root)
├── setup_models.sh      # 模型下載腳本
└── SherpaOnnxTts.slnx   # 解決方案檔
```

### 執行測試

```bash
dotnet test
```

---

## Docker 部署與使用

### 1. 使用現有 Image (假設發佈)

本專案可以直接打包成 Docker Image，讓其他使用者無需安裝 .NET 環境即可執行：

```bash
# 建立 Image (若下載了原始碼)
docker build -t sherpa-onnx-tts .

### Linux / macOS (Bash)
```bash
docker run -d \
  -p 5364:5364 \
  -v "$(pwd)/volumes/models:/app/models" \
  --name my-tts-service \
  sherpa-onnx-tts
```

### Windows (PowerShell)

```powershell
docker run -d `
  -p 5364:5364 `
  -v "${PWD}/volumes/models:/app/models" `
  --name my-tts-service `
  sherpa-onnx-tts
```

> **ℹ️ 提示**：此專案不會維護，所以不建議用此方式。

### 2. 使用 Docker Compose (開發/生產)

```bash
docker compose up -d --build
```

---

## License

- **此封裝專案**：MIT License
- **sherpa-onnx**：Apache-2.0
- **TTS 模型**：各模型授權不同（請檢查 `./volumes/models/<模型名稱>/`）

**⚠️ 使用者責任**：使用任何模型前，請確認其授權條款，特別是商業用途。詳情請參閱 [LICENSE.md](./LICENSE.md)。
