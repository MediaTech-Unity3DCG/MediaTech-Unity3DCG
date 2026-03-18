using UnityEngine;

namespace Exercise2
{
    [RequireComponent(typeof(MeshFilter))]
    public class ComputeVolume : MonoBehaviour
    {
        MeshFilter filt;
        // Start is called before the first frame update
        void Start()
        {
            filt = this.GetComponent<MeshFilter>();

            var text = new GameObject();
            text.GetComponent<Transform>().parent = this.transform;
            var textMesh = text.gameObject.AddComponent<TextMesh>();
            textMesh.GetComponent<Transform>().position = new Vector3(this.GetComponent<Transform>().position.x - 1f, 1.5f, 0);
            textMesh.text = this.gameObject.name + ": " + Volume().ToString("0.000");
            textMesh.color = Color.black;
            textMesh.fontSize = 32;
            textMesh.characterSize = 0.08f;
        }

        float Volume()
        {
            var mesh = filt.mesh;
            var sum = 0f;
            for (var i = 0; i < mesh.triangles.Length / 3; i++)
            {
                var v1 = mesh.vertices[mesh.triangles[i * 3]];
                var v2 = mesh.vertices[mesh.triangles[i * 3 + 1]];
                var v3 = mesh.vertices[mesh.triangles[i * 3 + 2]];

                sum += Vector3.Dot(Vector3.Cross(v1, v2), v3);
            }
            var s = GetComponent<Transform>().localScale;
            return sum / 6 * Mathf.Abs(s.x * s.y * s.z);
        }
    } // class
} // namespace