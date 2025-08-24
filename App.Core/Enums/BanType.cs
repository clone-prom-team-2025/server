namespace App.Core.Enums;

[Flags]
public enum BanType
{
    None = 0,
    Comments = 1,
    Orders = 2,
    Login = 4,
    Messaging = 8
}