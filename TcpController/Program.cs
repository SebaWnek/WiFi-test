using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpController
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint clientEP = new IPEndPoint(IPAddress.Parse("192.168.1.121"), 22222);
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 33333);
            TcpClient client = new TcpClient(localEP);
            Console.ReadLine();
            client.Connect(clientEP);
            Console.WriteLine("Connected!");
            byte[] buffer;
            NetworkStream stream = client.GetStream();
            while (true)
            {
                string message = Console.ReadLine();
                buffer = Encoding.ASCII.GetBytes(message);
                stream.Write(buffer, 0, buffer.Length);
                stream.Flush();
                //stream.Read(buffer, 0, 64);
                //Console.WriteLine(Encoding.ASCII.GetString(buffer));
            }
        }
    }
}
