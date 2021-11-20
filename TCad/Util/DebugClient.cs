using System.IO;
using System.Net.Sockets;
using System.Text;

namespace TCad.Util
{
    public class DebugClient
    {
        NetworkStream NetStream;
        StreamWriter Writer;

        public bool IsValid
        {
            get => NetStream != null;
        }

        public DebugClient()
        {
            ConnectServer();
        }

        public void ConnectServer()
        {
            //string ipOrHost = "127.0.0.1";
            string ipOrHost = "localhost";
            int port = 2001;

            try
            {
                TcpClient tcpClient = new TcpClient(ipOrHost, port);
                NetStream = tcpClient.GetStream();
                Writer = new StreamWriter(NetStream, Encoding.UTF8);
            }
            catch
            {
            }
        }

        public void Write(string s)
        {
            if (Writer == null)
            {
                return;
            }
            Writer.Write(s);
            Writer.Flush();
        }

        public void WriteLine(string s)
        {
            if (Writer == null)
            {
                return;
            }
            Writer.WriteLine(s);
            Writer.Flush();
        }
    }
}
