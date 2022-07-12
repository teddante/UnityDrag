using UnityEngine;
using Vector3 = UnityEngine.Vector3;

public class Drag : MonoBehaviour
{
    public Vector3 Wind;

    public Mesh Mesh;
    public Rigidbody Rigidbody;

    public int[] Triangles;
    public Vector3[] Vertices;
    public Vector3[] Normals;

    public float[] Area;
    public Vector3[] GlobalCenter;
    public Vector3[] FaceNormal;
    public Vector3[] MeshTriangleVelocities;
    public Vector3[] LastTrianglePosition;
    public float[] Angle;
    public float[] ForceMagnitude;
    public Vector3[] Force;

    // Start is called before the first frame update
    void Start()
    {
        Mesh = GetComponent<MeshFilter>().mesh;
        Rigidbody = GetComponent<Rigidbody>();

        Triangles = Mesh.triangles;
        Vertices = Mesh.vertices;
        Normals = Mesh.normals;

        Area = new float[Triangles.Length / 3];
        GlobalCenter = new Vector3[Triangles.Length / 3];
        FaceNormal = new Vector3[Triangles.Length / 3];
        MeshTriangleVelocities = new Vector3[Triangles.Length / 3];
        LastTrianglePosition = new Vector3[Triangles.Length / 3];
        Angle = new float[Triangles.Length / 3];
        ForceMagnitude = new float[Triangles.Length / 3];
        Force = new Vector3[Triangles.Length / 3];

        for (int triangle = 0; triangle < Triangles.Length; triangle += 3)
        {
            LastTrianglePosition[triangle / 3] = (transform.TransformPoint(Vertices[Triangles[triangle]]) +
                                                  transform.TransformPoint(Vertices[Triangles[triangle + 1]]) +
                                                  transform.TransformPoint(Vertices[Triangles[triangle + 2]])) /
                                                 3;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Triangles = Mesh.triangles;
        Vertices = Mesh.vertices;
        Normals = Mesh.normals;

        //Force at specific triangle selected in loop
        for (int triangle = 0; triangle < Triangles.Length; triangle += 3)
        {
            Vector3 corner = transform.TransformPoint(Vertices[Triangles[triangle]]);
            Vector3 a = transform.TransformPoint(Vertices[Triangles[triangle + 1]]) - corner;
            Vector3 b = transform.TransformPoint(Vertices[Triangles[triangle + 2]]) - corner;

            int i = triangle / 3;

            Area[i] = Vector3.Cross(a, b).magnitude / 2;

            GlobalCenter[i] = (transform.TransformPoint(Vertices[Triangles[triangle]]) +
                               transform.TransformPoint(Vertices[Triangles[triangle + 1]]) +
                               transform.TransformPoint(Vertices[Triangles[triangle + 2]])) /
                              3;

            FaceNormal[i] = (transform.TransformDirection(Normals[Triangles[triangle]]) +
                             transform.TransformDirection(Normals[Triangles[triangle + 1]]) +
                             transform.TransformDirection(Normals[Triangles[triangle + 2]])) /
                            3;

            MeshTriangleVelocities[i] = (GlobalCenter[i] - LastTrianglePosition[i]) / Time.deltaTime;

            LastTrianglePosition[i] = GlobalCenter[i];

            Angle[i] = Vector3.Angle(MeshTriangleVelocities[i] + Wind, FaceNormal[i]);

            ForceMagnitude[i] = (float)(0.5 * 1.2 * -Mathf.Pow(MeshTriangleVelocities[i].magnitude + Wind.magnitude, 2) * Mathf.Clamp(Mathf.Cos(Angle[i] * Mathf.Deg2Rad), 0, 1) * Area[i]);

            Force[i] = FaceNormal[i] * ForceMagnitude[i];

            Rigidbody.AddForceAtPosition(Force[i], GlobalCenter[i]);

            //Debug.DrawLine(GlobalCenter[td3], GlobalCenter[td3] + FaceNormal[td3]);

            //if (ForceMagnitude[i] != 0)
            //{
            //    Debug.DrawLine(GlobalCenter[i], GlobalCenter[i] + MeshTriangleVelocities[i].normalized, Color.green);
            //}
            //else
            //{
            //    Debug.DrawLine(GlobalCenter[i], GlobalCenter[i] + MeshTriangleVelocities[i].normalized, Color.red);
            //}

            //Debug.DrawLine(GlobalCenter[i], GlobalCenter[i] + Force[i], Color.green);
        }
    }
}