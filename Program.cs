using DiscordRPC;
using Swed32;
using System.Text.RegularExpressions;
using System.Diagnostics;
namespace DiscordGameIntegration
{
    public class Program
    {
        static void Main(string[] args)
        {

            static string GetNamePropertyValue(string filePath)
            {
                if (!File.Exists(filePath))
                {
                    return "";
                }
                string[] lines = File.ReadAllLines(filePath);
                string pattern = @"^\s*name\s*""([^""]*)""";
                foreach (string line in lines)
                {
                    Match match = Regex.Match(line, pattern);

                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
                return "";
            }
            try
            {
                using (DiscordRpcClient client = new DiscordRpcClient("1230499422404739183"))
                {
                    // Get all processes with the specified name
                    Process[] processes = Process.GetProcessesByName("hl");

                    if (processes.Length <= 0)
                    {
                        throw new Exception("hl.exe not found.");
                    }


                    Swed swed = new("hl");
                    IntPtr moduleBase = swed.GetModuleBase("hw.dll");
                    IntPtr mapAddress = swed.ReadPointer(moduleBase, 0x000B587C) + 0x4;
                    IntPtr IPAddress = swed.ReadPointer(moduleBase, 0x00024BA0);

                    string gameCfgPath = processes[0].MainModule!.FileName.Replace("\\hl.exe", "") + "\\cstrike\\config.cfg";

                    string currentMap = "";
                    string currentNick = GetNamePropertyValue(gameCfgPath);
                    string currentIP = "";

                    client.Initialize();
                    client.SetPresence(new RichPresence()
                    {
                        Details = "Playing on map: ",
                        State = $"Nickname: {currentNick}",
                        Timestamps = Timestamps.Now,
                        Assets = new Assets()
                        {
                            LargeImageKey = "logo",
                            LargeImageText = "Counter-Strike 1.6",
                        }
                    });


                    while (true)
                    {
                        UpdatePresence(swed, client, mapAddress, IPAddress, ref currentMap, ref currentNick, ref currentIP);
                        Thread.Sleep(1000);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.ReadLine();
            }
        }

        static void UpdatePresence(Swed swed, DiscordRpcClient client, IntPtr mapAddress, IntPtr IPAddress, ref string currentMap, ref string currentNick, ref string currentIP)
        {
            string mapString = ReadString(swed, mapAddress, 16);
            string IPString = ReadString(swed, IPAddress, 32);
            System.Console.WriteLine(mapString);
            string map = SliceMap(mapString, '/', '.');
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
        }

        static string ReadString(Swed swed, IntPtr address, int size)
        {
            byte[] bytes = swed.ReadBytes(address, size);
            return System.Text.Encoding.Default.GetString(bytes);
        }

        static string SliceMap(string input, char startChar, char endChar)
        {
            int startIndex = input.IndexOf(startChar);
            int endIndex = input.IndexOf(endChar);

            if (startIndex >= 0 && endIndex > startIndex)
            {
                return input.Substring(startIndex + 1, endIndex - startIndex - 1);
            }
            else
            {
                return "de_dust2";
            }
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
