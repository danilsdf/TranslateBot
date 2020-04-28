using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Runtime.CompilerServices;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotTranslate.Users;
using TelegramBotTranslate.Words;

namespace TelegramBotTranslate
{
    class Program
    {
        private static TelegramBotClient BotClient;
        public static InlineKeyboardMarkup keyboardTopic = new InlineKeyboardMarkup(new[]
        {
        new [] { InlineKeyboardButton.WithCallbackData("At the Moment", "Now")},
        new [] { InlineKeyboardButton.WithCallbackData("Clothes", "Clothes"),  InlineKeyboardButton.WithCallbackData("Phrases", "Phrases")},
        new [] { InlineKeyboardButton.WithCallbackData("Irregular Verbs", "Irregular"), InlineKeyboardButton.WithCallbackData("House", "House") }
        });
        private static Excel excel;
        private static Random random = new Random();
        private static bool here = false;
        public static WordContext DBWord = new WordContext();
        public static UserContext DBUser = new UserContext();
        public static List<int> usedwords = new List<int>();
        private static string path = @"C:\Users\Олег\Desktop\Words.xlsx";

        static void Main(string[] args)
        {
            BotClient = new TelegramBotClient("Token");
            excel = new Excel(path, 1); ;
            BotClient.OnMessage += BotClient_OnMessage;
            BotClient.OnMessageEdited += BotClient_OnMessage;
            BotClient.OnCallbackQuery += BotClient_OnCallbackQuery;
            BotClient.StartReceiving();
            Console.ReadKey();
            excel.Close();
        }

        private static async void BotClient_OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text == null || e.Message.Type != MessageType.Text) return;
            var message = e.Message;
            Console.WriteLine("The " + e.Message.Chat.FirstName + " Write: " + e.Message.Text);
            //return;
            if (e.Message.Chat.Id != 386219611) { await BotClient.SendTextMessageAsync(message.Chat, $"Not for you, Kristina)");return; }
            var users = DBUser.Users;
            foreach (User u in users)
            {
                if (u.ChatId == message.Chat.Id) here = true;

            }
            if (!here)
            {

                DBUser.Users.Add(new User { ChatId = message.Chat.Id });
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
                            await BotClient.SendTextMessageAsync(message.Chat, $"Select the topic:\n",
                                replyMarkup: keyboardTopic); break;
                        case "/stop":
                            u.Topic = string.Empty;
                            await BotClient.SendTextMessageAsync(message.Chat, $"Good game\n" +
                                $"Hurry back, I`m waiting for you)*");
                            break;
                        default:
                            if(u.Topic == string.Empty)
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
                                    var wordss = DBWord.Words;
                                    if (u.Usedwords.Count == u.ColumpRow)
                                    {
                                        u.Usedwords = new List<int>();
                                        await BotClient.SendTextMessageAsync(message.Chat, "Good job, you translate all word\n" +
                                            "write:\n" +
                                            "/topic - to reselect a topic\n" +
                                            "/stop - to stop a game"); return;
                                    }
                                    int nt = random.Next(0, u.ColumpRow);
                                    while (u.Usedwords.Contains(nt))
                                    { nt = random.Next(0, u.ColumpRow); }
                                    foreach (var word in wordss)
                                    {
                                        if (word.IdWord == nt)
                                        {
                                            u.Usedwords.Add(nt);
                                            u.Output = GetStringList(word.English);
                                            await BotClient.SendTextMessageAsync(message.Chat, "Please, translate it)\n" +
                                                $"{word.Russian}");
                                        }
                                    }
                                    return;
                                }
                            }
                            await BotClient.SendTextMessageAsync(message.Chat, $"Cool, you are right\n" +
                                        $"Translate is \"{ToStringList(u.Output)}\"");
                            var words = DBWord.Words;
                            if (u.Usedwords.Count == u.ColumpRow)
                            {
                                u.Usedwords = new List<int>();
                                await BotClient.SendTextMessageAsync(message.Chat, "Good job, you translate all word\n" +
                                    "write:\n" +
                                    "/topic - to reselect a topic\n" +
                                    "/stop - to stop a game"); return;
                            }
                            int n = random.Next(0, u.ColumpRow);
                            while (u.Usedwords.Contains(n))
                            { n = random.Next(0, u.ColumpRow); }
                            foreach (var word in words)
                            {
                                if (word.IdWord == n)
                                {
                                    u.Usedwords.Add(n);
                                    u.Output = GetStringList(word.English);
                                    await BotClient.SendTextMessageAsync(message.Chat, "Please, translate it)\n" +
                                        $"{word.Russian}");
                                }
                            }

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
                if (u.ChatId == e.CallbackQuery.From.Id)
                {
                    await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, wait a second)");
                    if (callbackQuery.Data == "Now")
                    {
                        if (u.Topic != callbackQuery.Data)
                        {
                            excel.SelectWorkSheet(1);
                            u.ColumpRow = excel.GetColump();
                            u.Topic = callbackQuery.Data;
                            DBWord.DeleteWords();
                            u.Usedwords = new List<int>();
                            for (int i = 0; i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();

                        }
                        u.ColumpRow = excel.GetColump();
                        if ((u.ColumpRow - DBWord.Words.Count()) > 0)
                        {
                            for (int i = DBWord.Words.Count(); i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();
                        }
                        var words = DBWord.Words;
                        if (u.Usedwords.Count == u.ColumpRow)
                        {
                            u.Usedwords = new List<int>();
                            await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Good job, you translate all word\n" +
                                "write:\n" +
                                "/topic - to reselect a topic\n" +
                                "/stop - to stop a game"); return;
                        }
                        int n = random.Next(0, u.ColumpRow);
                        while (u.Usedwords.Contains(n))
                        { n = random.Next(0, u.ColumpRow); }
                        foreach (var word in words)
                        {
                            if (word.IdWord == n)
                            {
                                u.Usedwords.Add(n);
                                u.Output = GetStringList(word.English);
                                await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, translate it)\n" +
                                    $"{word.Russian}");
                            }
                        }
                    }
                    else if (callbackQuery.Data == "House")
                    {
                        if (u.Topic != callbackQuery.Data)
                        {
                            excel.SelectWorkSheet(4);
                            u.ColumpRow = excel.GetColump();
                            u.Topic = callbackQuery.Data;
                            DBWord.DeleteWords();
                            u.Usedwords = new List<int>();
                            for (int i = 0; i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();

                        }
                        u.ColumpRow = excel.GetColump();
                        if ((u.ColumpRow - DBWord.Words.Count()) > 0)
                        {
                            for (int i = DBWord.Words.Count(); i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();
                        }
                        var words = DBWord.Words;
                        if (u.Usedwords.Count == u.ColumpRow)
                        {
                            u.Usedwords = new List<int>();
                            await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Good job, you translate all word\n" +
                                "write:\n" +
                                "/topic - to reselect a topic\n" +
                                "/stop - to stop a game"); return;
                        }
                        int n = random.Next(0, u.ColumpRow);
                        while (u.Usedwords.Contains(n))
                        { n = random.Next(0, u.ColumpRow); }
                        foreach (var word in words)
                        {
                            if (word.IdWord == n)
                            {
                                u.Usedwords.Add(n);
                                u.Output = GetStringList(word.English);
                                await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, translate it)\n" +
                                    $"{word.Russian}");
                            }
                        }
                    }
                    else if (callbackQuery.Data == "Clothes")
                    {
                        if (u.Topic != callbackQuery.Data)
                        {
                            excel.SelectWorkSheet(5);
                            u.ColumpRow = excel.GetColump();
                            u.Topic = callbackQuery.Data;
                            DBWord.DeleteWords();
                            u.Usedwords = new List<int>();
                            for (int i = 0; i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();

                        }
                        u.ColumpRow = excel.GetColump();
                        if ((u.ColumpRow - DBWord.Words.Count()) > 0)
                        {
                            for (int i = DBWord.Words.Count(); i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();
                        }
                        var words = DBWord.Words;
                        if (u.Usedwords.Count == u.ColumpRow)
                        {
                            u.Usedwords = new List<int>();
                            await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Good job, you translate all word\n" +
                                "write:\n" +
                                "/topic - to reselect a topic\n" +
                                "/stop - to stop a game"); return;
                        }
                        int n = random.Next(0, u.ColumpRow);
                        while (u.Usedwords.Contains(n))
                        { n = random.Next(0, u.ColumpRow); }
                        foreach (var word in words)
                        {
                            if (word.IdWord == n)
                            {
                                u.Usedwords.Add(n);
                                u.Output = GetStringList(word.English);
                                await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, translate it)\n" +
                                    $"{word.Russian}");
                            }
                        }
                    }
                    else if (callbackQuery.Data == "Phrases")
                    {
                        if (u.Topic != callbackQuery.Data)
                        {
                            excel.SelectWorkSheet(2);
                            u.ColumpRow = excel.GetColump();
                            u.Topic = callbackQuery.Data;
                            DBWord.DeleteWords();
                            u.Usedwords = new List<int>();
                            for (int i = 0; i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();

                        }
                        u.ColumpRow = excel.GetColump();
                        if ((u.ColumpRow - DBWord.Words.Count()) > 0)
                        {
                            for (int i = DBWord.Words.Count(); i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();
                        }
                        var words = DBWord.Words;
                        if (u.Usedwords.Count == u.ColumpRow)
                        {
                            u.Usedwords = new List<int>();
                            await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Good job, you translate all word\n" +
                                "write:\n" +
                                "/topic - to reselect a topic\n" +
                                "/stop - to stop a game"); return;
                        }
                        int n = random.Next(0, u.ColumpRow);
                        while (u.Usedwords.Contains(n))
                        { n = random.Next(0, u.ColumpRow); }
                        foreach (var word in words)
                        {
                            if (word.IdWord == n)
                            {
                                u.Usedwords.Add(n);
                                u.Output = GetStringList(word.English);
                                await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, translate it)\n" +
                                    $"{word.Russian}");
                            }
                        }
                    }
                    else if (callbackQuery.Data == "Irregular")
                    {
                        if (u.Topic != callbackQuery.Data)
                        {
                            u.ColumpRow = excel.GetColump();
                            excel.SelectWorkSheet(3);
                            u.Topic = callbackQuery.Data;
                            DBWord.DeleteWords();
                            u.Usedwords = new List<int>();
                            for (int i = 0; i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();
                        }
                        u.ColumpRow = excel.GetColump();
                        if ((u.ColumpRow - DBWord.Words.Count()) > 0)
                        {
                            for (int i = DBWord.Words.Count(); i <= u.ColumpRow; i++)
                            {
                                DBWord.Words.Add(new Word() { IdWord = i, English = excel.ReadExcelString(i, 2), Russian = excel.ReadExcelString(i, 0) });
                            }
                            DBWord.SaveChanges();
                        }
                        var words = DBWord.Words;
                        if (u.Usedwords.Count == u.ColumpRow)
                        {
                            u.Usedwords = new List<int>();
                            await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Good job, you translate all word\n" +
                                "write:\n" +
                                "/topic - to reselect a topic\n" +
                                "/stop - to stop a game"); return;
                        }
                        int n = random.Next(0, u.ColumpRow);
                        while (u.Usedwords.Contains(n))
                        { n = random.Next(0, u.ColumpRow); }
                        foreach (var word in words)
                        {
                            if (word.IdWord == n)
                            {
                                u.Usedwords.Add(n);
                                u.Output = GetStringList(word.English);
                                await BotClient.SendTextMessageAsync(callbackQuery.From.Id, "Please, translate it)\n" +
                                    $"{word.Russian}");
                            }
                        }
                    }
                }
            }
            await DBUser.SaveChangesAsync();
        }
        //public static async void SendWord(int id)
        //{
        //    DbSet<User> users = DBUser.Users;
        //    foreach (User u in users)
        //    {
        //        if (u.ChatId == id)
        //        {
        //            var words = DBWord.Words;
        //            if (u.Usedwords.Count == u.ColumpRow)
        //            {
        //                u.Usedwords = new List<int>();
        //                await BotClient.SendTextMessageAsync(id, "Good job, you translate all word\n" +
        //                    "write:\n" +
        //                    "/topic - to reselect a topic\n" +
        //                    "/stop - to stop a game"); return;
        //            }
        //            int n = random.Next(0, u.ColumpRow);
        //            while (u.Usedwords.Contains(n))
        //            { n = random.Next(0, u.ColumpRow); }
        //            foreach (var word in words)
        //            {
        //                if (word.IdWord == n)
        //                {
        //                    u.Usedwords.Add(n);
        //                    u.Output = GetStringList(word.English);
        //                    await BotClient.SendTextMessageAsync(id, "Please, translate it)\n" +
        //                        $"{word.Russian}");
        //                }
        //            }
        //        }
        //    }
        //    await DBUser.SaveChangesAsync();
        //}
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
    }
}
