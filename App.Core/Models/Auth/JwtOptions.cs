namespace App.Core.Models.Auth;

public class JwtOptions
{
    //     "Jwt": {
    //     "Key": "c27dbcf354c36ea96bfa3f738b5453b6dd2e50cd7895e06a06e1f8335b6e0535",
    //     "Issuer": "SellPoint",
    //     "Audience": "Client",
    //     "ExpiresInMinutes": 1440
    //   },
    public string Key { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public int ExpiresInMinutes { get; set; } = 0;
}