using System;
using System.Collections.Generic;
using System.Diagnostics;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBotTranslate
{
    class Program
    {
        private static TelegramBotClient BotClient;
        public static InlineKeyboardMarkup keyboardTopic = new InlineKeyboardMarkup(new[]
        {
        new [] { InlineKeyboardButton.WithCallbackData("Programming", "Programming"),  InlineKeyboardButton.WithCallbackData("Personality", "Personality") },
        new [] { InlineKeyboardButton.WithCallbackData("ITverbs", "ITverbs"),  InlineKeyboardButton.WithCallbackData("Hardware", "Hardware") },
        new [] { InlineKeyboardButton.WithCallbackData("FromBook_2", "FromBook_2"),  InlineKeyboardButton.WithCallbackData("FromBook", "FromBook") },
        new [] { InlineKeyboardButton.WithCallbackData("Phrases", "Phrases"),  InlineKeyboardButton.WithCallbackData("Different", "Different") },
        new [] { InlineKeyboardButton.WithCallbackData("Phrases_2", "Phrases_2"), InlineKeyboardButton.WithCallbackData("Different_2", "Different_2") },
        new [] { InlineKeyboardButton.WithCallbackData("Irragular verbs", "Irragular verbs"), InlineKeyboardButton.WithCallbackData("House Home", "House Home") },
        new [] { InlineKeyboardButton.WithCallbackData("Clothes", "Clothes"), InlineKeyboardButton.WithCallbackData("Crime", "Crime") },
        new [] { InlineKeyboardButton.WithCallbackData("Verbs for cooking", "Verbs for cooking"), InlineKeyboardButton.WithCallbackData("Phrases for holiday", "Phrases for holiday"), },
        new [] { InlineKeyboardButton.WithCallbackData("Ingredients", "Ingredients"), InlineKeyboardButton.WithCallbackData("MeatFishBerryTaste", "MeatFishBerryTaste") },
        new [] { InlineKeyboardButton.WithCallbackData("In the kitchen", "In the kitchen"), InlineKeyboardButton.WithCallbackData("Different_3", "Different_3") },
        });
        private static Excel excel;
        private static Random random = new Random();
        private static bool here = false;
        public static UserContext DBUser = new UserContext();
        public static Dictionary<string, List<Word>> words = new Dictionary<string, List<Word>>();

        private static string path = @"C:\Users\Олег\Desktop\Words.xlsx";

        static void Main()
        {
            excel = new Excel(path, 1);
            string[] names = excel.GetNames();
            for (int i = 1; i <= excel.GetSheet(); i++)
            {
                List<Word> words1 = new List<Word>();
                excel.SelectWorkSheet(i);
                for (int j = 0; j <= excel.GetColump(); j++)
                {
                    words1.Add(new Word() { English = excel.ReadExcelString(j, 2), Russian = excel.ReadExcelString(j, 0) });
                }
                words.Add(names[i - 1], words1);
            }
            excel.Close();
            Console.WriteLine("Ready");
            BotClient = new TelegramBotClient("1123330550:AAH0_SQ4540XLfnugm-m6jLQ78fghew80uM");
            BotClient.OnMessage += BotClient_OnMessage;
            BotClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            BotClient.StartReceiving();
            Console.ReadKey();
        }

        private static async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text == null || e.Message.Type != MessageType.Text) return;
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
                {
                    Dictionary<string, List<Word>> words = new Dictionary<string, List<Word>>();
                    string[] names = excel.GetNames();
                    for (int i = 1; i <= excel.GetSheet(); i++)
                    {
                        List<Word> words1 = new List<Word>();
                        excel.SelectWorkSheet(i);

                        for (int j = 0; j <= excel.GetColump(); j++)
                        {
                            words1.Add(new Word() { English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                        }
                        words.Add(names[i - 1], words1);
                    }
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
                                await BotClient.SendTextMessageAsync(message.Chat, $"I don`t understand you\n" +
                                    $"Please, write /topic - to select a topic and start a game");
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
                                await BotClient.SendTextMessageAsync(message.Chat, "Good job, you translate all word\n" +
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
                        u.TopicWords = words[callbackQuery.Data];
                    }
                    int nt = random.Next(1, u.TopicWords.Count);
                    u.Output = GetStringList(u.TopicWords[nt].English);
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
