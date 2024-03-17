using Darts_for_people.Entities;
using Quartz;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Darts_for_people
{
    public class BotMethods : IJob
    {
        private static Timer _timerOfSilence = null!;

        /// <summary>
        /// Вызывается для запуска методов голосования и поздравления с ДР, когда срабатывает тригер.
        /// </summary>
        public async Task Execute(IJobExecutionContext context)
        {
            // Вытаскиваем нужные нам объекты из context
            ITelegramBotClient botClient = (ITelegramBotClient)context.JobDetail.JobDataMap["botClient"];
            CancellationToken token = (CancellationToken)context.JobDetail.JobDataMap["token"];

            // Вызываем методы
            await ScheduleDartsPollAsync(botClient, token);
            await SendBirthdayWishesAsync(botClient, token);
        }

        /// <summary>
        /// Отправляет голосование в указанные дни недели в заданное время.
        /// </summary>
        /// <param name="botClient">Клиент Telegram для отправки сообщений.</param>
        /// <param name="token">Токен отмены операции.</param>
        public async Task ScheduleDartsPollAsync(ITelegramBotClient botClient, CancellationToken token)
        {
            try
            {
                List<DayOfWeek> scheduledDays = await BotSettings.GetScheduledDaysOfWeekAsync(); // Получаем расписание
                DayOfWeek now = DateTime.Now.DayOfWeek;
                long chatID = await BotSettings.GetChatIdAsync();

                // Если сегодняшний день есть в расписании
                if (scheduledDays.Contains(now) && DateTime.Now.Hour == 8)
                {
                    // Отправляем голосование в чат
                    await botClient.SendPollAsync(
                    chatId: chatID,
                    question: "В дартс идём?",
                    isAnonymous: false,
                    options: new[]
                    {
                        "Даааа",
                        "Неет(("
                    },
                    cancellationToken: token);

                    await Console.Out.WriteLineAsync("Голосование успешно создано.");
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Голосование было отменено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправляет поздравления с Днём Рождения пользователям, если таковой имеется.
        /// </summary>
        /// <param name="botClient">Клиент Telegram для отправки сообщений.</param>
        /// <param name="token">Токен отмены операции.</param>
        public async Task SendBirthdayWishesAsync(ITelegramBotClient botClient, CancellationToken token)
        {
            try
            {
                List<Person> listOfPersons = await BotSettings.GetPersonAsync(); // Получаем список пользователей
                List<string> birthdayCaseWord = await BotSettings.GetBirthdayCaseWordAsync(); // Получаем варианты поздравлений
                long chatID = await BotSettings.GetChatIdAsync();                

                // Проверяем, есть ли сегодня у кого-нибудь ДР
                foreach (var item in listOfPersons)
                {
                    DateOnly dateNow = DateOnly.FromDateTime(DateTime.Now);
                    DateOnly birthDate = item.BirthDate;

                    // Если сегодня у кого-то ДР
                    if (dateNow.Day == birthDate.Day && dateNow.Month == birthDate.Month)
                    {
                        Random random = new();

                        // Собираем сообщение с поздравлением
                        string messageText = $"Сегодня празднует День Рождения <a href=\"https://t.me/{item.Id}\">{item.Name} {item.Surname}</a>! " +
                        $"Ему исполнилось {dateNow.Year - birthDate.Year} лет! " + birthdayCaseWord[random.Next(birthdayCaseWord.Count)];

                        // Отправляем сообщение
                        Message message = await botClient.SendTextMessageAsync(
                            chatId: chatID,
                            parseMode: ParseMode.Html,
                            disableWebPagePreview: true,
                            text: messageText,
                            cancellationToken: token
                        );

                        await Console.Out.WriteLineAsync($"Пользователь {item.Name} поздравлен");
                    }
                }
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Отправка поздравления была отменена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Контроллирует активность в чате. Если после последнего сообщения прошло 12 часов, отправляет сообщение в чат.
        /// </summary>
        /// <param name="botClient">Клиент Telegram для отправки сообщений.</param>
        /// <param name="chatId">Id чата.</param>
        /// <param name="token">Токен отмены операции.</param>
        public static async void SilenceControlTimer(ITelegramBotClient botClient, long chatId, CancellationToken token)
        {
            // Если id чата нет, не запускам таймер
            if (chatId == 0)
                return;

            List<string> silenceCase = await BotSettings.GetSilenceCaseWordAsync(); // Получаем варианты сообщений на тишину в чате
            Random random = new();

            // Запускаем таймер на 12 часов, если в течении 12 часов нет активности, то бот пишет сообщение в чат
            _timerOfSilence = new((obj) =>
            {
                botClient.SendTextMessageAsync(chatId, silenceCase[random.Next(silenceCase.Count)], cancellationToken: token);
            }, null, 43200000, 0);
        }

        /// <summary>
        /// Сбрасывает таймер отправки сообщения.
        /// </summary>
        public static void ResetSilenceControlTimer() => _timerOfSilence.Change(43200000, 0); // Сбрасываем таймер

    }
}