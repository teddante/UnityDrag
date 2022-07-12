using UnityEngine;

namespace Assets
{
    public static class World
    {
        public static bool Debug = false;

        public static Vector3 Wind = Vector3.zero;

        public static Vector3 Gravity = Physics.gravity;
    };
}