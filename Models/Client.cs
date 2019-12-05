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

    public delegate void ConnectEventHandler(IPEndPoint connectedEndPoint);
    public delegate void DisconnectEventHandler(IPEndPoint disconnectedEndPoint, bool wasLastConnection);
    public delegate void ConnectionFailedHandler();

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
        public event ConnectEventHandler RemoteConnected;
        public event DisconnectEventHandler RemoteDisconnected;
        public event ConnectionFailedHandler ConnectionFailed;

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

            if (realPacket)
            {
                PacketReceived?.Invoke(state.buffer);
            }
        }

        protected virtual void FireRemoteConnected(IPEndPoint remote)
        {
            RemoteConnected?.Invoke(remote);
        }

        protected virtual void FireRemoteDisconnected(IPEndPoint remote, bool wasLastConnection = true)
        {
            RemoteDisconnected?.Invoke(remote, wasLastConnection);
        }
        protected virtual void FireConnectionFailed()
        {
            ConnectionFailed?.Invoke();
        }

        protected virtual void DefaultSendCallback(IAsyncResult result)
        {
            StateObject state = (StateObject)result.AsyncState;
            state.workSocket.EndSend(result);
        }
    }
}
