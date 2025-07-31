namespace App.Core.Models.Email;

public class EmailMessage
{
    public string From { get; set; } = null!;
    public List<string> To { get; set; } = new();
    public string Subject { get; set; } = null!;
    public string HtmlBody { get; set; } = null!;
}