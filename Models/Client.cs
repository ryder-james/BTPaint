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
        public const int DefaultPort = 25565;

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

            StateObject state = new StateObject();
            state.workSocket = clientSocket;
            state.buffer = byteData;

            clientSocket.BeginSend(state.buffer, 0, state.buffer.Length, flags, DefaultSendCallback, state);
        }

        public virtual void Close()
        {
            clientSocket.Close();
            connectionSocket.Close();
        }

        protected void OnPacketReceived(IAsyncResult result)
        {
            Debug.WriteLine("Packet received");
            StateObject state = (StateObject)result.AsyncState;

            if (PacketReceived != null)
            {
                PacketReceived(state.buffer);
            }

            int packetSize = state.workSocket.EndReceive(result);

            byte[] data = new byte[packetSize];

            state.buffer = data;

            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceived, state);
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

        protected class StateObject
        {
            public const int BufferSize = 1024;

            public Socket workSocket = null;
            public byte[] buffer = new byte[BufferSize];
        }
    }
}
