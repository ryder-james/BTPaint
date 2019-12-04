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

    public delegate void DisconnectEventHandler(IPEndPoint disconnectedEndPoint, bool wasLastConnection);

    public abstract class Client
    {
        protected static readonly byte[] BlockPacket = BitConverter.GetBytes(0xDEADC0DE);

        protected class StateObject
        {
            public const int BufferSize = 1024;

            public Socket workSocket = null;
            public byte[] buffer = new byte[BufferSize];
        }

        public const int DefaultPort = 10000;

        public event PacketReceivedEventHandler PacketReceived;
        public event DisconnectEventHandler RemoteDisconnectedHandler;

        protected Socket connectionSocket;

        public Client()
        {
            connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public virtual void Send(IPacket packet, SocketFlags flags = SocketFlags.None)
        {
            Send(packet.ToByteArray(), flags);
        }

        public abstract void Send(byte[] buffer, SocketFlags flags = SocketFlags.None);

        public virtual void Close()
        {
            if (connectionSocket != null)
                connectionSocket.Close();
        }

        protected virtual void OnPacketReceived(IAsyncResult result)
        {
            Debug.WriteLine("Packet received");

            StateObject state = (StateObject)result.AsyncState;

            bool realPacket = false;
            foreach (byte b in state.buffer)
            {
                if (b != 0)
                {
                    realPacket = true;
                    break;
                }
            }

            if (realPacket && PacketReceived != null)
            {
                PacketReceived(state.buffer);
            }
        }

        protected virtual void RemoteDisconnected(IPEndPoint remote, bool wasLastConnection = true)
        {
            if (RemoteDisconnectedHandler != null)
            {
                RemoteDisconnectedHandler(remote, wasLastConnection);
            }
        }

        protected virtual void DefaultSendCallback(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;
            state.workSocket.EndSend(result);
        }
    }
}
