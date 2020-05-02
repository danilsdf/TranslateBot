using System.Collections.Generic;

namespace TelegramBotTranslate
{
    class User
    {
        public long Id { get; set; }
        public long ChatId { get; set; }

        public List<string> Output { get; set; } = new List<string>();
        public List<string> Input { get; set; } = new List<string>();
       // public Dictionary<string, List<Word>> Words { get; set; } = new Dictionary<string, List<Word>>();

        public string Topic { get; set; }
        public List<Word> TopicWords { get; set; } = new List<Word>();
    }
}
