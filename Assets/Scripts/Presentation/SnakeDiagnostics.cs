using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnakeTrio
{
    public sealed class SnakeDiagnostics : MonoBehaviour
    {
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
                sound.SetMusicVolume(musicBefore); sound.SetEffectsVolume(effectsBefore);
                menu.CloseModal(); menu.OpenHelp(); if (menu.ModalName != "Ayuda") Fail("Ayuda"); menu.CloseModal();
                Debug.Log("SMOKE_SETTINGS_OK: sonido y formato de tiempo.");
            }
            if (smoke || captureGame || capturePause) SceneManager.LoadScene("Juego");
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
                // Mantiene las pruebas aisladas: no guardar el récord de una partida automatizada.
                SceneManager.LoadScene("Resultado"); yield return null; yield return null;
                if (FindFirstObjectByType<SnakeScreen>() == null) Fail("Escena de resultado");
                Debug.Log("SMOKE_RESULT_OK: transición y pantalla final."); Application.Quit(0); yield break;
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
        void Fail(string message) { Debug.LogError("SMOKE_FAIL " + message); Application.Quit(1); throw new Exception(message); }
    }
}
