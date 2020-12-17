using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Udt;

namespace UDTChat
{
    class ChatServer
    {
        class ClientItem
        {
            public Socket skt = null;
            public Thread thClient = null;
            public ChatServer parent = null;

            public void ClientThread()
            {
                byte[] buffer = new byte[1024];
                int len;
                try
                {
                    while(true)
                    {
                        len = skt.Receive(buffer);
                        if (len <= 0)
                            break;
                        lock(parent)
                        {
                            foreach(ClientItem item in parent.m_clients)
                            {
                                if (item == this)
                                    continue;
                                item.skt.Send(buffer, 0, len);
                            }
                        }
                        if(parent.Parent!=null)
                        {
                            parent.Parent.PutMessage(Encoding.ASCII.GetString(buffer, 0, len));
                        }
                    }
                }
                catch(SocketException)
                {
                }
                finally
                {
                    lock (parent)
                    {
                        parent.m_clients.Remove(this);
                    }
                    try
                    {
                        skt.Close();
                    }
                    catch(SocketException)
                    {

                    }

                }
            }
        }
        Socket m_mainSkt = null;
        List<ClientItem> m_clients = new List<ClientItem>();
        Thread m_thServer = null;

        public ChatForm Parent = null;
        public ChatServer()
        {

        }

        public bool StartServer(int nPort)
        {
            try
            {
                m_mainSkt = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);
                m_mainSkt.Bind(System.Net.IPAddress.Parse("0.0.0.0"), nPort);
                m_mainSkt.Listen(10);
                m_thServer = new Thread(new ThreadStart(this.ServerThread));
                m_thServer.Start();
            }
            catch(SocketException)
            {
                if(m_mainSkt!=null)
                {
                    m_mainSkt.Close();
                }
                m_mainSkt = null;
                return false;
            }
            return true;
        }

        public void Close()
        {
            if (m_mainSkt != null)
                m_mainSkt.Close();
        }

        public bool Broadcast(string strMsg)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(strMsg);
            lock(this)
            {
                if (m_mainSkt == null)
                    return false;

                foreach(ClientItem ci in m_clients)
                {
                    try
                    {
                        ci.skt.Send(buffer);
                    }
                    catch(SocketException)
                    {
                        ci.skt.Close();
                        m_clients.Remove(ci);
                    }
                }
            }
            return true;
        }

        void ServerThread()
        {
            Socket skt;
            ClientItem ci;
            try
            {
                while(true)
                {
                    skt = m_mainSkt.Accept();
                    ci = new ClientItem();
                    ci.skt = skt;
                    ci.thClient = new Thread(new ThreadStart(ci.ClientThread));
                    ci.parent = this;
                    lock (this)
                    {
                        m_clients.Add(ci);
                    }
                    ci.thClient.Start();
                }
            }
            catch(SocketException)
            {
            }
            finally
            {
                lock (this)
                {
                    foreach (ClientItem item in m_clients)
                    {
                        item.skt.Close();
                    }
                    m_clients.Clear();
                }
                m_mainSkt.Close();

            }
        }
    }
}
