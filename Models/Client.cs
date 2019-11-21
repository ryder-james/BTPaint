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


    public abstract class Client
    {
        protected class StateObject
        {
            public const int BufferSize = 1024;

            public Socket workSocket = null;
            public byte[] buffer = new byte[BufferSize];
        }

        public const int DefaultPort = 10000;

        public event PacketReceivedEventHandler PacketReceived;

        protected Socket connectionSocket;

        public Client()
        {
            connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public virtual void Close()
        {
            if (connectionSocket != null)
                connectionSocket.Close();
        }

        protected virtual void OnPacketReceived(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;

            if (PacketReceived != null)
            {
                PacketReceived(state.buffer);
            }
        }

        protected void DefaultSendCallback(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;
            state.workSocket.EndSend(result);
        }

        protected void DefaultConnectCallback(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;
            state.workSocket.EndConnect(result);
        }


    }
}
