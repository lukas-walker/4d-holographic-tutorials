using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using Tutorials.ResearchMode;


#if ENABLE_WINMD_SUPPORT
using HL2UnityPlugin;
#endif


namespace Tutorials.ResearchMode
{
    public class ResearchModeDataController : MonoBehaviour
    {
#if ENABLE_WINMD_SUPPORT
        HL2ResearchMode researchMode;
#endif

        enum DepthSensorMode
        {
            ShortThrow,
            LongThrow,
            None
        };

        [SerializeField] DepthSensorMode depthSensorMode = DepthSensorMode.ShortThrow;
        
        [SerializeField] bool enablePointCloud = true;

        TCPClient tcpClient;

        public GameObject pointCloudRendererGo;
        public Color pointColor = Color.white;
        private PointCloudRenderer pointCloudRenderer;

        bool startRealtimePreview = true;
        long pingCtr = 0;

#if ENABLE_WINMD_SUPPORT
        Windows.Perception.Spatial.SpatialCoordinateSystem unityWorldOrigin;
#endif
        
        #region Button Event Functions
        bool renderPointCloud = true;
        public void TogglePointCloudEvent()
        {
            renderPointCloud = !renderPointCloud;
            if (renderPointCloud)
            {
                pointCloudRendererGo.SetActive(true);
            }
            else
            {
                pointCloudRendererGo.SetActive(false);
            }
        }
        #endregion


        private void Awake()
        {
#if ENABLE_WINMD_SUPPORT
            IntPtr WorldOriginPtr = UnityEngine.XR.WSA.WorldManager.GetNativeISpatialCoordinateSystemPtr();
            unityWorldOrigin =
                Marshal.GetObjectForIUnknown(WorldOriginPtr) as Windows.Perception.Spatial.SpatialCoordinateSystem;
#endif
        }

        void Start()
        {
            if (pointCloudRendererGo != null)
            {
                pointCloudRenderer = pointCloudRendererGo.GetComponent<PointCloudRenderer>();
            }

            tcpClient = GetComponent<TCPClient>();
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
            researchMode = new HL2ResearchMode();
            
            if (depthSensorMode == DepthSensorMode.LongThrow) researchMode.InitializeLongDepthSensor();
            else if (depthSensorMode == DepthSensorMode.ShortThrow) researchMode.InitializeDepthSensor();

            researchMode.InitializeSpatialCamerasFront();
            researchMode.SetReferenceCoordinateSystem(unityWorldOrigin);
            researchMode.SetPointCloudDepthOffset(0);

            if (depthSensorMode == DepthSensorMode.LongThrow) researchMode.StartLongDepthSensorLoop(enablePointCloud);
            else if (depthSensorMode == DepthSensorMode.ShortThrow) researchMode.StartDepthSensorLoop(enablePointCloud);
            researchMode.StartSpatialCamerasFrontLoop();
#endif
        }

        private void UpdatePointCloud()
        {
#if ENABLE_WINMD_SUPPORT
            if (enablePointCloud && renderPointCloud && pointCloudRendererGo != null)
            {
                if ((depthSensorMode == DepthSensorMode.LongThrow && !researchMode.LongThrowPointCloudUpdated()) ||
                    (depthSensorMode == DepthSensorMode.ShortThrow && !researchMode.PointCloudUpdated())) return;

                float[] pointCloud = new float[] { };
                if (depthSensorMode == DepthSensorMode.LongThrow)
                    pointCloud = researchMode.GetLongThrowPointCloudBuffer();
                else if (depthSensorMode == DepthSensorMode.ShortThrow) pointCloud = researchMode.GetPointCloudBuffer();

                if (pointCloud.Length > 0)
                {
                    int pointCloudLength = pointCloud.Length / 3;
                    Vector3[] pointCloudVector3 = new Vector3[pointCloudLength];
                    for (int i = 0; i < pointCloudLength; i++)
                    {
                        pointCloudVector3[i] =
                            new Vector3(pointCloud[3 * i], pointCloud[3 * i + 1], pointCloud[3 * i + 2]);
                    }
                    pointCloudRenderer.Render(pointCloudVector3, pointColor);
                }
            }
#endif
        }

        private void SendPing()
        {
#if ENABLE_WINMD_SUPPORT
            if ((pingCtr++) % 1000 == 0)
            {
                if (tcpClient != null)
                {
                    tcpClient.sendPing();
                }
            }
#endif
        }
        
    }
}