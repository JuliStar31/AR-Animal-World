# UI Concept and Design System

## App name
AR Animal World

## Tagline
Yuk, kenali dunia hewan!

## Screen flow

1. Welcome / Main Menu
2. Animal Learning Menu
3. AR Camera / Scan Screen
4. Animal Detail / Learning Screen
5. Quiz Screen
6. Quiz Results Screen

## Visual style

### Color palette
- Sky Blue: #DDF4FF
- Blue Ink: #3A7DCA
- Fresh Green: #74C96E
- Grass Dark: #2D9C62
- Sun Yellow: #FFD95B
- Cream: #FFF9F1
- Peach: #FFB36C
- Soft Orange: #FF8C5A
- White: #FFFFFF
- Text Dark: #4A4A4A
- Soft Gray: #E7EEF5

### Typography
Use rounded modern fonts such as:
- Nunito
- Fredoka
- Baloo 2

Suggested scale:
- Hero title: 32–36 px
- Screen title: 26–30 px
- Subtitle: 18–20 px
- Body: 16–18 px
- Button: 20–22 px
- Small label: 12–14 px

### Buttons
- Primary: rounded pill, bright green or blue, white text
- Secondary: cream or white background, colored border
- Sound button: yellow or orange circular icon button
- Touch target: minimum 56 px

### Cards
- Rounded corners: 24–28 px
- Soft shadow
- White or cream surface
- Large illustration and short clear label

## Screen attributes

### 1. Welcome / Main Menu
- title: AR Animal World
- subtitle: Yuk, kenali dunia hewan!
- central hero lion character
- main button: Mulai Belajar
- secondary button: Main Kuis
- star badge in top right or lower corner

### 2. Animal Learning Menu
- title: Pilih Hewan
- cards for Singa, Gajah, Kelinci
- simple large illustration, name, and AR scan button

### 3. AR Camera / Scan Screen
- real camera preview background
- top guide text: Arahkan kamera ke kartu hewan!
- scanning frame with pulse effect
- small animal card hint preview
- bottom info panel with name and fact
- large sound button
- back button
- camera permission/error state in a gentle message

### 4. Animal Detail / Learning Screen
- large animal artwork
- fact: e.g. Singa punya suara auman yang keras!
- button: Dengarkan Suara
- button: Coba Hewan Lain

### 5. Quiz Screen
- title: Tebak Hewan!
- large clue image and short question
- three answer cards
- feedback: Hebat sekali! or Ayo coba lagi!
- star score and progress indicator

### 6. Quiz Results Screen
- title: Kamu Hebat!
- stars earned
- happy mascot or celebration artwork
- primary button: Main Lagi
- secondary button: Kembali ke Beranda

## Implementation notes for Unity

- Use Canvas with Screen Space - Overlay
- Use TextMeshPro for text
- Use UI buttons with rounded images
- Use simple 3D animal placeholders or stylized art assets
- Use Vuforia image targets for printed cards
- Keep camera overlays minimal so real-world AR stays visible
