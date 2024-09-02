using Haruka.Arcade.SEGA835Lib.Debugging;
using Haruka.Arcade.SEGA835Lib.Devices;
using Haruka.Arcade.SEGA835Lib.Devices.LED._837_15093;
using Haruka.Arcade.SEGA835Lib.Misc;
using System.IO.Pipes;

namespace SynchronicaLEDToSega {

    internal class Program {

        private static byte[] led1 = new byte[30];
        private static byte[] led2 = new byte[30];
        private static byte[] led3 = new byte[30];
        private static byte[] led4 = new byte[30];
        private static byte[] led5 = new byte[30];
        private static byte[] ledl = new byte[6];
        private static byte[] ledr = new byte[6];
        private static byte[] ledc = new byte[21];

        static int Main(string[] args) {

            if (args.Length < 1) {
                Console.WriteLine("Usage: SynchronicaLEDToSega.exe <led_port_no>");
                return 1;
            }

            Console.WriteLine("SynchronicaLEDToSega v1.0");
            Console.WriteLine("2024 Haruka");
            Console.WriteLine("-----------------------------");

            int port = Int32.Parse(args[0]);

            NamedPipeServerStream pipe = new NamedPipeServerStream("led_debug_port", PipeDirection.In);

            LED_837_15093_06 led = new LED_837_15093_06(port);
            DeviceStatus ret = led.Connect();
            if (ret != DeviceStatus.OK) {
                Console.WriteLine("LED connection failed");
                return 2;
            }

            ret = led.GetBoardInfo(out string bn, out string cn, out byte fv);
            if (ret != DeviceStatus.OK) {
                Console.WriteLine("Retrieving board info failed");
                return 3;
            }

            ret = led.SetResponseDisabled(true);
            if (ret != DeviceStatus.OK) {
                Console.WriteLine("Failed to disable responses");
                return 4;
            }

            Console.WriteLine("Board Name: " + bn);
            Console.WriteLine("Chip Number: " + cn);
            Console.WriteLine("Firmware Version: " + fv);

            Console.WriteLine("Waiting for Synchronica...");
            pipe.WaitForConnection();

            Console.WriteLine("Running...");

            while (true) {

                int cmd = pipe.ReadByte();

                if (cmd == 0x80) {
                    pipe.Read(led1, 0, led1.Length);
                } else if (cmd == 0x81) {
                    pipe.Read(led2, 0, led2.Length);
                } else if (cmd == 0x82) {
                    pipe.Read(led3, 0, led3.Length);
                } else if (cmd == 0x83) {
                    pipe.Read(led4, 0, led4.Length);
                } else if (cmd == 0x84) {
                    pipe.Read(led5, 0, led5.Length);
                } else if (cmd == 0x86) {
                    pipe.Read(ledl, 0, ledl.Length);
                } else if (cmd == 0x87) {
                    pipe.Read(ledr, 0, ledr.Length);
                } else if (cmd == 0x85) {
                    pipe.Read(ledc, 0, ledc.Length);
                } else if (cmd == 0xFF) {
                    List<Color> c = new List<Color>();
                    AddAllLeds(c, led1);
                    AddAllLeds(c, led2);
                    AddAllLeds(c, led3);
                    AddAllLeds(c, led4);
                    AddAllLeds(c, led5);
                    AddAllLeds(c, ledl);
                    AddAllLeds(c, ledr);
                    AddAllLeds(c, ledc, true);
                    led.SetLEDs(c);
                } else {
                    Console.WriteLine("Invalid cmd: " + cmd.ToString("X2"));
                }

                Thread.Sleep(1);

            }

        }

        private static void AddAllLeds(List<Color> c, byte[] leds, bool is_abs = false) {
            for (int i = 0; i < leds.Length / 3; i++) {
                if (is_abs) {
                    c.Add(Color.FromArgb((byte)(leds[i * 3] != 1 ? 255 : 0), (byte)(leds[i * 3 + 1] != 1 ? 255 : 0), (byte)(leds[i * 3 + 2] != 1 ? 255 : 0)));
                } else {
                    c.Add(Color.FromArgb(leds[i * 3], leds[i * 3 + 1], leds[i * 3 + 2]));
                }
            }
        }
    }
}
