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
using Meadow.Foundation.Displays.Lcd;
using System.Runtime.Remoting;

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

                ConnectToClient();

                Communicate();

                //ConnectToClient();

                //Communicate();
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
            //Thread conThread = new Thread(() =>
            //{
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
            //});
            //conThread.Start();
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
            clientEP = new IPEndPoint(IPAddress.Parse("192.168.1.173"), 33333);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(localEP);
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

        void Initialize()
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
            ConnectionResult result = Device.WiFiAdapter.Connect("Meadow", "testtest");

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
            if (Device.WiFiAdapter.Networks.Count > 0)
            {
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                Console.WriteLine("|         Network Name             | RSSI |       BSSID       | Channel |");
                Console.WriteLine("|-------------------------------------------------------------|---------|");
                foreach (WifiNetwork accessPoint in Device.WiFiAdapter.Networks)
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