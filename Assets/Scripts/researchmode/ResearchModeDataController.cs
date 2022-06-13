using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using System;
using System.Runtime.InteropServices;
using Tutorials.ResearchMode;
using TMPro;
using Tutorials;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;


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
        private bool isRecordingPoints;
        public GameObject boundingBox;
        GameObject convexHullObject;

        public GameObject existingObject;

        public ObjectManager objectManager;
        bool shouldUpdate = true;
        

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

            isRecordingPoints = false;
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
            if (shouldUpdate)
            {
                pointCloudRenderer.Render(data, pointColor);
            }
        }

        public void TogglePointCloudRendering() {
            _renderPointCloud = !_renderPointCloud;
            pointCloudRendererGo.SetActive(_renderPointCloud);
        }

        public void ToggleBoundingBox()
        {
            boundingBox.transform.SetPositionAndRotation(
                objectManager.objectManagerPanel.transform.position + new Vector3(.0f, .1f, .0f),
                objectManager.objectManagerPanel.transform.rotation * Quaternion.Euler(-45, 0, 0));
            boundingBox.transform.localScale = new Vector3(0.025f, 0.025f, 0.025f);
            boundingBox.SetActive(!boundingBox.activeSelf);
        }

        public void TogglePointCloudCapture()
        {
            if (!isRecordingPoints)
            {
                // Not currently recording so start
                pointCloudPoints.Clear();
                CaptureBoundingBoxPointCloud();
                ConvertPointsToMesh();
            }
            else
            {
                // Remove old object
                existingObject.SetActive(false);
                pointCloudPoints.Clear();
            }
            isRecordingPoints = !isRecordingPoints;
        }

        /// <summary>
        /// Record the point cloud for the current bounding box to be displayed later
        /// </summary>
        public void CaptureBoundingBoxPointCloud()
        {
            Collider boundingCollider = boundingBox.GetComponent<Collider>();

            List<Vector3> containingElements = new List<Vector3>();

            Mesh mesh = pointCloudRenderer.GetPointCloudMesh();

            foreach (Vector3 vec in mesh.vertices)
            {
                if (boundingCollider.bounds.Contains(vec))
                {
                    // Grab points inside the bounding box
                    containingElements.Add(new Vector3(vec.x, vec.y, vec.z));
                }
            }

            pointCloudPoints.AddRange(containingElements);
            
        }

        public void ConvertPointsToMesh()
        {
            int numNewPoints = pointCloudPoints.Count;

            text.text = numNewPoints.ToString();

            int[] indices = new int[numNewPoints];
            Color[] colors = new Color[numNewPoints];

            for (int i = 0; i < numNewPoints; i++)
            {
                indices[i] = i;
                colors[i] = Color.green;
            }


            Mesh newMesh = new Mesh();
            // Calcualte new mesh vertices from a convex hull
            var calc = new ConvexHullCalculator();
            var verts = new List<Vector3>();
            var tris = new List<int>();
            var normals = new List<Vector3>();
            
            if (convexHullObject != null)
            {
                // Remove the old object from the scene (even if we aren't able to replace it)
                Destroy(convexHullObject);
            }

            try
            {
                calc.GenerateHull(pointCloudPoints, false, ref verts, ref tris, ref normals);
            } catch (Exception e)
            {
                Debug.Log("Error capturing points to use for the mesh. Are you capturing more than 4 co-planar points?");
                return;
            }

            
            newMesh.SetVertices(verts);
            newMesh.SetTriangles(tris, 0);
            newMesh.SetNormals(normals);
            newMesh.RecalculateBounds();

            existingObject.AddComponent<MeshFilter>();
            var mr2 = existingObject.AddComponent<MeshRenderer>();
            
            // Give object correct scale
            existingObject.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
           
            // Apply mesh to object
            existingObject.GetComponent<MeshFilter>().mesh = newMesh;
            
            // Move object to location it was captured
            existingObject.transform.position = boundingBox.transform.position;

            // Activate and display object
            existingObject.SetActive(true);
            objectManager.AddSpawnedObject(existingObject);
            shouldUpdate = false;

        }


    }
}

