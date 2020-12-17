using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Udt;

namespace UDTChat
{
    class ChatClient
    {
        Socket m_mainSkt = null;
        Thread m_th = null;
        ChatForm m_form = null;

        private ChatClient()
        {

        }

        public static ChatClient Connect(string strAddress,int nPort,ChatForm form)
        {
            ChatClient cc = new ChatClient();
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
            m_mainSkt.Close();
            m_mainSkt = null;
        }

        void ClientThread()
        {
            byte[] buffer = new byte[1024];
            int len;
            try
            {
                while(true)
                {
                    len = m_mainSkt.Receive(buffer);
                    m_form.PutMessage(Encoding.ASCII.GetString(buffer, 0, len));
                }
            }
            catch(SocketException)
            {

            }
        }
    }
}
