using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SnakeTrio
{
    public sealed class SnakeGame : MonoBehaviour
    {
        public GameObject headPrefab, bodyPrefab, foodPrefab;
        public SnakeModel Model { get; private set; }
        public bool Paused { get; private set; }
        public float ReadyTime { get; private set; } = 1.5f;
        public static readonly float Cell = .45f;
        public static readonly Vector2 Center = new Vector2(0, -.25f);
        public IReadOnlyList<GameObject> Segments => segments;
        readonly List<GameObject> segments = new List<GameObject>();
        GameObject food;
        float elapsed;
        SnakeScreen screen;
        bool ending;

        void Start()
        {
            Model = new SnakeModel(); screen = FindFirstObjectByType<SnakeScreen>();
            food = Instantiate(foodPrefab); food.name = "Comida";
            RefreshSprites();
        }
        void Update()
        {
            if (Model == null || ending) return;
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P)) TogglePause();
            if (Paused || ReadyTime > 0) return;
            var requested = ReadDirection(); if (requested != Vector2Int.zero) Model.QueueDirection(requested);
        }
        public static Vector2Int ReadDirection()
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) return Vector2Int.up;
            if (Input.GetKeyDown(KeyCode.DownArrow)) return Vector2Int.down;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) return Vector2Int.left;
            if (Input.GetKeyDown(KeyCode.RightArrow)) return Vector2Int.right;
            return Vector2Int.zero;
        }
        void FixedUpdate()
        {
            if (Model == null || Paused || ending) return;
            if (ReadyTime > 0) { ReadyTime = Mathf.Max(0, ReadyTime - Time.fixedDeltaTime); return; }
            elapsed += Time.fixedDeltaTime;
            if (elapsed < SnakeSession.Speeds[SnakeSession.Difficulty]) return;
            elapsed -= SnakeSession.Speeds[SnakeSession.Difficulty]; Step();
        }
        public void TogglePause()
        { if (ending) return; Paused = !Paused; SnakeAudio.Instance.Click(); screen.ShowPause(Paused); }

        public StepOutcome Step()
        {
            Physics2D.SyncTransforms();
            // Los Collider2D de paredes y segmentos participan realmente en la derrota.
            // Se consulta antes de mover la cabeza; se excluye la cola que se libera.
            bool blocked = false;
            string reason = "";
            bool grows = Model.NextHead == Model.Food;
            foreach (var hit in Physics2D.OverlapBoxAll(World(Model.NextHead), Vector2.one * Cell * .55f, 0))
            {
                var contact = hit.GetComponent<SnakeContact2D>();
                if (contact == null) continue;
                bool bodyCollision = contact.kind == ContactKind.Body && contact.segmentIndex > 0
                    && !(!grows && contact.segmentIndex == segments.Count - 1);
                if (contact.kind == ContactKind.Wall || bodyCollision)
                {
                    blocked = true;
                    reason = bodyCollision ? "Chocaste con tu serpiente" : "Chocaste contra el borde";
                    break;
                }
            }
            var outcome = Model.Advance(blocked, reason);
            RefreshSprites();
            if (outcome == StepOutcome.Ate) SnakeAudio.Instance.Eat();
            if (outcome == StepOutcome.Lost || outcome == StepOutcome.Won)
            {
                ending = true;
                if (outcome == StepOutcome.Won) SnakeAudio.Instance.Win(); else SnakeAudio.Instance.Hit();
                SnakeSession.LastScore = Model.Score; SnakeSession.LastFruits = Model.Fruits;
                SnakeSession.Won = Model.Victory; SnakeSession.Reason = Model.EndReason;
                PlayerPrefs.SetInt("SnakeTrio_Record", Mathf.Max(Model.Score, PlayerPrefs.GetInt("SnakeTrio_Record", 0)));
                PlayerPrefs.Save(); StartCoroutine(Finish());
            }
            screen.RefreshScore(); return outcome;
        }
        IEnumerator Finish() { yield return new WaitForSecondsRealtime(.65f); SceneManager.LoadScene("Resultado"); }
        void RefreshSprites()
        {
            while (segments.Count < Model.Body.Count)
            {
                var part = Instantiate(segments.Count == 0 ? headPrefab : bodyPrefab);
                part.name = segments.Count == 0 ? "Cabeza" : "Segmento " + segments.Count;
                part.GetComponent<SnakeContact2D>().segmentIndex = segments.Count; segments.Add(part);
            }
            for (int i = 0; i < segments.Count; i++)
            {
                segments[i].transform.position = World(Model.Body[i]);
                if (i == 0) segments[i].transform.rotation = Quaternion.Euler(0, 0,
                    Mathf.Atan2(Model.Direction.y, Model.Direction.x) * Mathf.Rad2Deg);
            }
            food.transform.position = World(Model.Food);
            food.SetActive(!Model.Victory);
        }
        public static Vector2 World(Vector2Int cell)
        { return Center + new Vector2(cell.x - 13.5f, cell.y - 9.5f) * Cell; }
        public void SkipReadyForValidation() { ReadyTime = 0; }
        public void FreezeForCapture() { Paused = true; ReadyTime = 0; }
        public void PlaceFoodForValidation(Vector2Int cell) { Model.SetFoodForValidation(cell); RefreshSprites(); }
        public bool HasFoodColliderAt(Vector2Int cell)
        {
            Physics2D.SyncTransforms();
            foreach (var hit in Physics2D.OverlapBoxAll(World(cell), Vector2.one * Cell * .5f, 0))
            { var c = hit.GetComponent<SnakeContact2D>(); if (c != null && c.kind == ContactKind.Food) return true; }
            return false;
        }
    }
}
