using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darts_for_people
{
    public class BotSettings
    {
        public static async Task<List<DayOfWeek>> GetDays() 
        {
            Dictionary<string, string> ini = await ReadIniFileAsync();
            var daysOfWeek = ini["Days"].Split(',').Select(day => Enum.Parse<DayOfWeek>(day, ignoreCase: true)).ToList();

            return daysOfWeek;
        }

        public static async Task<string> GetToken()
        {
            Dictionary<string, string> ini = await ReadIniFileAsync();
            string token = ini["Token"].ToString();

            return token;
        }

        public static async Task<List<DayOfWeek>> GetWordCase()
        {
            Dictionary<string, string> ini = await ReadIniFileAsync();
            var daysOfWeek = ini["Days"].Split(',').Select(day => Enum.Parse<DayOfWeek>(day, ignoreCase: true)).ToList();

            return daysOfWeek;
        }

        public static async Task<List<DayOfWeek>> GetPerson()
        {
            Dictionary<string, string> ini = await ReadIniFileAsync();
            var daysOfWeek = ini["Days"].Split(',').Select(day => Enum.Parse<DayOfWeek>(day, ignoreCase: true)).ToList();

            return daysOfWeek;
        }

        private static async Task<Dictionary<string, string>> ReadIniFileAsync()
        {
            var config = new Dictionary<string, string>();
            var lines = await File.ReadAllLinesAsync($"{Environment.CurrentDirectory}\\Settings.ini");

            foreach (var line in lines)
            {
                if (line.Contains('='))
                {
                    var parts = line.Split('=');
                    if (parts.Length == 2)
                    {
                        config[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }

            return config;
        }
    }
}
