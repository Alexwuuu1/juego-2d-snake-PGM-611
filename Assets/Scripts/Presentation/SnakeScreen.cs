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
        Text scoreText, fruitText, readyText;
        GameObject pausePanel, readyPanel;
        SnakeGame game;

        void Awake()
        {
            if (SnakeAudio.Instance == null) new GameObject("Audio del juego").AddComponent<SnakeAudio>();
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
            string scene = SceneManager.GetActiveScene().name;
            if (Input.GetKeyDown(KeyCode.Return) && scene != "Juego") Play();
            if (Input.GetKeyDown(KeyCode.R) && scene == "Resultado") Play();
            if (Input.GetKeyDown(KeyCode.F12))
            {
                string dir = System.IO.Path.GetDirectoryName(Application.dataPath);
                ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(dir, "SnakeTrio-Captura.png"));
            }
            if (game != null && readyPanel != null)
            { readyPanel.SetActive(game.ReadyTime > 0 && !game.Paused);
              readyText.text = "PREPÁRATE  ·  " + Mathf.CeilToInt(game.ReadyTime); }
        }
        public void Play() { SceneManager.LoadScene("Juego"); }
        public void RefreshScore()
        {
            if (game == null || game.Model == null) return;
            scoreText.text = game.Model.Score.ToString("000"); fruitText.text = "FRUTAS  " + game.Model.Fruits;
        }
        public void ShowPause(bool paused) { pausePanel.SetActive(paused); }

        void BuildMenu()
        {
            Box(canvas, "Fondo", 0, 0, 1280, 720, Background);
            Label(canvas, "PGM-611  /  VIDEOJUEGO 2D", 72, 627, 520, 28, 16, Mint);
            Label(canvas, "SNAKE\nTRÍO", 68, 416, 520, 186, 82, Light, FontStyle.Bold);
            Box(canvas, "Acento", 74, 403, 85, 4, Gold);
            Label(canvas, "Un jardín. Una serpiente.\n¿Cuánto puedes crecer?", 74, 319, 495, 66, 24, Muted);
            Label(canvas, "ELIGE TU RITMO", 74, 270, 450, 25, 13, Muted);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                var button = Button(canvas, "Dificultad " + i, SnakeSession.DifficultyNames[i], 74 + i * 159, 220, 149, 39,
                    SnakeSession.Difficulty == i ? Mint : PanelColor, SnakeSession.Difficulty == i ? Background : Light,
                    () => { SnakeSession.Difficulty = index; SceneManager.LoadScene("Menu"); });
            }
            Button(canvas, "JUGAR", "JUGAR  →", 74, 143, 302, 58, Mint, Background, Play, 22);
            Button(canvas, "SALIR", "SALIR", 393, 143, 148, 58, PanelColor, Light, Application.Quit, 18);
            Label(canvas, "Flechas para moverte  ·  Enter para empezar", 74, 103, 525, 23, 14, Muted);
            MiniBoard(canvas, 655, 187, 28);
            Label(canvas, "COME. CRECE. EVITA LOS BORDES.", 652, 136, 530, 30, 15, Gold);
            Label(canvas, "RÉCORD LOCAL  " + PlayerPrefs.GetInt("SnakeTrio_Record", 0).ToString("000"), 652, 107, 530, 27, 17, Light);
            Credits(canvas);
        }
        void BuildGame()
        {
            Box(canvas, "Barra superior", 0, 610, 1280, 110, Background);
            Label(canvas, "SNAKE TRÍO", 48, 650, 300, 36, 27, Light, FontStyle.Bold);
            Label(canvas, "PGM-611  ·  " + SnakeSession.DifficultyNames[SnakeSession.Difficulty], 49, 623, 310, 22, 13, Muted);
            Label(canvas, "PUNTOS", 492, 671, 120, 20, 12, Muted);
            scoreText = Label(canvas, "000", 492, 624, 150, 47, 36, Mint, FontStyle.Bold);
            fruitText = Label(canvas, "FRUTAS  0", 680, 641, 160, 28, 16, Gold);
            Button(canvas, "PAUSA", "PAUSA  /  ESC", 1030, 638, 201, 45, PanelColor, Light, game.TogglePause, 14);
            Label(canvas, "01", 56, 514, 200, 65, 48, Mint, FontStyle.Bold);
            Label(canvas, "BUSCA LA\nFRUTA DORADA", 58, 447, 215, 60, 21, Light, FontStyle.Bold);
            Label(canvas, "+10 puntos\npor cada fruta.\n\nLa serpiente crece\ncon cada bocado.", 58, 263, 210, 161, 18, Muted);
            Label(canvas, "NO CHOQUES", 1032, 515, 210, 31, 18, Gold, FontStyle.Bold);
            Label(canvas, "Evita los bordes\ny tu propio cuerpo.\n\nFLECHAS\nCambiar dirección\n\nESC / P\nPausa", 1032, 271, 206, 221, 17, Muted);
            Label(canvas, "RÉCORD\n" + PlayerPrefs.GetInt("SnakeTrio_Record", 0).ToString("000"), 1032, 170, 190, 75, 23, Mint);
            Label(canvas, "UN GIRO POR PASO  ·  NO PUEDES GIRAR DIRECTAMENTE HACIA ATRÁS", 306, 57, 680, 24, 12, Muted);
            readyPanel = Box(canvas, "Cuenta de inicio", 393, 320, 493, 73, Background).gameObject;
            readyText = Label(readyPanel.transform, "PREPÁRATE", 0, 0, 493, 73, 24, Mint, FontStyle.Bold, TextAnchor.MiddleCenter);
            pausePanel = Box(canvas, "Pausa", 0, 0, 1280, 720, new Color(0, .08f, .08f, .89f)).gameObject;
            Label(pausePanel.transform, "RESPIRA UN MOMENTO", 310, 433, 660, 66, 40, Light, FontStyle.Bold, TextAnchor.MiddleCenter);
            Label(pausePanel.transform, "La partida está en pausa", 370, 380, 540, 37, 21, Muted, FontStyle.Normal, TextAnchor.MiddleCenter);
            Button(pausePanel.transform, "CONTINUAR", "CONTINUAR", 477, 292, 326, 61, Mint, Background, game.TogglePause, 22);
            Button(pausePanel.transform, "MENÚ", "VOLVER AL MENÚ", 477, 222, 326, 49, PanelColor, Light, () => SceneManager.LoadScene("Menu"), 17);
            pausePanel.SetActive(false);
        }
        void BuildResult()
        {
            Box(canvas, "Fondo", 0, 0, 1280, 720, Background);
            Label(canvas, "SNAKE TRÍO  /  RESULTADO", 73, 628, 550, 30, 16, Mint);
            Label(canvas, SnakeSession.Won ? "¡JARDÍN\nCOMPLETO!" : "HASTA AQUÍ\nLLEGASTE", 69, 431, 645, 176, 61, Light, FontStyle.Bold);
            Label(canvas, SnakeSession.Reason, 76, 379, 610, 42, 22, Gold);
            Label(canvas, "PUNTOS", 76, 327, 200, 24, 13, Muted);
            Label(canvas, SnakeSession.LastScore.ToString("000"), 73, 239, 285, 85, 70, Mint, FontStyle.Bold);
            Label(canvas, SnakeSession.LastFruits + " FRUTAS  ·  RÉCORD " + PlayerPrefs.GetInt("SnakeTrio_Record", 0), 77, 209, 510, 28, 17, Muted);
            Button(canvas, "REINTENTAR", "VOLVER A JUGAR", 76, 122, 307, 60, Mint, Background, Play, 20);
            Button(canvas, "MENÚ", "MENÚ", 401, 122, 164, 60, PanelColor, Light, () => SceneManager.LoadScene("Menu"), 18);
            MiniBoard(canvas, 697, 198, 24);
            Label(canvas, "CADA PARTIDA ES UN NUEVO COMIENZO.", 697, 137, 520, 40, 14, Gold);
            Credits(canvas);
        }
        void Credits(Transform parent)
        {
            Box(parent, "Divisor", 73, 75, 1134, 1, Hex("31504a"));
            Label(parent, "ALEJANDRO VILLALPANDO ROJAS\n@Alexwuuu1", 74, 20, 371, 45, 12, Muted);
            Label(parent, "GALILEA ALISON LLUSCO ASISTIRI\n@Galileya", 474, 20, 371, 45, 12, Muted);
            Label(parent, "CRISTOPHER IORI LAZCANO GUTIERREZ\n@Crisshubb", 856, 20, 371, 45, 12, Muted);
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
              image.sprite = i == 0 ? headSprite : bodySprite; }
            var fruit = Box(parent, "Fruta", x + 14 * cell, y + 4 * cell, cell, cell, Color.white); fruit.sprite = fruitSprite;
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
            var colors = button.colors; colors.highlightedColor = new Color(.85f, 1, .91f); colors.pressedColor = new Color(.62f,.85f,.7f); button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            Label(image.transform, caption, 0, 0, w, h, size, foreground, FontStyle.Bold, TextAnchor.MiddleCenter);
            button.onClick.AddListener(() => { SnakeAudio.Instance.Click(); action(); }); return button;
        }
        public static Color Hex(string value) { ColorUtility.TryParseHtmlString("#" + value, out var color); return color; }
    }
}
