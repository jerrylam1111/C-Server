using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MySocketServer
{
    class MyFriend
    {
        public Socket socket;
        public byte[] Rcvbuffer;
        public MyFriend(Socket s)
        {
            socket = s;
        }
        public void ClearBuffer()
        {
            Rcvbuffer = new byte[1024];
        }
        //
        public void Dispose()
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }
            finally
            {
                socket = null;
                Rcvbuffer = null;
            }
        }
    }
}
