using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Models
{
    public delegate void PacketReceivedEventHandler(byte[] packetBytes);

    public class Client
    {
        public event PacketReceivedEventHandler PacketReceived;

        private Socket clientSocket, connectionSocket;

        public Client()
        {
            connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void BeginConnect(IPEndPoint targetAddress, AsyncCallback callback = null)
        {
            connectionSocket.BeginConnect(targetAddress, (callback != null ? callback : DefaultConnectCallback), connectionSocket);
        }

        public void Send(IPacket packet, SocketFlags flags = SocketFlags.None)
        {
            if (!clientSocket.Connected)
            {
                return;
            }

            byte[] byteData = packet.ToByteArray();

            clientSocket.BeginSend(byteData, 0, byteData.Length, flags, DefaultSendCallback, clientSocket);
        }

        public virtual void Close()
        {
            clientSocket.Close();
            connectionSocket.Close();
        }

        protected void DefaultSendCallback(IAsyncResult result)
        {
            ((Socket)result.AsyncState).EndSend(result);
        }

        protected void DefaultConnectCallback(IAsyncResult result)
        {
            Debug.WriteLine("Connect call back called");

            clientSocket = (Socket)result.AsyncState;

            clientSocket.EndConnect(result);
        }
    }
}
