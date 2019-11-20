using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Models
{
    public class Client
    {
        private Socket clientSocket, connectionSocket;

        public Client()
        {
            connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public void BeginConnect(IPEndPoint targetAddress, AsyncCallback callback = null)
        {
            connectionSocket.BeginConnect(targetAddress, (callback != null ? callback : DefaultConnectCallback), connectionSocket);
        }

        public void Send(IPacket packet)
        {
            if (!clientSocket.Connected)
            {
                return;
            }

            byte[] byteData = packet.ToByteArray();

            clientSocket.BeginSend(byteData, 0, byteData.Length, SocketFlags.None, DefaultSendCallback, clientSocket);
        }

        private void DefaultSendCallback(IAsyncResult result)
        {

        }

        private void DefaultConnectCallback(IAsyncResult result)
        {
            clientSocket = (Socket)result.AsyncState;

            clientSocket.EndConnect(result);
        }
    }
}
