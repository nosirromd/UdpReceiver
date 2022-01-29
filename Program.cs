using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpReceiver
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (!ParseCommandLine(args, out int port, out string groupAddress))
            {
                ShowUsage();
                return;
            }
            
            //open an endpoint and read util bye is sent
            await ReaderAsync(port, groupAddress);
            Console.ReadLine();
        }

        private static void ShowUsage() =>
            Console.WriteLine("Usage: UdpReceiver -p port  [-g groupaddress]");

        private static bool ParseCommandLine(string[] args, out int port, out string groupAddress)
        {
            //initalise
            port = 0;
            groupAddress = string.Empty;
            
            
            //exit if don't get the noumber if args we expect
            if (args.Length < 2 || args.Length > 5)
            {
                return false;
            }

            //exit if don't get a portnumber ti listen on
            if (args.SingleOrDefault(a => a == "-p") == null)
            {
                Console.WriteLine("-p required");
                return false;
            }

            // get port number
            string port1 = GetValueForKey(args, "-p");
            if (port1 == null || !int.TryParse(port1, out port))
            {
                return false;
            }

            // get optional group address
            groupAddress = GetValueForKey(args, "-g");
            return true;
        }

        private static string GetValueForKey(string[] args, string key)
        {
            //transform arg array into a collection of anon structs comprising argument and it's index
            //get the index of the arg that mtaches the supplied key
            //increment by one
            //use the index to locate the parameter value and return it to the calling context
            int? nextIndex = args.Select((a, i) => new { Arg = a, Index = i }).SingleOrDefault(a => a.Arg == key)?.Index + 1;
            if (!nextIndex.HasValue)
            {
                return null;
            }
            return args[nextIndex.Value];
        }

        private static async Task ReaderAsync(int port, string groupAddress)
        {
            //open a udp client object
            using (var client = new UdpClient(port))
            {
                //join a multicast group if one is specified
                if (groupAddress != null)
                {
                    client.JoinMulticastGroup(IPAddress.Parse(groupAddress));
                    Console.WriteLine($"joining the multicast group {IPAddress.Parse(groupAddress)}");
                }

                // while bye is not recived
                //read datagram bytes and convert into string character
                //write them on the console until we recieve bye
                //drop out the mullllticast group when we exit
                bool completed = false;
                do
                {
                    Console.WriteLine("starting the receiver");
                    UdpReceiveResult result = await client.ReceiveAsync();
                    byte[] datagram = result.Buffer;
                    string received = Encoding.UTF8.GetString(datagram);
                    Console.WriteLine($"received {received}");
                    if (received == "bye")
                    {
                        completed = true;
                    }
                } while (!completed);
                Console.WriteLine("receiver closing");

                if (groupAddress != null)
                {
                    client.DropMulticastGroup(IPAddress.Parse(groupAddress));
                }
            }
        }
    }
}
