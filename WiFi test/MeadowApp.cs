using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Meadow;
using Meadow.Devices;
using Meadow.Gateway.WiFi;
using Meadow.Hardware;
using System.IO;
using Meadow.Foundation.Leds;
using Meadow.Foundation;
using System.Net.Sockets;
using System.Text;

namespace WiFi_Basics
{
    // **** WINDOWS USERS ****
    // due to a VSWindows bug that links out the System.Net.Http.dll, you must
    // manually edit the .csproj file, uncomment the post-build step, and
    // change the [your user] text to your username.
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        RgbPwmLed onboardLed;
        UdpClient client;
        Socket socket;
        Socket socket2;
        EndPoint localEP;
        EndPoint clientEP;
        IPEndPoint ipLocalEP;
        IPEndPoint ipClientEP;
        TcpListener listener;
        TcpClient tcpClient;

        public MeadowApp()
        {
            try
            {
                Initialize();

                AcceptCommunicationUDP();

                CommunicateUDP();

                //ConnectToClient();

                //CommunicateUDP();
            }
            catch (Exception e)
            {
                onboardLed.StartBlink(Color.Red);
                Console.WriteLine(e.Message);
                throw e;
            }

        }

        private void Communicate()
        {
            Thread conThread = new Thread(() =>
            {
                byte[] buffer = new byte[64];
                while (true)
                {
                    socket.Receive(buffer);
                    onboardLed.SetColor(Color.Green);
                    Console.WriteLine(Encoding.ASCII.GetString(buffer));
                    onboardLed.SetColor(Color.Red);
                    buffer = new byte[64];

                    byte[] response = Encoding.ASCII.GetBytes("Received!");
                    socket.Send(response);
                    onboardLed.SetColor(Color.Blue);
                }
            });
            conThread.Start();
        }

        private void CommunicateUDP()
        {
            Thread conThread = new Thread(() =>
            {
                byte[] buffer;
                while (true)
                {
                    buffer = client.Receive(ref ipClientEP);
                    onboardLed.SetColor(Color.Green);
                    string msg = Encoding.ASCII.GetString(buffer);
                    Console.WriteLine(msg);
                    onboardLed.SetColor(Color.Red);

                    byte[] response = Encoding.ASCII.GetBytes("Received:" + msg);
                    client.Send(response, response.Length);
                    onboardLed.SetColor(Color.Blue);
                }
            });
            conThread.Start();
        }

        private void CommunicateTCP()
        {
            Thread conThread = new Thread(() =>
            {
                NetworkStream stream = tcpClient.GetStream();
                byte[] buffer = new byte[64];
                while (true)
                {
                    stream.Read(buffer, 0, 64);
                    onboardLed.SetColor(Color.Green);
                    Console.WriteLine(Encoding.ASCII.GetString(buffer));
                    onboardLed.SetColor(Color.Red);
                    buffer = new byte[64];

                    byte[] response = Encoding.ASCII.GetBytes("Received!");
                    stream.Write(response, 0, response.Length);
                    stream.Flush();
                    onboardLed.SetColor(Color.Blue);
                }
            });
            conThread.Start();
        }

        private void ConnectToClientTCP()
        {
            ipLocalEP = new IPEndPoint(IPAddress.Any, 22222);
            //ipClientEP = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 33333);
            listener = new TcpListener(ipLocalEP);
            try
            {
                listener.Start();
                Console.WriteLine("Awaiting connection");
                tcpClient = listener.AcceptTcpClient();
                Console.WriteLine("Connected!");
                onboardLed.SetColor(Color.Blue);
                Console.WriteLine("Ready!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void ConnectToClient()
        {
            localEP = new IPEndPoint(IPAddress.Any, 22222);
            ipClientEP = new IPEndPoint(IPAddress.Any, 33333);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            bool connected = false;
            byte[] buffer = new byte[64];
            IPAddress address = null;
            try
            {
                socket.Bind(localEP);
                socket.Receive(buffer);
                ipClientEP = (IPEndPoint)socket.RemoteEndPoint;
                Console.WriteLine(ipClientEP.Address + ":" +ipClientEP.Port);
                socket.Connect(clientEP);
                Console.WriteLine("Connected!");
                onboardLed.SetColor(Color.Blue);
                Console.WriteLine("Ready!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void AcceptCommunication()
        {
            localEP = new IPEndPoint(IPAddress.Any, 22222);
            Console.WriteLine(1);
            socket2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                Console.WriteLine(2);
                socket2.Bind(localEP);
                Console.WriteLine(3);
                socket2.Listen(10);
                Console.WriteLine(4);
                socket = socket2.Accept();
                Console.WriteLine(5);
                Console.WriteLine("Connected!");
                onboardLed.SetColor(Color.Blue);
                Console.WriteLine("Ready!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        private void AcceptCommunicationUDP()
        {
            try
            {
                ipLocalEP = new IPEndPoint(IPAddress.Any, 22222);
                //ipClientEP = /new IPEndPoint(IPAddress.Any, 33333);
                client = new UdpClient(ipLocalEP);
                client.Receive(ref ipClientEP);
                Console.WriteLine(ipClientEP.Address + ":" + ipClientEP.Port);
                client.Connect(ipClientEP);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                throw;
            }
        }

        async void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            onboardLed = new RgbPwmLed(device: Device,
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                3.3f, 3.3f, 3.3f,
                Meadow.Peripherals.Leds.IRgbLed.CommonType.CommonAnode);
            onboardLed.SetColor(Color.Red);


            Console.WriteLine($"Connecting to WiFi Network Meadow");
            Console.WriteLine("Connecting to WiFi Network Meadow");

            Device.InitWiFiAdapter().Wait();
            ScanForAccessPoints();
            ConnectionResult result = Device.WiFiAdapter.Connect("Meadow", "testtest").Result;

            if (result.ConnectionStatus != ConnectionStatus.Success)
            {
                //if (Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD).ConnectionStatus != ConnectionStatus.Success) {
                throw new Exception($"Cannot connect to network: {result.ConnectionStatus}");
            }
            Console.WriteLine("Connection request completed.");
            Console.WriteLine("Connection request completed.");
            onboardLed.SetColor(Color.Green);
        }

        protected void ScanForAccessPoints()
        {
            Console.WriteLine("Getting list of access points.");
            Device.WiFiAdapter.Scan();
            var networks = Device.WiFiAdapter.Scan();
            if (networks.Count > 0)
            {
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                Console.WriteLine("|         Network Name             | RSSI |       BSSID       | Channel |");
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                foreach (WifiNetwork accessPoint in networks)
                {
                    Console.WriteLine($"| {accessPoint.Ssid,-32} | {accessPoint.SignalDbStrength,4} | {accessPoint.Bssid,17} |   {accessPoint.ChannelCenterFrequency,3}   |");
                }
            }
            else
            {
                Console.WriteLine($"No access points detected.");
            }
        }
    }
}