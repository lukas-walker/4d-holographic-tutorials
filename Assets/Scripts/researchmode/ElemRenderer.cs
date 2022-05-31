using UnityEngine;
using System.Collections;
using System.Linq;

namespace Tutorials.ResearchMode
{
    /// <summary>
    /// Copies vector3d to unity mesh
    /// Inspired by: https://github.com/petergu684/HoloLens2-ResearchMode-Unity
    /// </summary>
    public class ElemRenderer : MonoBehaviour
    {
        public Mesh mesh;
        
        public void UpdateMesh(Vector3[] arrVertices, int nPointsToRender, int nPointsRendered, Color pointColor)
        {
            int nPoints;

            if (arrVertices == null)
                nPoints = 0;
            else
                nPoints = System.Math.Min(nPointsToRender, arrVertices.Length - nPointsRendered);
            nPoints = System.Math.Min(nPoints, 65535);

            Vector3[] points = arrVertices.Skip(nPointsRendered).Take(nPoints).ToArray();
            int[] indices = new int[nPoints];
            Color[] colors = new Color[nPoints];

            for (int i = 0; i < nPoints; i++)
            {
                indices[i] = i;
                colors[i] = pointColor;
            }

            if (mesh != null)
                Destroy(mesh);
            mesh = new Mesh();
            mesh.vertices = points;
            mesh.colors = colors;
            mesh.SetIndices(indices, MeshTopology.Points, 0);
            GetComponent<MeshFilter>().mesh = mesh;
        }
    }
}