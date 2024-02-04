using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Darts_for_people
{
    internal class Program
    {
        static async Task Main(string[] args) // Todo: переделать на эксплицитную типизацию и прокомментировать код
        {
            // Создание экземпляра бота с использованием токена
            var botClient = new TelegramBotClient(BotSettings.GetToken().Result);
            using CancellationTokenSource cts = new();

            // Настройка параметров получения обновлений
            ReceiverOptions receiverOptions = new()
            {
                AllowedUpdates = Array.Empty<UpdateType>() // Получение всех типов обновлений, кроме связанных с изменениями членства в чате
            };

            // Запуск асинхронного приема обновлений
            botClient.StartReceiving(
                updateHandler: HandleUpdateAsync,               // Обработчик новых обновлений
                pollingErrorHandler: HandlePollingErrorAsync,   // Обработчик ошибок
                receiverOptions: receiverOptions,               // Параметры получения обновлений
                cancellationToken: cts.Token                    // Токен для отмены операции
            );


            // Запускаем таску на отправку голосования
            var scheduledTask = Task.Run(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var daysOfWeek = BotSettings.GetDays().Result; // Дни недели считанные с инишника
                    var now = DateTime.Now.DayOfWeek;
                    if (daysOfWeek.Contains(now) && DateTime.Now.Hour == 9)
                    {
                        await BotMethods.SendPollAsync(botClient, cts);
                        await Task.Delay(TimeSpan.FromDays(1), cts.Token); // Ожидание до следующего дня, чтобы избежать повторного выполнения в тот же день
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromHours(23), cts.Token); // Ожидание в течение короткого времени перед следующей проверкой
                    }
                }
            }, cts.Token);

            Console.ReadKey();
            await Console.Out.WriteLineAsync("Bot stopped");
            cts.Cancel(); // Отправка запроса остановкe бота

            // Асинхронный обработчик для каждого нового обновления
            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.Message is not { } message)      // Проверка на наличие сообщения в обновлении
                    return;
                if (message.Text is not { } messageText)    // Проверка на наличие текста в сообщении
                    return;


            }

            // Асинхронный обработчик ошибок
            Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
            {
                var ErrorMessage = exception switch
                {
                    ApiRequestException apiRequestException
                        => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                    _ => exception.ToString()
                };

                Console.WriteLine(ErrorMessage);    // Вывод сообщения об ошибке в консоль
                return Task.CompletedTask;          // Завершение задачи без возврата результата
            }
        }
    }
}
