using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Udt;

namespace UDTChat
{
    class ChatClient
    {
        Socket m_mainSkt = null, m_tranSkt = null;
        Thread m_th = null;
        ChatForm m_form = null;
        bool m_bTranverse = false;

        private ChatClient()
        {

        }

        public static ChatClient Connect(string strAddress,int nPort,ChatForm form,bool bTranverse=false)
        {
            ChatClient cc = new ChatClient();
            cc.m_bTranverse = bTranverse;
            cc.m_form = form;
            try
            {
                cc.m_mainSkt = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);
                cc.m_mainSkt.Connect(strAddress, nPort);
                cc.m_th = new Thread(new ThreadStart(cc.ClientThread));
                cc.m_th.Start();
            }
            catch(SocketException)
            {
                if (cc.m_mainSkt != null)
                    cc.m_mainSkt.Close();
                return null;
            }
            return cc;
        }

        public bool SendMessage(string strMsg)
        {
            byte[] buffer;
            if (m_mainSkt == null)
                return false;
            try
            {
                buffer = Encoding.ASCII.GetBytes(strMsg);
                m_mainSkt.Send(buffer);
            }
            catch(SocketException)
            {
                m_mainSkt.Close();
                m_mainSkt = null;
                return false;
            }
            return true;
        }

        public void Close()
        {
            if(m_tranSkt!=null)
            {
                m_tranSkt.Close();
                m_tranSkt = null;
            }
            m_mainSkt.Close();
            m_mainSkt = null;
        }

        void ClientThread()
        {
            byte[] buffer = new byte[1024];
            int len;
            UInt16 port;
            System.Net.IPAddress ipaddr;

            Socket skt;
            try
            {
                if(m_bTranverse)
                {
                    len = m_mainSkt.Receive(buffer);
                    Array.Reverse(buffer, 2, 2);
                    port = BitConverter.ToUInt16(buffer, 2);
                    ipaddr = new System.Net.IPAddress(BitConverter.ToInt64(buffer, 4));
                    skt = new Socket(System.Net.Sockets.AddressFamily.InterNetwork, System.Net.Sockets.SocketType.Stream);
                    skt.SetSocketOption(SocketOptionName.Rendezvous, true);
                    skt.Bind(m_mainSkt.LocalEndPoint);
                    skt.Connect(ipaddr, port);
                    m_tranSkt = m_mainSkt;
                    m_mainSkt = skt;
                }
                while(true)
                {
                    len = m_mainSkt.Receive(buffer);
                    m_form.PutMessage(Encoding.ASCII.GetString(buffer, 0, len));
                }
            }
            catch(SocketException)
            {
                m_form.OnDisconnect();
            }
        }
    }
}
