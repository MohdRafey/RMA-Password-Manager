**RMA** (Remember Me Always) is an offline-first, local-only password manager. It is designed for users who want total control over their data without relying on cloud services.

> **Status:** 🏗️ Early Development (Windows Phase)

---

## 🍃 Core Philosophy
* **Offline-Only:** No data ever leaves your local network.
* **Phone-as-a-Key:** Use your Android device to unlock your Windows vault via a secure QR handshake.
* **Warm Aesthetics:** A modern, light-mode Glassmorphism UI that feels secure and inviting.

---

## 🛠️ Tech Stack
* **Windows:** .NET 9 + WPF (WPF-UI for Mica/Acrylic effects).
* **Android:** Kotlin + Jetpack Compose.
* **Security:** SQLite (SQLCipher), Argon2id Key Derivation, and AES-256-GCM.

---

## 🚀 Immediate Roadmap
- [ ] Initialize Encrypted SQLite (.rma) vault structure.
- [ ] Build the "Warm Glass" Login UI for Windows.
- [ ] Implement Argon2id PIN-to-Key derivation.
- [ ] Create the Android "Sentinel" for Biometric unlocking.

---

## 📄 License
Distributed under the MIT License.