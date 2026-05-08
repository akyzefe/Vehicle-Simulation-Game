# 🚗 Advanced Vehicle Physics & Procedural Environment (WIP)

<p align="center">
  🌍 <b><a href="#english">English</a> | 🇹🇷 <a href="#türkçe">Türkçe</a></b>
</p>

---

<a name="english"></a>
## 🇬🇧 English

![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?style=flat-square&logo=unity)
![C#](https://img.shields.io/badge/Language-C%23-blue?style=flat-square&logo=csharp)
![Status](https://img.shields.io/badge/Status-Work_in_Progress-orange?style=flat-square)

### 📝 About the Project
This project is a comprehensive vehicle simulation framework built from scratch in Unity. It goes beyond basic movement, focusing on realistic wheel friction physics, transmission logic, procedural environment design, and custom editor automation tools to streamline the development pipeline.

### ✨ Key Features

#### 🏎️ Vehicle Dynamics (`CarController.cs`)
- **Multi-Drive System:** Support for FWD, RWD, and AWD with dynamic torque splitting.
- **Driver Assist Systems:** Real-time **ABS** and **TCS** algorithms calculating slip ratios for optimal traction.
- **Advanced Transmission:** Realistic gear shifting with power cut-offs (Shift Delay) and RPM-based automatic logic.
- **Telemetry & Performance:** Built-in **0-100 km/h** timer for physics testing.
- **Aerodynamics & Stability:** Speed-dependent **Downforce** and an **Anti-Roll Bar** system to prevent flipping.

#### 🎥 Dynamic Camera (`CameraFollow.cs`)
- Velocity-aware lateral offset system that leans the camera during drifts and sharp turns.
- Smooth damping and rotation interpolation (Slerp).

#### 🛠️ Automation & Pipeline Tools
- **`AutoWheelSetup.cs`:** Automatically calculates wheel radius from mesh bounds and sets up physics.
- **`WheelPivotFixer.cs` & `AutoWheelFixer.cs`:** Solves Unity's wheel rotation issues via hierarchical wrapping and pivot alignment.

---

<a name="türkçe"></a>
## 🇹🇷 Türkçe

### 📝 Proje Hakkında
Bu proje, Unity üzerinde sıfırdan geliştirilmiş kapsamlı bir araç simülasyon altyapısıdır. Sadece basit bir araç hareketi değil; gerçekçi sürtünme fizikleri, şanzıman mantığı, prosedürel çevre tasarımı ve geliştirme sürecini hızlandıran özel editör otomasyon araçlarına odaklanmaktadır.

### ✨ Öne Çıkan Özellikler

#### 🏎️ Araç Dinamikleri (`CarController.cs`)
- **Çekiş Sistemi:** FWD, RWD ve AWD (dinamik tork dağılımlı) sistem desteği.
- **Sürüş Destek Sistemleri:** Optimum yol tutuşu için kayma oranlarını hesaplayan gerçek zamanlı **ABS** ve **TCS**.
- **Gelişmiş Şanzıman:** Vites geçişlerinde güç kesintisi (Shift Delay) ve devir bazlı otomatik vites mantığı.
- **Telemetri:** Fizik testleri için entegre **0-100 km/h** ölçüm sistemi.
- **Aerodinamik ve Stabilite:** Hıza bağlı **Downforce** ve devrilmeyi önleyen **Anti-Roll Bar** sistemi.

#### 🎥 Dinamik Kamera (`CameraFollow.cs`)
- Drift ve sert dönüşlerde kamerayı yatıran, hıza duyarlı yan kayma (Lateral Offset) sistemi.
- Yumuşak takip ve sönümlü rotasyon geçişleri.

#### 🛠️ Otomasyon ve İş Akışı Araçları
- **`AutoWheelSetup.cs`:** Mesh boyutlarından tekerlek yarıçapını otomatik hesaplar ve kurulumu yapar.
- **`WheelPivotFixer.cs` & `AutoWheelFixer.cs`:** Hiyerarşik kılıflama ve pivot hizalama ile Unity tekerlek rotasyon sorunlarını çözer.

## 📬 Contact / İletişim
[Efe Akyüz] - [https://www.linkedin.com/in/efe-aky%C3%BCz-b31236308/] - [akyz.efe@gmail.com]
