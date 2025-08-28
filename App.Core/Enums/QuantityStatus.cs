namespace App.Core.Enums;

[Flags]
public enum QuantityStatus
{
    None = 0,
    InStock = 1 << 0,
    Ending = 1 << 1,
    OutOfStock = 1 << 2
}