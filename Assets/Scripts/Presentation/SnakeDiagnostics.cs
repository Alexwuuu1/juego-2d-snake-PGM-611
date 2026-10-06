using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnakeTrio
{
    public sealed class SnakeDiagnostics : MonoBehaviour
    {
        readonly Dictionary<string, int> savedInts = new Dictionary<string, int>();
        readonly Dictionary<string, float> savedFloats = new Dictionary<string, float>();
        readonly HashSet<string> missingKeys = new HashSet<string>();
        FullScreenMode savedScreenMode;

        void SavePreferences()
        {
            savedScreenMode = Screen.fullScreenMode;
            foreach (string key in new[] { "SnakeTrio_Difficulty", "SnakeTrio_Muted", "SnakeTrio_Fullscreen",
                "SnakeTrio_Record", "SnakeTrio_Record_0", "SnakeTrio_Record_1", "SnakeTrio_Record_2" })
            { if (PlayerPrefs.HasKey(key)) savedInts[key] = PlayerPrefs.GetInt(key); else missingKeys.Add(key); }
            foreach (string key in new[] { "SnakeTrio_Music", "SnakeTrio_Effects" })
            { if (PlayerPrefs.HasKey(key)) savedFloats[key] = PlayerPrefs.GetFloat(key); else missingKeys.Add(key); }
        }
        void RestorePreferences()
        {
            foreach (var pair in savedInts) PlayerPrefs.SetInt(pair.Key, pair.Value);
            foreach (var pair in savedFloats) PlayerPrefs.SetFloat(pair.Key, pair.Value);
            foreach (string key in missingKeys) PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save(); Screen.fullScreenMode = savedScreenMode;
        }
        void Click(string name)
        {
            var target = GameObject.Find(name);
            var button = target != null ? target.GetComponent<UnityEngine.UI.Button>() : null;
            if (button == null || !button.interactable) Fail("Botón no disponible: " + name);
            button.onClick.Invoke();
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch()
        {
            string[] args = Environment.GetCommandLineArgs();
            if (Array.Exists(args, a => a.StartsWith("--capture-") || a == "--smoke-test"))
            {
                Application.runInBackground = true;
                var root = new GameObject("Validación runtime"); DontDestroyOnLoad(root); root.AddComponent<SnakeDiagnostics>();
            }
        }
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            bool smoke = Array.Exists(args, a => a == "--smoke-test");
            bool captureGame = Array.Exists(args, a => a == "--capture-game");
            bool captureResult = Array.Exists(args, a => a == "--capture-result");
            bool capturePause = Array.Exists(args, a => a == "--capture-pause");
            if (smoke)
            {
                SavePreferences();
                var play = GameObject.Find("JUGAR");
                if (play == null || play.GetComponent<UnityEngine.UI.Button>() == null) Fail("Acción JUGAR ausente");
                var sound = SnakeAudio.Instance;
                bool muted = sound.Muted; sound.ToggleMute();
                if (sound.Muted == muted) Fail("Alternar silencio");
                sound.ToggleMute();
                if (SnakeSession.Clock(125.9f) != "02:05") Fail("Formato del cronómetro");
                var menu = FindFirstObjectByType<SnakeScreen>(); int difficulty = SnakeSession.Difficulty;
                menu.SelectDifficulty(2); if (SnakeSession.Difficulty != 2) Fail("Seleccionar dificultad");
                menu.SelectDifficulty(difficulty);
                menu.OpenCredits(); if (menu.ModalName != "Créditos") Fail("Abrir créditos");
                menu.CloseModal(); menu.OpenSettings();
                if (menu.ModalName != "Ajustes" || FindObjectsByType<UnityEngine.UI.Slider>(FindObjectsSortMode.None).Length != 2) Fail("Ajustes de audio");
                float musicBefore = sound.MusicVolume, effectsBefore = sound.EffectsVolume;
                foreach (var slider in FindObjectsByType<UnityEngine.UI.Slider>(FindObjectsSortMode.None)) slider.value = .62f;
                if (Mathf.Abs(sound.MusicVolume - .62f) > .001f || Mathf.Abs(sound.EffectsVolume - .62f) > .001f) Fail("Cambiar volumen desde la interfaz");
                if (!sound.Muted) sound.ToggleMute();
                Click("RESTABLECER"); yield return null;
                if (Mathf.Abs(sound.MusicVolume - .25f) > .001f || Mathf.Abs(sound.EffectsVolume - .32f) > .001f || sound.Muted)
                    Fail("Restablecer sonido desde el botón");
                foreach (var slider in FindObjectsByType<UnityEngine.UI.Slider>(FindObjectsSortMode.None))
                {
                    float expected = slider.name == "MÚSICA volumen" ? .25f : .32f;
                    if (Mathf.Abs(slider.value - expected) > .001f) Fail("Actualizar controles después de restablecer");
                }
                var sources = sound.GetComponents<AudioSource>();
                if (sources.Length != 2 || sources[0].mute || sources[1].mute
                    || Mathf.Abs(sources[0].volume - .25f) > .001f || Mathf.Abs(sources[1].volume - .32f) > .001f)
                    Fail("Aplicar sonido restablecido a AudioSource");
                sound.SetMusicVolume(musicBefore); sound.SetEffectsVolume(effectsBefore);
                if (sound.Muted != muted) sound.ToggleMute();
                menu.CloseModal(); menu.OpenHelp(); if (menu.ModalName != "Ayuda") Fail("Ayuda"); menu.CloseModal();
                bool fullscreen = SnakePreferences.Fullscreen;
                menu.ToggleFullscreen(); yield return new WaitForSecondsRealtime(.5f);
                if (SnakePreferences.Fullscreen == fullscreen) Fail("Guardar pantalla completa");
                if (SystemInfo.graphicsDeviceType != UnityEngine.Rendering.GraphicsDeviceType.Null
                    && Screen.fullScreenMode != (fullscreen ? FullScreenMode.Windowed : FullScreenMode.FullScreenWindow))
                    Fail("Cambiar modo de pantalla");
                menu.ToggleFullscreen(); yield return new WaitForSecondsRealtime(.5f);
                if (SnakePreferences.Fullscreen != fullscreen) Fail("Volver al modo de pantalla anterior");
                Debug.Log("SMOKE_SETTINGS_OK: ajustes, botón restablecer, AudioSource y pantalla completa.");
            }
            if (smoke) Click("JUGAR");
            else if (captureGame || capturePause) SceneManager.LoadScene("Juego");
            if (captureResult) { SnakeSession.LastScore = 120; SnakeSession.LastFruits = 12; SnakeSession.LastDuration = 98; SnakeSession.LastLength = 15; SnakeSession.LastMoves = 720; SnakeSession.LastDifficulty = SnakeSession.Difficulty; SnakeSession.Reason = "Chocaste contra el borde"; SceneManager.LoadScene("Resultado"); }
            yield return new WaitForSecondsRealtime(.8f);
            if (smoke)
            {
                var game = FindFirstObjectByType<SnakeGame>();
                if (game == null || game.Model == null || game.Segments.Count != 3 || SnakeAudio.Instance == null) Fail("Inicialización");
                if (game.Segments[0].GetComponent<Rigidbody2D>() == null || game.Segments[0].GetComponent<Collider2D>() == null) Fail("Componentes físicos");
                if (!game.HasFoodColliderAt(game.Model.Food)) Fail("Collider2D de comida");
                game.TogglePause(); if (!game.Paused) Fail("Pausa");
                var ui = FindFirstObjectByType<SnakeScreen>();
                ui.Confirm("PRUEBA", "Confirmar reinicio", game.RestartGame);
                ui.CloseModal(); if (!game.Paused) Fail("Cancelar confirmación conserva la partida");
                var headBefore = game.Model.Body[0]; float timeBefore = game.PlayTime; yield return new WaitForSecondsRealtime(.3f);
                if (game.Model.Body[0] != headBefore || game.PlayTime != timeBefore) Fail("La pausa no detuvo el movimiento");
                game.TogglePause(); if (game.ReadyTime < .9f) Fail("Cuenta de reanudación");
                game.PauseForFocusLoss(); if (!game.Paused) Fail("Pausa al perder foco");
                game.TogglePause(); game.PlaceFoodForValidation(game.Model.NextHead);
                if (game.Step() != StepOutcome.Ate || game.Model.Score != 10 || game.Segments.Count != 4) Fail("Comida y crecimiento");
                Debug.Log("SMOKE_GAME_OK: sprites, rigidbody, colliders, pausa, comida, crecimiento y puntaje.");
                game.SetPaused(true); Click("REINICIAR"); Click("CONFIRMAR"); yield return null; yield return null;
                game = FindFirstObjectByType<SnakeGame>();
                if (game == null || game.Model.Score != 0 || game.Segments.Count != 3) Fail("Reiniciar desde pausa");
                game.SetPaused(true); Click("MENÚ"); Click("CONFIRMAR"); yield return null; yield return null;
                if (SceneManager.GetActiveScene().name != "Menu") Fail("Abandonar partida desde pausa");
                Click("JUGAR"); yield return null; yield return null;
                game = FindFirstObjectByType<SnakeGame>(); game.SetPaused(true);
                game.PlaceFoodForValidation(Vector2Int.zero);
                StepOutcome outcome = StepOutcome.Moved;
                for (int i = 0; i <= game.Model.Width && outcome != StepOutcome.Lost; i++) outcome = game.Step();
                if (outcome != StepOutcome.Lost || SnakeSession.LastScore != 0 || !SnakeSession.Reason.Contains("borde"))
                    Fail("Choque y registro de resultado");
                yield return new WaitForSecondsRealtime(.8f);
                if (SceneManager.GetActiveScene().name != "Resultado" || FindFirstObjectByType<SnakeScreen>() == null)
                    Fail("Transición automática al resultado");
                Click("REINTENTAR"); yield return null; yield return null;
                game = FindFirstObjectByType<SnakeGame>();
                if (game == null || game.Model.Score != 0) Fail("Volver a jugar desde resultado");
                game.ReturnToMenu(); yield return null; yield return null;
                if (GameObject.Find("JUGAR") == null) Fail("Regresar al menú");
                RestorePreferences();
                Debug.Log("SMOKE_RESULT_OK: reinicio, menú, choque real, resultado y reintento."); Application.Quit(0); yield break;
            }
            string captureName = captureGame ? "Juego" : captureResult ? "Resultado" : capturePause ? "Pausa" : "Menu";
            var screen = FindFirstObjectByType<SnakeScreen>();
            if (Array.Exists(args, a => a == "--capture-credits")) { screen.OpenCredits(); captureName = "Creditos"; }
            if (Array.Exists(args, a => a == "--capture-settings")) { screen.OpenSettings(); captureName = "Ajustes"; }
            if (Array.Exists(args, a => a == "--capture-help")) { screen.OpenHelp(); captureName = "Ayuda"; }
            string output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath), "SnakeTrio-" + captureName + ".png");
            if (captureGame) FindFirstObjectByType<SnakeGame>().FreezeForCapture();
            if (capturePause) FindFirstObjectByType<SnakeGame>().SetPaused(true);
            yield return null;
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(output);
            yield return new WaitForSecondsRealtime(1); Debug.Log("CAPTURE_OK " + output); Application.Quit(0);
        }
        void Fail(string message) { RestorePreferences(); Debug.LogError("SMOKE_FAIL " + message); Application.Quit(1); throw new Exception(message); }
    }
}
