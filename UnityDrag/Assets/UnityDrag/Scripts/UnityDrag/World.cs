using UnityEngine;

namespace Assets.Scripts.UnityDrag
{
    public static class World
    {
        public static bool Debug = true;

        public static double AirDensity = 1.2; //1.2 = Average Earth Air Density
        public static Vector3 Wind = Vector3.zero;

        public static Vector3 Gravity = Physics.gravity;
    };
}