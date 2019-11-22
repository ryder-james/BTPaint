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
        private Socket serverSocket;
        private List<Socket> clientSockets;
        private IPHostEntry ipHost;
        private IPAddress ipAddr;

        public HostClient()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            clientSockets = new List<Socket>();
            ipHost = Dns.GetHostEntry(Dns.GetHostName());
            ipAddr = ipHost.AddressList[1];
        }

        public void BeginAccept(int maxConnections = 100, int port = Client.DefaultPort)
        {
            IPEndPoint ipEnd = new IPEndPoint(IPAddress.Parse(ipAddr.ToString()), port);
            Debug.WriteLine("Endpoint created at: " + ipAddr.ToString());

            serverSocket.Bind(ipEnd);
            serverSocket.Listen(maxConnections);
            serverSocket.BeginAccept(OnConnectionEstablished, serverSocket);
        }

        private void OnConnectionEstablished(IAsyncResult result)
        {
            Debug.WriteLine("Attempting Connection");

            Socket stateSocket = (Socket)result.AsyncState;
            Socket newConnection = stateSocket.EndAccept(result);

            if (newConnection.Connected)
            {
                Debug.WriteLine("Connection established");

                StateObject state = new StateObject();
                state.workSocket = newConnection;
                state.buffer = new byte[StateObject.BufferSize];

                // opens up the remote side of the client's socket (that's us) to begin receiving messages
                newConnection.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromClient, state);

                Socket localToRemoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                StateObject state2 = new StateObject();
                state2.workSocket = localToRemoteSocket;
                localToRemoteSocket.BeginConnect(newConnection.LocalEndPoint, ar =>
                {
                    StateObject state3 = (StateObject)ar.AsyncState;
                    state3.workSocket.EndConnect(result);
                    clientSockets.Add(state3.workSocket);
                }, state2);

                Socket remoteSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                remoteSocket.Bind(newConnection.LocalEndPoint);
                remoteSocket.Listen(1);
                remoteSocket.Accept();

                //clientSockets.Add(newConnection);
            }

            stateSocket.BeginAccept(OnConnectionEstablished, stateSocket);
        }

        public override void Send(IPacket packet, SocketFlags flags = SocketFlags.None)
        {
            if (clientSockets.Count > 0)
            {
                clientSockets[0].Send(new byte[] { 1, 2, 3, 4 });
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

            if (serverSocket != null)
                serverSocket.Close();
        }

        protected void OnPacketReceivedFromClient(IAsyncResult result)
        {
            base.OnPacketReceived(result);

            StateObject state = (StateObject)result.AsyncState;

            int packetSize = state.workSocket.EndReceive(result);
            state.buffer = new byte[packetSize];

            // this "workSocket" is essentially the connection to the client
            // opens up the remote side of the client's socket (that's us) to begin receiving messages again
            state.workSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, OnPacketReceivedFromClient, state);
        }
    }
}
