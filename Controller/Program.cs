using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Controller
{
    class Program
    {
        static void Main(string[] args)
        {
            IPAddress address;
            Console.WriteLine("Client IP:");
            string ipString = Console.ReadLine();
            IPEndPoint clientEP = new IPEndPoint(IPAddress.Parse(ipString), 22222);
            IPEndPoint localEP = new IPEndPoint(IPAddress.Any, 33333);
            //Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            //socket.Bind(localEP);
            //socket.Connect(clientEP);
            byte[] buffer;
            UdpClient client = new UdpClient(localEP);
            client.Connect(clientEP);
            Stopwatch stopwatch = new Stopwatch();
            Console.WriteLine("Send anything to connect:");

            Thread reader = new Thread(()=>
            {
                byte[] receiveBuffer = new byte[64];
                while (true)
                {
                    receiveBuffer = client.Receive(ref clientEP);
                    Console.WriteLine(Encoding.ASCII.GetString(receiveBuffer));
                    stopwatch.Stop();
                    Console.WriteLine(stopwatch.ElapsedMilliseconds);
                }
            });
            reader.Start();

            while (true)
            {
                string message = Console.ReadLine();
                stopwatch.Reset();
                stopwatch.Start();
                buffer = Encoding.ASCII.GetBytes(message);
                client.Send(buffer, buffer.Length);
            }
        }
    }
}
