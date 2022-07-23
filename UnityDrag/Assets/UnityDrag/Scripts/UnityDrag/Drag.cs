using UnityEngine;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.UnityDrag
{
    public class Drag : MonoBehaviour
    {
        public Mesh DragMesh;
        public Rigidbody DragRigidbody;

        private int[] _triangles;
        private Vector3[] _vertices;
        private Vector3[] _normals;

        private float[] _area;
        private Vector3[] _worldTriangleCenter;
        private Vector3[] _worldTriangleNormal;
        private Vector3[] _meshTriangleVelocities;
        private Vector3[] _lastTrianglePosition;
        private float[] _angle;
        private float[] _forceMagnitude;
        private Vector3[] _force;

        void Awake()
        {
            if (DragMesh == null)
            {
                DragMesh = GetComponent<MeshFilter>().mesh;
            }

            //DragRigidbody = GetComponent<Rigidbody>();

            _triangles = DragMesh.triangles;
            _vertices = DragMesh.vertices;
            _normals = DragMesh.normals;

            _area = new float[_triangles.Length / 3];
            _worldTriangleCenter = new Vector3[_triangles.Length / 3];
            _worldTriangleNormal = new Vector3[_triangles.Length / 3];
            _meshTriangleVelocities = new Vector3[_triangles.Length / 3];
            _lastTrianglePosition = new Vector3[_triangles.Length / 3];
            _angle = new float[_triangles.Length / 3];
            _forceMagnitude = new float[_triangles.Length / 3];
            _force = new Vector3[_triangles.Length / 3];

            for (int triangle = 0; triangle < _triangles.Length; triangle += 3)
            {
                _lastTrianglePosition[triangle / 3] = (transform.TransformPoint(_vertices[_triangles[triangle]]) +
                                                       transform.TransformPoint(_vertices[_triangles[triangle + 1]]) +
                                                       transform.TransformPoint(_vertices[_triangles[triangle + 2]])) /
                                                      3;

                Vector3 corner = transform.TransformPoint(_vertices[_triangles[triangle]]);
                Vector3 a = transform.TransformPoint(_vertices[_triangles[triangle + 1]]) - corner;
                Vector3 b = transform.TransformPoint(_vertices[_triangles[triangle + 2]]) - corner;

                int i = triangle / 3;

                _area[i] = Vector3.Cross(a, b).magnitude / 2;
            }
        }

        void FixedUpdate()
        {
            //Force at specific triangle selected in loop
            for (int triangle = 0; triangle < _triangles.Length; triangle += 3)
            {
                int i = triangle / 3;

                _worldTriangleCenter[i] = (transform.TransformPoint(_vertices[_triangles[triangle]]) +
                                           transform.TransformPoint(_vertices[_triangles[triangle + 1]]) +
                                           transform.TransformPoint(_vertices[_triangles[triangle + 2]])) /
                                          3;

                _worldTriangleNormal[i] = (transform.TransformDirection(_normals[_triangles[triangle]]) +
                                           transform.TransformDirection(_normals[_triangles[triangle + 1]]) +
                                           transform.TransformDirection(_normals[_triangles[triangle + 2]])) /
                                          3;

                _meshTriangleVelocities[i] = (_worldTriangleCenter[i] - _lastTrianglePosition[i]) / Time.fixedDeltaTime;

                _lastTrianglePosition[i] = _worldTriangleCenter[i];

                _angle[i] = Vector3.Angle(_meshTriangleVelocities[i] + World.Wind, _worldTriangleNormal[i]);

                _forceMagnitude[i] = (float)(0.5 * World.AirDensity * -Mathf.Pow(_meshTriangleVelocities[i].magnitude + World.Wind.magnitude, 2) * Mathf.Clamp(Mathf.Cos(_angle[i] * Mathf.Deg2Rad), 0, 1) * _area[i]);

                _force[i] = _worldTriangleNormal[i] * _forceMagnitude[i];

                DragRigidbody.AddForceAtPosition(_force[i], _worldTriangleCenter[i]);

                if (World.Debug) DebugDrag(i);
            }
        }

        private void DebugDrag(int i)
        {
            //Debug.DrawLine(WorldTriangleCenter[td3], WorldTriangleCenter[td3] + WorldTriangleNormal[td3]);

            Debug.DrawLine(_worldTriangleCenter[i], _worldTriangleCenter[i] + _meshTriangleVelocities[i].normalized, _forceMagnitude[i] != 0 ? Color.green : Color.red);

            Debug.DrawLine(_worldTriangleCenter[i], _worldTriangleCenter[i] + _force[i], Color.green);
        }
    }
}