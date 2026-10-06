using UnityEngine;
using UnityEngine.UI;

namespace SnakeTrio
{
    public static class SnakeTheme
    {
        static Sprite rounded;
        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                const int size = 32; const float radius = 8;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.name = "Esquinas de interfaz"; texture.filterMode = FilterMode.Bilinear;
                for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
                {
                    float dx = Mathf.Max(Mathf.Abs(x - 15.5f) - 7.5f, 0);
                    float dy = Mathf.Max(Mathf.Abs(y - 15.5f) - 7.5f, 0);
                    float alpha = Mathf.Clamp01(radius - Mathf.Sqrt(dx * dx + dy * dy) + .5f);
                    texture.SetPixel(x, y, new Color(1, 1, 1, alpha));
                }
                texture.Apply();
                rounded = Sprite.Create(texture, new Rect(0, 0, size, size), Vector2.one * .5f, 100, 0,
                    SpriteMeshType.FullRect, new Vector4(10, 10, 10, 10));
                return rounded;
            }
        }
        public static void Round(Image image) { image.sprite = Rounded; image.type = Image.Type.Sliced; }
        public static void Style(Button button)
        {
            Round((Image)button.targetGraphic);
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.16f, 1.16f, 1.16f);
            colors.selectedColor = new Color(1.16f, 1.16f, 1.16f);
            colors.pressedColor = new Color(.78f, .86f, .82f);
            colors.disabledColor = new Color(.5f, .5f, .5f, .6f);
            colors.fadeDuration = .12f; button.colors = colors;
            button.navigation = new Navigation { mode = Navigation.Mode.Automatic };
        }
    }
}
