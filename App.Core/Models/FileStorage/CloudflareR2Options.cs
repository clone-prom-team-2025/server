namespace App.Core.Models.FileStorage;

public class CloudflareR2Options
{
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string BucketName { get; set; } = null!;
    public string AccountId { get; set; } = null!;
    public string Region { get; set; } = "auto";
    public string Endpoint { get; set; } = null!;
    public string PublicBaseUrl { get; set; } = null!;
    
}