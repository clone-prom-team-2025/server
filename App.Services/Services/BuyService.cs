using App.Core.DTOs.Notification;
using App.Core.DTOs.Sell;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces;
using App.Core.Models.FileStorage;
using App.Core.Models.Sell;
using App.Core.Utils;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services.Services;

/// <summary>
/// Service responsible for handling product purchases, payments, shipping, and order management.
/// </summary>
public class BuyService(IBuyInfoRepository buyInfoRepository, ILogger<BuyService> logger, IMapper mapper, IStoreRepository storeRepository, IUserRepository userRepository, IProductRepository productRepository, IProductMediaRepository productMediaRepository, IFileService fileService, INotificationService notificationService) : IBuyService
{
    private readonly ILogger<BuyService> _logger = logger;
    private readonly IMapper _mapper =  mapper;
    private readonly IBuyInfoRepository _buyInfoRepository = buyInfoRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IProductMediaRepository _productMediaRepository = productMediaRepository;
    private readonly IFileService _fileService = fileService;
    private readonly INotificationService _notificationService = notificationService;

    /// <summary>
    /// Creates a new purchase for the specified product.
    /// </summary>
    /// <param name="dto">The purchase creation data.</param>
    /// <param name="userId">The ID of the user making the purchase.</param>
    public async Task BuyProductAsync(BuyCreateDto dto, string userId)
    {
        using (_logger.BeginScope("BuyProductAsync"))
        {
            _logger.LogInformation("BuyProductAsync called");
            var parsedUserId = ObjectId.Parse(userId);
            var parsedProductId = ObjectId.Parse(dto.ProductId);
            if (!(await _userRepository.ExistsById(parsedUserId)))
            {
                _logger.LogWarning("User not found");
                throw new KeyNotFoundException("User not found");
            }
            
            var product = await _productRepository.GetByIdAsync(parsedProductId);

            if (product == null)
            {
                _logger.LogWarning("Product not found");
                throw new KeyNotFoundException("Product not found");
            }

            if (!(await _storeRepository.ExistsById(product.SellerId)))
            {
                _logger.LogWarning("Store not found");
                throw new KeyNotFoundException("Store not found");
            }

            if (product.Quantity <= 0)
            {
                _logger.LogWarning("Product quantity not set");
                throw new KeyNotFoundException("Product is out of stock.");
            }
            
            var media = await _productMediaRepository.GetByProductIdAsync(product.Id.ToString());
            BaseFile? image = null;
            if (media != null && media.Count > 0)
            {
                var stream = await WebpDownloader.GetWebpStreamAsync(media.First().Files.SourceUrl);
                image = new BaseFile();
                (image.SourceUrl, image.CompressedUrl, image.SourceFileName, image.CompressedFileName) = await _fileService.SaveImageAsync(stream, media.First().Files.SourceUrl, "buy-product");
            }

            var miniInfo = new MiniProductInfo()
            {
                Image = image ?? new BaseFile(),
                Price = product.DiscountPrice ?? product.Price,
                ProductId = product.Id,
                ProductName = product.Name,
            };
            
            var status = dto.DeliveryPayment == DeliveryPayment.AfterPayment ? DeliveryStatus.WaitingForShipment : DeliveryStatus.PendingPayment;
            string? trackingNumber = null;
            if (status == DeliveryStatus.WaitingForShipment)
            {
                trackingNumber = Guid.NewGuid().ToString("N");
                await _notificationService.SendNotificationAsync(new NotificationCreateDto()
                {
                    From = null, IsHighPriority = false,
                    Message = $"Дякуємо за покупку, очікуйте відправку від продавця. Номер накладної {trackingNumber}", MetadataUrl = null, To = userId,
                    Type = NotificationType.Info
                });
            }
            else
            {
                await _notificationService.SendNotificationAsync(new NotificationCreateDto()
                {
                    From = null, IsHighPriority = true,
                    Message = "Продовжіть покупку, оплатіть замовлення.", MetadataUrl = null, To = userId,
                    Type = NotificationType.Info
                });
            }

            var buy = new BuyInfo()
            {
                Id = ObjectId.GenerateNewId(),
                SellerId = product.SellerId,
                TrackingNumber = trackingNumber,
                UserId = parsedUserId,
                DeliveryToInfo = dto.DeliveryTo,
                Payment = dto.DeliveryPayment,
                Status = status,
                MiniProductInfo = miniInfo,
                Payed = dto.DeliveryPayment == DeliveryPayment.Card ?  false : null,
            };
            
            _logger.LogInformation("BuyProduct successes");
            
            await _buyInfoRepository.CreateAsync(buy);
        }
    }

    /// <summary>
    /// Accepts a pending sell request for a given order.
    /// </summary>
    /// <param name="buyId">The ID of the purchase order.</param>
    /// <param name="userId">The ID of the seller’s user.</param>
    public async Task AcceptSellAsync(string buyId, string userId)
    {
        using (_logger.BeginScope("AcceptSell"))
        {
            _logger.LogInformation("AcceptSell called");
            var parsedBuyId = ObjectId.Parse(buyId);
            var parsedUserId = ObjectId.Parse(userId);
            
            if (!(await _userRepository.ExistsById(parsedUserId)))
            {
                _logger.LogWarning("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var store = await _storeRepository.GetStoreByUserId(parsedUserId);
            
            if (store == null)
            {
                _logger.LogWarning("Store not found");
                throw new KeyNotFoundException("Store not found");
            }

            if (!store.Roles.ContainsKey(userId))
            {
                _logger.LogWarning("It's not your store");
                throw new AccessDeniedException("It's not your store");
            }
            
            var buyInfo = await _buyInfoRepository.GetByIdAsync(parsedBuyId);
            if (buyInfo == null)
            {
                _logger.LogWarning("BuyInfo not found");
                throw new KeyNotFoundException("BuyInfo not found");
            }

            var product = await _productRepository.GetByIdAsync(buyInfo.MiniProductInfo.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product not found");
                throw new KeyNotFoundException("Product not found");
            }

            if (product.Quantity <= 0)
            {
                _logger.LogWarning("Product is out of stock.");
                throw new InvalidOperationException("Product is out of stock.");
            }
            
            product.Quantity -= 1;
            product.QuantityStatus = product.Quantity switch
            {
                > 0 and <= 4 => QuantityStatus.Ending,
                <= 0 => QuantityStatus.OutOfStock,
                _ => QuantityStatus.InStock
            };

            buyInfo.Status = buyInfo.Payment == DeliveryPayment.Card ? DeliveryStatus.PendingPayment : DeliveryStatus.WaitingForShipment;
            
            var result = await _productRepository.UpdateAsync(product);
            if (!result)
            {
                _logger.LogWarning("Product failed to update");
                throw new InvalidOperationException("Product failed to update");
            }
            
            var result1 = await _buyInfoRepository.UpdateAsync(buyInfo);
            if (!result1)
            {
                _logger.LogWarning("Failed to accept");
                throw new InvalidOperationException("Failed to accept");
            }
            
            _logger.LogInformation("BuyProduct successes");
        }
    }
    
    /// <summary>
    /// Simulates payment for a product (for card payments).
    /// </summary>
    /// <param name="buyId">The ID of the purchase order.</param>
    /// <param name="userId">The ID of the buyer.</param>
    public async Task PayForProductAsync(string buyId, string userId)
    {
        using (_logger.BeginScope("PayForProductAsync"))
        {
            _logger.LogInformation("PayForProductAsync called");
            var parsedBuyId = ObjectId.Parse(buyId);
            var parsedUserId = ObjectId.Parse(userId);
            if (!(await _userRepository.ExistsById(parsedUserId)))
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }
            
            var buyInfo = await _buyInfoRepository.GetByIdAsync(parsedBuyId);
            if (buyInfo == null)
            {
                _logger.LogInformation("Failed to pay for product. BuyInfo not found");
                throw new InvalidOperationException("Failed to pay for product");
            }

            if (buyInfo.Payment == DeliveryPayment.AfterPayment)
            {
                _logger.LogInformation("Payment upon receipt");
                throw new InvalidOperationException("Payment upon receipt");
            }

            if (buyInfo.Payed == true)
            {
                _logger.LogInformation("Already payed");
                throw new InvalidOperationException("Already payed");
            }
            
            // Симуляція оплати
            var value = Random.Shared.Next(0, 101);
            if (value <= 98)
                buyInfo.Payed = true;
            else
            {
                _logger.LogInformation("Failed to pay for product.");
                throw new InvalidOperationException("Transaction declined by the bank");
            }

            buyInfo.Status = DeliveryStatus.WaitingForShipment;
            buyInfo.TrackingNumber = Guid.NewGuid().ToString("N");
            
            var result = await _buyInfoRepository.UpdateAsync(buyInfo);
            if (!result)
            {
                _logger.LogWarning("PayForProduct failed");
                throw new InvalidOperationException("PayForProduct failed");
            }
            
            await _notificationService.SendNotificationAsync(new NotificationCreateDto()
            {
                From = null, IsHighPriority = false,
                Message = $"Дякуємо за покупку, очікуйте відправку від продавця. Номер накладної {buyInfo.TrackingNumber}", MetadataUrl = null, To = userId,
                Type = NotificationType.Info
            });
            
            _logger.LogInformation("PayForProduct successes");
        }
    }

    /// <summary>
    /// Marks a product as sent by the seller.
    /// </summary>
    /// <param name="buyId">The ID of the purchase order.</param>
    /// <param name="userId">The ID of the seller’s user.</param>
    public async Task SendProductAsync(string buyId, string userId)
    {
        using (_logger.BeginScope("SendProduct"))
        {
            _logger.LogInformation("SendProduct called");
            var parsedBuyId = ObjectId.Parse(buyId);
            var parsedUserId = ObjectId.Parse(userId);
            // імітація отрмання від поштового оператора інформації про відправку товара
            // ...
            if (!(await _userRepository.ExistsById(parsedUserId)))
            {
                _logger.LogWarning("User not found");
                throw new KeyNotFoundException("User not found");
            }
            
            var store = await _storeRepository.GetStoreByUserId(parsedUserId);
            if (store == null)
            {
                _logger.LogWarning("Store not found");
                throw new KeyNotFoundException("Store not found");
            }

            if (!store.Roles.ContainsKey(userId))
            {
                _logger.LogWarning("It's not your store");
                throw new AccessDeniedException("It's not your store");
            }
            
            var buyInfo = await _buyInfoRepository.GetByIdAsync(parsedBuyId);
            
            if (buyInfo == null)
            {
                _logger.LogWarning("BuyInfo not found");
                throw new KeyNotFoundException("BuyInfo not found");
            }

            if (buyInfo.Status != DeliveryStatus.WaitingForShipment)
            {
                _logger.LogInformation("BuyInfo not waiting for shipment");
                throw new InvalidOperationException("Product not waiting for shipment");
            }
            
            //імітація трекового номера(накладна/експерс накладна)
            buyInfo.TrackingNumber = Guid.NewGuid().ToString("N");
            buyInfo.Status = DeliveryStatus.InTransit;
            
            var result = await _buyInfoRepository.UpdateAsync(buyInfo);
            if (!result)
            {
                _logger.LogWarning("Failed to send product");
                throw new InvalidOperationException("Failed to send product");
            }
            
            await _notificationService.SendNotificationAsync(new NotificationCreateDto()
            {
                From = null, IsHighPriority = false,
                Message = "Продавець відправив товар, очікуйте доставки.", MetadataUrl = null, To = userId,
                Type = NotificationType.Info
            });
            
            _logger.LogInformation("SendProduct successes");
        }
    }

    /// <summary>
    /// Cancels an active purchase by the buyer.
    /// </summary>
    /// <param name="buyId">The ID of the purchase order.</param>
    /// <param name="userId">The ID of the buyer.</param>
    public async Task CancelSellAsync(string buyId, string userId)
    {
        using (_logger.BeginScope("CancelSellAsync"))
        {
            _logger.LogInformation("CancelSellAsync called");
            var parsedBuyId = ObjectId.Parse(buyId);
            var parsedUserId = ObjectId.Parse(userId);
            if (!(await _userRepository.ExistsById(parsedUserId)))
            {
                _logger.LogWarning("User not found");
                throw new KeyNotFoundException("User not found");
            }
            var buyInfo = await _buyInfoRepository.GetByIdAsync(parsedBuyId);
            if (buyInfo == null)
            {
                _logger.LogWarning("BuyInfo not found");
                throw new KeyNotFoundException("BuyInfo not found");
            }

            if (buyInfo.Status == DeliveryStatus.Canceled 
                || buyInfo.Status == DeliveryStatus.Received
                || buyInfo.Status == DeliveryStatus.Declined)
            {
                _logger.LogInformation("You cannot cancel your order");
                throw new InvalidOperationException("You cannot cancel your order");
            }
            
            buyInfo.Status = DeliveryStatus.Canceled;
            var result = await _buyInfoRepository.UpdateAsync(buyInfo);
            if (!result)
            {
                _logger.LogWarning("Failed to cancel product");
                throw new InvalidOperationException("Failed to cancel product");
            }

            if (buyInfo.Payment == DeliveryPayment.Card)
            {
                _logger.LogInformation("Імітація повернення коштів");
            }
            
            await _notificationService.SendNotificationAsync(new NotificationCreateDto()
            {
                From = null, IsHighPriority = false,
                Message = "Замовлення скасовано.", MetadataUrl = null, To = userId,
                Type = NotificationType.Info
            });
            
            _logger.LogInformation("CancelSellAsync successes");
        }
    }

    /// <summary>
    /// Declines a purchase request by the seller.
    /// </summary>
    /// <param name="buyId">The ID of the purchase order.</param>
    /// <param name="userId">The ID of the seller’s user.</param>
    public async Task DeclineSellAsync(string buyId, string userId)
    {
        using (_logger.BeginScope("DeclinedSellAsync"))
        {
            _logger.LogInformation("DeclinedSellAsync called");
            var parsedBuyId = ObjectId.Parse(buyId);
            var parsedUserId = ObjectId.Parse(userId);

            if (!(await _userRepository.ExistsById(parsedUserId)))
            {
                _logger.LogWarning("User not found");
                throw new KeyNotFoundException("User not found");
            }
            
            var buyInfo = await _buyInfoRepository.GetByIdAsync(parsedBuyId);

            if (buyInfo == null)
            {
                _logger.LogWarning("BuyInfo not found");
                throw new KeyNotFoundException("BuyInfo not found");
            }
            
            if (!(await _storeRepository.ExistsByUserId(buyInfo.UserId)))
            {
                _logger.LogWarning("It's not your store");
                throw new AccessDeniedException("It's not your store");
            }

            if (buyInfo.Status != DeliveryStatus.AwaitingConfirmation && buyInfo.Status != DeliveryStatus.WaitingForShipment)
            {
                _logger.LogInformation("You cannot cancel this order");
                throw new InvalidOperationException("You cannot cancel this order");
            }
            
            buyInfo.Status = DeliveryStatus.Declined;
            
            var result = await _buyInfoRepository.UpdateAsync(buyInfo);
            if (!result)
            {
                _logger.LogWarning("Failed to cancel product");
                throw new InvalidOperationException("Failed to cancel product");
            }
            
            await _notificationService.SendNotificationAsync(new NotificationCreateDto()
            {
                From = null, IsHighPriority = false,
                Message = "Продавець скасував замовлення.", MetadataUrl = null, To = userId,
                Type = NotificationType.Info
            });
            
            _logger.LogInformation("CancelSellAsync successes");
        }
    }

    /// <summary>
    /// Retrieves purchase information by ID.
    /// </summary>
    /// <param name="buyId">The ID of the purchase order.</param>
    public async Task<BuyInfoDto> GetBuyInfoAsync(string buyId)
    {
        using (_logger.BeginScope("GetBuyInfoAsync"))
        {
            _logger.LogInformation("GetBuyInfoAsync called");
            var parsedBuyId = ObjectId.Parse(buyId);
            var buyInfo = await _buyInfoRepository.GetByIdAsync(parsedBuyId);
            if (buyInfo == null)
                _logger.LogWarning("BuyInfo not found");
            _logger.LogInformation("GetBuyInfoAsync successes");
            return _mapper.Map<BuyInfoDto>(buyInfo);
        }
    }

    /// <summary>
    /// Retrieves all purchases made by a specific user.
    /// </summary>
    /// <param name="userId">The ID of the buyer.</param>
    public async Task<IEnumerable<BuyInfoDto>> GetBuyInfoByUserIdAsync(string userId)
    {
        using (_logger.BeginScope("GetBuyInfoByUserIdAsync"))
        {
            _logger.LogInformation("GetBuyInfoByUserIdAsync called");
            var parsedUserId = ObjectId.Parse(userId);
            var buyInfo = await _buyInfoRepository.GetByUserIdAsync(parsedUserId);
            if (buyInfo == null)
                _logger.LogWarning("BuyInfo not found");
            _logger.LogInformation("GetBuyInfoByUserIdAsync successes");
            return _mapper.Map<IEnumerable<BuyInfoDto>>(buyInfo);
        }
    }

    /// <summary>
    /// Retrieves all purchases related to a specific seller.
    /// </summary>
    /// <param name="sellerId">The ID of the seller.</param>
    public async Task<IEnumerable<BuyInfoDto>> GetBuyInfoBySellerIdAsync(string sellerId)
    {
        using (_logger.BeginScope("GetBuyInfoBySellerIdAsync"))
        {
            _logger.LogInformation("GetBuyInfoBySellerIdAsync called");
            var parsedSellerId = ObjectId.Parse(sellerId);
            var buyInfo = await _buyInfoRepository.GetBySellerId(parsedSellerId);
            if (buyInfo == null)
                _logger.LogWarning("BuyInfo not found");
            _logger.LogInformation("GetBuyInfoBySellerIdAsync successes");
            return _mapper.Map<IEnumerable<BuyInfoDto>>(buyInfo);
        }
    }
}