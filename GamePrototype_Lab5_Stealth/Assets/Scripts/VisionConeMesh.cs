using UnityEngine;

namespace Lab5Stealth
{
    [RequireComponent(typeof(MeshFilter))]
    public class VisionConeMesh : MonoBehaviour
    {
        public EnemyVision vision;
        public int segments = 36;

        private MeshFilter meshFilter;
        private Mesh mesh;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            mesh = new Mesh { name = "Runtime Vision Cone" };
            meshFilter.sharedMesh = mesh;
        }

        private void LateUpdate()
        {
            if (vision == null)
            {
                return;
            }

            int vertexCount = segments + 2;
            Vector3[] vertices = new Vector3[vertexCount];
            int[] triangles = new int[segments * 3];

            vertices[0] = Vector3.zero;
            float halfAngle = vision.viewAngle * 0.5f;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                float angle = Mathf.Lerp(-halfAngle, halfAngle, t);
                vertices[i + 1] = Quaternion.Euler(0f, angle, 0f) * Vector3.forward * vision.viewRadius;
            }

            for (int i = 0; i < segments; i++)
            {
                int index = i * 3;
                triangles[index] = 0;
                triangles[index + 1] = i + 1;
                triangles[index + 2] = i + 2;
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }
    }
}
