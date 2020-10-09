using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Controller
{
    class Program
    {
        static void Main(string[] args)
        {
            EndPoint clientEP = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 22222);
            EndPoint localEP = new IPEndPoint(IPAddress.Parse("192.168.1.121"), 33333);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(localEP);
            byte[] buffer;

            while (true)
            {
                string message = Console.ReadLine();
                buffer = Encoding.ASCII.GetBytes(message);
                socket.SendTo(buffer, clientEP);
            }
        }
    }
}
