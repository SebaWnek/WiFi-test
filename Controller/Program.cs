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
            EndPoint clientEP = new IPEndPoint(IPAddress.Parse("192.168.1.121"), 22222);
            EndPoint localEP = new IPEndPoint(IPAddress.Any, 33333);
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.Bind(localEP);
            byte[] buffer;
            Stopwatch stopwatch = new Stopwatch();

            Thread reader = new Thread(()=>
            {
                byte[] receiveBuffer = new byte[64];
                while (true)
                {
                    socket.Receive(receiveBuffer);
                    Console.WriteLine(Encoding.ASCII.GetString(receiveBuffer));
                    stopwatch.Stop();
                    Console.WriteLine(stopwatch.ElapsedMilliseconds);
                    receiveBuffer = new byte[64];
                }
            });
            reader.Start();

            while (true)
            {
                string message = Console.ReadLine();
                stopwatch.Reset();
                stopwatch.Start();
                buffer = Encoding.ASCII.GetBytes(message);
                socket.SendTo(buffer, clientEP);
                Console.WriteLine(IPAddress.TryParse(message, out address));
            }
        }
    }
}
