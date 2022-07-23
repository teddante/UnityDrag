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

        private Vector3[] _fixedUpdateWorldTriangleCenter;
        private Vector3[] _fixedUpdateWorldTriangleNormal;
        private Vector3[] _fixedUpdateMeshTriangleVelocities;
        private Vector3[] _fixedUpdateLastTrianglePosition;
        private float[] _fixedUpdateAngle;
        private float[] _fixedUpdateForceMagnitude;
        private Vector3[] _fixedUpdateForce;

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

            _fixedUpdateWorldTriangleCenter = new Vector3[_triangles.Length / 3];
            _fixedUpdateWorldTriangleNormal = new Vector3[_triangles.Length / 3];
            _fixedUpdateMeshTriangleVelocities = new Vector3[_triangles.Length / 3];
            _fixedUpdateLastTrianglePosition = new Vector3[_triangles.Length / 3];
            _fixedUpdateAngle = new float[_triangles.Length / 3];
            _fixedUpdateForceMagnitude = new float[_triangles.Length / 3];
            _fixedUpdateForce = new Vector3[_triangles.Length / 3];

            for (int triangle = 0; triangle < _triangles.Length; triangle += 3)
            {
                _fixedUpdateLastTrianglePosition[triangle / 3] = (transform.TransformPoint(_vertices[_triangles[triangle]]) +
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

        void Update()
        {
        }

        void FixedUpdate()
        {
            //Force at specific triangle selected in loop
            for (int triangle = 0; triangle < _triangles.Length; triangle += 3)
            {
                int i = triangle / 3;

                _fixedUpdateWorldTriangleCenter[i] = (transform.TransformPoint(_vertices[_triangles[triangle]]) +
                                                      transform.TransformPoint(_vertices[_triangles[triangle + 1]]) +
                                                      transform.TransformPoint(_vertices[_triangles[triangle + 2]])) /
                                                     3;

                _fixedUpdateWorldTriangleNormal[i] = (transform.TransformDirection(_normals[_triangles[triangle]]) +
                                                      transform.TransformDirection(_normals[_triangles[triangle + 1]]) +
                                                      transform.TransformDirection(_normals[_triangles[triangle + 2]])) /
                                                     3;

                _fixedUpdateMeshTriangleVelocities[i] = (_fixedUpdateWorldTriangleCenter[i] - _fixedUpdateLastTrianglePosition[i]) / Time.fixedDeltaTime;

                _fixedUpdateLastTrianglePosition[i] = _fixedUpdateWorldTriangleCenter[i];

                _fixedUpdateAngle[i] = Vector3.Angle(_fixedUpdateMeshTriangleVelocities[i] + World.Wind, _fixedUpdateWorldTriangleNormal[i]);

                _fixedUpdateForceMagnitude[i] = (float)(0.5 * World.AirDensity * -Mathf.Pow(_fixedUpdateMeshTriangleVelocities[i].magnitude + World.Wind.magnitude, 2) * Mathf.Clamp(Mathf.Cos(_fixedUpdateAngle[i] * Mathf.Deg2Rad), 0, 1) * _area[i]);

                _fixedUpdateForce[i] = _fixedUpdateWorldTriangleNormal[i] * _fixedUpdateForceMagnitude[i];

                DragRigidbody.AddForceAtPosition(_fixedUpdateForce[i], _fixedUpdateWorldTriangleCenter[i]);

                if (World.Debug) DebugDrag(i);
            }
        }

        private void DebugDrag(int i)
        {
            //Debug.DrawLine(WorldTriangleCenter[td3], WorldTriangleCenter[td3] + WorldTriangleNormal[td3]);

            Debug.DrawLine(_fixedUpdateWorldTriangleCenter[i], _fixedUpdateWorldTriangleCenter[i] + _fixedUpdateMeshTriangleVelocities[i].normalized, _fixedUpdateForceMagnitude[i] != 0 ? Color.green : Color.red);

            Debug.DrawLine(_fixedUpdateWorldTriangleCenter[i], _fixedUpdateWorldTriangleCenter[i] + _fixedUpdateForce[i], Color.green);
        }
    }
}