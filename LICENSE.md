# License

## 1. This Project (Wrapper Code)

The API wrapper, Docker configuration, shell scripts, and documentation in this repository are licensed under the **MIT License** (see below).

**What is covered by MIT License:**

- `main.py`, `setup_models.sh`, `compose.yml`, `Dockerfile`
- Documentation (`README.md` and this file)
- Any original code written for this wrapper

**What is NOT covered:**

- sherpa-onnx library (see section 2)
- Pre-trained TTS models (see section 3)

---

## 2. Core Engine: sherpa-onnx

This project depends on **[sherpa-onnx](https://github.com/k2-fsa/sherpa-onnx)**, which is licensed under the **Apache License 2.0**.

- **Not bundled**: This project does not redistribute sherpa-onnx source code
- **Installed separately**: sherpa-onnx is installed via `pip` during Docker build
- **Original license applies**: Apache-2.0 terms govern your use of sherpa-onnx

---

## 3. Pre-trained TTS Models

### ⚠️ CRITICAL - READ BEFORE USE

#### Different models = Different licenses

Models downloaded via `setup_models.sh` are stored in `./volumes/models/<model_name>/` and each has its own license.

#### Common license types you may encounter

- ✅ **Apache-2.0 / MIT**: Generally allows commercial use (with attribution)
- ⚠️ **CC BY-4.0**: Requires attribution, check derivatives policy
- ❌ **Academic/Research-only**: **PROHIBITS commercial use**
- ❌ **Non-commercial**: **PROHIBITS commercial use**

### How to Check a Model's License

Before using any model, **you MUST**:

1. Navigate to the model directory:

   ```bash
   cd ./volumes/models/<model_name>/
   ```

2. Read the license files:

   ```bash
   cat README.md
   cat LICENSE      # if exists
   cat LICENCE      # some use British spelling
   ```

3. Look for keywords:
   - "commercial use"
   - "research only"
   - "non-commercial"
   - "attribution required"

### Examples (for reference only - always verify yourself)

| Model | Typical License | Commercial Use? |
| :--- | :--- | :--- |
| vits-zh-aishell3 | Academic | ❌ Research only |
| matcha-icefall-zh-en | Varies | ⚠️ Check directory |

> **Note**: The table above is for illustration only.
> License terms can change. Always check the actual files in your `./volumes/models/` directory.

---

## 4. Your Responsibilities

### 🚨 Legal Disclaimer

**YOU (the end user) are solely responsible for:**

1. ✅ Reviewing the license of each model you use
2. ✅ Ensuring your use case complies with each license
3. ✅ Providing proper attribution where required
4. ✅ Obtaining commercial licenses if your use case requires them
5. ✅ Any legal consequences of license violations

**This wrapper project:**

- ❌ Does NOT grant you rights to third-party models beyond their original licenses
- ❌ Does NOT make academic models "commercial-safe" just by wrapping them
- ❌ Does NOT provide legal advice on license compliance

**When in doubt:**

- Contact the original model authors
- Consult a lawyer for commercial deployments
- Use only models with clearly permissive licenses (Apache-2.0, MIT)

---

## 5. No Warranty

**FOR ALL COMPONENTS** (wrapper code, sherpa-onnx, and models):

This software is provided "AS IS" without warranty of any kind.  
See the full MIT License text below for wrapper code warranties.  
Third-party components have their own warranty disclaimers.

---

## 6. MIT License (Wrapper Code Only)

Copyright (c) 2025-present Shang-Wei (Wing) Chou

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

---

## 7. Summary for Quick Reference

| Component | License | Commercial Use? | Check Where |
| :--- | :--- | :--- | :--- |
| This wrapper | MIT | ✅ Yes | This file |
| sherpa-onnx | Apache-2.0 | ✅ Yes | [GitHub](https://github.com/k2-fsa/sherpa-onnx/blob/master/LICENSE) |
| TTS Models | **Varies** | ⚠️ **Check each** | `./volumes/models/<name>/` |

**Bottom line:** The wrapper is free to use commercially, but **each model must be verified separately.**
