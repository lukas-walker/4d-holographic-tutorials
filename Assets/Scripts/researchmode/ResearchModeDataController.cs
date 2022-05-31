using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Tutorials.ResearchMode;
using TMPro;


#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
#endif

// #define ENABLE_WINMD_SUPPORT

namespace Tutorials.ResearchMode
{
    /// <summary>Controller for research mode, does sensor access management and provides event
    /// handler for point cloud updates </summary>
    public class ResearchModeDataController : MonoBehaviour
    {
#if ENABLE_WINMD_SUPPORT
        HL2ResearchMode researchMode;
#endif
        public enum DepthSensorMode
        {
            ShortThrow,
            LongThrow,
            None
        };

        [SerializeField] DepthSensorMode depthSensorMode = DepthSensorMode.ShortThrow;
        
        [SerializeField] bool enablePointCloud = true;

        RemoteConnection _remoteConnection;
        private long _pingCtr = 0;

        public GameObject pointCloudRendererGo;
        public Color pointColor = Color.white;
        private PointCloudRenderer pointCloudRenderer;
        bool _renderPointCloud = false;

        public TextMeshPro text;

        List<Vector3> pointCloudPoints;
        private bool isRecordingPoints = false;
        public GameObject boundingBox;
        

#if ENABLE_WINMD_SUPPORT
        Windows.Perception.Spatial.SpatialCoordinateSystem unityWorldOrigin;
#endif
        private void Awake()
        {
            
#if ENABLE_WINMD_SUPPORT
            // XXX: We use WindowsMREnvironment.OriginSpatialCoordinateSystem not WorldManager.GetNativeISpatialCoordinateSystemPtr()
            // to fix positional transformation issues
            IntPtr WorldOriginPtr = UnityEngine.XR.WindowsMR.WindowsMREnvironment.OriginSpatialCoordinateSystem;
            unityWorldOrigin = Marshal.GetObjectForIUnknown(WorldOriginPtr) as Windows.Perception.Spatial.SpatialCoordinateSystem;
#endif
        }

        void Start()
        {
            if (pointCloudRendererGo != null)
            {
                pointCloudRenderer = pointCloudRendererGo.GetComponent<PointCloudRenderer>();
                pointCloudRendererGo.SetActive(_renderPointCloud);
            }

            _remoteConnection = GetComponent<RemoteConnection>();
            InitResearchMode();

            pointCloudPoints = new List<Vector3>();
        }


        void LateUpdate()
        {
            SendPing();
            UpdatePointCloud();
        }

        private void InitResearchMode()
        {
#if ENABLE_WINMD_SUPPORT
            if (depthSensorMode == DepthSensorMode.None)
            {
                return;
            }
            researchMode = new HL2ResearchMode();
            if (depthSensorMode == DepthSensorMode.LongThrow)
            {
                researchMode.InitializeLongDepthSensor();
            }
            else if (depthSensorMode == DepthSensorMode.ShortThrow)
            {
                researchMode.InitializeDepthSensor();
            }

            researchMode.InitializeSpatialCamerasFront();
            researchMode.SetReferenceCoordinateSystem(unityWorldOrigin);
            researchMode.SetPointCloudDepthOffset(0);

            if (depthSensorMode == DepthSensorMode.LongThrow)
            {
                researchMode.StartLongDepthSensorLoop(enablePointCloud);
            }
            else if (depthSensorMode == DepthSensorMode.ShortThrow)
            {
                researchMode.StartDepthSensorLoop(enablePointCloud);
            }
            researchMode.StartSpatialCamerasFrontLoop();
#endif
        }
        
        private void UpdatePointCloud()
        {
 #if ENABLE_WINMD_SUPPORT
            if (enablePointCloud && _renderPointCloud)
            {
                if ((depthSensorMode == DepthSensorMode.LongThrow && !researchMode.LongThrowPointCloudUpdated()) ||
                    (depthSensorMode == DepthSensorMode.ShortThrow && !researchMode.PointCloudUpdated()))
                {
                    // no new data
                    return;
                }

                float[] pointCloud = new float[] { };
                if (depthSensorMode == DepthSensorMode.LongThrow)
                {
                    pointCloud = researchMode.GetLongThrowPointCloudBuffer();
                }
                else if (depthSensorMode == DepthSensorMode.ShortThrow)
                {
                    pointCloud = researchMode.GetPointCloudBuffer();
                }
                if (pointCloud.Length > 0)
                {
                    int pointCloudLength = pointCloud.Length / 3;
                    Vector3[] pointCloudVector3 = new Vector3[pointCloudLength];
                    for (int i = 0; i < pointCloudLength; i++)
                    {
                        pointCloudVector3[i] =
                            new Vector3(pointCloud[3 * i], pointCloud[3 * i + 1], pointCloud[3 * i + 2]);
                    }
                    OnNewPointCloudData(pointCloudVector3);
                }
            }
#endif
        }
        
        private void SendPing()
        {
#if ENABLE_WINMD_SUPPORT
            if ((_pingCtr++) % 1000 == 0 && _remoteConnection != null)
            {
                _remoteConnection.sendPing();
            }
#endif
        }
        
        protected void OnNewPointCloudData(Vector3[] data)
        {
            pointCloudRenderer.Render(data, pointColor);
            // TODO: HERE DO THE UPDATE TO THE CAPTURED POINTS IN THE BOUNDING BOX IF WE 
            //       ARE CURRENTLY CAPTURING
            if (isRecordingPoints)
            {
                CaptureBoundingBoxPointCloud();
            }
        }

        public void TogglePointCloudRendering() {
            _renderPointCloud = !_renderPointCloud;
            pointCloudRendererGo.SetActive(_renderPointCloud);
            
            // TODO: Remove. This is manually adding a few points for testing
            Vector3[] datatest = new Vector3[5];
            datatest[0] = new Vector3(0.0f, 0.0f, -0.01f);
            datatest[1] = new Vector3(0.01f, 0.0f, 0.0f);
            datatest[2] = new Vector3(0.0f, 0.01f, 0.0f);
            datatest[3] = new Vector3(0.01f, 0.01f, 0.01f);
            datatest[4] = new Vector3(0.02f, 0.02f, 0.0f);
            pointCloudRenderer.Render(datatest, pointColor);
        }

        public void TogglePointCloudCapture()
        {
            CaptureBoundingBoxPointCloud();
            ConvertPointsToMesh();
            if (!isRecordingPoints)
            {
                // Not currently recording so start

                // TODO: Need to remove old mesh
                Debug.Log("need to remove old mesh");
                pointCloudPoints.Clear();
                CaptureBoundingBoxPointCloud();
            }
            else
            {
                ConvertPointsToMesh();
                Debug.Log("Creating mesh");
            }
            isRecordingPoints = !isRecordingPoints;
        }

        /// <summary>
        /// Record the point cloud for the current bounding box to be displayed later
        /// </summary>
        public void CaptureBoundingBoxPointCloud()
        {
            Debug.Log("Capturing points in a loop");
            Collider boundingCollider = boundingBox.GetComponent<Collider>();

            List<Vector3> containingElements = new List<Vector3>();

            // Checking point by point might be bad....
            Mesh mesh = pointCloudRenderer.GetPointCloudMesh();

            foreach (Vector3 vec in mesh.vertices)
            {
                //Debug.Log(vec.ToString());
                if (boundingCollider.bounds.Contains(vec))// && !pointCloudPoints.Contains(vec))
                {

                    // TODO: CHECK THAT THE POINT IS NOT ALREADY IN THE lIST IS ACTUALLY WORKING

                    containingElements.Add(new Vector3(vec.x, vec.y, vec.z));
                }
            }

            pointCloudPoints.AddRange(containingElements);
            
        }

        public void ConvertPointsToMesh()
        {
            Debug.Log("Creating mesh");
            int numNewPoints = pointCloudPoints.Count;
            Debug.Log("Num points: " + numNewPoints.ToString());
            Debug.Log(pointCloudPoints);

            foreach (Vector3 elem in pointCloudPoints)
            {
                Debug.Log(elem.x.ToString() + " " + elem.y.ToString() + " " + elem.z.ToString());
            }

            text.text = numNewPoints.ToString();

            int[] indices = new int[numNewPoints];
            Color[] colors = new Color[numNewPoints];

            for (int i = 0; i < numNewPoints; i++)
            {
                indices[i] = i;
                colors[i] = Color.blue;
            }

            Mesh newMesh = new Mesh();
            // Create a vertice copy and convert to array
            //newMesh.vertices = (new List<Vector3>(containingElements)).ToArray();
            //newMesh.colors = colors;
            //newMesh.SetIndices(indices, MeshTopology.Points, 0);
            //newMesh.triangles = new int[] { 0, 1, 2 };

            var calc = new ConvexHullCalculator();
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var normals = new List<Vector3>();
            calc.GenerateHull(pointCloudPoints, false, ref verts, ref tris, ref normals);


            newMesh.SetVertices(verts);
            newMesh.SetTriangles(tris, 0);
            newMesh.SetNormals(normals);

            GameObject newObject = new GameObject("Empty");
            newObject.AddComponent<MeshFilter>();
            var mr = newObject.AddComponent<MeshRenderer>();
            Shader shades = Shader.Find("Standard");
            mr.material.shader = shades;
            //mr.material = Resources.Load<Material>("defaultObjectMat");
            mr.material.color = Color.white;
            newObject.GetComponent<MeshFilter>().mesh = newMesh;

            newObject.SetActive(true);
        }


        /*public Mesh BuildSimplifiedConvexMesh(Mesh mesh)
        {
            Debug.Log(mesh.triangles.Length / 3 + " tris");

            SplitMeshBuilder builder = new SplitMeshBuilder();

            for (int i = 0; i < 64; i++)
            {
                int index = Random.Range(0, mesh.triangles.Length / 3) * 3;

                Vector3[] triangle = new Vector3[] { mesh.vertices[mesh.triangles[index]], mesh.vertices[mesh.triangles[index + 1]], mesh.vertices[mesh.triangles[index + 2]] };
                Vector2[] uvs = new Vector2[] { mesh.uv[mesh.triangles[index]], mesh.uv[mesh.triangles[index + 1]], mesh.uv[mesh.triangles[index + 2]] };

                builder.AddTriangleToMesh(triangle, uvs);
            }

            Mesh polygonSoup = builder.Build();
            Debug.Log(polygonSoup.triangles.Length / 3 + " tris");

            return polygonSoup;
        }*/

    }
}