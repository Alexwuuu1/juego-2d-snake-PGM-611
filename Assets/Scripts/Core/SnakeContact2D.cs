using UnityEngine;

namespace SnakeTrio
{
    public enum ContactKind { Wall, Body, Food }

    // Cada prefab identifica qué está tocando la cabeza en una consulta Physics2D.
    public sealed class SnakeContact2D : MonoBehaviour
    {
        public ContactKind kind;
        public int segmentIndex;
    }
}
