namespace App.Core.Enums;

[Flags]
public enum ProductDeliveryType
{
    NovaPost = 1 << 0,
    Rozetka = 1 << 1,
    Self = 1 << 2
}