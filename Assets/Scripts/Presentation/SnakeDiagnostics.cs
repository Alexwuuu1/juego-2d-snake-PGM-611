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
            { var root = new GameObject("Validación runtime"); DontDestroyOnLoad(root); root.AddComponent<SnakeDiagnostics>(); }
        }
        IEnumerator Start()
        {
            var args = Environment.GetCommandLineArgs();
            bool smoke = Array.Exists(args, a => a == "--smoke-test");
            bool captureGame = Array.Exists(args, a => a == "--capture-game");
            bool captureResult = Array.Exists(args, a => a == "--capture-result");
            if (smoke && FindObjectsByType<UnityEngine.UI.Button>(FindObjectsSortMode.None).Length != 5) Fail("Botones del menú");
            if (smoke || captureGame) SceneManager.LoadScene("Juego");
            if (captureResult) { SnakeSession.LastScore = 120; SnakeSession.LastFruits = 12; SnakeSession.Reason = "Chocaste contra el borde"; SceneManager.LoadScene("Resultado"); }
            yield return new WaitForSecondsRealtime(.8f);
            if (smoke)
            {
                var game = FindFirstObjectByType<SnakeGame>();
                if (game == null || game.Model == null || game.Segments.Count != 3 || SnakeAudio.Instance == null) Fail("Inicialización");
                if (game.Segments[0].GetComponent<Rigidbody2D>() == null || game.Segments[0].GetComponent<Collider2D>() == null) Fail("Componentes físicos");
                if (!game.HasFoodColliderAt(game.Model.Food)) Fail("Collider2D de comida");
                game.TogglePause(); if (!game.Paused) Fail("Pausa");
                var headBefore = game.Model.Body[0]; yield return new WaitForSecondsRealtime(.3f);
                if (game.Model.Body[0] != headBefore) Fail("La pausa no detuvo el movimiento");
                game.TogglePause(); game.PlaceFoodForValidation(game.Model.NextHead);
                if (game.Step() != StepOutcome.Ate || game.Model.Score != 10 || game.Segments.Count != 4) Fail("Comida y crecimiento");
                Debug.Log("SMOKE_GAME_OK: sprites, rigidbody, colliders, pausa, comida, crecimiento y puntaje.");
                // Mantiene las pruebas aisladas: no guardar el récord de una partida automatizada.
                SceneManager.LoadScene("Resultado"); yield return null; yield return null;
                if (FindFirstObjectByType<SnakeScreen>() == null) Fail("Escena de resultado");
                Debug.Log("SMOKE_RESULT_OK: transición y pantalla final."); Application.Quit(0); yield break;
            }
            string output = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Application.dataPath),
                captureGame ? "SnakeTrio-Juego.png" : captureResult ? "SnakeTrio-Resultado.png" : "SnakeTrio-Menu.png");
            if (captureGame) FindFirstObjectByType<SnakeGame>().FreezeForCapture();
            yield return new WaitForEndOfFrame(); ScreenCapture.CaptureScreenshot(output);
            yield return new WaitForSecondsRealtime(1); Debug.Log("CAPTURE_OK " + output); Application.Quit(0);
        }
        void Fail(string message) { Debug.LogError("SMOKE_FAIL " + message); Application.Quit(1); throw new Exception(message); }
    }
}
