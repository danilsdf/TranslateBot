﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Polling;

namespace TelegramBotTranslate
{
    internal class Program
    {
        private static TelegramBotClient _botClient;
        public static InlineKeyboardMarkup KeyboardTopic;
        private static Excel _excel;
        private static readonly Random Random = new Random();
        private static bool _here;
        public static UserContext DbUser = new UserContext();

        private static string _path;
        private const string PathToWords = @"C:\Users\Admin\Desktop\Words.xlsx";
        private const string PathToAllWords = @"C:\Users\Admin\Desktop\All_Words.xlsx";
        private const string PathToNewWords = @"C:\Users\Admin\Desktop\Repeat_Words.xlsx";

        private static void Main()
        {
            _path = PathToNewWords;
            _botClient = new TelegramBotClient("1123330550:AAH0_SQ4540XLfnugm-m6jLQ78fghew80uM");
            
            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = new[] { UpdateType.Message, UpdateType.CallbackQuery },
            };

            _botClient.StartReceiving(
                HandleUpdateAsync,
                HandleErrorAsync,
                receiverOptions,
                cts.Token);

            CreateKeyBoard();
            Console.ReadKey();
        }

        private static async Task HandleUpdateAsync(
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            var handler = update.Type switch
            {
                UpdateType.Message => BotClient_OnMessage(botClient, update.Message),
                UpdateType.CallbackQuery => BotClient_OnCallbackQuery(botClient, update.CallbackQuery!),
                _ => UnknownUpdateHandlerAsync(botClient, update)
            };

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        private static async Task BotClient_OnMessage(ITelegramBotClient sender, Message message)
        {
            if (message.Text == null) return;
            Console.WriteLine("The " + message.Chat.FirstName + " Write: " + message.Text);
            //return;
            //if (e.Message.Chat.Id == 386219611) { await BotClient.SendTextMessageAsync(message.Chat, $"Not for you, Kristina)"); return; }
            //if (e.Message.Chat.Id == 425901772) { await BotClient.SendTextMessageAsync(message.Chat, $"Пошел нахууй!"); return; }
            var users = DbUser.Users;
            foreach (var u in users)
            {
                if (u.ChatId == message.Chat.Id)
                {
                    _here = true;
                }
            }
            if (!_here)
            {
                if (!SqlModel.Check("Programming"))
                {
                    InsertFromExcel();
                }
                DbUser.Users.Add(new User { ChatId = message.Chat.Id});
                DbUser.SaveChanges();
            }

            users = DbUser.Users;
            foreach (var u in users)
            {
                if (u.ChatId == message.Chat.Id)
                {
                    switch (message.Text)
                    {
                        case "/start":
                            await _botClient.SendTextMessageAsync(message.Chat, $"Hi, {message.Chat.FirstName}\n" +
                                $"I was created only for Danil Kravchenko))\n" +
                                $"So, if you are not Danil, be happy, you are his friend))\n" +
                                $"Write 3 forms like \"first/second/third\"");
                            u.Topic = "Topic";
                            await _botClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: KeyboardTopic);
                            break;
                        case "/insert_all":
                            _path = PathToAllWords;
                            await _botClient.SendTextMessageAsync(message.Chat, $"I am working");
                            InsertFromExcel();
                            u.Topic = "Topic";
                            await _botClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: KeyboardTopic);
                            break;
                        case "/insert":
                            _path = PathToWords;
                            await _botClient.SendTextMessageAsync(message.Chat, $"I am working");
                            InsertFromExcel();
                            u.Topic = "Topic";
                            await _botClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: KeyboardTopic);
                            break;
                        case "/insert_new":
                            _path = PathToNewWords;
                            await _botClient.SendTextMessageAsync(message.Chat, $"I am working");
                            InsertFromExcel();
                            u.Topic = "Topic";
                            await _botClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: KeyboardTopic);
                            break;
                        case "/topic":
                            u.Topic = "Topic";
                            await _botClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: KeyboardTopic); break;
                        case "/stop":
                            u.Topic = string.Empty;
                            u.TopicWords = new List<Word>();
                            await _botClient.SendTextMessageAsync(message.Chat, $"Good game\n" +
                                $"Hurry back, I`m waiting for you)*");
                            break;
                        default:
                            if (u.Topic == string.Empty)
                            {
                                if (message.Type == MessageType.Unknown)
                                {
                                    await _botClient.SendTextMessageAsync(message.Chat, $"its Doc");
                                }
                                else
                                {
                                    await _botClient.SendTextMessageAsync(message.Chat, $"I don`t understand you\n" +
                                                                                       $"Please, write /topic - to select a topic and start a game");
                                }

                                return;
                            }
                            u.Input = GetStringList(message.Text);
                            foreach (var str in u.Input)
                            {
                                if (!u.Output.Contains(str))
                                {
                                    await _botClient.SendTextMessageAsync(message.Chat, $"Oh no, you made mistake\n" +
                                        $"The right translate is \"{ToStringList(u.Output)}\"");
                                    var wordsCount = u.TopicWords.Count;
                                    if (wordsCount == 0)
                                    {
                                        u.Topic = string.Empty;
                                        await _botClient.SendTextMessageAsync(message.Chat, "Good job, you translate all word\n" +
                                            "write:\n" +
                                            "/topic - to reselect a topic\n" +
                                            "/stop - to stop a game"); return;
                                    }
                                    var n = Random.Next(0, wordsCount);
                                    u.Output = GetStringList(u.TopicWords[n].English);
                                    await _botClient.SendTextMessageAsync(message.Chat, $"Please, translate it({wordsCount})\n" +
                                        $"{u.TopicWords[n].Russian}");
                                    u.TopicWords.RemoveAt(n);
                                    return;
                                }
                            }
                            await _botClient.SendTextMessageAsync(message.Chat, $"Cool, you are right\n" +
                                        $"Translate is \"{ToStringList(u.Output)}\"");
                            var wordCount = u.TopicWords.Count;
                            if (wordCount == 0)
                            {
                                u.Topic = string.Empty;
                                await _botClient.SendTextMessageAsync(message.Chat, "Good job, you translate all words\n" +
                                    "write:\n" +
                                    "/topic - to reselect a topic\n" +
                                    "/stop - to stop a game"); return;
                            }
                            var nt = Random.Next(0, wordCount);
                            u.Output = GetStringList(u.TopicWords[nt].English);
                            await _botClient.SendTextMessageAsync(message.Chat, $"Please, translate it({wordCount})\n" +
                                $"{u.TopicWords[nt].Russian}");
                            u.TopicWords.RemoveAt(nt);
                            break;
                    }
                }
            }
            await DbUser.SaveChangesAsync();
        }
        private static async Task BotClient_OnCallbackQuery(ITelegramBotClient botClient, CallbackQuery callbackQuery)
        {   
            Console.WriteLine("The " + callbackQuery.From.Id + " Select: " + callbackQuery.Data);
            //return;
            var users = DbUser.Users;
            foreach (var u in users)
            {
                if (u.ChatId != callbackQuery.From.Id || u.Topic != "Topic") continue;
                await _botClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, wait a second)");
                {
                    u.Topic = callbackQuery.Data;
                    u.TopicWords = SqlModel.GetWords(callbackQuery.Data).Select(s => s.Trim()).ToList();
                }
                var wordCount = u.TopicWords.Count;
                if (wordCount == 0)
                {
                    await _botClient.SendTextMessageAsync(callbackQuery.From.Id, "There are some problems with table, try to insert one more time");
                    Console.WriteLine("There are some problems with table, try to insert one more time");
                    break;
                }
                var nt = Random.Next(1, wordCount);
                u.Output = GetStringList(u.TopicWords[nt].English.Trim());
                await _botClient.SendTextMessageAsync(callbackQuery.From.Id, $"Please, translate it({wordCount})\n" +
                                                                            $"{u.TopicWords[nt].Russian}");
                u.TopicWords.RemoveAt(nt);
                break;
            }
            await DbUser.SaveChangesAsync();
        }
        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }
        private static Task UnknownUpdateHandlerAsync(ITelegramBotClient botClient, Update update)
        {
            Console.WriteLine($"Unknown update type: {update.Type}");
            return Task.CompletedTask;
        }

        private static List<string> GetStringList(string s)
        {
            var list = new List<string>();
            for (var index = 0; index < s.Split('/').Length; index++)
            {
                var str = s.Split('/')[index];
                for (var i = 0; i < str.Split(' ').Length; i++)
                {
                    var st = str.Split(' ')[i];
                    list.Add(st.ToLower());
                }
            }

            if (!list.Any()) list.Add(s);

            return list;
        }
        private static void CreateKeyBoard()
        {
            var names = SqlModel.GetAllTables();
            var inlineKeyboardButtons = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < names.Count; i += 2)
            {
                var first = InlineKeyboardButton.WithCallbackData(names[i], names[i]);
                if (i + 2 > names.Count)
                {
                    inlineKeyboardButtons.Add(new[] { first });
                }
                else
                {
                    var second = InlineKeyboardButton.WithCallbackData(names[i + 1], names[i + 1]);
                    inlineKeyboardButtons.Add(new[] { first, second });
                }
            }
            KeyboardTopic = new InlineKeyboardMarkup(inlineKeyboardButtons);
        }
        private static void InsertFromExcel()
        {
            _excel = new Excel(_path, 1);
            var names = _excel.GetNames();
            try
            {
                for (var i = 0; i < _excel.GetSheet(); i++)
                {
                    var list = new List<Word>();
                    Console.WriteLine($"I am working on {names[i]}");
                    SqlModel.RecreateTable(names[i]);
                    _excel.SelectWorkSheet(i + 1);
                    for (var j = 0; j <= _excel.GetColumn(); j++)
                    {
                        if (j % 50 == 0) Console.WriteLine($"{j} elements");
                        var word = _excel.ReadWordFromExcelString(j);
                        if (word != null) list.Add((Word)word);
                    }
                    SqlModel.InsertTable(names[i], list);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _excel.Close();
                CreateKeyBoard();
            }
        }
        public static string ToStringList(List<string> strs)
        {
            try
            {
                var s = strs[0];
                if (strs.Count <= 1) return s;
                for (var i = 1; i < strs.Count; i++)
                {
                    s = s + " " + strs[i];
                }
                return s;

            }
            catch (Exception e)
            {

                Console.WriteLine($"Error {e.Message}");
                return string.Empty;
            }
        }
    }
}
