using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Tutorials.ResearchMode;


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
        }
        
        public void TogglePointCloudRendering() {
            _renderPointCloud = !_renderPointCloud;
            pointCloudRendererGo.SetActive(_renderPointCloud);
        }
    }
}