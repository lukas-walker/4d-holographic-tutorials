using UnityEngine;
using System.Collections.Generic;

namespace Tutorials.ResearchMode
{
    /// <summary>
    /// Keeps state of rendered objects
    /// Inspired by: https://github.com/petergu684/HoloLens2-ResearchMode-Unity 
    /// </summary>
    public class PointCloudRenderer : MonoBehaviour
    {
        public int maxChunkSize = 65535;
        public float pointSize = 0.005f;
        public GameObject pointCloudElem;
        public Material pointCloudMaterial;

        public List<GameObject> elems;

        void Start()
        {
            Debug.Log("Starting");
            elems = new List<GameObject>();
            UpdatePointSize();
        }

        void Update()
        {
            if (transform.hasChanged)
            {
                UpdatePointSize();
                transform.hasChanged = false;
            }
        }

        public void UpdatePointSize()
        {
            pointCloudMaterial.SetFloat("_PointSize", pointSize * transform.localScale.x);
        }

        public void Render(Vector3[] arrVertices, Color pointColor)
        {
            Debug.Log("rendering points!");
            int nPoints, nChunks;
            if (arrVertices == null)
            {
                nPoints = 0;
                nChunks = 0;
            }
            else
            {
                nPoints = arrVertices.Length;
                nChunks = 1 + nPoints / maxChunkSize;
            }

            if (elems.Count < nChunks)
                AddElems(nChunks - elems.Count);
            if (elems.Count > nChunks)
                RemoveElems(elems.Count - nChunks);

            int offset = 0;
            for (int i = 0; i < nChunks; i++)
            {
                int nPointsToRender = System.Math.Min(maxChunkSize, nPoints - offset);

                ElemRenderer renderer = elems[i].GetComponent<ElemRenderer>();
                renderer.UpdateMesh(arrVertices, nPointsToRender, offset, pointColor);

                offset += nPointsToRender;
            }
        }

        void AddElems(int nElems)
        {
            for (int i = 0; i < nElems; i++)
            {
                GameObject newElem = GameObject.Instantiate(pointCloudElem);
                newElem.transform.parent = transform;
                newElem.transform.localPosition = new Vector3(0.0f, 0.0f, 0.0f);
                newElem.transform.localRotation = Quaternion.identity;
                newElem.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

                elems.Add(newElem);
            }
        }

        void RemoveElems(int nElems)
        {
            for (int i = 0; i < nElems; i++)
            {
                Destroy(elems[0]);
                elems.Remove(elems[0]);
            }
        }

        public Vector3[] GetAllMeshPoints()
        {
            Vector3[] allPoints = new Vector3[GetNumberOfMeshPoints()];

            int counter = 0;
            for (int i = 0; i < elems.Count; i++)
            {
                ElemRenderer renderer = elems[i].GetComponent<ElemRenderer>();
                for (int j = 0; j < renderer.mesh.vertices.Length; j++)
                {
                    allPoints[counter] = renderer.mesh.vertices[j];
                    counter++;
                }
                
            }

            return allPoints;
        }


        public int GetNumberOfMeshPoints()
        {
            int totalPoints = 0;
            for (int i = 0; i < elems.Count; i++)
            {
                ElemRenderer renderer = elems[i].GetComponent<ElemRenderer>();
                totalPoints += renderer.mesh.vertices.Length;
            }

            return totalPoints;
        }

        public Mesh GetPointCloudMesh()
        {
            // TODO: Fix whatever this index is so I know which mesh to return
            ElemRenderer renderer = elems[0].GetComponent<ElemRenderer>();
            return renderer.mesh;

        }
    }
}