namespace AIChat.Models;
public class ChatItem
{
    public string Original { get; set; }
    public string Translated { get; set; }
    public ChatItem(string original, string translated)
    {
        Original = original;
        Translated = translated;
    }
}
