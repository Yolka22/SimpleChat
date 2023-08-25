using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ChatServer
{
    public partial class Form1 : Form
    {
        private int port;
        private IPAddress ip_address;
        private IPEndPoint end_point;
        private Socket server;
        private Thread workThread;


        public Form1()
        {
            InitializeComponent();
            startButton.Enabled = true;
            stopButton.Enabled = false;
        }

        private void ThreadMethod()
        {
            byte[] buffer1 = new byte[1024];
            byte[] buffer2 = new byte[1024];

            string clientMessage = "";
            string serverMessage = "";

            int recieveBytes = 0;

            try
            {
                while (true)
                {
                    Socket handler = server.Accept();
                    recieveBytes = handler.Receive(buffer1);
                    clientMessage = Encoding.UTF8.GetString(buffer1);

                    if(clientMessage == "/GET CHAT")
                    {
                        messagesBox.Invoke(new Action(() =>{
                            foreach (var item in messagesBox.Items)
                            {
                                serverMessage+= item.ToString();
                            }
                        })
                        );
                        handler.Send(Encoding.UTF8.GetBytes(serverMessage));
                        
                    }
                    else
                    {
                        messagesBox.Invoke(new Action(() => {
                            messagesBox.Items.Add(DateTime.Now.ToString()+" ->" + clientMessage);
                        })
                        );
                    }
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message, "Exception in server thread",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void startButton_Click(object sender, EventArgs e)
        {
            try
            {
                ip_address = IPAddress.Parse(ipTextBox.Text);
                port = int.Parse(portTextBox.Text);
                end_point = new IPEndPoint(ip_address, port);

                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                server.Bind(end_point);
                server.Listen(1000);

                workThread = new Thread(new ThreadStart(ThreadMethod))
                {
                    IsBackground = true
                };
                workThread.Start();

                startButton.Enabled = false;
                stopButton.Enabled = true;

                statusLabel.ForeColor = Color.Green;

                statusLabel.Text = "server online";
                messagesBox.Items.Add(DateTime.Now.ToString() + " -> Server Online");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception on server starting",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        [Obsolete]
        private void stopButton_Click(object sender, EventArgs e)
        {
            try
            {
                workThread.Suspend();
                server.Close();

                statusLabel.ForeColor = Color.Red;

                statusLabel.Text = "server ofline";
                messagesBox.Items.Add(DateTime.Now.ToString() + " -> Server Ofline");

                startButton.Enabled = true;
                stopButton.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Exception on program closing",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);

            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {

        }
    }
}
