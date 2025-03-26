using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace L20250318_UDPServer
{
    class Program
    {
        static void Main(string[] args)
        {

            Socket serverSocket = new Socket(AddressFamily.InterNetwork,SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint serverEndPoint = new IPEndPoint(IPAddress.Any, 6000);

            serverSocket.Bind(serverEndPoint);

            byte[] buffer = new byte[1024];
            string message = "안녕하세요.";
            buffer = Encoding.UTF8.GetBytes(message);
            
            int SendLength = serverSocket.SendTo(buffer, buffer.Length, SocketFlags.None, serverEndPoint);

            byte[] buffer2 = new byte[1024];
            EndPoint remoteEndPoint = serverEndPoint;
            //EndPoint clientEndPoint = (EndPoint)serverEndPoint;
            int RecvLength = serverSocket.ReceiveFrom(buffer2, ref remoteEndPoint);

            //int RecvLength = serverSocket.ReceiveFrom(buffer, ref clientEndPoint);

            //int SendLenght = serverSocket.SendTo(buffer, clientEndPoint);

            serverSocket.Close();

            //length = BitConverter.ToInt16(lengthBuffer2, 0);

            // Buffer.BloackCopy(4 paramter) 용법 
        }
    }
}
