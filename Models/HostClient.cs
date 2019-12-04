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
    public class HostClient : Client
    {
        private List<Socket> clientSockets; // list of everybody connected to the service
        private Dictionary<Socket, IPEndPoint> clientEndPoints;

        private bool accepting = false;

        public HostClient(int maxConnections = 100, int port = Client.DefaultPort)
        {
            connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSockets = new List<Socket>();
            clientEndPoints = new Dictionary<Socket, IPEndPoint>();

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddr = ipHost.AddressList[1];

            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(ipAddr.ToString()), port);

            connectionSocket.Bind(ipEnd);
            connectionSocket.Listen(maxConnections);
        }

        public void BeginAccept()
        {
            accepting = true;

            connectionSocket.BeginAccept(OnConnectionReceived, connectionSocket);
        }

        public void StopAccepting()
        {
            accepting = false;
        }

        private void OnConnectionReceived(IAsyncResult result)
        {
            Socket newConnection = null;
            Socket stateSocket = (Socket)result.AsyncState;
            try 
            {
                newConnection = stateSocket.EndAccept(result);

                if (!accepting)
                {
                    StateObject state = new StateObject();
                    state.workSocket = newConnection;

                    byte[] buffer = new byte[StateObject.BufferSize];
                    for (int i = 0; i < Client.BlockPacket.Count(); i++)
                    {
                        buffer[i] = Client.BlockPacket[i];
                    }

                    state.buffer = new byte[StateObject.BufferSize];

                    newConnection.BeginSend(state.buffer, 0, state.buffer.Length, SocketFlags.None, DefaultSendCallback, state);
                    newConnection.Close();
                }
            }
            catch (ObjectDisposedException ex)
            {
                return;
            }

            if (accepting && newConnection.Connected)
            {
                StateObject state = new StateObject();
                state.workSocket = newConnection;
                state.buffer = new byte[StateObject.BufferSize];

                // gets us ready to receive the first packet from the new client
                newConnection.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromClient, state);

                clientSockets.Add(newConnection);
                clientEndPoints.Add(newConnection, (IPEndPoint) newConnection.RemoteEndPoint);
            }

            if (accepting)
                stateSocket.BeginAccept(OnConnectionReceived, stateSocket);
        }

        public override void Send(byte[] buffer, SocketFlags flags = SocketFlags.None)
        {
            if (clientSockets.Count > 0)
            {
                foreach (Socket client in clientSockets)
                {
                    StateObject state = new StateObject();
                    state.workSocket = client;
                    state.buffer = buffer;

                    client.BeginSend(state.buffer, 0, state.buffer.Length, SocketFlags.None, DefaultSendCallback, state);
                }
            }
        }

        public override void Close()
        {
            base.Close();

            foreach (Socket s in clientSockets)
            {
                if (s != null)
                    s.Close();
            }

            if (connectionSocket != null)
                connectionSocket.Close();
        }

        protected void OnPacketReceivedFromClient(IAsyncResult result)
        {
            base.OnPacketReceived(result);

            StateObject state = (StateObject)result.AsyncState;
            int packetSize;

            try
            {
                packetSize = state.workSocket.EndReceive(result);
            }
            catch (Exception e) when (e is ObjectDisposedException || e is SocketException)
            {
                clientSockets.Contains(state.workSocket);
                clientSockets.Remove(state.workSocket);
                base.RemoteDisconnected(clientEndPoints[state.workSocket], clientSockets.Count == 0);
                clientEndPoints.Remove(state.workSocket);
                return;
            }
            
            Send(state.buffer.Take(packetSize).ToArray());

            state.buffer = new byte[packetSize];


            // this "workSocket" is essentially the connection to the client
            // gets us ready to receive something from the client once again
            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromClient, state);
        }
    }
}
