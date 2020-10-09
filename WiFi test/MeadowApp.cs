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
        II2cBus bus;
        I2cCharacterDisplay display;
        EndPoint localEP;
        EndPoint clientEP;
        IPEndPoint ipLocalEP;
        IPEndPoint ipClientEP;

        public MeadowApp()
        {
            try
            {
                Initialize();

                ConnectToClientTCP();

                CommunicateTCP(); 
                
                //ConnectToClient();

                //Communicate();
            }
            catch (Exception e)
            {
                onboardLed.StartBlink(Color.Red);
                display.ClearLines();
                display.Write(e.Message);
                throw e;
            }

        }

        private void Communicate()
        {
            Task.Run(() =>
            {
            byte[] buffer = new byte[1024];
            while (true)
                {
                    socket.ReceiveFrom(buffer, ref clientEP);
                    Console.WriteLine(Encoding.ASCII.GetString(buffer));

                    display.ClearLines();
                    display.Write(Encoding.ASCII.GetString(buffer));
                    buffer = new byte[1024];

                    byte[] response = Encoding.ASCII.GetBytes("Received!");
                    socket.SendTo(response, clientEP);
                }
            });
        }

        private void CommunicateTCP()
        {
            throw new NotImplementedException();
        }

        private void ConnectToClientTCP()
        {
            ipLocalEP = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 22222);
            ipClientEP = new IPEndPoint(IPAddress.Parse("192.168.1.121"), 33333);
            TcpListener listener = new TcpListener(ipLocalEP);
            try
            {
                listener.Start();
                Console.WriteLine("Connected!");
                onboardLed.SetColor(Color.Blue);
                display.Write("Ready!");
            }
            catch (Exception e)
            {
                display.Write(e.Message);
                throw;
            }
        }

        private void ConnectToClient()
        {
            localEP = new IPEndPoint(IPAddress.Parse("192.168.1.100"), 22222);
            clientEP = new IPEndPoint(IPAddress.Parse("192.168.1.121"), 33333);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            try
            {
                socket.Bind(localEP);
                Console.WriteLine("Connected!");
                onboardLed.SetColor(Color.Blue);
                display.Write("Ready!");
            }
            catch (Exception e)
            {
                display.Write(e.Message);
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

            bus = Device.CreateI2cBus();
            display = new I2cCharacterDisplay(bus, 0x27, 2, 16);

            Console.WriteLine($"Connecting to WiFi Network Meadow");
            display.Write("Connecting to WiFi Network Meadow");

            Device.InitWiFiAdapter().Wait();
            ConnectionResult result = Device.WiFiAdapter.Connect("Meadow", "testtest");

            if (result.ConnectionStatus != ConnectionStatus.Success)
            {
                //if (Device.WiFiAdapter.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD).ConnectionStatus != ConnectionStatus.Success) {
                throw new Exception($"Cannot connect to network: {result.ConnectionStatus}");
            }
            Console.WriteLine("Connection request completed.");
            display.ClearLines();
            display.Write("Connection request completed.");
            onboardLed.SetColor(Color.Green);
        }
    }
}