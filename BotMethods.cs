using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace Darts_for_people
{
    public class BotMethods
    {
        public static async Task SendPollAsync(ITelegramBotClient botClient, CancellationTokenSource cts)
        {
            try
            {
                await botClient.SendPollAsync(
                    chatId: -4120049531,        // ID чата, в который отправляется опрос
                    question: "В дартс идём?",
                    options: new[]
                    {
                        "Даааа",
                        "Я петух"
                    },
                    cancellationToken: cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Голосование было отменено отменена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка: {ex.Message}");
            }
        }
    }
}
