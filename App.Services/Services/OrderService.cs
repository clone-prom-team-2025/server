using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Security.Cryptography;
using App.Core.DTOs.Notification;
using App.Core.DTOs.Sell;
using App.Core.Enums;
using App.Core.Exceptions;
using App.Core.Interfaces;
using App.Core.Models.Email;
using App.Core.Models.FileStorage;
using App.Core.Models.Sell;
using App.Core.Utils;
using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace App.Services.Services;

/// <summary>
/// Service responsible for handling product purchases, payments, shipping, and order management.
/// </summary>
public class OrderService(
    IOrderRepository orderRepository,
    ILogger<OrderService> logger,
    IMapper mapper,
    IStoreRepository storeRepository,
    IUserRepository userRepository,
    IProductRepository productRepository,
    IProductMediaRepository productMediaRepository,
    IFileService fileService,
    INotificationService notificationService,
    ICartRepository cartRepository,
    IEmailService emailService,
    IMemoryCache memoryCache) : IOrderService
{
    private readonly ILogger<OrderService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IMemoryCache _cache = memoryCache;
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IProductMediaRepository _productMediaRepository = productMediaRepository;
    private readonly IFileService _fileService = fileService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ICartRepository _cartRepository = cartRepository;
    private readonly IEmailService _emailService = emailService;
    
    public async Task BuyRegistered(
        string userId, 
        DeliveryPayment deliveryPayment, 
        PointsOfDelivery deliveryTo,
        string? phoneNumber, 
        string? firstName, 
        string? lastName, 
        string? middleName)
    {
        using var scope = _logger.BeginScope("BuyRegistered");
        _logger.LogInformation("BuyRegistered called for user {UserId}", userId);

        var parsedUserId = ObjectId.Parse(userId);
        var user = await _userRepository.GetUserByIdAsync(parsedUserId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", parsedUserId);
            throw new KeyNotFoundException("User not found");
        }

        var userPhone = phoneNumber ?? user.PhoneNumber;
        if (string.IsNullOrWhiteSpace(userPhone))
        {
            _logger.LogWarning("User {UserId} doesn't have phone number", parsedUserId);
            throw new KeyNotFoundException("Phone number not found");
        }

        var carts = await _cartRepository.GetByUserIdAsync(parsedUserId);
        if (carts == null || carts.Count == 0)
        {
            _logger.LogWarning("User {UserId} doesn't have any carts", parsedUserId);
            throw new KeyNotFoundException("Cart not found");
        }

        var orders = new List<Order>();
        
        var bytes = new byte[10];
        RandomNumberGenerator.Fill(bytes);
        string orderNumber = Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        string Base36Encode(long value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";
            do
            {
                result = chars[(int)(value % 36)] + result;
                value /= 36;
            } while (value > 0);
            return result;
        }
        
        foreach (var cart in carts)
        {
            var product = await _productRepository.GetByIdAsync(cart.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found for User {user}, Cart {cart}", cart.ProductId, userId, cart.Id.ToString());
                throw new KeyNotFoundException($"{cart.ProductId} product not found");
            }
            if (product.Quantity < cart.Pcs)
                throw new InvalidOperationException($"{cart.ProductId} not enough pcs");
        }
        
        foreach (var cart in carts)
        {
            var product = await _productRepository.GetByIdAsync(cart.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", cart.ProductId);
                throw new KeyNotFoundException($"{cart.ProductId} product not found");
            }
            
            var productMedia = await _productMediaRepository.GetByProductIdAsync(product.Id.ToString());
            
            Stream? stream = null;

            if (productMedia?.Count > 0)
            {
                var firstUrl = productMedia.First().Files.SourceUrl;
                stream = await WebpDownloader.GetWebpStreamAsync(firstUrl);
            }
            
            var file = new BaseFile();
            if (stream != null)
                (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) =
                    await _fileService.SaveImageAsync(stream, product.Id.ToString(), "orders", 100, 70);

            var buyInfo = new Order
            {
                Id = ObjectId.GenerateNewId(),
                UserId = parsedUserId,
                FirstName = firstName ?? user.FirstName,
                LastName = lastName ?? user.LastName,
                MiddleName = middleName ?? user.MiddleName,
                PhoneNumber = userPhone,
                Status = DeliveryStatus.AwaitingConfirmation,
                Payment = deliveryPayment,
                DeliveryToInfo = deliveryTo,
                TotalPrice = (product.DiscountPrice ?? product.Price) * cart.Pcs,
                SellerId = product.SellerId,
                Confirmed = false,
                Registered = true,
                Email = user.Email,
                OrderNumber = orderNumber,
                CreatedAt = DateTime.UtcNow,
                
                MiniProductsInfo = new MiniProductInfo
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Image = file
                },
                Pcs = cart.Pcs,
            };

            product.Quantity -= cart.Pcs;
            product.QuantityStatus = product.Quantity switch
            {
                0 => QuantityStatus.OutOfStock,
                <= 4 => QuantityStatus.Ending,
                _ => QuantityStatus.InStock
            };

            var updateResult = await _productRepository.UpdateAsync(product);
            if (!updateResult)
            {
                _logger.LogWarning("Failed to update product {ProductId} for User {user}, Cart {cart}", product.Id, userId, cart.Id.ToString());
                foreach (var created in orders)
                {
                    await _orderRepository.DeleteAsync(created.Id);
                    var prodToRestore = await _productRepository.GetByIdAsync(created.MiniProductsInfo.ProductId);
                    if (prodToRestore != null)
                    {
                        prodToRestore.Quantity += cart.Pcs;
                        prodToRestore.QuantityStatus = prodToRestore.Quantity switch
                        {
                            0 => QuantityStatus.OutOfStock,
                            <= 4 => QuantityStatus.Ending,
                            _ => QuantityStatus.InStock
                        };
                        await _productRepository.UpdateAsync(prodToRestore);
                    }
                }
                throw new InvalidOperationException($"Failed to buy product {product.Id}");
            }
            
            orders.Add(buyInfo);
        }

        await _orderRepository.CreateManyAsync(orders);
        await _cartRepository.DeleteByUserIdAsync(parsedUserId);
        var msg = $"Дякуємо за покупку! Очікуйте підтвердження від {(orders.Count == 1 ? "продавця" : "продавців")}";

        var notification = new NotificationCreateDto()
        {
            Type = NotificationType.Info,
            Message = msg,
            From = null,
            To = userId,
            IsHighPriority = false,
        };
        await _notificationService.SendNotificationAsync(notification);
        
        _logger.LogInformation("Start sending email to {Email}", user.Email);
        if (deliveryPayment != DeliveryPayment.AfterPayment) await SendEmailPayCard(user.Email, orders);
        else await SendEmailAfterPayment(user.Email, orderNumber);
        _logger.LogInformation("Finished sending email to {Email}", user.Email);

        _logger.LogInformation("BuyRegistered successfully completed for user {UserId}", userId);
    }
    
    public async Task BuyUnRegistered(
        Dictionary<string, int> products,
        DeliveryPayment deliveryPayment, 
        PointsOfDelivery deliveryTo,
        string email,
        string phoneNumber, 
        string firstName, 
        string lastName, 
        string? middleName)
    {
        using var scope = _logger.BeginScope("BuyRegistered");
        _logger.LogInformation("BuyUnRegistered called");
        
        if (string.IsNullOrWhiteSpace(phoneNumber))
        {
            _logger.LogWarning("Phone number missing");
            throw new KeyNotFoundException("Phone number not found");
        }

        var orders = new List<Order>();
        
        var bytes = new byte[10];
        RandomNumberGenerator.Fill(bytes);
        string orderNumber = Convert.ToBase64String(bytes)
            .Replace("+", "")
            .Replace("/", "")
            .Replace("=", "");

        string Base36Encode(long value)
        {
            const string chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string result = "";
            do
            {
                result = chars[(int)(value % 36)] + result;
                value /= 36;
            } while (value > 0);
            return result;
        }
        
        foreach (var productId in products.Keys)
        {
            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", productId);
                throw new KeyNotFoundException($"{productId} product not found");
            }
            if (product.Quantity < 1)
                throw new InvalidOperationException($"{productId} not enough pcs");
        }
        
        foreach (var prod in products)
        {
            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(prod.Key));
            if (product == null)
            {
                _logger.LogWarning("Product {ProductId} not found", prod.Key);
                throw new KeyNotFoundException($"{prod.Key} product not found");
            }
            
            var productMedia = await _productMediaRepository.GetByProductIdAsync(product.Id.ToString());
            
            Stream? stream = null;

            if (productMedia?.Count > 0)
            {
                var firstUrl = productMedia.First().Files.SourceUrl;
                stream = await WebpDownloader.GetWebpStreamAsync(firstUrl);
            }
            
            var file = new BaseFile();
            if (stream != null)
                (file.SourceUrl, file.CompressedUrl, file.SourceFileName, file.CompressedFileName) =
                    await _fileService.SaveImageAsync(stream, product.Id.ToString(), "orders", 100, 70);

            var buyInfo = new Order
            {
                Id = ObjectId.GenerateNewId(),
                UserId = null,
                FirstName = firstName,
                LastName = lastName,
                MiddleName = middleName,
                PhoneNumber = phoneNumber,
                Status = DeliveryStatus.AwaitingConfirmation,
                Payment = deliveryPayment,
                DeliveryToInfo = deliveryTo,
                TotalPrice = (product.DiscountPrice ?? product.Price) * prod.Value,
                SellerId = product.SellerId,
                Confirmed = false,
                Registered = true,
                Email = email,
                OrderNumber = orderNumber,
                CreatedAt = DateTime.UtcNow,
                
                MiniProductsInfo = new MiniProductInfo
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Image = file
                },
                Pcs = prod.Value,
            };

            product.Quantity -= prod.Value;
            product.QuantityStatus = product.Quantity switch
            {
                0 => QuantityStatus.OutOfStock,
                <= 4 => QuantityStatus.Ending,
                _ => QuantityStatus.InStock
            };

            var updateResult = await _productRepository.UpdateAsync(product);
            if (!updateResult)
            {
                _logger.LogWarning("Failed to update product {ProductId}", product.Id);
                foreach (var created in orders)
                {
                    await _orderRepository.DeleteAsync(created.Id);
                    var prodToRestore = await _productRepository.GetByIdAsync(created.MiniProductsInfo.ProductId);
                    if (prodToRestore != null)
                    {
                        prodToRestore.Quantity += prod.Value;
                        prodToRestore.QuantityStatus = prodToRestore.Quantity switch
                        {
                            0 => QuantityStatus.OutOfStock,
                            <= 4 => QuantityStatus.Ending,
                            _ => QuantityStatus.InStock
                        };
                        await _productRepository.UpdateAsync(prodToRestore);
                    }
                }
                throw new InvalidOperationException($"Failed to buy product {product.Id}");
            }
            
            orders.Add(buyInfo);
        }

        await _orderRepository.CreateManyAsync(orders);
        
        if (deliveryPayment != DeliveryPayment.AfterPayment) await SendEmailPayCard(email, orders);
        else await SendEmailAfterPayment(email, orderNumber);

        _logger.LogInformation("BuyUnRegistered successfully completed");
    }

    public async Task<DeliveryAndPaymentDto> GetDeliveryTypeAsync(string userId)
    {
        using var scope = _logger.BeginScope("GetDeliveryTypeAsync");
        _logger.LogInformation("GetDeliveryTypeAsync called for user {UserId}", userId);

        var parsedUserId = ObjectId.Parse(userId);
        var user = await _userRepository.GetUserByIdAsync(parsedUserId);
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", parsedUserId);
            throw new KeyNotFoundException("User not found");
        }
        
        var carts = await _cartRepository.GetByUserIdAsync(parsedUserId);
        if (carts == null || carts.Count == 0)
        {
            _logger.LogWarning("Cart not found for user {UserId}", parsedUserId);
            throw new KeyNotFoundException("Cart not found");
        }
        
        ProductDeliveryType? commonDeliveryType = null;
        PaymentOptions? commonPaymentOptions = null;

        foreach (var cart in carts)
        {
            var product = await _productRepository.GetByIdAsync(cart.ProductId);
            if (product == null)
            {
                _logger.LogWarning("Product not found {ProductId}", cart.ProductId);
                throw new KeyNotFoundException($"Product {cart.ProductId} not found");
            }

            if (commonDeliveryType == null)
                commonDeliveryType = product.DeliveryType;
            else
                commonDeliveryType &= product.DeliveryType;

            if (commonPaymentOptions == null)
                commonPaymentOptions = product.PaymentOptions;
            else
                commonPaymentOptions &= product.PaymentOptions;
        }
        
        if (commonDeliveryType == 0)
        {
            _logger.LogWarning("No common delivery options found for user {UserId}", userId);
            throw new InvalidOperationException("No common delivery options available for the products in the cart.");
        }
        if (commonPaymentOptions == 0)
        {
            _logger.LogWarning("No common payment options found for user {UserId}", userId);
            throw new InvalidOperationException("No common payment options available for the products in the cart.");
        }

        return new DeliveryAndPaymentDto
        {
            ProductDeliveryType = commonDeliveryType ?? 0,
            PaymentOptions = commonPaymentOptions ?? 0
        };
    }
    
    public async Task<IEnumerable<OrderDto>> GetByUserId(string userId)
    {
        using var scope = _logger.BeginScope("GetByUserId");
        _logger.LogInformation("Getting buy infos for user {UserId}", userId);
        var result = await _orderRepository.GetByUserIdAsync(ObjectId.Parse(userId));
        _logger.LogInformation("Getting buy infos successfully for user {UserId}", userId);
        return _mapper.Map<IEnumerable<OrderDto>>(result);
    }
    
    public async Task<IEnumerable<GroupedOrders>> GetByUserIdGrouped(string userId)
    {
        using var scope = _logger.BeginScope("GetByUserId");
        _logger.LogInformation("Getting buy infos for user {UserId}", userId);
        var result = await _orderRepository.GetByUserIdAsync(ObjectId.Parse(userId));
        var mapped = _mapper.Map<IEnumerable<OrderDto>>(result);
        var grouped = mapped
            .GroupBy(o => o.OrderNumber)
            .Select(g => new GroupedOrders
            {
                OrderNumber = g.Key,
                Orders = g.ToList()
            });
        
        _logger.LogInformation("Getting buy infos successfully for user {UserId}", userId);
        return grouped;
    }

    public async Task<IEnumerable<OrderDto>> GetByStoreNeedToAccept(string userId)
    {
        using var scope = _logger.BeginScope("GetByStoreNeedToAccept");
        _logger.LogInformation("GetByStoreNeedToAccept called for UserId={userId}", userId);
        var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
        if (store == null)
        {
            _logger.LogWarning("Store not found");
            throw new KeyNotFoundException("Store not found");
        }

        var orders = await _orderRepository.GetBySellerId(store.Id);
        if (orders == null)
        {
            _logger.LogWarning("Order not found");
            return [];
        }
        var filteredBuyInfos = orders.Where(b => !b.Confirmed);
        _logger.LogInformation("Getting buy infos successfully for user {UserId}", userId);
        return _mapper.Map<IEnumerable<OrderDto>>(filteredBuyInfos);
    }
    
    public async Task<IEnumerable<OrderDto>> GetByStoreAccepted(string userId)
    {
        using var scope = _logger.BeginScope("GetByStoreAccepted");
        _logger.LogInformation("GetByStoreAccepted called for UserId={userId}", userId);
        var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
        if (store == null)
        {
            _logger.LogWarning("Store not found");
            throw new KeyNotFoundException("Store not found");
        }

        var orders = await _orderRepository.GetBySellerId(store.Id);
        if (orders == null)
        {
            _logger.LogWarning("Order not found");
            return [];
        }
        var filteredBuyInfos = orders.Where(b => b.Confirmed);
        _logger.LogInformation("Getting buy infos successfully for user {UserId}", userId);
        return _mapper.Map<IEnumerable<OrderDto>>(filteredBuyInfos);
    }

    public async Task RejectOrder(string userId, string orderId, string reason)
    {
        using var scope = _logger.BeginScope("RejectOrder");
        _logger.LogInformation("RejectOrder called for userId {UserId}", userId);
        var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
        if (store == null)
        {
            _logger.LogWarning("Store not found for User {user}", userId);
            throw new KeyNotFoundException("Store not found");
        }
        
        var order = await _orderRepository.GetByIdAsync(ObjectId.Parse(orderId));
        if (order == null)
        {
            _logger.LogWarning("Order {order} not found for User {user}", orderId, userId);
            throw new KeyNotFoundException("Order not found");
        }

        if (order.SellerId != store.Id)
        {
            _logger.LogWarning("User {userId} does not own order {order} and tried to accept order", userId, orderId);
            throw new AccessDeniedException("It's not your order");
        }

        if (order.Status is not (DeliveryStatus.AwaitingConfirmation or DeliveryStatus.WaitingForShipment))
        {
            _logger.LogWarning("User {userId} tried reject order but status is not AwaitingConfirmation or WaitingForShipment", userId);
        }

        order.Confirmed = false;
        order.Status = DeliveryStatus.Declined;
        order.SellerMessage = reason;
        
        var result = await _orderRepository.UpdateAsync(order);
        _logger.LogInformation("Rejected order {OrderId}", order.Id);
        
        if (order.Registered)
        {
            var notification = new NotificationCreateDto()
            {
                Type = NotificationType.Info,
                Message = $"Підзамовлення #{order.Id.ToString()} у складі замовлення #{order.OrderNumber} скасовано продавцем! Причина: {reason}",
                From = store.Name,
                To = order.UserId.ToString(),
                IsHighPriority = false,
            };
            await _notificationService.SendNotificationAsync(notification);
        }
        var assembly = Assembly.GetExecutingAssembly();
        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.OrderCanceled.html");
        using var readerOrderEmail = new StreamReader(streamOrderEmail!);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmail = htmlOrderEmail.Replace("__ORDERID__", order.OrderNumber)
            .Replace("__ORDERID2__", order.Id.ToString())
            .Replace("__TRACK__", order.TrackingNumber)
            .Replace("__COMMENT__", reason);
        
        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [order.Email],
            Subject = "Ваше замовлення відхилене",
            HtmlBody = readyOrderEmail
        };
        
        await _emailService.SendEmailAsync(mail);
    }

    public async Task CancelOrder(string userId, string orderId)
    {
        using var scope = _logger.BeginScope("CancelOrder");
        _logger.LogInformation("CancelOrder called for userId {UserId}", userId);
        
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
        {
            _logger.LogWarning("User not found for User {user}", userId);
            throw new KeyNotFoundException("User not found");
        }
        
        var order = await _orderRepository.GetByIdAsync(ObjectId.Parse(orderId));
        if (order == null)
        {
            _logger.LogWarning("Order {order} not found for User {user}", orderId, userId);
            throw new KeyNotFoundException("Order not found");
        }

        if (order.UserId == null || order.UserId.ToString() != userId)
        {
            _logger.LogWarning("User {userId} does not own order {order} and tried to cancel order", userId, orderId);
            throw new AccessDeniedException("It's not your order");
        }

        if (order.Status is (DeliveryStatus.Canceled or DeliveryStatus.Declined or DeliveryStatus.Received))
        {
            _logger.LogWarning("User {userId} tried cancel order but status is not AwaitingConfirmation or WaitingForShipment", userId);
        }

        order.Confirmed = false;
        order.Status = DeliveryStatus.Canceled;
        
        var result = await _orderRepository.UpdateAsync(order);
        if (!result)
        {
            _logger.LogWarning("Failed to update order");
            throw new InvalidOperationException($"Failed to update order");
        }
        
        var assembly = Assembly.GetExecutingAssembly();
        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.OrderCanceledByUser.html");
        using var readerOrderEmail = new StreamReader(streamOrderEmail!);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmailHtml = htmlOrderEmail.Replace("__ORDERID__", order.OrderNumber).Replace("__ORDERID2__", order.Id.ToString());
        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [user.Email],
            Subject = "Ваше замовлення скасоване",
            HtmlBody = readyOrderEmailHtml
        };
        await _emailService.SendEmailAsync(mail);

        _logger.LogInformation("Canceled order {OrderId}", order.Id);
    }
    
    public async Task CancelOrdersByOrderNumber(string userId, string orderNumber)
    {
        using var scope = _logger.BeginScope("CancelOrder");
        _logger.LogInformation("CancelOrder called for userId {UserId}", userId);
        
        var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
        if (user == null)
        {
            _logger.LogWarning("User not found for User {user}", userId);
            throw new KeyNotFoundException("User not found");
        }
        
        var orders = await _orderRepository.GetByOrderNumberAsync(orderNumber);
        if (orders == null || orders.Count == 0)
        {
            _logger.LogWarning("Order {order} not found for User {user}", orderNumber, userId);
            throw new KeyNotFoundException("Order not found");
        }
        foreach (var order in orders)
        {
            if (order.UserId == null || order.UserId.ToString() != userId)
            {
                _logger.LogWarning("User {userId} does not own order {order} and tried to cancel order", userId, order.Id.ToString());
                throw new AccessDeniedException("It's not your order");
            }

            if (order.Status is (DeliveryStatus.Canceled or DeliveryStatus.Declined or DeliveryStatus.Received))
            {
                _logger.LogWarning("User {userId} tried cancel order but status is not AwaitingConfirmation or WaitingForShipment", userId);
                continue;
            }

            order.Confirmed = false;
            order.Status = DeliveryStatus.Canceled;
        
            var result = await _orderRepository.UpdateAsync(order);
            if (!result)
            {
                _logger.LogWarning("Failed to update order");
                throw new InvalidOperationException($"Failed to update order");
            }
            _logger.LogInformation("Canceled order {OrderId}", order.Id);
        }
        
        var assembly = Assembly.GetExecutingAssembly();
        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.OrderCanceledByUser.html");
        using var readerOrderEmail = new StreamReader(streamOrderEmail!);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmailHtml = htmlOrderEmail.Replace("__ORDERID__", orders.First().OrderNumber).Replace("__ORDERID2__", string.Join(", ", orders.Select(o => o.Id.ToString())));
        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [user.Email],
            Subject = "Ваше замовлення скасоване",
            HtmlBody = readyOrderEmailHtml
        };
        await _emailService.SendEmailAsync(mail);
    }

    public async Task SendGetOrdersByEmail(string email)
    {
        using var scope = _logger.BeginScope("SendGetOrdersByEmail");
        _logger.LogInformation("SendGetOrdersByEmail called for email {Email}", email);
        
        var code = CodeGenerator.GenerateCode(6);
        var assembly = Assembly.GetExecutingAssembly();
        await using var stream = assembly.GetManifestResourceStream("App.Services.EmailTemplates.EmailCode.html");
        using var reader = new StreamReader(stream!);
        var html = await reader.ReadToEndAsync();
        var readyEmail = html.Replace("__CODE__", code);
        
        SaveVerificationCode(email, code, 15);
        
        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [email],
            Subject = "Підтвердіть дію",
            HtmlBody = readyEmail
        };
        
        await _emailService.SendEmailAsync(mail);
        _logger.LogInformation("SendGetOrdersByEmail successfully completed for email {Email}", email);
    }

    // public async Task<IEnumerable<BuyUnRegisteredRequest>> GetByEmailCode(string email, string code)
    // {
    //     using var scope = _logger.BeginScope("GetByEmailCode");
    //     _logger.LogInformation("GetByEmailCode called for email {Email}", email);
    //     if (!TryGetVerificationCode(email, out var savedCode) || savedCode != code)
    //     {
    //         _logger.LogWarning("Invalid or expired code for email {Email}", email);
    //         throw new InvalidOperationException("Invalid or expired code");
    //     }
    // }

    public async Task AcceptOrder(string userId, string buyInfoId)
    {
        using var scope = _logger.BeginScope("AcceptBuyInfo");
        _logger.LogInformation("AcceptBuyInfo called for UserId={userId}", userId);
        var store = await _storeRepository.GetStoreByUserId(ObjectId.Parse(userId));
        if (store == null)
        {
            _logger.LogWarning("Store not found for User {userId}", userId);
            throw new KeyNotFoundException("Store not found");
        }
        
        var order = await _orderRepository.GetByIdAsync(ObjectId.Parse(buyInfoId));
        if (order == null)
        {
            _logger.LogWarning("Order not found for Store {storeId}", store.Id.ToString());
            throw new KeyNotFoundException("Order not found");
        }

        if (order.Confirmed)
        {
            _logger.LogInformation("Order already confirmed");
            return;
        }
        
        order.Confirmed = true;
        order.Status = DeliveryStatus.WaitingForShipment;
        var trackNumber = Guid.NewGuid().ToString("N");
        order.TrackingNumber = trackNumber;

        var result = await _orderRepository.UpdateAsync(order);
        if (!result)
        {
            _logger.LogWarning("Failed to update order");
            throw new InvalidOperationException($"Failed to update order");
        }


        if (order.Registered)
        {
            var notification = new NotificationCreateDto()
            {
                Type = NotificationType.Info,
                Message = $"Підзамовлення #{order.Id.ToString()} у складі замовлення #{order.OrderNumber} підтверджено продавцем!",
                From = store.Name,
                To = order.UserId.ToString(),
                IsHighPriority = false,
            };
            await _notificationService.SendNotificationAsync(notification);
        }
        var assembly = Assembly.GetExecutingAssembly();
        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.OrderConfirmed.html");
        using var readerOrderEmail = new StreamReader(streamOrderEmail!);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmail = htmlOrderEmail.Replace("__ORDERID__", order.OrderNumber)
            .Replace("__ORDERID2__", order.Id.ToString())
            .Replace("__TRACK__", order.TrackingNumber);
        
        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [order.Email],
            Subject = "Ваше замовлення прийняте",
            HtmlBody = readyOrderEmail
        };
        
        await _emailService.SendEmailAsync(mail);
        _logger.LogInformation("Order accepted");
    }

    private async Task SendEmailAfterPayment(string email, string orderNumber)
    {
        _logger.LogInformation("SendEmailAfterPayment started for email {Email} and order {OrderNumber}", email, orderNumber);

        var assembly = Assembly.GetExecutingAssembly();
        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.SuccessOrderAfterPayment.html");
        if (streamOrderEmail == null)
        {
            _logger.LogWarning("Email template 'SuccessOrderAfterPayment.html' not found in assembly resources.");
            return;
        }

        using var readerOrderEmail = new StreamReader(streamOrderEmail);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmailHtml = htmlOrderEmail.Replace("__ORDERID__", orderNumber);

        _logger.LogInformation("Email HTML prepared for order {OrderNumber}", orderNumber);

        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [email],
            Subject = "Ваше замовлення зареєстроване",
            HtmlBody = readyOrderEmailHtml
        };

        await _emailService.SendEmailAsync(mail);
        _logger.LogInformation("Email successfully sent to {Email} for order {OrderNumber}", email, orderNumber);
    }

    private async Task SendEmailPayCard(string email, List<Order> order)
    {
        if (order.Count == 0)
        {
            _logger.LogWarning("SendEmailPayCard called with empty order list for email {Email}", email);
            return;
        }

        _logger.LogInformation("SendEmailPayCard started for email {Email} with {OrderCount} items", email, order.Count);

        decimal totalPrice = 0;
        foreach (var orderItem in order)
        {
            totalPrice += orderItem.TotalPrice;
        }
        _logger.LogInformation("Total price calculated: {TotalPrice}", totalPrice);

        var htmlRows = new List<string>();
        foreach (var item in order)
        {
            var row = @"
    <tr>
        <td>АРТ.№ __ARTNO__ __PRODUCTNAME__</td>
        <td align='right'>__PRICE__ Б</td>
    </tr>
    <tr>
        <td style='font-size:12px;'>__QUANTITY__ шт × __PRICE__</td>
        <td align='right' style='font-size:12px;'>= __TOTALPRICE__</td>
    </tr>";

            row = row.Replace("__ARTNO__", item.MiniProductsInfo.ProductId.ToString())
                .Replace("__PRODUCTNAME__", item.MiniProductsInfo.ProductName)
                .Replace("__PRICE__", item.MiniProductsInfo.Price.ToString("F2", CultureInfo.InvariantCulture))
                .Replace("__QUANTITY__", item.Pcs.ToString())
                .Replace("__TOTALPRICE__", item.TotalPrice.ToString("F2", CultureInfo.InvariantCulture));

            htmlRows.Add(row);
        }

        var productsHtml = string.Join(Environment.NewLine, htmlRows);

        var kyivTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");
        var kyivTime = TimeZoneInfo.ConvertTimeFromUtc(order.First().CreatedAt, kyivTimeZone);
        var date = kyivTime.ToString("dd-MM-yyyy HH:mm:ss");

        _logger.LogInformation("Order date converted to Kyiv time: {KyivTime}", date);

        var assembly = Assembly.GetExecutingAssembly();
        await using var streamOrder = assembly.GetManifestResourceStream("App.Services.EmailTemplates.Order.html");
        if (streamOrder == null)
        {
            _logger.LogWarning("Email template 'Order.html' not found in assembly resources.");
            return;
        }

        using var readerOrder = new StreamReader(streamOrder);
        var htmlOrder = await readerOrder.ReadToEndAsync();
        var readyOrderHtml = htmlOrder
            .Replace("__ORDERID__", order.First().OrderNumber.ToString())
            .Replace("__ORDERTO__", order.First().DeliveryToInfo.Region + ", " + order.First().DeliveryToInfo.Settlement)
            .Replace("__ADDRESS__", order.First().DeliveryToInfo.Address)
            .Replace("__FINALPRICE__", totalPrice.ToString("F2"))
            .Replace("__BONUS__", "0")
            .Replace("__PRICETOPAY__", totalPrice.ToString("F2"))
            .Replace("__DATE__", date)
            .Replace("__PRODUCTS__", productsHtml);

        _logger.LogInformation("Order HTML prepared for PDF generation for order {OrderNumber}", order.First().OrderNumber);

        _logger.LogInformation("Starting PDF generation for order {OrderNumber}", order.First().OrderNumber);
        var file = await GenerateOrderPdfAsync(order, readyOrderHtml);
        _logger.LogInformation("PDF generation completed for order {OrderNumber}", order.First().OrderNumber);

        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.SuccessOrderWithCard.html");
        using var readerOrderEmail = new StreamReader(streamOrderEmail!);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmailHtml = htmlOrderEmail.Replace("__ORDERID__", order.First().OrderNumber);

        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [ email ],
            Subject = "Ваше замовлення зареєстроване",
            HtmlBody = readyOrderEmailHtml
        };

        _logger.LogDebug("Order HTML for logging: {ReadyOrderHtml}", readyOrderHtml);

        await _emailService.SendEmailAsync(mail, file, $"Замовлення{order.First().OrderNumber}.pdf");
        _logger.LogInformation("Email with PDF successfully sent to {Email} for order {OrderNumber}", email, order.First().OrderNumber);
    }


    private static readonly SemaphoreSlim _pdfSemaphore = new SemaphoreSlim(2);
    
    public async Task<byte[]> GenerateOrderPdfAsync(IEnumerable<Order> order, string readyOrderHtml)
    {
        if (!order.Any())
        {
            _logger.LogWarning("GenerateOrderPdfAsync called with empty order list.");
            return Array.Empty<byte>();
        }

        var orderId = order.First().OrderNumber;
        _logger.LogInformation("GenerateOrderPdfAsync started for order {OrderId}", orderId);
        _logger.LogDebug("HTML length for order {OrderId}: {HtmlLength}", orderId, readyOrderHtml.Length);

        await _pdfSemaphore.WaitAsync();
        _logger.LogDebug("Entered PDF semaphore for order {OrderId}", orderId);

        try
        {
            // Завантажуємо Chromium (одноразово, кешується)
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();
            
            var launchOptions = new LaunchOptions
            {
                Headless = true,
                Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
            };

            using var browser = await Puppeteer.LaunchAsync(launchOptions);
            using var page = await browser.NewPageAsync();

            await page.SetContentAsync(readyOrderHtml, new NavigationOptions
            {
                WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
            });

            var pdfOptions = new PdfOptions
            {
                Format = PaperFormat.A4,
                PrintBackground = true
            };

            var sw = Stopwatch.StartNew();
            _logger.LogInformation("Starting PDF conversion for order {OrderId} at {Time}", orderId, DateTime.UtcNow);

            var pdfBytes = await page.PdfDataAsync(pdfOptions);

            sw.Stop();
            _logger.LogInformation(
                "PDF conversion completed for order {OrderId} at {Time}, elapsed {ElapsedMilliseconds} ms, PDF size: {PdfSize} bytes",
                orderId, DateTime.UtcNow, sw.ElapsedMilliseconds, pdfBytes.Length
            );

            return pdfBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during PDF generation for order {OrderId}", orderId);
            throw;
        }
        finally
        {
            _pdfSemaphore.Release();
            _logger.LogDebug("Released PDF semaphore for order {OrderId}", orderId);
        }
    }
    
    // private async Task<byte[]> GenerateOrderPdfAsync(IEnumerable<Order> order, string readyOrderHtml)
    // {
    //     if (!order.Any()) return Array.Empty<byte>();
    //
    //     var orderId = order.First().OrderNumber;
    //
    //     var pdfDoc = new HtmlToPdfDocument()
    //    
    //
    //     var converter = new SynchronizedConverter(new PdfTools());
    //     byte[] pdfBytes = await Task.Run(() => converter.Convert(pdfDoc));
    //     
    //     return pdfBytes;
    // }
    
    private void SaveVerificationCode(string email, string code, int expires)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(expires));
        _cache.Set($"verify:{email}", code, cacheEntryOptions);
    }
}