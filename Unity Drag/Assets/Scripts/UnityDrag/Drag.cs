using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.UnityDrag
{
    public class Drag : MonoBehaviour
    {
        private Mesh DragMesh;
        public Rigidbody DragRigidbody;

        private int[] _triangles;
        private Vector3[] _vertices;
        private Vector3[] Normals;

        private float[] Area;
        private Vector3[] WorldTriangleCenter;
        private Vector3[] WorldTriangleNormal;
        private Vector3[] MeshTriangleVelocities;
        private Vector3[] LastTrianglePosition;
        private float[] Angle;
        private float[] ForceMagnitude;
        private Vector3[] Force;

        void Awake()
        {
            DragMesh = GetComponent<MeshFilter>().mesh;
            //DragRigidbody = GetComponent<Rigidbody>();

            _triangles = DragMesh.triangles;
            _vertices = DragMesh.vertices;
            Normals = DragMesh.normals;

            Area = new float[_triangles.Length / 3];
            WorldTriangleCenter = new Vector3[_triangles.Length / 3];
            WorldTriangleNormal = new Vector3[_triangles.Length / 3];
            MeshTriangleVelocities = new Vector3[_triangles.Length / 3];
            LastTrianglePosition = new Vector3[_triangles.Length / 3];
            Angle = new float[_triangles.Length / 3];
            ForceMagnitude = new float[_triangles.Length / 3];
            Force = new Vector3[_triangles.Length / 3];

            for (int triangle = 0; triangle < _triangles.Length; triangle += 3)
            {
                LastTrianglePosition[triangle / 3] = (transform.TransformPoint(_vertices[_triangles[triangle]]) +
                                                      transform.TransformPoint(_vertices[_triangles[triangle + 1]]) +
                                                      transform.TransformPoint(_vertices[_triangles[triangle + 2]])) /
                                                     3;

                Vector3 corner = transform.TransformPoint(_vertices[_triangles[triangle]]);
                Vector3 a = transform.TransformPoint(_vertices[_triangles[triangle + 1]]) - corner;
                Vector3 b = transform.TransformPoint(_vertices[_triangles[triangle + 2]]) - corner;

                int i = triangle / 3;

                Area[i] = Vector3.Cross(a, b).magnitude / 2;
            }
        }

        void FixedUpdate()
        {
            //Force at specific triangle selected in loop
            for (int triangle = 0; triangle < _triangles.Length; triangle += 3)
            {
                int i = triangle / 3;

                WorldTriangleCenter[i] = (transform.TransformPoint(_vertices[_triangles[triangle]]) +
                                          transform.TransformPoint(_vertices[_triangles[triangle + 1]]) +
                                          transform.TransformPoint(_vertices[_triangles[triangle + 2]])) /
                                         3;

                WorldTriangleNormal[i] = (transform.TransformDirection(Normals[_triangles[triangle]]) +
                                          transform.TransformDirection(Normals[_triangles[triangle + 1]]) +
                                          transform.TransformDirection(Normals[_triangles[triangle + 2]])) /
                                         3;

                MeshTriangleVelocities[i] = (WorldTriangleCenter[i] - LastTrianglePosition[i]) / Time.fixedDeltaTime;

                LastTrianglePosition[i] = WorldTriangleCenter[i];

                Angle[i] = Vector3.Angle(MeshTriangleVelocities[i] + World.Wind, WorldTriangleNormal[i]);

                ForceMagnitude[i] = (float)(0.5 * World.AirDensity * -Mathf.Pow(MeshTriangleVelocities[i].magnitude + World.Wind.magnitude, 2) * Mathf.Clamp(Mathf.Cos(Angle[i] * Mathf.Deg2Rad), 0, 1) * Area[i]);

                Force[i] = WorldTriangleNormal[i] * ForceMagnitude[i];

                DragRigidbody.AddForceAtPosition(Force[i], WorldTriangleCenter[i]);

                if (World.Debug) DebugDrag(i);
            }
        }

        protected void DebugDrag(int i)
        {
            //Debug.DrawLine(WorldTriangleCenter[td3], WorldTriangleCenter[td3] + WorldTriangleNormal[td3]);

            Debug.DrawLine(WorldTriangleCenter[i], WorldTriangleCenter[i] + MeshTriangleVelocities[i].normalized, ForceMagnitude[i] != 0 ? Color.green : Color.red);

            Debug.DrawLine(WorldTriangleCenter[i], WorldTriangleCenter[i] + Force[i], Color.green);
        }
    }
}