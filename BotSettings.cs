using Darts_for_people.Entities;
using System.Data;

namespace Darts_for_people
{
    public static class BotSettings
    {
        private static string _connectionString = ReadConnectionStringAsync().Result; // Строка подключения к БД
        private static readonly DatabaseManager _database = new(_connectionString);

        /// <summary>
        /// получает расписание из БД и возвращает список дней недели, которые были запланированы.
        /// </summary>
        /// <returns>Список дней недели.</returns>
        public static async Task<List<DayOfWeek>> GetScheduledDaysOfWeekAsync()
        {
            DataTable data = await _database.ExecuteQueryAsync("select days from settings"); // Вытягиваем расписание из БД
            List<DayOfWeek>? scheduledDays = new();

            // Парсим данные
            scheduledDays = data.Rows[0]["days"]
                .ToString()
                ?.Split(',')
                .Select(day => Enum.Parse<DayOfWeek>(day, ignoreCase: true))
                .ToList();

            // Если в БД ничего нет, возвращаем пустой список
            if (scheduledDays is null)
                return new();

            return scheduledDays;
        }

        /// <summary>
        /// получает токен бота из БД и возвращает его.
        /// </summary>
        /// <returns>Токен бота.</returns>
        public static async Task<string> GetTokenAsync()
        {
            DataTable data = await _database.ExecuteQueryAsync("select token from settings"); // Вытягиваем токен из БД
            string token = data?.Rows[0]["token"].ToString() ?? "";

            return token;
        }

        /// <summary>
        /// Получает идентификатор чата из БД и возвращает его.
        /// </summary>
        /// <returns>Id чата</returns>
        /// <remarks>Если Id чата отсутсвует в БД, метод возвращает 0.</remarks>
        public static async Task<long> GetChatIdAsync()
        {
            DataTable data = await _database.ExecuteQueryAsync("select chat_id from settings"); // Вытягиваем расписание из БД
            long chatId = 0;

            // Парсим Id чата 
            if (long.TryParse(data?.Rows[0]["chat_id"].ToString(), out long result))
                chatId = result;

            return chatId;
        }

        /// <summary>
        /// Получает список поздравлений из БД и возвращает их в виде списка строк.
        /// </summary>
        /// <returns>Список поздравлений.</returns>
        public async static Task<List<string>> GetBirthdayCaseWordAsync()
        {
            DataTable data = await _database.ExecuteQueryAsync("select birthday_case from word_case"); // Вытягиваем поздравления из БД
            List<string> birthdayMessages = new();

            // Перебираем полученную таблицу
            for (int i = 0; i < data?.Rows.Count; i++)
            {
                string? message = data?.Rows[i]["birthday_case"].ToString();

                if (!string.IsNullOrEmpty(message))
                    birthdayMessages.Add(message);
            }

            return birthdayMessages;
        }

        /// <summary>
        /// Получает получает список реакций на тишину в чате из БД и возвращает их в виде списка строк.
        /// </summary>
        /// <returns>Список реакций</returns>
        public async static Task<List<string>> GetSilenceCaseWordAsync()
        {
            DataTable data = await _database.ExecuteQueryAsync("select silence_case from word_case"); // Вытягиваем расписание из БД
            List<string> silenceMessages = new();

            // Перебираем полученную таблицу
            for (int i = 0; i < data?.Rows.Count; i++)
            {
                string? message = data?.Rows[i]["silence_case"].ToString();

                if (!string.IsNullOrEmpty(message))
                    silenceMessages.Add(message);
            }

            return silenceMessages;
        }

        /// <summary>
        /// Получает список людей и информацию о них из БД и возвращает их в виде списка объектов Person
        /// </summary>
        /// <returns>Список людей и информацию о них.</returns>
        public static async Task<List<Person>> GetPersonAsync()
        {
            DataTable data = await _database.ExecuteQueryAsync("select * from person"); // Вытягиваем данные о людях из БД
            List<Person> listOfPersons = new();

            // Перебираем полученную таблицу
            for (int i = 0; i < data?.Rows.Count; i++)
            {
                Person person = new()
                {
                    Name = data.Rows[i]["first_name"].ToString() ?? string.Empty,
                    Surname = data.Rows[i]["second_name"].ToString() ?? string.Empty,
                    Id = data.Rows[i]["user_id"].ToString() ?? string.Empty,
                    BirthDate = DateOnly.FromDateTime((DateTime)data.Rows[i]["birth_date"])
                };

                // Если поля пустые, перебираем таблицу дальше
                if (string.IsNullOrEmpty(person.Name) || string.IsNullOrEmpty(person.Surname))
                    continue;

                listOfPersons.Add(person);
            }

            return listOfPersons;
        }

        /// <summary>
        /// Читает INI-файл и возвращает строку подключения к БД.
        /// </summary>
        /// <returns>Строка подключения к БД.</returns>
        public static async Task<string> ReadConnectionStringAsync()
        {
            using (StreamReader reader = new StreamReader($"{Environment.CurrentDirectory}\\Settings.ini"))
            {
                string line = await reader.ReadLineAsync() ?? string.Empty;
                return line;
            }
        }

        /// <summary>
        /// Записывает Id чата в БД.
        /// </summary>
        /// <param name="chatId">Id чата</param>
        public static async Task UpdateChatId(long chatId)
        {
            await _database.ExecuteQueryAsync(@$"UPDATE settings SET chat_id = {chatId} WHERE token = (SELECT token FROM settings LIMIT 1);");
        }
    }
}