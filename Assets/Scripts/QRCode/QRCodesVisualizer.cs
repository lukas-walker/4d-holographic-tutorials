using System;
using System.Collections.Generic;
using UnityEngine;

namespace QRTracking
{
    /// <summary>
    /// Instantiates and handles QR Codes objects in the scene. 
    /// </summary>
    /// <seealso cref="UnityEngine.MonoBehaviour" />
    public class QRCodesVisualizer : MonoBehaviour
    {
        [Tooltip("The QR Code prefab that will be instantiated for each new tracked QR Code in the surroundings of the Hololens.")]
        public GameObject qrCodePrefab;
        public Transform origin;


        [SerializeField]
        [Tooltip("Text in the QR Code that indicates to recognize a QRCode as Origin")]
        private string OriginCodeText = "4DTutorials";

        [SerializeField]
        [Tooltip("Accept any text in any QR Code. (Ignore Origin Code Text)")]
        private bool AcceptAnyText = false;


        [SerializeField]
        [Tooltip("Update QR Code location constantly, i.e. whenever it is tracked. (almost each frame)")]
        private bool UpdateConstantly = false;

        private DateTimeOffset startTime;

        private System.Collections.Generic.SortedDictionary<System.Guid, GameObject> qrCodesObjectsList;
        private bool clearExisting = false;

        private bool active = false;
        private bool hide = false;

        struct ActionData
        {
            public enum Type
            {
                Added,
                Updated,
                Removed
            };
            public Type type;
            public Microsoft.MixedReality.QR.QRCode qrCode;

            public ActionData(Type type, Microsoft.MixedReality.QR.QRCode qRCode) : this()
            {
                this.type = type;
                qrCode = qRCode;
            }
        }

        private System.Collections.Generic.Queue<ActionData> pendingActions = new Queue<ActionData>();

        void Start()
        {
            qrCodesObjectsList = new SortedDictionary<System.Guid, GameObject>();

            QRCodesManager.Instance.QRCodesTrackingStateChanged += Instance_QRCodesTrackingStateChanged;
            QRCodesManager.Instance.QRCodeAdded += Instance_QRCodeAdded;
            QRCodesManager.Instance.QRCodeUpdated += Instance_QRCodeUpdated;
            QRCodesManager.Instance.QRCodeRemoved += Instance_QRCodeRemoved;
            if (qrCodePrefab == null)
            {
                throw new System.Exception("Prefab not assigned");
            }

            startTime = DateTimeOffset.Now;
        }

        private void Instance_QRCodesTrackingStateChanged(object sender, bool status)
        {
            if (!status)
            {
                clearExisting = true;
            }
        }

        private void Instance_QRCodeAdded(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Added, e.Data));
            }
        }

        private void Instance_QRCodeUpdated(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Updated, e.Data));
            }
        }

        private void Instance_QRCodeRemoved(object sender, QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode> e)
        {

            lock (pendingActions)
            {
                pendingActions.Enqueue(new ActionData(ActionData.Type.Removed, e.Data));
            }
        }

        /// <summary>
        /// Handles the events raised when any QR Codes change their tracking state / update their values. This method will ignore QR codes that have been tracked for the last time before this program was started. 
        /// </summary>
        private void HandleEvents()
        {
            lock (pendingActions)
            {
                while (pendingActions.Count > 0)
                {
                    var action = pendingActions.Dequeue();
                    if (action.type == ActionData.Type.Added)
                    {
                        // only handle QR Code if it's been tracked after this program's start time
                        if (startTime >= action.qrCode.LastDetectedTime) continue;

                        if (AcceptAnyText || action.qrCode.Data == OriginCodeText) 
                        {
                            GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                            qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                            qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                            qrCodeObject.GetComponent<QRCode>().SetOrigin(origin);
                            qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                        
                        
                            qrCodeObject.GetComponent<QRCode>().UpdateOrigin();
                        }
                    }
                    else if (action.type == ActionData.Type.Updated)
                    {
                        // only handle QR Code if it's been tracked after this program's start time
                        if (startTime >= action.qrCode.LastDetectedTime) continue;

                        // if there's no QR Code Object associated with the updated QR Code yet, a new QR Code will be instantiated
                        if (!qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            if (AcceptAnyText || action.qrCode.Data == OriginCodeText)
                            {
                                GameObject qrCodeObject = Instantiate(qrCodePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                                qrCodeObject.GetComponent<SpatialGraphNodeTracker>().Id = action.qrCode.SpatialGraphNodeId;
                                qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                                qrCodeObject.GetComponent<QRCode>().SetOrigin(origin);
                                qrCodesObjectsList.Add(action.qrCode.Id, qrCodeObject);
                            
                                qrCodeObject.GetComponent<QRCode>().UpdateOrigin();
                            }
                        }
                        else {
                            if ((AcceptAnyText || action.qrCode.Data == OriginCodeText) && UpdateConstantly)
                            {
                                // not sure if this is necessary, I think the qrCode object from the QRCodesManager stays the same object.
                                GameObject qrCodeObject;
                                qrCodesObjectsList.TryGetValue(action.qrCode.Id, out qrCodeObject);
                                if (qrCodeObject != null) {
                                    qrCodeObject.GetComponent<QRCode>().qrCode = action.qrCode;
                                    qrCodeObject.GetComponent<QRCode>().UpdateOrigin();
                                }
                            }
                        }
                    }
                    else if (action.type == ActionData.Type.Removed)
                    {
                        if (qrCodesObjectsList.ContainsKey(action.qrCode.Id))
                        {
                            Destroy(qrCodesObjectsList[action.qrCode.Id]);
                            qrCodesObjectsList.Remove(action.qrCode.Id);
                        }
                    }
                }
            }
            if (clearExisting)
            {
                clearExisting = false;
                foreach (var obj in qrCodesObjectsList)
                {
                    Destroy(obj.Value);
                }
                qrCodesObjectsList.Clear();

            }
        }

        void Update()
        {
            HandleEvents();
            if (!hide) HideNonDetectedCodes();
        }

        /// <summary>
        /// Inverts the value uf active. Active can be set to false to hide all QR Code objects in the scene. 
        /// </summary>
        public void ToggleActive() {
            active = !active;
        }

        /// <summary>
        /// Can temporarily hide all QR Codes. 
        /// </summary>
        public void Hide(bool hide)
        {
            this.hide = hide;

            foreach (GameObject qrCodeObject in qrCodesObjectsList.Values)
            {
                qrCodeObject.SetActive(false);
            }
        }

        /// <summary>
        /// Check if QR Codes have recently been tracked. Otherwise they are hidden from view after 3 seconds. 
        /// </summary>
        private void HideNonDetectedCodes() {

            foreach (GameObject qrCodeObject in qrCodesObjectsList.Values) {
                QRCode code = qrCodeObject.GetComponent<QRCode>();

                if (code.LastTimeStampIsOldBy(new TimeSpan(TimeSpan.TicksPerSecond * 3)) || active)
                {
                    if (qrCodeObject.activeSelf) {
                        qrCodeObject.SetActive(false);

                    }                    
                }
                else {
                    // if hide is false and QR code was recently detected it will be set active
                    if (!qrCodeObject.activeSelf) 
                    {
                        qrCodeObject.SetActive(true);

                    }
                }
            }
        }
    }

}