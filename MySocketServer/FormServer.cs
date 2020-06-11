using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Globalization;
using System.IO;

namespace MySocketServer
{
    public partial class FormServer : Form
    {
        public FormServer()
        {
            InitializeComponent();
        }

        ArrayList friends = new ArrayList();

        TcpListener listener;

        bool IsStart = false;

        delegate void AppendDelegate(string str);
        AppendDelegate AppendString;

        delegate void AddDelegate(MyFriend frd);
        AddDelegate Addfriend;

        delegate void RemoveDelegate(MyFriend frd);
        RemoveDelegate Removefriend;

        private void AppendMethod(string str)
        {
            listBoxStatu.Items.Add(str);
            listBoxStatu.SelectedIndex = listBoxStatu.Items.Count - 1;
            listBoxStatu.ClearSelected();
        }
        private void AddMethod(MyFriend frd)
        {
            lock (friends)
            {
                friends.Add(frd);
            }
        }
        private void RemoveMethod(MyFriend frd)
        {
            int i = friends.IndexOf(frd);
            lock (friends)
            {
                friends.Remove(frd);
            }
            frd.Dispose();
        }

        private void FormServer_Load(object sender, EventArgs e)
        {
            AppendString = new AppendDelegate(AppendMethod);
            Addfriend = new AddDelegate(AddMethod);
            Removefriend = new RemoveDelegate(RemoveMethod);
            //getting local IPv4 address
            List<string> listIP = getIP();
            if (listIP.Count == 0)
            {
                this.comboBoxIP.Items.Clear();
                this.comboBoxIP.Text = "Can not get IP address";
            }
            else if (listIP.Count == 1)
            {
                this.comboBoxIP.Items.Add(listIP[0]);
                this.comboBoxIP.SelectedIndex = 0;
            }
            else
            {
                foreach (string str in listIP)
                {
                    this.comboBoxIP.Items.Add(str);
                }
                this.comboBoxIP.Text = "Pls select or enter a IP address";
            }
            //default port
            textBoxServerPort.Text = "6000";
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            DateTime localDate = DateTime.Now;
            String[] cultureNames = { "en-US" };
            if (IsStart)
                return;
            //start listening
            IPEndPoint localep = new IPEndPoint(IPAddress.Parse(comboBoxIP.Text), int.Parse(textBoxServerPort.Text));
            listener = new TcpListener(localep);
            listener.Start(100);
            IsStart = true;
            listBoxStatu.Invoke(AppendString, string.Format("Start listening to: ", listener.LocalEndpoint.ToString()));
            AsyncCallback callback = new AsyncCallback(AcceptCallBack);
            listener.BeginAcceptSocket(callback, listener);
            this.buttonStart.Enabled = false;
        }
        private void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                Socket handle = listener.EndAcceptSocket(ar);
                MyFriend frd = new MyFriend(handle);
                AsyncCallback callback;
                if (IsStart)
                {
                    callback = new AsyncCallback(AcceptCallBack);
                    listener.BeginAcceptSocket(callback, listener);
                }
                //async
                frd.ClearBuffer();
                callback = new AsyncCallback(ReceiveCallback);
                frd.socket.BeginReceive(frd.Rcvbuffer, 0, frd.Rcvbuffer.Length, SocketFlags.None, callback, frd);
            }
            catch
            {
                IsStart = false;
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            var csv = new StringBuilder();
            MyFriend frd = (MyFriend)ar.AsyncState;
            try
            {
                int i = frd.socket.EndReceive(ar);
                if (i == 0)
                {
                    return;
                }
                else
                {
                    string data = Encoding.UTF8.GetString(frd.Rcvbuffer, 0, i);
                    data = string.Format("From[{0}]:{1}", frd.socket.RemoteEndPoint.ToString(), data);
                    listBoxStatu.Invoke(AppendString, data);
                    frd.ClearBuffer();
                    AsyncCallback callback = new AsyncCallback(ReceiveCallback);
                    frd.socket.BeginReceive(frd.Rcvbuffer, 0, frd.Rcvbuffer.Length, SocketFlags.None, callback, frd);

                    TextWriter tw = new StreamWriter("C:/Users/jerry/Desktop/data.csv", true);
                    tw.WriteLine(data);
                    tw.Close();
                }
            }
            catch
            {

            }
        }



        /*private void SendData(MyFriend frd, string data)
        {
            try
            {
                byte[] msg = Encoding.UTF8.GetBytes(data);
                AsyncCallback callback = new AsyncCallback(SendCallback);
                frd.socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, callback, frd);
                data = string.Format("To[{0}]:{1}", frd.socket.RemoteEndPoint.ToString(), data);
                listBoxStatu.Invoke(AppendString, data);
            }
            catch
            {
                
            }
        }*/

            /*
        private void SendCallback(IAsyncResult ar)
        {
            MyFriend frd = (MyFriend)ar.AsyncState;
            try
            {
                frd.socket.EndSend(ar);
            }
            catch
            {
                
            }
        }
        */

        private void buttonStop_Click(object sender, EventArgs e)
        {
            if (!IsStart)
                return;
            listener.Stop();
            IsStart = false;
            listBoxStatu.Invoke(AppendString, "Ended listening");
            this.buttonStart.Enabled = true;
        }

        private void buttonClear_Click(object sender, EventArgs e)
        {
            this.listBoxStatu.Items.Clear();
        }

        //getting local IPv4 address
        public List<string> getIP()
        {
            List<string> listIP = new List<string>();
            try
            {
                string HostName = Dns.GetHostName(); //getting the host name
                IPHostEntry IpEntry = Dns.GetHostEntry(HostName);
                for (int i = 0; i < IpEntry.AddressList.Length; i++)
                {
                    if (IpEntry.AddressList[i].AddressFamily == AddressFamily.InterNetwork)
                    {
                        listIP.Add(IpEntry.AddressList[i].ToString());
                    }
                }
                return listIP;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not get the local IP address" + ex.Message);
                listIP.Clear();
                return listIP;
            }
        }
    }
}
