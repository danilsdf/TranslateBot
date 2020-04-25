using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBotTranslate.Words
{
    class Word
    {
        public long Id { get; set; }
        public int IdWord { get; set; }

        public string Russian { get; set; }
        public string English { get; set; }
    }
}
