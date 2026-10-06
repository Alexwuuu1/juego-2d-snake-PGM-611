using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SnakeTrio
{
    public sealed class SnakeScreen : MonoBehaviour
    {
        public Sprite headSprite, bodySprite, fruitSprite, tileSprite;
        static readonly Color Background = Hex("102a2a"), PanelColor = Hex("193d3b");
        static readonly Color Mint = Hex("82edb9"), Gold = Hex("ffc75d"), Light = Hex("eef6ef"), Muted = Hex("93b3a7");
        RectTransform canvas;
        Font font;
        Text scoreText, fruitText, readyText, difficultyText, recordText, muteText, timeText, lengthText;
        Button[] difficultyButtons = new Button[3];
        GameObject pausePanel, readyPanel, modal;
        GameObject previousSelection;
        int modalInputFrame = -1;
        public bool BlocksGameplayInput => ModalOpen || modalInputFrame == Time.frameCount;
        public bool ModalOpen => modal != null;
        public string ModalName => modal != null ? modal.name : "";
        SnakeGame game;

        void Awake()
        {
            if (SnakeAudio.Instance == null) new GameObject("Audio del juego").AddComponent<SnakeAudio>();
            Screen.fullScreenMode = SnakePreferences.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var go = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = go.GetComponent<RectTransform>(); go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 720); scaler.matchWidthOrHeight = .5f;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            game = FindFirstObjectByType<SnakeGame>();
            if (SceneManager.GetActiveScene().name == "Menu") BuildMenu();
            else if (game != null) BuildGame(); else BuildResult();
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11))
            {
                SnakePreferences.Fullscreen = !SnakePreferences.Fullscreen;
                Screen.fullScreenMode = SnakePreferences.Fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
            }
            string scene = SceneManager.GetActiveScene().name;
            if (Input.GetKeyDown(KeyCode.M)) { SnakeAudio.Instance.ToggleMute(); RefreshMute(); }
            if (Input.GetKeyDown(KeyCode.F1) && scene != "Juego" && !ModalOpen) OpenHelp();
            if (Input.GetKeyDown(KeyCode.Escape) && ModalOpen) { modalInputFrame = Time.frameCount; CloseModal(); return; }
            if (Input.GetKeyDown(KeyCode.Return) && scene != "Juego" && !ModalOpen
                && EventSystem.current.currentSelectedGameObject == null) Play();
            if (Input.GetKeyDown(KeyCode.R) && scene == "Resultado" && !ModalOpen) Play();
            if (Input.GetKeyDown(KeyCode.F12))
            {
                string dir = System.IO.Path.GetDirectoryName(Application.dataPath);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, "SnakeTrio-Captura.png"));
            }
            if (game != null && game.Model != null)
            {
                timeText.text = SnakeSession.Clock(game.PlayTime);
                lengthText.text = game.Model.Body.Count + " / " + (game.Model.Width * game.Model.Height);
            }
            if (game != null && readyPanel != null)
            { readyPanel.SetActive(game.ReadyTime > 0 && !game.Paused);
              readyText.text = "PREPÁRATE  ·  " + Mathf.CeilToInt(game.ReadyTime); }
        }
        public void Play() { if (!ModalOpen) SceneManager.LoadScene("Juego"); }
        public void RefreshScore()
        {
            if (game == null || game.Model == null) return;
            scoreText.text = game.Model.Score.ToString("000"); fruitText.text = "FRUTAS  " + game.Model.Fruits;
        }
        public void ShowPause(bool paused)
        {
            if (!paused) CloseModal();
            pausePanel.SetActive(paused);
            EventSystem.current.SetSelectedGameObject(paused ? GameObject.Find("CONTINUAR") : null);
        }

        void BuildMenu()
        {
            Box(canvas, "Fondo", 0, 0, 1280, 720, Background);
            Label(canvas, "SNAKE TRÍO", 72, 649, 340, 35, 22, Light, FontStyle.Bold);
            Label(canvas, "PGM-611    /    ARCADE 2D", 912, 653, 300, 27, 13, Muted, FontStyle.Normal, TextAnchor.MiddleRight);
            Box(canvas, "Línea de cabecera", 74, 630, 1132, 1, Hex("31504a"));
            Label(canvas, "UN JARDÍN POR RECORRER", 74, 548, 520, 27, 14, Mint, FontStyle.Bold);
            Label(canvas, "SNAKE", 68, 449, 548, 95, 82, Light, FontStyle.Bold);
            Label(canvas, "Encuentra tu ritmo. Come, crece y supera\ntu mejor partida.", 74, 378, 510, 65, 22, Muted);
            Label(canvas, "DIFICULTAD", 74, 339, 480, 23, 12, Muted, FontStyle.Bold);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                difficultyButtons[i] = Button(canvas, "Dificultad " + i, SnakeSession.DifficultyNames[i],
                    74 + i * 157, 282, 147, 45, PanelColor, Light, () => SelectDifficulty(index), 13);
            }
            difficultyText = Label(canvas, "", 74, 245, 495, 25, 14, Muted);
            Button(canvas, "JUGAR", "JUGAR   →", 74, 158, 312, 62, Mint, Background, Play, 22);
            Button(canvas, "SALIR", "SALIR", 404, 158, 131, 62, PanelColor, Light, Application.Quit, 16);
            Label(canvas, "Flechas o WASD para moverte  ·  Enter para empezar", 74, 117, 525, 24, 14, Muted);
            var card = Box(canvas, "Tarjeta de jardín", 650, 171, 557, 437, PanelColor); SnakeTheme.Round(card);
            Label(canvas, "EL JARDÍN", 684, 574, 200, 26, 12, Mint, FontStyle.Bold);
            Label(canvas, "28 × 20 CELDAS", 978, 574, 194, 26, 12, Muted, FontStyle.Normal, TextAnchor.MiddleRight);
            MiniBoard(canvas, 684, 195, 26);
            recordText = Label(canvas, "", 650, 117, 557, 31, 16, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            SelectDifficulty(SnakeSession.Difficulty);
            Credits(canvas);
        }
        public void SelectDifficulty(int index)
        {
            SnakeSession.Difficulty = index;
            for (int i = 0; i < difficultyButtons.Length; i++)
            {
                bool selected = i == SnakeSession.Difficulty;
                difficultyButtons[i].GetComponent<Image>().color = selected ? Mint : PanelColor;
                difficultyButtons[i].GetComponentInChildren<Text>().color = selected ? Background : Light;
            }
            string[] descriptions = { "Más tiempo para planear cada giro.", "El equilibrio entre ritmo y precisión.", "Reflejos rápidos. Cada movimiento cuenta." };
            difficultyText.text = descriptions[SnakeSession.Difficulty];
            recordText.text = "RÉCORD  /  " + SnakeSession.DifficultyNames[SnakeSession.Difficulty] + "    " + SnakeSession.Record(SnakeSession.Difficulty).ToString("000");
        }
        void BuildGame()
        {
            Box(canvas, "Barra superior", 0, 610, 1280, 110, Background);
            Label(canvas, "SNAKE TRÍO", 48, 650, 320, 36, 27, Light, FontStyle.Bold);
            Label(canvas, "PGM-611   /   " + SnakeSession.DifficultyNames[SnakeSession.Difficulty], 49, 623, 330, 22, 13, Muted);
            MetricCard(canvas, "PUNTOS", "000", 451, 625, 174, out scoreText, Mint);
            MetricCard(canvas, "TIEMPO", "00:00", 641, 625, 174, out timeText, Light);
            fruitText = Label(canvas, "FRUTAS  0", 838, 637, 169, 30, 16, Gold, FontStyle.Bold);
            Button(canvas, "PAUSA", "PAUSA  /  ESC", 1030, 638, 201, 45, PanelColor, Light, game.TogglePause, 14);
            Label(canvas, "OBJETIVO", 52, 519, 210, 31, 13, Mint, FontStyle.Bold);
            Label(canvas, "LLENA\nEL JARDÍN", 52, 430, 216, 78, 27, Light, FontStyle.Bold);
            Label(canvas, "+10 puntos por fruta.\nCrece sin chocar con\nlos bordes o tu cuerpo.", 52, 321, 216, 91, 17, Muted);
            MetricCard(canvas, "LONGITUD", "3 / 560", 52, 209, 205, out lengthText, Mint);
            Label(canvas, "TU MEJOR PARTIDA", 1032, 507, 204, 30, 12, Muted, FontStyle.Bold);
            Label(canvas, SnakeSession.Record(SnakeSession.Difficulty).ToString("000"), 1032, 442, 200, 60, 44, Mint, FontStyle.Bold);
            Box(canvas, "Divisor lateral", 1032, 416, 194, 1, Hex("31504a"));
            Label(canvas, "FLECHAS / WASD\nCambiar dirección\n\nESC / P\nPausa\n\nM\nSilenciar sonido", 1032, 199, 206, 196, 16, Muted);
            Label(canvas, "UN GIRO POR PASO   /   PLANEA TU SIGUIENTE MOVIMIENTO", 306, 54, 680, 27, 12, Muted, FontStyle.Normal, TextAnchor.MiddleCenter);
            readyPanel = Box(canvas, "Cuenta de inicio", 393, 320, 493, 73, Background).gameObject; SnakeTheme.Round(readyPanel.GetComponent<Image>());
            readyText = Label(readyPanel.transform, "PREPÁRATE", 0, 0, 493, 73, 24, Mint, FontStyle.Bold, TextAnchor.MiddleCenter);
            pausePanel = Box(canvas, "Pausa", 0, 0, 1280, 720, new Color(0, .06f, .06f, .92f)).gameObject;
            pausePanel.GetComponent<Image>().raycastTarget = true;
            var card = Box(pausePanel.transform, "Tarjeta de pausa", 380, 137, 520, 452, PanelColor); SnakeTheme.Round(card);
            Label(card.transform, "PARTIDA EN PAUSA", 36, 346, 448, 58, 30, Light, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(card.transform, "Tómate un momento. El jardín espera.", 36, 302, 448, 38, 17, Muted, FontStyle.Normal, TextAnchor.MiddleCenter);
            Button(card.transform, "CONTINUAR", "CONTINUAR", 36, 223, 448, 55, Mint, Background, game.TogglePause, 18);
            Button(card.transform, "REINICIAR", "REINICIAR PARTIDA", 36, 164, 448, 46, Background, Light,
                () => Confirm("¿EMPEZAR DE NUEVO?", "Esta partida se perderá. El récord se conserva.", game.RestartGame), 15);
            Button(card.transform, "AJUSTES PAUSA", "AJUSTES DE SONIDO", 36, 108, 448, 46, Background, Light, OpenSettings, 15);
            Button(card.transform, "MENÚ", "VOLVER AL MENÚ", 36, 52, 448, 46, Background, Light,
                () => Confirm("¿VOLVER AL MENÚ?", "Esta partida se perderá. El récord se conserva.", game.ReturnToMenu), 15);
            pausePanel.SetActive(false);
        }
        void MetricCard(Transform parent, string title, string value, float x, float y, float width, out Text metric, Color color)
        {
            var card = Box(parent, "Tarjeta " + title, x, y, width, 79, PanelColor); SnakeTheme.Round(card);
            Label(card.transform, title, 17, 49, width - 34, 20, 11, Muted, FontStyle.Bold);
            metric = Label(card.transform, value, 17, 7, width - 34, 41, 29, color, FontStyle.Bold);
        }
        public void Confirm(string title, string description, Action accepted)
        {
            var panel = OpenModal("Confirmación", title, description);
            Label(panel, "¿Quieres continuar?", 36, 202, 568, 55, 24, Light);
            Button(panel, "CONFIRMAR", "SÍ, CONTINUAR", 36, 130, 568, 50, Gold, Background,
                () => { CloseModal(); accepted(); }, 17);
        }
        void BuildResult()
        {
            Box(canvas, "Fondo", 0, 0, 1280, 720, Background);
            Label(canvas, "SNAKE TRÍO    /    RESULTADO", 74, 649, 650, 35, 17, Mint, FontStyle.Bold);
            Box(canvas, "Divisor", 74, 630, 1132, 1, Hex("31504a"));
            Label(canvas, SnakeSession.Won ? "JARDÍN COMPLETO" : "BUENA PARTIDA", 69, 499, 1138, 90, 58, Light, FontStyle.Bold);
            Label(canvas, SnakeSession.Reason, 75, 452, 1100, 36, 21, Muted);
            Label(canvas, SnakeSession.NewRecord ? "NUEVO RÉCORD PERSONAL" : "TU PRÓXIMO RÉCORD TE ESPERA", 75, 405, 1130, 28, 14, Gold, FontStyle.Bold);
            Text value;
            MetricCard(canvas, "PUNTOS", SnakeSession.LastScore.ToString("000"), 74, 275, 267, out value, Mint);
            MetricCard(canvas, "FRUTAS", SnakeSession.LastFruits.ToString(), 359, 275, 267, out value, Gold);
            MetricCard(canvas, "TIEMPO", SnakeSession.Clock(SnakeSession.LastDuration), 644, 275, 267, out value, Light);
            MetricCard(canvas, "LONGITUD", SnakeSession.LastLength.ToString(), 929, 275, 277, out value, Light);
            Label(canvas, SnakeSession.DifficultyNames[SnakeSession.LastDifficulty] + "   /   RÉCORD " + SnakeSession.Record(SnakeSession.LastDifficulty).ToString("000")
                + "   /   " + SnakeSession.LastMoves + " MOVIMIENTOS", 75, 221, 1130, 31, 15, Muted);
            Button(canvas, "REINTENTAR", "VOLVER A JUGAR   →", 74, 121, 395, 64, Mint, Background, Play, 20);
            Button(canvas, "MENÚ", "IR AL MENÚ", 487, 121, 220, 64, PanelColor, Light, () => SceneManager.LoadScene("Menu"), 16);
            Label(canvas, "R para reintentar", 918, 131, 288, 38, 14, Muted, FontStyle.Normal, TextAnchor.MiddleRight);
            Credits(canvas);
        }
        void Credits(Transform parent)
        {
            Button(parent, "CRÉDITOS", "CRÉDITOS", 74, 48, 138, 37, PanelColor, Light, OpenCredits, 12);
            Button(parent, "AJUSTES", "AJUSTES", 225, 48, 138, 37, PanelColor, Light, OpenSettings, 12);
            Button(parent, "AYUDA", "CÓMO JUGAR", 376, 48, 159, 37, PanelColor, Light, OpenHelp, 12);
            Label(parent, "PROYECTO ACADÉMICO   /   PGM-611", 807, 48, 400, 37, 12, Muted, FontStyle.Normal, TextAnchor.MiddleRight);
        }
        RectTransform OpenModal(string name, string title, string subtitle)
        {
            CloseModal(); previousSelection = EventSystem.current.currentSelectedGameObject;
            modal = Box(canvas, name, 0, 0, 1280, 720, new Color(0, .06f, .06f, .92f)).gameObject;
            modal.GetComponent<Image>().raycastTarget = true;
            var panel = Box(modal.transform, "Tarjeta", 320, 134, 640, 452, PanelColor); SnakeTheme.Round(panel);
            Label(panel.transform, title, 36, 362, 568, 48, 30, Light, FontStyle.Bold);
            Label(panel.transform, subtitle, 36, 316, 568, 39, 16, Muted);
            var close = Button(panel.transform, "CERRAR", "VOLVER", 36, 28, 568, 46, Mint, Background, CloseModal, 16);
            EventSystem.current.SetSelectedGameObject(close.gameObject);
            // El teclado solo navega por la tarjeta activa.
            foreach (var selectable in canvas.GetComponentsInChildren<Selectable>())
                if (!selectable.transform.IsChildOf(modal.transform)) selectable.interactable = false;
            return panel.rectTransform;
        }
        public void CloseModal()
        {
            if (modal == null) return;
            modal.SetActive(false); Destroy(modal); modal = null;
            foreach (var selectable in canvas.GetComponentsInChildren<Selectable>()) selectable.interactable = true;
            EventSystem.current.SetSelectedGameObject(previousSelection);
        }
        public void OpenHelp()
        {
            var panel = OpenModal("Ayuda", "CÓMO JUGAR", "Una fruta, diez puntos. Llena el jardín para ganar.");
            Label(panel, "FLECHAS / WASD", 36, 254, 157, 27, 14, Mint, FontStyle.Bold);
            Label(panel, "Cambia la dirección de la serpiente.", 210, 254, 394, 27, 16, Light);
            Label(panel, "ESC / P", 36, 210, 157, 27, 14, Mint, FontStyle.Bold);
            Label(panel, "Pausa o continúa la partida.", 210, 210, 394, 27, 16, Light);
            Label(panel, "M", 36, 166, 157, 27, 14, Mint, FontStyle.Bold);
            Label(panel, "Silencia o reactiva el sonido.", 210, 166, 394, 27, 16, Light);
            Label(panel, "Evita los bordes y tu propio cuerpo. No puedes girar\ndirectamente hacia atrás. Elige un giro por paso.", 36, 89, 568, 60, 17, Muted);
        }
        public void OpenSettings()
        {
            var panel = OpenModal("Ajustes", "A TU MEDIDA", "Volumen guardado · F11 cambia la pantalla");
            VolumeControl(panel, "MÚSICA", 226, SnakeAudio.Instance.MusicVolume, SnakeAudio.Instance.SetMusicVolume);
            VolumeControl(panel, "EFECTOS", 142, SnakeAudio.Instance.EffectsVolume, SnakeAudio.Instance.SetEffectsVolume);
            var mute = Button(panel, "SILENCIAR", "", 36, 87, 274, 40, Background, Light,
                () => { SnakeAudio.Instance.ToggleMute(); RefreshMute(); }, 14);
            muteText = mute.GetComponentInChildren<Text>(); RefreshMute();
            Button(panel, "RESTABLECER", "RESTABLECER", 330, 87, 274, 40, Background, Light,
                () => { SnakeAudio.Instance.ResetSettings(); OpenSettings(); }, 13);
        }
        void RefreshMute()
        { if (muteText != null) muteText.text = SnakeAudio.Instance.Muted ? "REACTIVAR  /  M" : "SILENCIAR  /  M"; }
        void VolumeControl(Transform parent, string title, float y, float value, Action<float> changed)
        {
            Label(parent, title, 36, y + 35, 360, 25, 13, Mint, FontStyle.Bold);
            var percent = Label(parent, Mathf.RoundToInt(value * 100) + "%", 474, y + 35, 130, 25, 14, Light, FontStyle.Normal, TextAnchor.MiddleRight);
            var root = Rect(parent, title + " volumen", 36, y, 568, 28);
            var track = Box(root, "Pista", 0, 10, 568, 8, Background); SnakeTheme.Round(track);
            var fillArea = Rect(root, "Área de relleno", 0, 10, 568, 8);
            var fill = Box(fillArea, "Relleno", 0, 0, 568, 8, Mint); SnakeTheme.Round(fill);
            fill.rectTransform.sizeDelta = Vector2.zero;
            var handleArea = Rect(root, "Área de control", 10, 4, 548, 20);
            var handle = Box(handleArea, "Control", 0, 0, 20, 20, Light); SnakeTheme.Round(handle); handle.raycastTarget = true;
            handle.rectTransform.sizeDelta = new Vector2(20, 0);
            var slider = root.gameObject.AddComponent<Slider>();
            handle.rectTransform.pivot = new Vector2(.5f, 0);
            slider.targetGraphic = handle; slider.fillRect = fill.rectTransform; slider.handleRect = handle.rectTransform;
            slider.minValue = 0; slider.maxValue = 1; slider.SetValueWithoutNotify(value);
            slider.onValueChanged.AddListener(v => { changed(v); percent.text = Mathf.RoundToInt(v * 100) + "%"; });
        }
        public void OpenCredits()
        {
            var panel = OpenModal("Créditos", "EL EQUIPO", "Snake Trío · Videojuego 2D · PGM-611");
            Label(panel, "Alejandro Villalpando Rojas", 36, 250, 568, 30, 21, Light, FontStyle.Bold);
            Label(panel, "@Alexwuuu1", 36, 222, 568, 25, 15, Mint);
            Label(panel, "Galilea Alison Llusco Asistiri", 36, 173, 568, 30, 21, Light, FontStyle.Bold);
            Label(panel, "@Galileya", 36, 145, 568, 25, 15, Mint);
            Label(panel, "Cristopher Iori Lazcano Gutierrez", 36, 96, 568, 30, 21, Light, FontStyle.Bold);
            Label(panel, "@Crisshubb", 36, 78, 568, 22, 15, Mint);
        }
        void MiniBoard(Transform parent, float x, float y, float cell)
        {
            Box(parent, "Marco de jardín", x - 12, y - 12, 18 * cell + 24, 14 * cell + 24, Hex("305956"));
            for (int row = 0; row < 14; row++) for (int col = 0; col < 18; col++)
            {
                var image = Box(parent, "Baldosa", x + col * cell, y + row * cell, cell, cell, Color.white);
                image.sprite = tileSprite; image.color = (row + col) % 2 == 0 ? Color.white : new Color(.8f, .85f, .85f);
            }
            Vector2Int[] path = { new Vector2Int(13,9), new Vector2Int(12,9), new Vector2Int(11,9), new Vector2Int(10,9),
                new Vector2Int(9,9),new Vector2Int(9,8),new Vector2Int(9,7),new Vector2Int(9,6),new Vector2Int(8,6),
                new Vector2Int(7,6),new Vector2Int(6,6),new Vector2Int(5,6),new Vector2Int(5,5),new Vector2Int(5,4) };
            for (int i = path.Length - 1; i >= 0; i--)
            { var image = Box(parent, "Serpiente", x + path[i].x * cell, y + path[i].y * cell, cell, cell, Color.white);
              PixelSprite(image, i == 0 ? headSprite : bodySprite, cell); }
            var fruit = Box(parent, "Fruta", x + 14 * cell, y + 4 * cell, cell, cell, Color.white); PixelSprite(fruit, fruitSprite, cell);
        }
        static void PixelSprite(Image image, Sprite sprite, float cell)
        {
            image.sprite = sprite;
            float scale = cell / 32f;
            // Conservar el lienzo original aunque el importador recorte la transparencia.
            image.rectTransform.sizeDelta = sprite.rect.size * scale;
            image.rectTransform.anchoredPosition += (Vector2.one * 16 - sprite.pivot) * scale;
        }
        static RectTransform Rect(Transform parent, string name, float x, float y, float w, float h)
        {
            var go = new GameObject(name, typeof(RectTransform)); var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false); rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            rt.anchoredPosition = new Vector2(x, y); rt.sizeDelta = new Vector2(w, h); return rt;
        }
        static Image Box(Transform parent, string name, float x, float y, float w, float h, Color color)
        { var image = Rect(parent, name, x, y, w, h).gameObject.AddComponent<Image>(); image.color = color; image.raycastTarget = false; return image; }
        Text Label(Transform parent, string value, float x, float y, float w, float h, int size, Color color,
            FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.MiddleLeft)
        {
            var text = Rect(parent, "Texto " + value.Split('\n')[0], x, y, w, h).gameObject.AddComponent<Text>();
            text.font = font; text.text = value; text.fontSize = size; text.color = color; text.fontStyle = style;
            text.alignment = alignment; text.raycastTarget = false; text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate; return text;
        }
        Button Button(Transform parent, string name, string caption, float x, float y, float w, float h,
            Color background, Color foreground, Action action, int size = 13)
        {
            var image = Box(parent, name, x, y, w, h, background); image.raycastTarget = true;
            var button = image.gameObject.AddComponent<Button>(); button.targetGraphic = image;
            SnakeTheme.Style(button);
            Label(image.transform, caption, 0, 0, w, h, size, foreground, FontStyle.Bold, TextAnchor.MiddleCenter);
            button.onClick.AddListener(() => { SnakeAudio.Instance.Click(); action(); }); return button;
        }
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
    }
}
