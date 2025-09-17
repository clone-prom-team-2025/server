namespace App.Core.Enums;

[Flags]
public enum BanType
{
    Comments = 1 << 1,
    Orders = 1 << 2,
    Login = 1 << 3,
    Messaging = 1 << 4
}