using Quartz;
using Quartz.Impl;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Darts_for_people
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Получаем необходимые параметры для работы юлта
            string token = await BotSettings.GetTokenAsync();
            long chatId = await BotSettings.GetChatIdAsync();

            // Если токена нет в БД
            if (string.IsNullOrEmpty(token))
            {
                await Console.Out.WriteLineAsync("Токена нет");
                return;
            }

            // Создаём бота с использованием токена
            TelegramBotClient botClient = new(token);
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

            // Создаём расписание выполнения методов
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            IScheduler sched = await schedFact.GetScheduler();
            sched?.Start();

            // Создаём работу, которая будет выполняться, когда сработает тригер
            IJobDetail job = JobBuilder.Create<BotMethods>()
                    .WithIdentity("job1", "group1")
                    .Build();

            // Мапим объекты, чтобы передавать их в методы другого класса
            job.JobDataMap.Put("botClient", botClient);
            job.JobDataMap.Put("token", cts.Token);

            // Создаём и настраиваем тригер
            ITrigger trigger = TriggerBuilder.Create()
                                             .WithIdentity("trigger1", "group1")
                                             .StartAt(DateBuilder.DateOf(8, 0, 0)) // Устанваливаем время, во сколько тригер начнёт работу
                                             .WithSimpleSchedule(x => x.WithIntervalInHours(24).RepeatForever()) // Устанавливаем интервал повторения
                                             .Build();
            
            // Добавляем задачу и тригер в расписание 
            sched?.ScheduleJob(job, trigger);

            // Если есть id, чата запускаем таймер контроля активности в чате
            if (chatId != 0)
                BotMethods.SilenceControlTimer(botClient, chatId, cts.Token);

            // Выводим информационные сообщения
            await Console.Out.WriteLineAsync("Для остановки бота нажми Ecscape");
            await Console.Out.WriteLineAsync("Для очистки консоли нажми F5");

            while (!cts.Token.IsCancellationRequested)
            {
                // Считываем нажатую кнопку
                ConsoleKey key = Console.ReadKey().Key;
                switch (key)
                {
                    case ConsoleKey.Escape:
                        await Console.Out.WriteLineAsync("ББот остановлен"); // Здесь две буквы "Б", потому что по какой-то причине первая буква в косноле не рисуется
                        cts.Cancel();
                        break;
                    case ConsoleKey.F5:
                        Console.Clear();
                        break;
                }
            }

            // Асинхронный обработчик для каждого нового обновления
            async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
            {
                if (update.Message is not { } message) return; // Если обновление не содержит сообщения

                // Если id чата неизвестно, записываем его в БД
                if (chatId == 0)
                {
                    chatId = update.Message.Chat.Id;
                    BotMethods.SilenceControlTimer(botClient, chatId, cancellationToken); // Запускаем таймер контроля активности в чате
                    await BotSettings.UpdateChatId(chatId); // Записывает Id чата в БД, т.к. его там нет
                }

                BotMethods.ResetSilenceControlTimer();

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
                return Task.CompletedTask;
            }
        }

    }
}