using System;
using System.Collections.Generic;
using UnityEngine;

namespace SnakeTrio
{
    public enum StepOutcome { Moved, Ate, Lost, Won }

    // Reglas independientes de la interfaz. Un paso equivale a una celda.
    public sealed class SnakeModel
    {
        public readonly int Width, Height;
        public readonly List<Vector2Int> Body = new List<Vector2Int>();
        public Vector2Int Direction { get; private set; } = Vector2Int.right;
        public Vector2Int Food { get; private set; }
        public int Score { get; private set; }
        public int Fruits { get; private set; }
        public bool Finished { get; private set; }
        public bool Victory { get; private set; }
        public string EndReason { get; private set; }
        readonly System.Random random;
        Vector2Int nextDirection = Vector2Int.right;
        bool turnQueued;
        public Vector2Int NextHead => Body[0] + nextDirection;

        public SnakeModel(int width = 28, int height = 20, int seed = -1)
        {
            if (width < 6 || height < 4) throw new ArgumentException("Tablero demasiado pequeño");
            Width = width; Height = height;
            random = seed < 0 ? new System.Random() : new System.Random(seed);
            var head = new Vector2Int(width / 2, height / 2);
            Body.Add(head); Body.Add(head + Vector2Int.left); Body.Add(head + Vector2Int.left * 2);
            SpawnFood();
        }

        public bool QueueDirection(Vector2Int candidate)
        {
            if (Finished || turnQueued || Math.Abs(candidate.x) + Math.Abs(candidate.y) != 1
                || candidate == -Direction || candidate == Direction) return false;
            nextDirection = candidate; turnQueued = true; return true;
        }

        public StepOutcome Advance(bool physicsBlocked = false, string physicsReason = "")
        {
            if (Finished) return Victory ? StepOutcome.Won : StepOutcome.Lost;
            Direction = nextDirection; turnQueued = false;
            var next = Body[0] + Direction;
            if (next.x < 0 || next.x >= Width || next.y < 0 || next.y >= Height)
                return Lose("Chocaste contra el borde");
            if (physicsBlocked) return Lose(physicsReason);
            bool grows = next == Food;
            // Al avanzar sin comer, la cola se libera: entrar en esa celda es válido.
            int occupied = grows ? Body.Count : Body.Count - 1;
            for (int i = 0; i < occupied; i++) if (Body[i] == next) return Lose("Chocaste con tu serpiente");
            Body.Insert(0, next);
            if (!grows) { Body.RemoveAt(Body.Count - 1); return StepOutcome.Moved; }
            Score += 10; Fruits++;
            if (Body.Count == Width * Height)
            { Finished = true; Victory = true; EndReason = "¡Completaste todo el tablero!"; return StepOutcome.Won; }
            SpawnFood(); return StepOutcome.Ate;
        }

        StepOutcome Lose(string reason)
        { Finished = true; EndReason = reason; return StepOutcome.Lost; }

        void SpawnFood()
        {
            var free = new List<Vector2Int>();
            for (int y = 0; y < Height; y++) for (int x = 0; x < Width; x++)
            { var cell = new Vector2Int(x, y); if (!Body.Contains(cell)) free.Add(cell); }
            if (free.Count != 0) Food = free[random.Next(free.Count)];
        }

        // Permite comprobar colisiones y puntuación sin alterar la partida real.
        public void SetFoodForValidation(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= Width || cell.y < 0 || cell.y >= Height || Body.Contains(cell))
                throw new ArgumentException("La fruta debe ocupar una celda libre");
            Food = cell;
        }
    }

    public static class SnakeSession
    {
        public static int Difficulty { get => SnakePreferences.Difficulty; set => SnakePreferences.Difficulty = value; }
        public static int LastScore, LastFruits;
        public static bool Won;
        public static string Reason = "";
        public static float[] Speeds = { .19f, .135f, .095f };
        public static string[] DifficultyNames = { "TRANQUILO", "CLÁSICO", "RÁPIDO" };
    }
}
