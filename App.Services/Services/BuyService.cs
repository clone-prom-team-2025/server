using App.Core.Enums;
using App.Core.Interfaces;
using App.Core.Models.Product;
using App.Core.Models.Sell;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services.Services;

public class BuyService(IBuyInfoRepository buyInfoRepository, ILogger<BuyService> logger, IMapper mapper, IStoreRepository storeRepository, IUserRepository userRepository, IProductRepository productRepository)
{
    private readonly ILogger<BuyService> _logger = logger;
    private readonly IMapper _mapper =  mapper;
    private readonly IBuyInfoRepository _buyInfoRepository = buyInfoRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IProductRepository _productRepository = productRepository;

    public async Task BuyProductAsync(string productId, string userId, DeliveryPayment deliveryPayment,
        PointsOfDelivery deliveryTo)
    {
        using (_logger.BeginScope("BuyProductAsync"))
        {
            _logger.LogInformation("BuyProductAsync called");
            var parsedUserId = ObjectId.Parse(userId);
            var parsedProductId = ObjectId.Parse(productId);
            if (!(await _userRepository.ExistsById(parsedUserId)))
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }
            
            var product = await _productRepository.GetByIdAsync(parsedProductId);

            if (product == null)
            {
                _logger.LogInformation("Product not found");
                throw new KeyNotFoundException("Product not found");
            }

            if (!(await _storeRepository.ExistsById(product.SellerId)))
            {
                _logger.LogInformation("Store not found");
                throw new KeyNotFoundException("Store not found");
            }

            if (product.Quantity <= 0)
            {
                _logger.LogInformation("Product quantity not set");
                throw new KeyNotFoundException("Product is out of stock.");
            }

            var miniInfo = new MiniProductInfo()
            {
                Image = new(),
                Price = product.DiscountPrice ?? product.Price,
                ProductId = product.Id,
                ProductName = product.Name,
            };

            var buy = new BuyInfo()
            {
                Id = ObjectId.GenerateNewId(),
                SellerId = product.SellerId,
                TrackingNumber = null,
                UserId = parsedUserId,
                DeliveryToInfo = deliveryTo,
                Payment = deliveryPayment,
                Status = DeliveryStatus.AwaitingConfirmation,
                MiniProductInfo = miniInfo,
                Payed = deliveryPayment == DeliveryPayment.Card ?  false : null,
            };
            
            await _buyInfoRepository.CreateAsync(buy);
        }
    }

    // public async Task AcceptSell(string buyId, string userId)
    // {
    //     using (_logger.BeginScope("AcceptSell"))
    //     {
    //         _logger.LogInformation("AcceptSell called");
    //         var parsedBuyId = ObjectId.Parse(buyId);
    //         var parsedUserId = ObjectId.Parse(userId);
    //         
    //     }
    // }
}