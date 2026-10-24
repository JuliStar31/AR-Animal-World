using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Vuforia;
using static ARAnimalWorld.EditorTools.UIKit;
using Image = UnityEngine.UI.Image;

namespace ARAnimalWorld.EditorTools
{
    // Menu: AR Animal World > Bangun Game di Scene Ini
    // Builds the whole game into the open scene, following the UI mockups:
    // managers, the screens, and wires any Vuforia Image Targets already in
    // the scene to the right animal. Safe to run again; it replaces what it
    // built before.
    public static class ARAnimalWorldSceneBuilder
    {
        private const string ManagersName = "AR Animal World";
        private const string CanvasName = "AR Animal World UI";
        private const string OldPreviewName = "AR Animal World Preview";

        private static Sprite chevronLeft, chevronRight, starOn, starOff, starOutline, play, camera,
            cameraOff, speaker, check, arrowUp, cloud, corner, happyLion;
        private static Sprite[] animals;

        [MenuItem("AR Animal World/Bangun Game di Scene Ini", priority = 0)]
        public static void BuildGame()
        {
            if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            {
                EditorUtility.DisplayDialog("TextMeshPro belum siap",
                    "Buka Window > TextMeshPro > Import TMP Essential Resources, klik Import, lalu jalankan menu ini lagi.",
                    "OK");
                return;
            }

            LoadArt();

            DestroyExisting(ManagersName);
            DestroyExisting(CanvasName);
            DestroyExisting(OldPreviewName);

            var flow = BuildManagers();
            EnsureEventSystem();
            EnsureCamera();
            EnsureLight();

            var canvas = BuildCanvas();
            var screens = new List<UIScreen>
            {
                BuildMainMenu(canvas),
                BuildAnimalMenu(canvas),
                BuildScan(canvas),
                BuildDetail(canvas),
                BuildQuiz(canvas),
                BuildResults(canvas)
            };

            var router = canvas.gameObject.AddComponent<ScreenRouter>();
            router.screens = screens.ToArray();
            flow.router = router;

            int wired = WireVuforia(flow.appManager);
            int models = AnimalModelImporter.CountReadyModels();

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            Selection.activeGameObject = flow.gameObject;

            string vuforiaNote = wired > 0
                ? $"{wired} Image Target terhubung ke hewan."
                : "Belum ada Vuforia Image Target di scene. Tambahkan AR Camera dan Image Target (lihat docs/Panduan-Build-APK.md), lalu jalankan menu ini lagi.";
            string modelNote = models == AnimalCatalog.Animals.Count
                ? "Semua hewan memakai model 3D asli."
                : $"{models} dari {AnimalCatalog.Animals.Count} hewan memakai model 3D asli; sisanya model sederhana. Lihat menu AR Animal World > Siapkan Model Hewan 3D.";
            EditorUtility.DisplayDialog("AR Animal World siap",
                "Semua layar sudah dibuat. Tekan Play untuk mencoba, lalu simpan scene (Ctrl+S).\n\n" + vuforiaNote + "\n\n" + modelNote,
                "Mantap");
        }

        [MenuItem("AR Animal World/Hapus Bintang Tersimpan", priority = 20)]
        public static void ResetStars()
        {
            PlayerPrefs.DeleteKey("araw_stars_total");
            PlayerPrefs.Save();
            Debug.Log("Bintang tersimpan sudah dihapus.");
        }

        private static void LoadArt()
        {
            GeneratedArt.CreateShapes();
            BoldFont = FontAssets.Get("Fredoka-Bold");
            MediumFont = FontAssets.Get("Fredoka-Medium");

            chevronLeft = GeneratedArt.Ui("chevron_left");
            chevronRight = GeneratedArt.Ui("chevron_right");
            starOn = GeneratedArt.Ui("star_on");
            starOff = GeneratedArt.Ui("star_off");
            starOutline = GeneratedArt.Ui("star_outline");
            play = GeneratedArt.Ui("play");
            camera = GeneratedArt.Ui("camera");
            cameraOff = GeneratedArt.Ui("camera_off");
            speaker = GeneratedArt.Ui("speaker");
            check = GeneratedArt.Ui("check");
            arrowUp = GeneratedArt.Ui("arrow_up");
            cloud = GeneratedArt.Ui("cloud");
            corner = GeneratedArt.Ui("scan_corner");
            happyLion = GeneratedArt.Ui("lion_happy");

            // Same order as AnimalCatalog.Animals.
            animals = new Sprite[AnimalCatalog.Animals.Count];
            for (int i = 0; i < animals.Length; i++)
            {
                animals[i] = GeneratedArt.Ui(ArtName(AnimalCatalog.Animals[i].id));
            }
        }

        private static string ArtName(AnimalId id)
        {
            switch (id)
            {
                case AnimalId.Lion: return "lion";
                case AnimalId.Elephant: return "elephant";
                default: return "rabbit";
            }
        }

        private static Sprite Art(AnimalId id)
        {
            int index = AnimalCatalog.Animals.FindIndex(a => a.id == id);
            return index >= 0 ? animals[index] : null;
        }

        private static void DestroyExisting(string name)
        {
            foreach (var root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
            {
                if (root.name == name)
                {
                    Object.DestroyImmediate(root);
                }
            }
        }

        private static GameFlow BuildManagers()
        {
            var go = new GameObject(ManagersName);
            var app = go.AddComponent<ARAnimalWorldManager>();
            var quiz = go.AddComponent<QuizManager>();
            quiz.appManager = app;
            go.AddComponent<AudioSource>();
            var audio = go.AddComponent<GameAudio>();
            var art = go.AddComponent<AnimalArt>();
            art.illustrations = animals;
            art.happyMascot = happyLion;
            art.starOn = starOn;
            art.starOff = starOff;
            var flow = go.AddComponent<GameFlow>();
            flow.appManager = app;
            flow.quizManager = quiz;
            flow.gameAudio = audio;
            flow.art = art;
            return flow;
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            }
        }

        private static void EnsureCamera()
        {
            // Vuforia's ARCamera counts; this only helps a scene with no camera.
            if (Object.FindObjectOfType<Camera>() == null)
            {
                var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                camGo.tag = "MainCamera";
                var cam = camGo.GetComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = Hex("#232C35");
            }

            if (Object.FindObjectOfType<AudioListener>() == null)
            {
                Object.FindObjectOfType<Camera>().gameObject.AddComponent<AudioListener>();
            }
        }

        // Real animal models look flat and dark without a sun.
        private static void EnsureLight()
        {
            if (Object.FindObjectsOfType<Light>().Any(l => l.type == LightType.Directional))
            {
                return;
            }

            var light = new GameObject("Cahaya Matahari").AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            light.shadows = LightShadows.None;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static Transform BuildCanvas()
        {
            var go = new GameObject(CanvasName, typeof(RectTransform));
            go.layer = 5;
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(DesignWidth, DesignHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            go.AddComponent<GraphicRaycaster>();
            return go.transform;
        }

        // Full-screen panel. Backgrounds (sky, hills) go on `screen`, which
        // covers the whole display; content goes on `content`, which stays
        // inside the safe area.
        private static T NewScreen<T>(Transform canvas, string name, Color? background, out RectTransform screen, out Transform content) where T : UIScreen
        {
            screen = Stretch(Rect(name, canvas));
            screen.gameObject.AddComponent<CanvasGroup>();
            if (background.HasValue)
            {
                var bg = screen.gameObject.AddComponent<Image>();
                bg.color = background.Value;
            }

            var safe = Stretch(Rect("SafeArea", screen));
            safe.gameObject.AddComponent<SafeAreaFitter>();
            content = safe;
            return screen.gameObject.AddComponent<T>();
        }

        private static TextMeshProUGUI Title(Transform parent, string text, float x, float y, float w, float size = 52f,
            TextAlignmentOptions align = TextAlignmentOptions.Left)
        {
            var title = Text("Title", parent, text, size, Ink, true, align);
            Put(title.rectTransform, Edge.Top, x, y, w, 80f);
            return title;
        }

        private static Image HaloIllustration(Transform parent, Edge edge, float x, float y, float size, Sprite sprite, float haloInset = 10f)
        {
            Put(Dot("Halo", parent, Halo).rectTransform, edge, x + haloInset, y + haloInset, size - haloInset * 2f, size - haloInset * 2f);
            var image = Picture("Illustration", parent, sprite);
            Put(image.rectTransform, edge, x, y, size, size);
            return image;
        }

        // A cream card with a soft blue lip and a text block.
        private static RectTransform InfoCard(Transform parent, Edge edge, float x, float y, float w, float h, Color shadow, out Image face)
        {
            face = Raised("Card", parent, Cream, shadow, 10f, 44f, out var container);
            Put(container, edge, x, y, w, h);
            return (RectTransform)face.transform;
        }

        // 1. Beranda
        private static UIScreen BuildMainMenu(Transform canvas)
        {
            var screen = NewScreen<MainMenuScreen>(canvas, "1 Beranda", Sky, out var root, out var c);

            Cloud(root, cloud, Edge.Top, -40f, 300f, 290f, 120f);
            Cloud(root, cloud, Edge.Middle, 600f, 560f, 230f, 140f);
            Hills(root, true);

            screen.starsLabel = StarBadge(c, starOn, 550f, 48f);

            var arFace = Raised("ARBadge", c, Yellow, YellowDark, 8f, 30f, out var arBadge);
            Put(arBadge, Edge.Top, 68f, 180f, 154f, 100f);
            Stretch(Text("Label", arFace.transform, "AR", 56f, Brown).rectTransform);
            Put(Text("Title", c, "Animal World", 64f, Ink, true, TextAlignmentOptions.Left).rectTransform, Edge.Top, 245f, 182f, 500f, 92f);
            Put(Text("Subtitle", c, "Yuk, kenali dunia hewan!", 34f, Ink, false).rectTransform, Edge.Top, 90f, 300f, 600f, 52f);

            Put(Dot("DotOrange", c, Orange).rectTransform, Edge.Middle, 668f, 460f, 32f, 32f);
            Put(Dot("DotYellow", c, Yellow).rectTransform, Edge.Middle, 58f, 800f, 42f, 42f);
            var hero = HaloIllustration(c, Edge.Middle, 95f, 520f, 590f, Art(AnimalId.Lion));
            hero.gameObject.AddComponent<PulseEffect>().amount = 0.02f;

            screen.startLearningButton = PillButton("StartLearningButton", c, Edge.Bottom, 48f, 1288f, 684f, 156f,
                "Mulai Belajar", Green, GreenDark, White, play);
            screen.quizButton = PillButton("QuizButton", c, Edge.Bottom, 48f, 1468f, 684f, 156f,
                "Main Kuis", Yellow, YellowDark, Brown, starOutline);
            return screen;
        }

        // 2. Pilih Hewan
        private static UIScreen BuildAnimalMenu(Transform canvas)
        {
            var screen = NewScreen<AnimalMenuScreen>(canvas, "2 PilihHewan", Sky, out var root, out var c);
            Cloud(root, cloud, Edge.Top, 600f, 100f, 230f, 130f);

            screen.backButton = BackButton(c, chevronLeft, CreamShadow);
            Title(c, "Pilih Hewan", 215f, 66f, 420f);

            var borders = new[] { Hex("#F5BE48"), Hex("#8AC0EA"), Hex("#F49AB8") };
            var lips = new[] { YellowDark, Hex("#5E9ACB"), Hex("#E07FA0") };
            var circles = new[] { Hex("#FFE08A"), Hex("#CFE6F8"), Hex("#FFD6E2") };

            var cards = new List<AnimalCardView>();
            for (int i = 0; i < AnimalCatalog.Animals.Count; i++)
            {
                var animal = AnimalCatalog.Animals[i];
                int style = i % borders.Length;
                float y = 208f + i * 369f;

                var border = Raised(animal.displayName + "Card", c, borders[style], lips[style], 14f, 44f, out var container);
                Put(container, Edge.Top, 48f, y, 684f, 345f);
                var cardButton = MakeButton(container.gameObject, border);
                var face = Round("Inner", border.transform, Cream, 40f);
                Stretch(face.rectTransform, 6f, 6f, 6f, 6f);

                var faceRect = face.rectTransform;
                Local(Dot("IconCircle", faceRect, circles[style]).rectTransform, 34f, 34f, 256f, 256f);
                var icon = Picture("Icon", faceRect, Art(animal.id));
                Local(icon.rectTransform, 44f, 44f, 236f, 236f);

                var name = Text("Name", faceRect, animal.displayName, 60f, Brown, true, TextAlignmentOptions.Left);
                Local(name.rectTransform, 322f, 100f, 230f, 120f);

                var open = Raised("OpenButton", faceRect, Green, GreenDark, 6f, 48f, out var openContainer);
                open.sprite = GeneratedArt.Circle;
                open.type = Image.Type.Simple;
                var openLip = openContainer.GetChild(0).GetComponent<Image>();
                openLip.sprite = GeneratedArt.Circle;
                openLip.type = Image.Type.Simple;
                Local(openContainer, 552f, 112f, 96f, 102f);
                Stretch(Picture("Chevron", open.transform, chevronRight).rectTransform, 30f, 30f, 26f, 26f);
                var openButton = MakeButton(openContainer.gameObject, open);

                cards.Add(new AnimalCardView
                {
                    animalId = animal.id,
                    cardButton = cardButton,
                    openButton = openButton,
                    nameLabel = name,
                    icon = icon
                });
            }
            screen.cards = cards.ToArray();

            screen.scanButton = PillButton("ScanButton", c, Edge.Bottom, 48f, 1468f, 684f, 156f,
                "Pindai Kartu AR", Orange, OrangeDark, Brown, camera, 42f);
            return screen;
        }

        // 3. Scan AR (3a searching, 3b found, 3c camera off)
        private static UIScreen BuildScan(Transform canvas)
        {
            // No background: the AR camera shows through.
            var screen = NewScreen<ScanScreen>(canvas, "3 ScanAR", null, out var root, out var c);
            var darkLip = Hex("#161C22");

            // 3a: guide bubble, scan frame, hint card.
            var guide = Raised("GuidePanel", c, Cream, darkLip, 8f, 50f, out var guideContainer);
            Put(guideContainer, Edge.Top, 184f, 48f, 548f, 145f);
            Stretch(Text("Text", guide.transform, "Arahkan kamera ke kartu hewan!", 38f, Brown).rectTransform, 30f, 30f, 8f, 8f);
            screen.guidePanel = guideContainer.gameObject;

            var frame = Rect("ScanFrame", c);
            Put(frame, Edge.Middle, 130f, 400f, 520f, 680f);
            float[] rotations = { 0f, -90f, 90f, 180f };
            Vector2[] anchors = { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) };
            for (int i = 0; i < 4; i++)
            {
                var piece = Picture("Corner" + i, frame, corner, Yellow);
                var r = piece.rectTransform;
                r.anchorMin = r.anchorMax = anchors[i];
                r.pivot = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(112f, 112f);
                r.anchoredPosition = new Vector2(anchors[i].x == 0f ? 56f : -56f, anchors[i].y == 0f ? 56f : -56f);
                r.localRotation = Quaternion.Euler(0f, 0f, rotations[i]);
            }
            var line = Round("ScanLine", frame, Yellow, 7f);
            line.rectTransform.anchorMin = line.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            line.rectTransform.sizeDelta = new Vector2(450f, 14f);
            line.gameObject.AddComponent<ScanLineEffect>().travel = 230f;
            screen.scanFrame = frame.gameObject;

            var hint = Rect("CardHint", c);
            Put(hint, Edge.Bottom, 260f, 1185f, 260f, 400f);
            var arrow = Picture("Arrow", hint, arrowUp, Yellow);
            Local(arrow.rectTransform, 100f, 5f, 60f, 60f);
            var hintCard = Round("Card", hint, Cream, 28f);
            Local(hintCard.rectTransform, 20f, 100f, 220f, 280f);
            hintCard.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -5f);
            var hintInner = Round("Inner", hintCard.transform, Hex("#FFE08A"), 22f);
            Stretch(hintInner.rectTransform, 22f, 22f, 22f, 22f);
            Stretch(Picture("Lion", hintInner.transform, Art(AnimalId.Lion)).rectTransform, 12f, 12f, 40f, 40f);
            hint.gameObject.AddComponent<PulseEffect>().amount = 0.03f;
            screen.cardHint = hint.gameObject;

            // 3b: found badge and info panel.
            var found = Raised("FoundBadge", c, Green, GreenDark, 8f, 46f, out var foundContainer);
            Put(foundContainer, Edge.Top, 180f, 80f, 420f, 104f);
            var foundRow = found.transform;
            Local(Picture("Check", foundRow, check).rectTransform, 44f, 28f, 40f, 40f);
            Local(Text("Text", foundRow, "Kartu ketemu!", 40f, White).rectTransform, 90f, 0f, 310f, 96f);
            foundContainer.gameObject.AddComponent<PopInEffect>();
            screen.foundBadge = foundContainer.gameObject;

            var info = Raised("InfoPanel", c, Cream, darkLip, 10f, 44f, out var infoContainer);
            Put(infoContainer, Edge.Bottom, 32f, 1365f, 716f, 285f);
            infoContainer.gameObject.AddComponent<PopInEffect>();
            screen.infoButton = MakeButton(infoContainer.gameObject, info);
            screen.nameText = Text("Name", info.transform, "Singa", 60f, Brown, true, TextAlignmentOptions.Left);
            Local(screen.nameText.rectTransform, 40f, 30f, 460f, 80f);
            screen.factText = Text("Fact", info.transform, "", 34f, Brown, false, TextAlignmentOptions.TopLeft);
            Local(screen.factText.rectTransform, 40f, 120f, 450f, 140f);
            var sound = Raised("SoundButton", info.transform, Orange, OrangeDark, 10f, 80f, out var soundContainer);
            sound.sprite = GeneratedArt.Circle;
            sound.type = Image.Type.Simple;
            var soundLip = soundContainer.GetChild(0).GetComponent<Image>();
            soundLip.sprite = GeneratedArt.Circle;
            soundLip.type = Image.Type.Simple;
            Local(soundContainer, 500f, 45f, 176f, 186f);
            Stretch(Picture("Speaker", sound.transform, speaker, Brown).rectTransform, 48f, 48f, 48f, 48f);
            screen.soundButton = MakeButton(soundContainer.gameObject, sound);

            screen.backButton = BackButton(c, chevronLeft, darkLip, Edge.Top, 48f, 64f);

            // Test buttons (Editor and development builds only).
            var simulate = Round("SimulatePanel", c, new Color(1f, 1f, 1f, 0.85f), 30f);
            Put(simulate.rectTransform, Edge.Top, 48f, 210f, 684f, 90f);
            Local(Text("Label", simulate.transform, "Tes:", 30f, Brown).rectTransform, 10f, 0f, 90f, 90f);
            var simButtons = new List<Button>();
            for (int i = 0; i < AnimalCatalog.Animals.Count; i++)
            {
                var face = Round("Simulate" + AnimalCatalog.Animals[i].id, simulate.transform, Orange, 30f);
                Local(face.rectTransform, 105f + i * 190f, 12f, 175f, 66f);
                Stretch(Text("Label", face.transform, AnimalCatalog.Animals[i].displayName, 30f, Brown).rectTransform);
                simButtons.Add(MakeButton(face.gameObject, face));
            }
            screen.simulatePanel = simulate.gameObject;
            screen.simulateButtons = simButtons.ToArray();

            // 3c: camera off, covers everything else.
            var panel = Rect("CameraPanel", root);
            Stretch(panel);
            panel.gameObject.AddComponent<Image>().color = Sky;
            var pc = Stretch(Rect("SafeArea", panel));
            pc.gameObject.AddComponent<SafeAreaFitter>();
            Put(Dot("Circle", pc, Cream).rectTransform, Edge.Middle, 190f, 365f, 400f, 400f);
            Put(Picture("CameraOff", pc, cameraOff).rectTransform, Edge.Middle, 290f, 465f, 200f, 200f);
            screen.cameraTitle = Text("Title", pc, "Kameranya belum menyala", 58f, Ink);
            Put(screen.cameraTitle.rectTransform, Edge.Middle, 60f, 805f, 660f, 160f);
            screen.cameraMessage = Text("Message", pc, "Ayo izinkan kamera supaya hewan bisa muncul!", 34f, Ink, false);
            Put(screen.cameraMessage.rectTransform, Edge.Middle, 90f, 1000f, 600f, 120f);
            screen.allowCameraButton = PillButton("AllowCameraButton", pc, Edge.Bottom, 48f, 1320f, 684f, 156f,
                "Izinkan Kamera", Green, GreenDark, White);
            screen.cameraBackButton = PillButton("CameraBackButton", pc, Edge.Bottom, 48f, 1497f, 684f, 136f,
                "Kembali", Cream, CreamShadow, Brown, null, 38f, 10f);
            BackButton(pc, chevronLeft, CreamShadow).onClick.AddListener(() => { });
            screen.cameraPanel = panel.gameObject;
            panel.gameObject.SetActive(false);

            guideContainer.SetAsLastSibling();
            return screen;
        }

        // 4. Detail Hewan
        private static UIScreen BuildDetail(Transform canvas)
        {
            var screen = NewScreen<AnimalDetailScreen>(canvas, "4 DetailHewan", Sky, out var root, out var c);
            Hills(root, false);

            screen.backButton = BackButton(c, chevronLeft, CreamShadow);
            screen.illustration = HaloIllustration(c, Edge.Top, 125f, 180f, 530f, Art(AnimalId.Lion));
            screen.illustration.gameObject.AddComponent<PopInEffect>();

            var card = InfoCard(c, Edge.Top, 48f, 740f, 684f, 315f, CreamShadow, out _);
            screen.nameText = Text("Name", card, "Singa", 66f, Brown);
            Local(screen.nameText.rectTransform, 40f, 36f, 604f, 90f);
            screen.factText = Text("Fact", card, "", 36f, Brown, false);
            Local(screen.factText.rectTransform, 50f, 140f, 584f, 140f);

            screen.listenButton = PillButton("ListenButton", c, Edge.Bottom, 48f, 1308f, 684f, 156f,
                "Dengarkan Suara", Orange, OrangeDark, Brown, speaker);
            screen.otherAnimalButton = PillButton("OtherAnimalButton", c, Edge.Bottom, 48f, 1485f, 684f, 136f,
                "Coba Hewan Lain", Green, GreenDark, White, null, 40f, 10f);
            return screen;
        }

        // 5. Tebak Hewan
        private static UIScreen BuildQuiz(Transform canvas)
        {
            var screen = NewScreen<QuizScreen>(canvas, "5 TebakHewan", Sky, out var root, out var c);

            screen.backButton = BackButton(c, chevronLeft, CreamShadow, Edge.Top, 40f, 48f);
            Title(c, "Tebak Hewan!", 180f, 64f, 400f, 50f, TextAlignmentOptions.Center);
            screen.starsText = StarBadge(c, starOn, 602f, 52f, 138f, 100f);

            var bar = Rect("Progress", c);
            Put(bar, Edge.Top, 48f, 192f, 684f, 24f);
            var segments = new List<Image>();
            for (int i = 0; i < 6; i++)
            {
                segments.Add(Round("Segment" + (i + 1), bar, PaleBlue, 12f));
            }
            screen.progressSegments = segments.ToArray();

            var question = InfoCard(c, Edge.Top, 40f, 248f, 700f, 265f, CreamShadow, out _);
            screen.clueImage = Picture("Clue", question, Art(AnimalId.Elephant));
            Local(screen.clueImage.rectTransform, 30f, 30f, 210f, 200f);
            screen.questionText = Text("Question", question, "", 42f, Brown, true, TextAlignmentOptions.Left);
            Local(screen.questionText.rectTransform, 262f, 20f, 410f, 215f);

            screen.answerButtons = new Button[3];
            screen.answerBorders = new Image[3];
            screen.answerLabels = new TMP_Text[3];
            screen.answerIcons = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                var border = Raised("Answer" + (i + 1), c, Hex("#F5BE48"), YellowDark, 12f, 40f, out var container);
                Put(container, Edge.Top, 40f + i * 242f, 537f, 217f, 310f);
                var inner = Round("Inner", border.transform, Cream, 34f);
                Stretch(inner.rectTransform, 7f, 7f, 7f, 7f);
                var icon = Picture("Icon", inner.transform, null);
                Local(icon.rectTransform, 24f, 26f, 155f, 150f);
                var label = Text("Label", inner.transform, "", 38f, Brown);
                Local(label.rectTransform, 6f, 190f, 191f, 80f);
                screen.answerButtons[i] = MakeButton(container.gameObject, border);
                screen.answerBorders[i] = border;
                screen.answerLabels[i] = label;
                screen.answerIcons[i] = icon;
            }

            screen.feedbackPanel = Round("Feedback", c, PaleBlue, 40f);
            Put(screen.feedbackPanel.rectTransform, Edge.Top, 40f, 868f, 700f, 150f);
            screen.feedbackPanel.gameObject.AddComponent<PopInEffect>();
            screen.feedbackText = Text("Text", screen.feedbackPanel.transform, "Pilih satu jawaban ya!", 44f, Ink);
            Stretch(screen.feedbackText.rectTransform, 20f, 20f, 10f, 10f);

            var replay = Text("ReplayLink", c, "<u>Ulangi contoh</u>", 34f, Ink);
            replay.raycastTarget = true;
            Put(replay.rectTransform, Edge.Bottom, 200f, 1400f, 380f, 70f);
            screen.replayButton = replay.gameObject.AddComponent<Button>();
            screen.replayButton.transition = Selectable.Transition.None;

            var next = PillButton("NextButton", c, Edge.Bottom, 40f, 1497f, 700f, 156f,
                "Lanjut", Hex("#D6D0C0"), Hex("#BAB2A0"), Hex("#5C5546"), null, 46f);
            screen.nextButton = next;
            var nextContainer = next.transform;
            screen.nextShadow = nextContainer.GetChild(0).GetComponent<Image>();
            screen.nextFace = nextContainer.GetChild(1).GetComponent<Image>();
            screen.nextLabel = screen.nextFace.GetComponentInChildren<TMP_Text>();
            return screen;
        }

        // 6. Hasil Kuis
        private static UIScreen BuildResults(Transform canvas)
        {
            var screen = NewScreen<ResultsScreen>(canvas, "6 HasilKuis", Sky, out var root, out var c);
            Hills(root, false);

            // Confetti pieces: (x, y, w, h, rotation, colour)
            var confetti = new (float x, float y, float w, float h, float rot, string color)[]
            {
                (60f, 118f, 34f, 56f, -30f, "#F59B3A"), (240f, 72f, 26f, 26f, 0f, "#1B7F3B"),
                (620f, 88f, 28f, 28f, 0f, "#F49AB8"), (410f, 138f, 26f, 40f, 30f, "#FFC93C"),
                (138f, 298f, 26f, 50f, -20f, "#F49AB8"), (692f, 262f, 32f, 50f, 40f, "#8B5CE0"),
                (28f, 400f, 24f, 24f, 0f, "#FFC93C"), (658f, 520f, 30f, 30f, 0f, "#F59B3A"),
                (38f, 660f, 30f, 50f, 35f, "#1B7F3B"), (708f, 760f, 26f, 26f, 0f, "#8B5CE0"),
            };
            foreach (var piece in confetti)
            {
                bool round = Mathf.Approximately(piece.w, piece.h);
                var image = round ? Dot("Confetti", c, Hex(piece.color)) : Round("Confetti", c, Hex(piece.color), piece.w / 2f);
                Put(image.rectTransform, Edge.Top, piece.x, piece.y, piece.w, piece.h);
                image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, piece.rot);
            }

            Put(Text("Title", c, "Kamu Hebat!", 76f, Ink).rectTransform, Edge.Top, 60f, 128f, 660f, 110f);

            screen.stars = new Image[3];
            var starBoxes = new[] { (110f, 350f, 145f), (290f, 280f, 200f), (525f, 350f, 145f) };
            for (int i = 0; i < 3; i++)
            {
                var star = Picture("Star" + (i + 1), c, starOn);
                Put(star.rectTransform, Edge.Top, starBoxes[i].Item1, starBoxes[i].Item2, starBoxes[i].Item3, starBoxes[i].Item3);
                star.gameObject.AddComponent<PopInEffect>();
                screen.stars[i] = star;
            }

            var mascot = Picture("Mascot", c, happyLion);
            Put(mascot.rectTransform, Edge.Middle, 180f, 512f, 420f, 420f);
            mascot.gameObject.AddComponent<PulseEffect>().amount = 0.03f;

            var card = InfoCard(c, Edge.Middle, 48f, 942f, 684f, 268f, CreamShadow, out _);
            screen.headlineText = Text("Headline", card, "3 Bintang!", 54f, Brown);
            Local(screen.headlineText.rectTransform, 30f, 30f, 624f, 80f);
            screen.messageText = Text("Message", card, "", 36f, Brown, false);
            Local(screen.messageText.rectTransform, 50f, 112f, 584f, 120f);

            screen.playAgainButton = PillButton("PlayAgainButton", c, Edge.Bottom, 48f, 1313f, 684f, 156f,
                "Main Lagi", Green, GreenDark, White);
            screen.homeButton = PillButton("HomeButton", c, Edge.Bottom, 48f, 1485f, 684f, 136f,
                "Kembali ke Beranda", Cream, Hex("#8CC486"), Brown, null, 38f, 10f);
            return screen;
        }

        // Adds AnimalImageTargetHandler to each Image Target, guessing the
        // animal from the target or object name (lion/singa, elephant/gajah,
        // rabbit/kelinci), and adds ARCameraToggle to the ARCamera.
        private static int WireVuforia(ARAnimalWorldManager app)
        {
            foreach (var behaviour in Object.FindObjectsOfType<VuforiaBehaviour>())
            {
                if (behaviour.GetComponent<ARCameraToggle>() == null)
                {
                    behaviour.gameObject.AddComponent<ARCameraToggle>();
                }
            }

            int wired = 0;
            foreach (var target in Object.FindObjectsOfType<ImageTargetBehaviour>(true))
            {
                var handler = target.GetComponent<AnimalImageTargetHandler>();
                if (handler == null)
                {
                    if (!TryGuessAnimal(target.TargetName + " " + target.name, out var id))
                    {
                        Debug.LogWarning($"Tidak tahu hewan untuk Image Target '{target.name}'. Beri nama yang mengandung singa/gajah/kelinci, atau pasang AnimalImageTargetHandler manual.", target);
                        continue;
                    }

                    handler = target.gameObject.AddComponent<AnimalImageTargetHandler>();
                    handler.animalId = id;
                }

                // The default handler would fight ours over showing children.
                var defaultHandler = target.GetComponent<DefaultObserverEventHandler>();
                if (defaultHandler != null)
                {
                    Object.DestroyImmediate(defaultHandler);
                }

                // Targets wired by older versions saved a straight-on turn;
                // a slight turn shows more of the animal.
                if (handler.modelLocalEuler == Vector3.zero)
                {
                    handler.modelLocalEuler = new Vector3(0f, 30f, 0f);
                }

                handler.appManager = app;
                EditorUtility.SetDirty(handler);
                wired++;
            }
            return wired;
        }

        public static bool TryGuessAnimal(string name, out AnimalId id)
        {
            string lower = name.ToLowerInvariant();
            if (lower.Contains("lion") || lower.Contains("singa"))
            {
                id = AnimalId.Lion;
                return true;
            }
            if (lower.Contains("elephant") || lower.Contains("gajah"))
            {
                id = AnimalId.Elephant;
                return true;
            }
            if (lower.Contains("rabbit") || lower.Contains("kelinci"))
            {
                id = AnimalId.Rabbit;
                return true;
            }
            id = AnimalId.Lion;
            return false;
        }
    }
}
