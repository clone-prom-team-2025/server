namespace App.Core.Enums;

public enum DeliveryStatus
{
    AwaitingConfirmation,

    // PendingPayment,
    WaitingForShipment,
    InTransit,
    Delivered,
    Received,
    Declined,
    Canceled
}