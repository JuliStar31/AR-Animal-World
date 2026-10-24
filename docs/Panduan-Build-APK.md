# Panduan Membuat APK AR Animal World

Panduan ini membawa kamu dari repo ini sampai file APK yang bisa dipasang di HP Android. Semua kode game sudah ada di `Assets/Scripts`. Yang perlu kamu lakukan di komputer adalah memasang Unity dan Vuforia, membuat database kartu, lalu menekan Build.

Perkiraan waktu: 1 sampai 2 jam untuk pertama kali (sebagian besar menunggu download).

## Isi game

| Layar | Isi |
| --- | --- |
| 1. Beranda | Judul, "Yuk, kenali dunia hewan!", tombol Mulai Belajar, Main Kuis, Scan Kartu, jumlah bintang |
| 2. Pilih Hewan | Kartu Singa, Gajah, Kelinci dengan tombol Belajar dan Scan AR |
| 3. Scan AR | Kamera, bingkai scan berdenyut, panel info saat kartu ditemukan, tombol suara |
| 4. Detail Hewan | Hewan 3D berputar, fakta, narasi, Dengarkan Suara, Coba Hewan Lain |
| 5. Kuis | "Tebak Hewan!", 3 pilihan jawaban bergambar, "Hebat sekali!" atau "Ayo coba lagi!" |
| 6. Hasil | "Kamu Hebat!", bintang yang didapat, Main Lagi, Kembali ke Beranda |

Bintang tersimpan di HP. Tombol kembali Android juga berfungsi.

Belum ada model 3D atau suara asli? Tidak masalah. Game otomatis membuat hewan 3D sederhana dari bentuk dasar dan suara sintetis, jadi semuanya bisa langsung dicoba. Nanti tinggal diganti (lihat bagian 9).

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

## 3. Pasang Vuforia dan license key

1. Buat akun gratis di https://developer.vuforia.com.
2. Buka **Plan & Licenses > Generate Basic License**, beri nama "AR Animal World", lalu salin license key-nya.
3. Download **Vuforia Engine for Unity** dari halaman Downloads, lalu klik dua kali file `.unitypackage` (atau seret ke Unity) dan klik **Import**. Unity akan meminta restart, ikuti saja.
4. Buka **Window > Vuforia Configuration** dan tempel license key ke kolom **App License Key**.

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

Menu ini membuat semua 6 layar, manager game, panggung preview 3D, ikon hewan, lalu memasang `AnimalImageTargetHandler` ke tiap Image Target sesuai namanya (singa, gajah, kelinci) dan `ARCameraToggle` ke AR Camera. Menu ini aman dijalankan ulang kapan saja.

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
   - **Minimum API Level**: Android 8.0 (API 26) atau lebih tinggi
   - **Scripting Backend**: IL2CPP
   - **Target Architectures**: centang **ARM64**
   - **Resolution and Presentation > Default Orientation**: Portrait
4. Kalau proyek ini sudah memakai scaffold Unity dari repo, sebagian besar pengaturan ini sudah terisi. Cukup dicek.

## 8. Build APK dan pasang di HP

1. Di **Build Settings**, klik **Build**, pilih folder, beri nama `ARAnimalWorld.apk`.
2. Kirim file APK ke HP (kabel USB, Google Drive, dan lain-lain), buka filenya, izinkan **Install dari sumber tidak dikenal**, lalu pasang.
3. Atau sambungkan HP dengan kabel, aktifkan **Opsi Developer > USB Debugging**, lalu klik **Build And Run**.
4. Saat pertama dibuka, izinkan akses kamera.

## 9. Mengganti placeholder dengan aset asli

Buat folder `Assets/Resources/Models` dan `Assets/Resources/Audio` jika belum ada.

| Untuk | Nama file |
| --- | --- |
| Model singa | `Assets/Resources/Models/LionModel.prefab` |
| Model gajah | `Assets/Resources/Models/ElephantModel.prefab` |
| Model kelinci | `Assets/Resources/Models/RabbitModel.prefab` |
| Suara singa | `Assets/Resources/Audio/lion_roar` (wav/mp3/ogg) |
| Suara gajah | `Assets/Resources/Audio/elephant_trumpet` |
| Suara kelinci | `Assets/Resources/Audio/rabbit_squeak` |
| Narasi (opsional) | `Assets/Resources/Audio/narration_lion`, `narration_elephant`, `narration_rabbit` |

Tips model: buat prefab dengan tinggi sekitar 1 unit, kaki di y = 0, menghadap ke arah -Z. Setelah menambah model, jalankan lagi menu **AR Animal World > Bangun Game di Scene Ini** supaya ikon di menu ikut diperbarui.

Model 3D gratis yang ramah anak bisa dicari di Unity Asset Store, Sketchfab (lisensi CC), atau Poly Pizza. Suara hewan bisa dari freesound.org (cek lisensinya).

## 10. Kalau ada masalah

| Masalah | Solusi |
| --- | --- |
| Teks berupa kotak atau tidak muncul | Jalankan Import TMP Essential Resources (langkah 2.4) |
| Ada error merah `Vuforia` saat membuka proyek | Vuforia belum terpasang. Ikuti langkah 3 |
| Layar Scan hitam atau muncul pesan "Kamera AR belum siap" | Cek license key di Vuforia Configuration |
| Kartu tidak terdeteksi | Pakai cahaya terang, kartu tidak mengkilap, dan gambar kartu yang lebih detail |
| Menu Vuforia Configuration tidak ada | Vuforia belum diimpor. Ikuti langkah 3 |
| Hewan di atas kartu terlalu besar atau kecil | Ubah **Model Scale** di komponen `AnimalImageTargetHandler` pada Image Target |
| Hewan berwarna pink | Proyek memakai URP tapi materialnya tidak cocok. Pakai Built-in Render Pipeline, atau ganti shader model ke URP/Lit |
| Kamera tidak menyala di layar Scan | Matikan centang **Camera Only On Scan Screen** di komponen `ARCameraToggle` pada AR Camera |
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
| `AR/ARCameraToggle.cs` | Kamera AR hanya menyala di layar Scan, dan pesan ramah kalau kamera gagal |
| `Game/GameAudio.cs`, `Game/PlaceholderSounds.cs` | Suara hewan, narasi, efek benar/salah |
| `Game/PlaceholderAnimalFactory.cs` | Hewan 3D sementara dari bentuk dasar |
| `Editor/ARAnimalWorldSceneBuilder.cs` | Menu yang membuat seluruh UI dan menghubungkan kartu Vuforia |
