using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TCad.Util
{
    internal class DebugServer
    {
        public const int DefaultPort = 2300;
        public const string DefaultAddr = "127.0.0.1";

        private TcpListener mlistener = null;
        private IPEndPoint mLocalEndPoint;

        private List<ClientWrapper> mClientList = new List<ClientWrapper>();

        private FastRingBuffer<string> mPool;

        public DebugServer()
        {
            mPool = new FastRingBuffer<string>(20);
        }

        public void Start()
        {
            Start(DefaultAddr, DefaultPort);
        }

        public void Start(string strIpAddr, int port)
        {
            IPAddress ipAdder = IPAddress.Parse(strIpAddr);

            mLocalEndPoint = new IPEndPoint(ipAdder, port);

            Thread t = new Thread(Listening);

            t.Start();
        }

        public void Stop()
        {
            mlistener.Stop();
        }

        public void Write(string s)
        {
            mPool.Add(s);

            lock (mClientList)
            {
                foreach (ClientWrapper client in mClientList)
                {
                    if (client.Connected)
                    {
                        client.Write(s);
                    }
                }

                RemoveDisconnectedClient();
            }
        }

        public void WriteLn(string s)
        {
            mPool.Add(s + "\n");

            lock (mClientList)
            {
                foreach (ClientWrapper client in mClientList)
                {
                    if (client.Connected)
                    {
                        client.WriteLn(s);
                    }
                }

                RemoveDisconnectedClient();
            }
        }

        private void RemoveDisconnectedClient()
        {
            lock (mClientList)
            {
                mClientList.RemoveAll((item) =>
                {
                    if (!item.Connected)
                    {
                        item.Dispose();
                        return true;
                    }

                    return false;
                });
            }
        }

        public void Listening()
        {
            mlistener = new TcpListener(mLocalEndPoint);

            try
            {
                mlistener.Start();

                while (true)
                {
                    var tcpClient = mlistener.AcceptTcpClient();

                    ClientWrapper client = new ClientWrapper(tcpClient);

                    SendPoolToClient(client);

                    lock (mClientList)
                    {
                        mClientList.Add(client);
                    }
                }
            }
            catch (SocketException e)
            {
            }
            catch
            {
            }

            WriteLn("Server stopped");
            Write("\n\n");
        }

        private void SendPoolToClient(ClientWrapper client)
        {
            for (int i = 0; i < mPool.Count; i++)
            {
                client.Write(mPool[i]);
            }
        }

        private class ClientWrapper
        {
            TcpClient mClient;
            NetworkStream mStream;
            StreamWriter mWriter;

            public ClientWrapper(TcpClient client)
            {
                mClient = client;
                mStream = client.GetStream();
                mWriter = new StreamWriter(mStream, Encoding.UTF8);
            }

            public bool Connected
            {
                get => mClient.Connected;
            }

            public void WriteLn(string s)
            {
                if (!mClient.Connected)
                {
                    return;
                }

                try
                {
                    mWriter.WriteLine(s);
                    mWriter.Flush();
                }
                catch
                {
                    Close();
                }
            }


            public void Write(string s)
            {
                if (!mClient.Connected)
                {
                    return;
                }

                try
                {
                    mWriter.Write(s);
                    mWriter.Flush();
                }
                catch
                {
                    Close();
                }
            }

            public void Close()
            {
                mClient.Close();
                mStream.Close();
                mWriter.Close();
            }

            public void Dispose()
            {
                mClient.Dispose();
                mStream.Dispose();
                mWriter.Dispose();
            }
        }
    }
}
