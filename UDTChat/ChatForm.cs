using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UDTChat
{
    public partial class ChatForm : Form
    {
        ChatClient m_client = null;
        ChatServer m_server = null;
        public ChatForm()
        {
            InitializeComponent();
        }

        void SwitchState(bool bConn)
        {
            textAddress.Enabled = !bConn;
            textBoxPort.Enabled = !bConn;
            buttonConnect.Enabled = !bConn;
            buttonListen.Enabled = !bConn;
            buttonSend.Enabled = bConn;
        }

        public void PutMessage(string strMsg)
        {
            this.BeginInvoke(new Action(() =>
            {
                textChat.Text += strMsg + "\r\n";
            }));
        }

        private void ButtonConnect_Click(object sender, EventArgs e)
        {
            int port = Int32.Parse(textBoxPort.Text);
            m_client = ChatClient.Connect(textAddress.Text, port, this);
            if(m_client==null)
            {
                MessageBox.Show("连接失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SwitchState(true);
        }

        private void ButtonListen_Click(object sender, EventArgs e)
        {
            int port = Int32.Parse(textBoxPort.Text);
            m_server = new ChatServer();
            m_server.Parent = this;
            if(!m_server.StartServer(port))
            {
                MessageBox.Show("开启服务器失败！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            SwitchState(true);
        }

        private void ButtonSend_Click(object sender, EventArgs e)
        {
            if(m_server!=null)
            {
                m_server.Broadcast(textMessage.Text);
            }
            else if(m_client!=null)
            {
                m_client.SendMessage(textMessage.Text);
            }
            else
            {
                return;
            }
            textMessage.Text = "";
        }

        private void ChatForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (m_server != null)
                m_server.Close();
            if (m_client != null)
                m_client.Close();
        }
    }
}
