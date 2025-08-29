namespace App.Core.Enums;

[Flags]
public enum ProductDeliveryType
{
    NovaPost = 1 << 0,
    UkrPost = 1 << 1,
    MeestExpress = 1 << 2
}