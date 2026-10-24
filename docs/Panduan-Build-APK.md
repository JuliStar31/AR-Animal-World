# Panduan Membuat APK AR Animal World

Panduan ini membawa kamu dari repo ini sampai file APK yang bisa dipasang di HP Android. Semua kode game sudah ada di `Assets/Scripts`. Yang perlu kamu lakukan di komputer adalah memasang Unity dan Vuforia, membuat database kartu, lalu menekan Build.

Perkiraan waktu: 1 sampai 2 jam untuk pertama kali (sebagian besar menunggu download).

## Isi game

| Layar | Isi |
| --- | --- |
| 1. Beranda | Maskot singa, judul Animal World, tombol Mulai Belajar dan Main Kuis, jumlah bintang |
| 2. Pilih Hewan | Kartu Singa, Gajah, Kelinci, dan tombol Pindai Kartu AR |
| 3a. Scan: mencari kartu | Kamera, bingkai scan dengan garis bergerak, "Arahkan kamera ke kartu hewan!" |
| 3b. Scan: kartu ketemu | Hewan 3D berdiri di atas kartu, "Kartu ketemu!", nama dan fakta hewan, tombol suara dan info |
| 3b. Scan: gambar hewan dikenali | Foto atau gambar singa, gajah, kelinci apa saja dikenali AI, hewan 3D muncul di depan kamera, "Hewan ketemu!" |
| 3c. Scan: izin kamera | "Kameranya belum menyala", tombol Izinkan Kamera |
| 4. Detail Hewan | Gambar hewan, fakta, Dengarkan Suara, Coba Hewan Lain |
| 5. Tebak Hewan | Progres soal, gambar petunjuk, 3 pilihan jawaban, "Ulangi contoh", tombol Lanjut |
| 6. Hasil Kuis | Bintang yang didapat, pesan semangat, Main Lagi, Kembali ke Beranda |

Bintang tersimpan di HP. Tombol kembali Android juga berfungsi.

Belum ada model 3D atau suara asli? Tidak masalah. Game otomatis membuat hewan 3D sederhana dari bentuk dasar dan suara sintetis, jadi semuanya bisa langsung dicoba. Model hewan asli dipasang lewat bagian 9.

## 1. Pasang Unity

1. Download **Unity Hub** dari https://unity.com/download dan login.
2. Di Unity Hub, buka **Installs > Install Editor** dan pilih **Unity 2022.3 LTS** (Unity 6 juga bisa).
3. Centang modul **Android Build Support** beserta **OpenJDK** dan **Android SDK & NDK Tools**.
4. Tunggu sampai selesai.

## 2. Buka proyek

1. Clone atau download repo ini.
2. Di Unity Hub: **Projects > Add > Add project from disk**, pilih folder repo ini.
3. Saat pertama dibuka, Unity akan mengimpor semua file. Tunggu sampai selesai.
4. Buka **Window > TextMeshPro > Import TMP Essential Resources** lalu klik **Import**. Tanpa ini teks tidak muncul.
5. Buka **Window > Package Manager**, pilih **Unity Registry**, cari **Inference Engine** (`com.unity.ai.inference`), lalu klik **Install**. Paket ini menjalankan AI pengenal gambar hewan.

## 3. Pasang Vuforia dan license key

1. Buat akun gratis di https://developer.vuforia.com.
2. Buka **Plan & Licenses > Generate Basic License**, beri nama "AR Animal World", lalu salin license key-nya.
3. Di halaman **Downloads > SDK**, download **Add Vuforia Engine to a Unity Project**. Di Unity klik **Assets > Import Package > Custom Package...**, pilih file `.unitypackage` itu, lalu klik **Import**. Kalau Unity meminta update atau restart, ikuti saja.
4. Buka **Window > Vuforia Configuration** (atau pilih AR Camera lalu klik **Open Vuforia Engine configuration** di Inspector) dan tempel license key ke kolom **App License Key**. Jangan commit license key ke repo publik.

## 4. Siapkan gambar kartu hewan

Vuforia perlu mengenali gambar kartu yang akan dicetak.

1. Siapkan 3 gambar kartu (JPG/PNG): singa, gajah, kelinci. Gambar yang **ramai dan penuh detail** (ilustrasi berwarna dengan latar bertekstur) jauh lebih mudah dikenali daripada gambar polos.
2. Seret ketiga gambar itu ke panel **Project** di Unity (misalnya ke folder `Assets/Cards`).
3. Cetak ketiga kartu dengan ukuran sekitar 8 cm.

Tidak perlu membuat database di website Vuforia. Image Target bisa dibuat langsung dari gambar di Unity (langkah 5).

## 5. Siapkan scene

1. Buka scene utama (`Assets/Scenes/MainScene.unity`, atau buat scene baru lewat **File > New Scene**).
2. Hapus **Main Camera** dari Hierarchy.
3. Tambahkan kamera AR: **GameObject > Vuforia Engine > AR Camera**.
4. Tambahkan 3 kartu: **GameObject > Vuforia Engine > Image Target**, tiga kali. Pada tiap Image Target di Inspector:
   - **Type**: From Image
   - **Image**: seret gambar kartu yang sesuai ke kolom ini
   - **Width**: `0.08` (8 cm)
   - Ganti nama objeknya menjadi `Singa`, `Gajah`, `Kelinci`
5. Klik menu **AR Animal World > Bangun Game di Scene Ini**.

Menu ini membuat semua layar sesuai desain (font Fredoka, gambar dan ikon dari `Assets/Art/UI`), manager game, cahaya matahari kalau scene belum punya, lalu memasang `AnimalImageTargetHandler` ke tiap Image Target sesuai namanya (singa, gajah, kelinci) dan `ARCameraToggle` ke AR Camera. Menu ini aman dijalankan ulang kapan saja.

6. Simpan scene dengan **Ctrl+S**.

Cara lain (opsional): membuat database di portal Vuforia, lewat **Develop > Target Manager > Add Database** (tipe Device). Tambahkan tiap gambar sebagai target Image dengan width 0.08, download untuk **Unity Editor**, impor `.unitypackage`-nya, lalu pada Image Target pilih **Type: From Database**. Cara ini juga menampilkan rating bintang tiap gambar (usahakan minimal 3).

## 6. Coba di komputer

1. Tekan **Play**.
2. Coba semua layar. Di layar Scan ada tombol **Tes: Singa / Gajah / Kelinci** untuk pura-pura menemukan kartu tanpa kamera. Tombol ini hanya muncul di Editor dan Development Build, tidak di APK final.
3. Kalau komputer punya webcam, kamu juga bisa mengarahkan kartu cetak ke webcam.

## 7. Atur build Android

1. **File > Build Settings**, pilih **Android**, klik **Switch Platform**.
2. Klik **Add Open Scenes** supaya scene utama masuk daftar build.
3. Klik **Player Settings** dan atur:
   - **Company Name** dan **Product Name**: `AR Animal World`
   - **Other Settings > Package Name**: misalnya `com.namakamu.aranimalworld`
   - **Minimum API Level**: Android 10 (API 29) atau lebih tinggi. Vuforia 11 tidak mendukung Android di bawah 10, jadi HP yang dipakai juga minimal Android 10
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: centang **ARM64**
   - **Resolution and Presentation > Default Orientation**: Portrait
4. Kalau proyek ini sudah memakai scaffold Unity dari repo, sebagian besar pengaturan ini sudah terisi. Cukup dicek.

## 8. Build APK dan pasang di HP

1. Di **Build Settings**, klik **Build**, pilih folder, beri nama `ARAnimalWorld.apk`.
2. Kirim file APK ke HP (kabel USB, Google Drive, dan lain-lain), buka filenya, izinkan **Install dari sumber tidak dikenal**, lalu pasang.
3. Atau sambungkan HP dengan kabel, aktifkan **Opsi Developer > USB Debugging**, lalu klik **Build And Run**.
4. Saat pertama dibuka, izinkan akses kamera.

## 9. Memasang model hewan 3D asli

Model dari paket hewan tidak disimpan di GitHub (berlisensi dan ukurannya besar), jadi disalin manual ke proyek:

1. Buat folder `Assets/ThirdParty`.
2. Ekstrak paket hewan. Salin folder `Realistic Animated Pack` (berisi `Realistic Animated Pack.fbx` dan folder `textures`) ke `Assets/ThirdParty/`. Isinya antara lain singa betina (Lioness) dan kelinci.
3. Ekstrak `elephant-gajah.zip` ke `Assets/ThirdParty/elephant-gajah/`, sehingga ada `source/ELEPHANT_M.3ds` dan `textures/ELPHSKIN.jpg`, `textures/ELETUSK.jpg`.
4. Tunggu Unity selesai mengimpor, lalu klik menu **AR Animal World > Siapkan Model Hewan 3D**.

Menu ini mencari singa (juga "lioness"), gajah, dan kelinci di semua model dalam `Assets`, lalu membuat `Assets/Resources/Models/LionModel.prefab`, `ElephantModel.prefab`, dan `RabbitModel.prefab`. Tiap hewan otomatis diputar supaya kepalanya menghadap anak yang memegang HP, diberi material yang cocok dengan URP, dan ukurannya disesuaikan dengan lebar kartu saat muncul. Foto cek tiap hewan disimpan di folder `ModelPreviews` (di samping folder `Assets`). Animasi bawaan paket tidak dipakai; hewan bergerak naik-turun pelan sebagai gantinya.

Hewan yang modelnya tidak ditemukan tetap memakai model sederhana. Model lain juga bisa dipakai: taruh di `Assets/ThirdParty`, pastikan nama file atau nama objeknya mengandung `lion`/`singa`, `elephant`/`gajah`, atau `rabbit`/`kelinci`, lalu jalankan menu ini lagi.

Suara asli dipasang dengan menaruh file di `Assets/Resources/Audio`:

| Untuk | Nama file |
| --- | --- |
| Suara singa | `Assets/Resources/Audio/lion_roar` (wav/mp3/ogg) |
| Suara gajah | `Assets/Resources/Audio/elephant_trumpet` |
| Suara kelinci | `Assets/Resources/Audio/rabbit_squeak` |
| Narasi (opsional) | `Assets/Resources/Audio/narration_lion`, `narration_elephant`, `narration_rabbit` |

Suara hewan bisa dari freesound.org (cek lisensinya).

## 10. AI pengenal gambar hewan

Selain kartu, layar Scan juga mengenali **gambar singa, gajah, atau kelinci apa saja**: foto di buku, poster, layar HP lain, atau mainan. Begitu dikenali, hewan 3D-nya muncul di depan kamera (tidak menempel di gambar seperti pada kartu), dengan suara dan panel "Hewan ketemu!".

- AI-nya adalah MobileNetV2 yang sudah dilatih dengan jutaan foto (ImageNet), jadi tidak perlu dilatih lagi. Filenya sudah ada di `Assets/AI/mobilenetv2-12.onnx`.
- Paling bagus untuk foto dan gambar yang realistis. Gambar kartun sederhana sering tidak dikenali; untuk itu tetap pakai kartu.
- Kalau kartu dan gambar terlihat bersamaan, kartu yang dipakai.
- Pengaturannya ada di komponen **AnimalPictureRecognizer** pada objek `AR Animal World`: seberapa yakin AI harus (**Threshold**), jarak dan ukuran hewan di depan kamera.
- Di Editor dan Development Build, di bawah tombol Tes ada kotak kecil yang menampilkan apa yang dilihat AI.

## 11. Kalau ada masalah

| Masalah | Solusi |
| --- | --- |
| Teks berupa kotak atau tidak muncul | Jalankan Import TMP Essential Resources (langkah 2.4) |
| Ada error merah `Vuforia` saat membuka proyek | Vuforia belum terpasang. Ikuti langkah 3 |
| Layar Scan hitam atau muncul pesan "Kamera AR belum siap" | Cek license key di Vuforia Configuration |
| Kartu tidak terdeteksi | Pakai cahaya terang, kartu tidak mengkilap, dan gambar kartu yang lebih detail |
| Menu Vuforia Configuration tidak ada | Vuforia belum diimpor. Ikuti langkah 3 |
| Kartu terdeteksi tapi hewan 3D tidak muncul | Pastikan layar Scan yang terbuka (hewan hanya muncul di layar Scan). Cek **Width** Image Target `0.08`, lalu jalankan lagi **Bangun Game di Scene Ini** |
| Hewan di atas kartu terlalu besar atau kecil | Ubah **Model Scale** di komponen `AnimalImageTargetHandler` pada Image Target. 1 berarti sepanjang lebar kartu |
| Hewan menghadap ke arah yang salah | Ubah **Model Local Euler** Y di komponen `AnimalImageTargetHandler` (misalnya 210 untuk membalik) |
| Hewan tampak gelap | Scene belum punya cahaya. Jalankan lagi **Bangun Game di Scene Ini**, menu ini menambahkan cahaya matahari |
| Hewan berwarna pink atau tanpa tekstur | Jalankan lagi **Siapkan Model Hewan 3D**. Pastikan folder `textures` ikut tersalin di samping model |
| Layar Scan menampilkan "Kameranya belum menyala" | Tekan **Izinkan Kamera**. Kalau izin pernah ditolak, buka Pengaturan HP > Aplikasi > AR Animal World > Izin > Kamera |
| Build gagal dengan error Gradle soal `android.useAndroidX` | Hapus paket **In-App Purchasing** (`com.unity.purchasing`) lewat Window > Package Manager. Paket bawaan template Unity 6 ini bentrok dengan Vuforia |
| Build ditolak karena Minimum API Level | Vuforia 11 butuh minimal Android 10 (API 29). Atur di Player Settings |
| Kamera AR tidak menyala walau license key benar | Setujui Vuforia Developer Agreement lewat **Help > Vuforia Engine > Show Developer Agreement** |
| Error `Unity.InferenceEngine` tidak ditemukan | Pasang paket **Inference Engine** (langkah 2.5) |
| Gambar hewan tidak dikenali AI | Pakai foto yang jelas dan terang, isi sebagian besar layar dengan hewannya. Gambar kartun sering tidak dikenali. Bisa juga turunkan **Threshold** di AnimalPictureRecognizer |
| Ingin menghapus bintang saat testing | Menu **AR Animal World > Hapus Bintang Tersimpan** |

## Struktur script

| File | Fungsi |
| --- | --- |
| `Game/GameFlow.cs` | Titik awal game, pindah antar layar |
| `UI/ScreenRouter.cs`, `UI/UIScreen.cs` | Menampilkan satu layar sekaligus dan riwayat tombol kembali |
| `UI/Screens/*.cs` | Logika keenam layar |
| `QuizManager.cs`, `Game/QuizSession.cs` | Kuis satu soal per hewan. Bintang hanya untuk jawaban benar di percobaan pertama, tapi anak boleh mencoba lagi sampai benar |
| `ARAnimalWorldManager.cs` | Hewan terpilih, kartu yang terdeteksi, total bintang (tersimpan) |
| `AR/AnimalImageTargetHandler.cs` | Memunculkan hewan di atas kartu dan memutar suaranya |
| `AR/ARCameraToggle.cs` | Menyalakan kamera AR, meminta izin kamera, dan memberi tahu layar Scan kalau kamera gagal |
| `Game/GameAudio.cs`, `Game/PlaceholderSounds.cs` | Suara hewan, narasi, efek benar/salah |
| `Game/PlaceholderAnimalFactory.cs` | Hewan 3D sementara dari bentuk dasar |
| `Game/ModelFitter.cs`, `Game/AnimalModelSpawner.cs` | Memunculkan model hewan dan menyesuaikan ukurannya |
| `AI/AnimalPictureRecognizer.cs` | AI yang mengenali gambar hewan dari kamera dan memunculkan hewan 3D |
| `Game/AnimalArt.cs` | Gambar hewan dan bintang untuk layar-layar |
| `Editor/ARAnimalWorldSceneBuilder.cs`, `Editor/UIKit.cs` | Menu yang membuat seluruh UI sesuai desain dan menghubungkan kartu Vuforia |
| `Editor/AnimalModelImporter.cs` | Menu yang mengambil singa, gajah, kelinci dari paket model dan menjadikannya prefab |
| `tools/make_ui_art.py` | Menggambar ilustrasi hewan, bintang, dan ikon di `Assets/Art/UI` |
