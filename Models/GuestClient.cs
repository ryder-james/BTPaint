using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Networking.Models
{
    public class GuestClient : Client
    {
        private Socket clientSocket;

        public void BeginConnect(IPEndPoint targetAddress, AsyncCallback callback = null)
        {
            StateObject state = new StateObject();
            state.workSocket = connectionSocket;

            connectionSocket.BeginConnect(targetAddress, (callback != null ? callback : DefaultConnectCallback), state);
        }

        public override void Send(IPacket packet, SocketFlags flags = SocketFlags.None)
        {
            if (clientSocket == null || !clientSocket.Connected)
            {
                return;
            }

            byte[] byteData = packet.ToByteArray();

            StateObject state = new StateObject();
            state.workSocket = clientSocket;
            state.buffer = byteData;

            clientSocket.BeginSend(state.buffer, 0, state.buffer.Length, flags, DefaultSendCallback, state);
        }

        public override void Close()
        {
            base.Close();

            if (clientSocket != null)
                clientSocket.Close();
        }

        protected void DefaultConnectCallback(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;
            state.workSocket.EndConnect(result);
            clientSocket = state.workSocket;
        }

        //protected override void OnPacketReceived(IAsyncResult result)
        //{
        //    base.OnPacketReceived(result);

        //    StateObject state = (StateObject)result.AsyncState;

        //    int packetSize = state.workSocket.EndReceive(result);
        //    state.buffer = new byte[packetSize];
        //    state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceived, state);
        //}
    }
}
