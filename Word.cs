namespace TelegramBotTranslate
{
    public struct Word
    {
        public string Russian { get; set; }
        public string English { get; set; }

        public Word Trim()
        {
            var rus = Russian.Trim();
            var eng = English.Trim();
            return new Word()
            {
                Russian = rus,
                English = eng
            };
        }
    }
}
