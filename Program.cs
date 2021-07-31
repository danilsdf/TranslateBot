using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotTranslate
{
    class Program
    {
        private static TelegramBotClient BotClient;
        public static InlineKeyboardMarkup keyboardTopic;
        private static Excel excel;
        private static Random random = new Random();
        private static bool here = false;
        public static UserContext DBUser = new UserContext();

        private static string path;
        private static string pathToWords = @"C:\Users\Admin\Desktop\Words.xlsx";
        private static string pathToAllWords = @"C:\Users\Admin\Desktop\All_Words.xlsx";
        private static string pathToNewWords = @"C:\Users\Admin\Desktop\New_Words.xlsx";

        static void Main()
        { 
            //path = pathToWords;
            path = pathToNewWords;
            //path = pathToAllWords;
            BotClient = new TelegramBotClient("1123330550:AAH0_SQ4540XLfnugm-m6jLQ78fghew80uM");
            BotClient.OnMessage += BotClient_OnMessage;
            BotClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            BotClient.StartReceiving();
            CreateKeyBoard();
            Console.ReadKey();
        }

        private static async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Type == MessageType.Document)
            {
                var doc = await BotClient.GetFileAsync(e.Message.Document.FileId);
                FileStream fs = new FileStream(@"C:\Users\Admin\Desktop\FileBot.xlsx", FileMode.Create);
                await BotClient.GetInfoAndDownloadFileAsync(doc.FileId, fs);
            }
            if (e.Message.Text == null) return;
            var message = e.Message;
            Console.WriteLine("The " + e.Message.Chat.FirstName + " Write: " + e.Message.Text);
            //return;
            //if (e.Message.Chat.Id == 386219611) { await BotClient.SendTextMessageAsync(message.Chat, $"Not for you, Kristina)"); return; }
            //if (e.Message.Chat.Id == 425901772) { await BotClient.SendTextMessageAsync(message.Chat, $"Пошел нахууй!"); return; }
            var users = DBUser.Users;
            foreach (User u in users)
            {
                if (u.ChatId == message.Chat.Id)
                {
                    here = true;
                }
            }
            if (!here)
            {
                if (!SqlModel.Check("Programming"))
                {
                    InsertFromExcel();
                }
                DBUser.Users.Add(new User { ChatId = message.Chat.Id});
                DBUser.SaveChanges();
            }

            users = DBUser.Users;
            foreach (User u in users)
            {
                if (u.ChatId == message.Chat.Id)
                {
                    switch (message.Text)
                    {
                        case "/start":
                            await BotClient.SendTextMessageAsync(message.Chat, $"Hi, {message.Chat.FirstName}\n" +
                                $"I was created only for Danil Kravchenko))\n" +
                                $"So, if you are not Danil, be happy, you are his friend))\n" +
                                $"Write 3 forms like \"first/second/third\"");
                            u.Topic = "Topic";
                            await BotClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: keyboardTopic);
                            break;
                        case "/insert":
                            await BotClient.SendTextMessageAsync(message.Chat, $"I am working");
                            InsertFromExcel();
                            u.Topic = "Topic";
                            await BotClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: keyboardTopic);
                            break;
                        case "/topic":
                            u.Topic = "Topic";
                            await BotClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: keyboardTopic); break;
                        case "/stop":
                            u.Topic = string.Empty;
                            u.TopicWords = new List<Word>();
                            await BotClient.SendTextMessageAsync(message.Chat, $"Good game\n" +
                                $"Hurry back, I`m waiting for you)*");
                            break;
                        default:
                            if (u.Topic == string.Empty)
                            {
                                if (e.Message.Type == MessageType.Unknown)
                                {
                                    await BotClient.SendTextMessageAsync(message.Chat, $"its Doc");
                                }
                                else
                                {
                                    await BotClient.SendTextMessageAsync(message.Chat, $"I don`t understand you\n" +
                                                                                       $"Please, write /topic - to select a topic and start a game");
                                }

                                return;
                            }
                            u.Input = GetStringList(message.Text);
                            foreach (string str in u.Input)
                            {
                                if (!u.Output.Contains(str))
                                {
                                    await BotClient.SendTextMessageAsync(message.Chat, $"Oh no, you made mistake\n" +
                                        $"The right translate is \"{ToStringList(u.Output)}\"");
                                    if (u.TopicWords.Count == 0)
                                    {
                                        u.Topic = string.Empty;
                                        await BotClient.SendTextMessageAsync(message.Chat, "Good job, you translate all word\n" +
                                            "write:\n" +
                                            "/topic - to reselect a topic\n" +
                                            "/stop - to stop a game"); return;
                                    }
                                    int n = random.Next(0, u.TopicWords.Count);
                                    u.Output = GetStringList(u.TopicWords[n].English);
                                    await BotClient.SendTextMessageAsync(message.Chat, "Please, translate it)\n" +
                                        $"{u.TopicWords[n].Russian}");
                                    u.TopicWords.RemoveAt(n);
                                    return;
                                }
                            }
                            await BotClient.SendTextMessageAsync(message.Chat, $"Cool, you are right\n" +
                                        $"Translate is \"{ToStringList(u.Output)}\"");
                            if (u.TopicWords.Count == 0)
                            {
                                u.Topic = string.Empty;
                                await BotClient.SendTextMessageAsync(message.Chat, "Good job, you translate all words\n" +
                                    "write:\n" +
                                    "/topic - to reselect a topic\n" +
                                    "/stop - to stop a game"); return;
                            }
                            int nt = random.Next(0, u.TopicWords.Count);
                            u.Output = GetStringList(u.TopicWords[nt].English);
                            await BotClient.SendTextMessageAsync(message.Chat, "Please, translate it)\n" +
                                $"{u.TopicWords[nt].Russian}");
                            u.TopicWords.RemoveAt(nt);
                            break;
                    }
                }
            }
            await DBUser.SaveChangesAsync();
        }
        private static async void BotClient_OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            var callbackQuery = e.CallbackQuery;
            Console.WriteLine("The " + e.CallbackQuery.From.Id + " Select: " + e.CallbackQuery.Data);
            //return;
            var users = DBUser.Users;
            foreach (User u in users)
            {
                if (u.ChatId == e.CallbackQuery.From.Id && u.Topic == "Topic")
                {
                    await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, wait a second)");
                    {
                        u.Topic = callbackQuery.Data;
                        u.TopicWords = SqlModel.GetWords(callbackQuery.Data).Select(s => s.Trim()).ToList();
                    }
                    int nt = random.Next(1, u.TopicWords.Count);
                    u.Output = GetStringList(u.TopicWords[nt].English.Trim());
                    await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, translate it)\n" +
                        $"{u.TopicWords[nt].Russian}");
                    u.TopicWords.RemoveAt(nt);
                }             
            }
            await DBUser.SaveChangesAsync();
        }
        private static List<string> GetStringList(string s)
        {
            List<string> list = new List<string>();
            foreach (string str in s.Split('/'))
            {
                foreach (string st in str.Split(' '))
                {
                    list.Add(st.ToLower());
                }
            }
            if (list == null) list.Add(s);
            return list;
        }
        private static void CreateKeyBoard()
        {
            excel = new Excel(path, 1);

            string[] names = excel.GetNames();
            List<InlineKeyboardButton[]> inlineKeyboardButtons = new List<InlineKeyboardButton[]>();
            for (int i = 0; i < names.Length; i += 2)
            {
                var first = InlineKeyboardButton.WithCallbackData(names[i], names[i]);
                if (i + 2 > names.Length)
                {
                    inlineKeyboardButtons.Add(new[] { first });
                }
                else
                {
                    var second = InlineKeyboardButton.WithCallbackData(names[i + 1], names[i + 1]);
                    inlineKeyboardButtons.Add(new[] { first, second });
                }
            }
            keyboardTopic = new InlineKeyboardMarkup(inlineKeyboardButtons);
            excel.Close();
        }
        private static void InsertFromExcel()
        {
            excel = new Excel(path, 1);
            string[] names = excel.GetNames();
            for (int i = 0; i < excel.GetSheet(); i++)
            {
                var list = new List<Word>();
                Console.WriteLine($"I am working on {names[i]}");
                SqlModel.RecreateTable(names[i]);
                excel.SelectWorkSheet(i+1);
                for (int j = 0; j <= excel.GetColumn(); j++)
                {
                    if (j % 50 == 0) Console.WriteLine($"{j} elements");
                    var word = excel.ReadWordFromExcelString(j);
                    if(word != null) list.Add((Word)word);
                }
                SqlModel.InsertTable(names[i], list);
            }
            excel.Close();
            CreateKeyBoard();
        }
        public static string ToStringList(List<string> strs)
        {
            try
            {
                string s = strs[0];
                if (strs.Count > 1)
                {
                    for (int i = 1; i < strs.Count; i++)
                    {
                        s = s + " " + strs[i];
                    }
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
