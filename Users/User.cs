using System.Collections.Generic;
using TelegramBotTranslate;

public class User
{
    public long Id { get; set; }
    public long ChatId { get; set; }

    public List<string> Output { get; set; } = new List<string>();
    public List<string> Input { get; set; } = new List<string>();
    public string Topic { get; set; }
    public int ColumpRow { get; set; }
    public List<int> Usedwords { get; set; } = new List<int>();
}