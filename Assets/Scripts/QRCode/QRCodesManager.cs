using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.QR;

namespace QRTracking
{
    /// <summary>
    /// Static wrapper class for generic event arg data
    /// </summary>
    public static class QRCodeEventArgs
    {
        public static QRCodeEventArgs<TData> Create<TData>(TData data)
        {
            return new QRCodeEventArgs<TData>(data);
        }
    }

    /// <summary>
    /// Static wrapper class for EventArgs with generic data
    /// </summary>
    [Serializable]
    public class QRCodeEventArgs<TData> : EventArgs
    {
        public TData Data { get; private set; }

        public QRCodeEventArgs(TData data)
        {
            Data = data;
        }
    }

    /// <summary>
    /// Singleton class (can only exist once in each runtime environment). This listens to events invoked when QR Codes in the surroundings of the Hololens change their tracking state. 
    /// </summary>
    /// <seealso cref="QRTracking.Singleton&lt;QRTracking.QRCodesManager&gt;" />
    public class QRCodesManager : Singleton<QRCodesManager>
    {
        [Tooltip("Determines if the QR codes scanner should be automatically started.")]
        public bool AutoStartQRTracking = true;
        public bool IsTrackerRunning { get; private set; }
        public bool IsSupported { get; private set; }

        public event EventHandler<bool> QRCodesTrackingStateChanged;
        public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeAdded;
        public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeUpdated;
        public event EventHandler<QRCodeEventArgs<Microsoft.MixedReality.QR.QRCode>> QRCodeRemoved;

        private System.Collections.Generic.SortedDictionary<System.Guid, Microsoft.MixedReality.QR.QRCode> qrCodesList = new SortedDictionary<System.Guid, Microsoft.MixedReality.QR.QRCode>();

        private QRCodeWatcher qrTracker;
        private bool capabilityInitialized = false;
        private QRCodeWatcherAccessStatus accessStatus;
        private System.Threading.Tasks.Task<QRCodeWatcherAccessStatus> capabilityTask;

        public System.Guid GetIdForQRCode(string qrCodeData)
        {
            lock (qrCodesList)
            {
                foreach (var ite in qrCodesList)
                {
                    if (ite.Value.Data == qrCodeData)
                    {
                        return ite.Key;
                    }
                }
            }
            return new System.Guid();
        }

        public System.Collections.Generic.IList<Microsoft.MixedReality.QR.QRCode> GetList()
        {
            lock (qrCodesList)
            {
                return new List<Microsoft.MixedReality.QR.QRCode>(qrCodesList.Values);
            }
        }

        async protected virtual void Start()
        {
            IsSupported = QRCodeWatcher.IsSupported();
            capabilityTask = QRCodeWatcher.RequestAccessAsync();
            accessStatus = await capabilityTask;
            capabilityInitialized = true;
        }

        private void OnDestroy()
        {
            StopQRTracking(); 
        }


        private void SetupQRTracking()
        {
            if (qrCodesList != null) qrCodesList.Clear();

            try
            {
                qrTracker = new QRCodeWatcher();
                IsTrackerRunning = false;
                qrTracker.Added += QRCodeWatcher_Added;
                qrTracker.Updated += QRCodeWatcher_Updated;
                qrTracker.Removed += QRCodeWatcher_Removed;
                qrTracker.EnumerationCompleted += QRCodeWatcher_EnumerationCompleted;
            }
            catch (Exception ex)
            {
                Debug.Log("QRCodesManager : exception starting the tracker " + ex.ToString());
            }

            if (AutoStartQRTracking)
            {
                StartQRTracking();
            }
        }

        public void StartQRTracking()
        {
            if (qrTracker != null && !IsTrackerRunning)
            {
                Debug.Log("QRCodesManager starting QRCodeWatcher");
                try
                {
                    qrTracker.Start();
                    IsTrackerRunning = true;
                    QRCodesTrackingStateChanged?.Invoke(this, true);
                }
                catch(Exception ex)
                {
                    Debug.Log("QRCodesManager starting QRCodeWatcher Exception:" + ex.ToString());
                }
            }
        }

        public void StopQRTracking()
        {
            if (IsTrackerRunning)
            {
                IsTrackerRunning = false;
                if (qrTracker != null)
                {
                    qrTracker.Stop();
                    qrCodesList.Clear();
                }

                var handlers = QRCodesTrackingStateChanged;
                if (handlers != null)
                {
                    handlers(this, false);
                }
            }
        }

        private void QRCodeWatcher_Removed(object sender, QRCodeRemovedEventArgs args)
        {

            bool found = false;
            lock (qrCodesList)
            {
                if (qrCodesList.ContainsKey(args.Code.Id))
                {
                    qrCodesList.Remove(args.Code.Id);
                    found = true;
                }
            }
            if (found)
            {
                var handlers = QRCodeRemoved;
                if (handlers != null)
                {
                    handlers(this, QRCodeEventArgs.Create(args.Code));
                }
            }
        }

        private void QRCodeWatcher_Updated(object sender, QRCodeUpdatedEventArgs args)
        {

            bool found = false;
            lock (qrCodesList)
            {
                if (qrCodesList.ContainsKey(args.Code.Id))
                {
                    found = true;
                    qrCodesList[args.Code.Id] = args.Code;
                }
            }
            if (found)
            {
                var handlers = QRCodeUpdated;
                if (handlers != null)
                {
                    handlers(this, QRCodeEventArgs.Create(args.Code));
                }
            }
        }

        private void QRCodeWatcher_Added(object sender, QRCodeAddedEventArgs args)
        {

            lock (qrCodesList)
            {
                qrCodesList[args.Code.Id] = args.Code;
            }
            var handlers = QRCodeAdded;
            if (handlers != null)
            {
                handlers(this, QRCodeEventArgs.Create(args.Code));
            }
        }

        private void QRCodeWatcher_EnumerationCompleted(object sender, object e)
        {
            Debug.Log("QRCodesManager QrTracker_EnumerationCompleted");
        }

        private void Update()
        {
            if (qrTracker == null && capabilityInitialized && IsSupported)
            {
                if (accessStatus == QRCodeWatcherAccessStatus.Allowed)
                {
                    SetupQRTracking();
                }
                else
                {  
                    Debug.Log("Capability access status : " + accessStatus);
                }
            }
        }
    }

}