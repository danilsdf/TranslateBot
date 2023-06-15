using System.Collections.Generic;

namespace TelegramBotTranslate
{
    class User
    {
        public long Id { get; set; }
        public long ChatId { get; set; }

        public List<string> Output { get; set; } = new List<string>();
        public List<string> Input { get; set; } = new List<string>();

        public string Topic { get; set; }
        public int? TopicWordCount { get; set; }
        public int? RightWordCount { get; set; }
        public List<Word> TopicWords { get; set; } = new List<Word>();
    }
}
