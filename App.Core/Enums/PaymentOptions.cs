namespace App.Core.Enums;

[Flags]
public enum PaymentOptions
{
    AfterPayment = 1 << 0,
    Card = 1 << 1,
    InstallmentsMono = 1 << 2,
    InstallmentsPrivat = 1 << 3,
    InstallmentsPUMB = 1 << 4
}