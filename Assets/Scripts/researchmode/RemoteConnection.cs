using System;
using UnityEngine;

#if WINDOWS_UWP
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif

namespace Tutorials.ResearchMode
{
    // Utility to set up tcp connection with a host
    public class RemoteConnection : MonoBehaviour
    {
        [SerializeField] string hostIPAddress, port;
        private bool connected = false;
        private bool lastMessageSent = true;

        #region Unity Functions
        
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
#if ENABLE_WINMD_SUPPORT
            StopConnection();
#endif
            }
        }

        #endregion

        public bool Connected
        {
            get { return connected; }
        }

#if ENABLE_WINMD_SUPPORT
    StreamSocket socket = null;
    public DataWriter dw;
    public DataReader dr;
#endif

        private async void StartConnection()
        {
            #if ENABLE_WINMD_SUPPORT
            if (socket != null) socket.Dispose();

            try
            {
                socket = new StreamSocket();
                var hostName = new Windows.Networking.HostName(hostIPAddress);
                await socket.ConnectAsync(hostName, port);
                dw = new DataWriter(socket.OutputStream);
                dr = new DataReader(socket.InputStream);
                dr.InputStreamOptions = InputStreamOptions.Partial;
                connected = true;
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
            #endif
        }

        private void StopConnection()
        {
            #if ENABLE_WINMD_SUPPORT
            dw?.DetachStream();
            dw?.Dispose();
            dw = null;

            dr?.DetachStream();
            dr?.Dispose();
            dr = null;

            socket?.Dispose();
            connected = false;
           #endif
        }

        public async void sendPing()
        {
            #if ENABLE_WINMD_SUPPORT
            if (!lastMessageSent) return;
            lastMessageSent = false;
            try
            {
                dw.WriteString("p");
                await dw.StoreAsync();
                await dw.FlushAsync();
            }
            catch (Exception ex)
            {
                SocketErrorStatus webErrorStatus = SocketError.GetStatus(ex.GetBaseException().HResult);
                Debug.Log(webErrorStatus.ToString() != "Unknown" ? webErrorStatus.ToString() : ex.Message);
            }
            lastMessageSent = true;
            #endif
        }
    }
}