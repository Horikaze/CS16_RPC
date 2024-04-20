using DiscordRPC;
using Swed32;

namespace DiscordGameIntegration
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (DiscordRpcClient client = new DiscordRpcClient("1230499422404739183"))
            {
                client.Initialize();

                client.SetPresence(new RichPresence()
                {
                    Details = "Playing on map: ",
                    State = "Nickname: ",
                    Timestamps = Timestamps.Now,
                    Assets = new Assets()
                    {
                        LargeImageKey = "logo",
                        LargeImageText = "Counter-Strike 1.6",
                    }
                });

                Swed swed = new("hl");
                IntPtr moduleBase = swed.GetModuleBase("hw.dll");
                IntPtr mapAddress = swed.ReadPointer(moduleBase, 0x000B587C) + 0x4;
                IntPtr nickAddress = swed.ReadPointer(moduleBase, 0x00019B8C) + 0x64;
                IntPtr IPAddress = swed.ReadPointer(moduleBase, 0x00024BA0);

                System.Console.WriteLine(swed);

                string currentMap = "";
                string currentNick = "";
                string currentIP = "";

                try
                {
                    while (true)
                    {
                        UpdatePresence(swed, client, mapAddress, nickAddress, IPAddress, ref currentMap, ref currentNick, ref currentIP);
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static void UpdatePresence(Swed swed, DiscordRpcClient client, IntPtr mapAddress, IntPtr nickAddress, IntPtr IPAddress, ref string currentMap, ref string currentNick, ref string currentIP)
        {
            string mapString = ReadString(swed, mapAddress, 16);
            string nickString = ReadString(swed, nickAddress, 32);
            string IPString = ReadString(swed, IPAddress, 32);

            string map = SliceString(mapString, '/', '.');
            string nick = SliceNick(nickString);
            string IP = SliceIP(IPString);

            if (currentIP != IP)
            {
                currentIP = IP;
                client.UpdateDetails($"Playing on: {currentIP}");
                Console.WriteLine("Updated ip");
            }

            if (currentMap != map)
            {
                currentMap = map;
                client.UpdateState($"Nickname: {currentNick} Map: {currentMap}");
                Console.WriteLine("Updated map");
            }

            if (currentNick != nick)
            {
                currentNick = nick;
                client.UpdateState($"Nickname: {currentNick} Map: {currentMap}");
                Console.WriteLine("Updated nick");
            }
        }

        static string ReadString(Swed swed, IntPtr address, int size)
        {
            byte[] bytes = swed.ReadBytes(address, size);
            return System.Text.Encoding.Default.GetString(bytes);


        }

        static string SliceString(string input, char startChar, char endChar)
        {
            int startIndex = input.IndexOf(startChar);
            int endIndex = input.IndexOf(endChar);

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return input.Substring(startIndex + 1, endIndex - startIndex - 1);
            }
            else
            {
                return "";
            }
        }

        static string SliceNick(string input)
        {

            string[] sliced = input.Split('\\');
            string nick = sliced[1];

            return nick;

        }
        static string SliceIP(string input)
        {
            if (input.StartsWith("local"))
            {
                return "local";
            }
            return input;
        }
    }
}
