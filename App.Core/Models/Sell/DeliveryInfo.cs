    using App.Core.Enums;
    using MongoDB.Bson;
    using MongoDB.Bson.Serialization.Attributes;

    namespace App.Core.Models.Sell;
    
    public class DeliveryInfo
    {
        [BsonId]
        public ObjectId Id { get; set; }
        
        public ObjectId SellerId { get; set; }

        public string? TrackingNumber { get; set; }
        
        public ObjectId ArchivedProductId { get; set; }
        
        public ObjectId UserId { get; set; }
        
        public MiniProductInfo MiniProductInfo { get; set; } = null!;
        
        public DeliveryPayment Payment { get; set; }

        public DeliveryStatus Status { get; set; } = DeliveryStatus.AwaitingConfirmation;
    }