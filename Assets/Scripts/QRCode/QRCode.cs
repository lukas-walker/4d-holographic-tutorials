using UnityEngine;
using System;

namespace QRTracking
{
    /// <summary>
    /// Component on the QR Code prefab that will be instantiated when a QR Code is found in the surroundings of the Hololens. 
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    [RequireComponent(typeof(QRTracking.SpatialGraphNodeTracker))]
    public class QRCode : MonoBehaviour
    {
        public Microsoft.MixedReality.QR.QRCode qrCode;
        private GameObject qrCodeCube;

        /// <summary>
        /// Gets the size of the physical QR code in meters.
        /// </summary>
        public float PhysicalSize { get; private set; }

        /// <summary>
        /// Gets the code text (the data in the QR code) that was found.
        /// </summary>
        public string CodeText { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this QR Code is active.
        /// </summary>
        public bool IsActive { get {
                return gameObject.activeSelf;    
            } 
        }

        // components of the QR Code object that will be updated throughout its lifetime
        private TextMesh QRID;
        private TextMesh QRNodeID;
        private TextMesh QRText;
        private TextMesh QRVersion;
        private TextMesh QRTimeStamp;
        private TextMesh QRSize;
        private GameObject QRInfo;
        private bool validURI = false;
        private bool launch = false;
        private System.Uri uriResult;
        public  long lastTimeStamp = 0;



        [SerializeField]
        [Tooltip("Material that determines the visual appearance of the QR Code")]
        private Material material;

        public void SetOrigin(Transform Origin) {
            this.Origin = Origin;
        }


        /// <summary>
        /// The first point of reference (origin). Will be set as soon as this QR Code is instatiated by the QRCodesVisualizer.
        /// </summary>
        private Transform Origin;

        void Start()
        {
            PhysicalSize = 0.1f; // dummy value, 0.1 meters
            CodeText = "Dummy";
            if (qrCode == null)
            {
                throw new System.Exception("QR Code Empty");
            }

            // fill initial values
            PhysicalSize = qrCode.PhysicalSideLength;
            CodeText = qrCode.Data;

            qrCodeCube = gameObject.transform.Find("Cube").gameObject;
            QRInfo = gameObject.transform.Find("QRInfo").gameObject;
            QRID = QRInfo.transform.Find("QRID").gameObject.GetComponent<TextMesh>();
            QRNodeID = QRInfo.transform.Find("QRNodeID").gameObject.GetComponent<TextMesh>();
            QRText = QRInfo.transform.Find("QRText").gameObject.GetComponent<TextMesh>();
            QRVersion = QRInfo.transform.Find("QRVersion").gameObject.GetComponent<TextMesh>();
            QRTimeStamp = QRInfo.transform.Find("QRTimeStamp").gameObject.GetComponent<TextMesh>();
            QRSize = QRInfo.transform.Find("QRSize").gameObject.GetComponent<TextMesh>();

            QRID.text = "Id:" + qrCode.Id.ToString();
            QRNodeID.text = "NodeId:" + qrCode.SpatialGraphNodeId.ToString();
            QRText.text = CodeText;

            if (System.Uri.TryCreate(CodeText, System.UriKind.Absolute,out uriResult))
            {
                validURI = true;
                QRText.color = Color.blue;
            }

            QRVersion.text = "Ver: " + qrCode.Version;
            QRSize.text = "Size:" + qrCode.PhysicalSideLength.ToString("F04") + "m";
            QRTimeStamp.text = "Time:" + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
            QRTimeStamp.color = Color.yellow;
        }

        void Update()
        {
            UpdatePropertiesDisplay();
            if (launch)
            {
                launch = false;
                LaunchUri();
            }
        }

        /// <summary>
        /// Updates the properties if the QR Code is detected anew. 
        /// </summary>
        void UpdatePropertiesDisplay()
        {
            // Update properties that change
            if (qrCode != null && lastTimeStamp != qrCode.SystemRelativeLastDetectedTime.Ticks)
            {
                QRSize.text = "Size:" + qrCode.PhysicalSideLength.ToString("F04") + "m";

                QRTimeStamp.text = "Time:" + qrCode.LastDetectedTime.ToString("MM/dd/yyyy HH:mm:ss.fff");
                QRTimeStamp.color = QRTimeStamp.color == Color.yellow ? Color.white : Color.yellow;
                PhysicalSize = qrCode.PhysicalSideLength;

                qrCodeCube.transform.localPosition = new Vector3(PhysicalSize / 2.0f, PhysicalSize / 2.0f, 0.0f);
                qrCodeCube.transform.localScale = new Vector3(PhysicalSize, PhysicalSize, 0.005f);

                lastTimeStamp = qrCode.SystemRelativeLastDetectedTime.Ticks;
                QRInfo.transform.localScale = new Vector3(PhysicalSize / 0.2f, PhysicalSize / 0.2f, PhysicalSize / 0.2f);
            }
            
        }

        /// <summary>
        /// Launches the URI if one is availabe. 
        /// </summary>
        void LaunchUri()
        {
#if WINDOWS_UWP
            // Launch the URI
            UnityEngine.WSA.Launcher.LaunchUri(uriResult.ToString(), true);
#endif
        }

        /// <summary>
        /// Will be called when the button on the QR Code is clicked.
        /// </summary>
        public void OnInputClicked()
        {
            if (validURI)
            {
                launch = true;
            }
            // eventData.Use(); // Mark the event as used, so it doesn't fall through to other handlers.
        }

        /// <summary>
        /// Helper method that returns true if this instance hasn't been tracked in the given time span. 
        /// </summary>
        /// <param name="timeSpan">The time span.</param>
        public bool LastTimeStampIsOldBy(TimeSpan timeSpan)
        {
            return qrCode.LastDetectedTime.Ticks < DateTimeOffset.Now.Ticks - (timeSpan.Ticks);

        }

        // whenever the qrcode is detected, this should be called s.t. the origin is aligned with the real world qrcode. 
        public void UpdateOrigin() {
            Origin.SetPositionAndRotation(gameObject.transform.position, gameObject.transform.rotation);
        }
    }
}