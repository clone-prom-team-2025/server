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
using DinkToPdf;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;

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
    IMongoClient client) : IOrderService
{
    private readonly ILogger<OrderService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IProductMediaRepository _productMediaRepository = productMediaRepository;
    private readonly IFileService _fileService = fileService;
    private readonly INotificationService _notificationService = notificationService;
    private readonly ICartRepository _cartRepository = cartRepository;
    private readonly IEmailService _emailService = emailService;
    private readonly IMongoClient _client = client;
    
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
            throw new KeyNotFoundException("User not found");
        }

        var carts = await _cartRepository.GetByUserIdAsync(parsedUserId);
        if (carts == null || carts.Count == 0)
        {
            _logger.LogWarning("User {UserId} doesn't have any carts", parsedUserId);
            throw new KeyNotFoundException("User not found");
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
                _logger.LogWarning("Product {ProductId} not found for User {user}, Cart {cart}", cart.ProductId, userId, cart.Id.ToString());
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

            //await _redisService.SetObjectAsync($"buy:{buyInfo.Id}", buyInfo, TimeSpan.FromMinutes(30));

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
        
        if (deliveryPayment != DeliveryPayment.AfterPayment) await SendEmailPayCard(user.Email, orders);
        else await SendEmailAfterPayment(user.Email, orderNumber);

        _logger.LogInformation("BuyRegistered successfully completed for user {UserId}", userId);
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
        var assembly = Assembly.GetExecutingAssembly();

        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.SuccessOrderAfterPayment.html");
        using var readerOrderEmail = new StreamReader(streamOrderEmail!);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmailHtml = htmlOrderEmail.Replace("__ORDERID__", orderNumber);
        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [email],
            Subject = "Ваше замовлення зареєстроване",
            HtmlBody = readyOrderEmailHtml
        };
        
        await _emailService.SendEmailAsync(mail);
    }

    private async Task SendEmailPayCard(string email, List<Order> order)
    {
        if (order.Count == 0) return;
        decimal totalPrice = 0;
        foreach (var orderItem in order)
        {
            totalPrice += orderItem.TotalPrice;
        }
        
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
        var assembly = Assembly.GetExecutingAssembly();
        await using var streamOrder = assembly.GetManifestResourceStream("App.Services.EmailTemplates.Order.html");
        using var readerOrder = new StreamReader(streamOrder!);
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
        
        var file = await GenerateOrderPdfAsync(order, readyOrderHtml);
        
        await using var streamOrderEmail = assembly.GetManifestResourceStream("App.Services.EmailTemplates.SuccessOrderWithCard.html");
        using var readerOrderEmail = new StreamReader(streamOrderEmail!);
        var htmlOrderEmail = await readerOrderEmail.ReadToEndAsync();
        var readyOrderEmailHtml = htmlOrderEmail.Replace("__ORDERID__", order.First().OrderNumber);

        var mail = new EmailMessage()
        {
            From = "no-reply@sellpoint.pp.ua",
            To = [email],
            Subject = "Ваше замовлення зареєстроване",
            HtmlBody = readyOrderEmailHtml
        };
        _logger.LogDebug("Order {order}", readyOrderHtml);
        
        await _emailService.SendEmailAsync(mail, file, $"Замовлення{order.First().OrderNumber}.pdf");
    }
    
    private async Task<byte[]> GenerateOrderPdfAsync(IEnumerable<Order> order, string readyOrderHtml)
    {
        if (!order.Any()) return Array.Empty<byte>();

        var orderId = order.First().OrderNumber;

        var pdfDoc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                DocumentTitle = $"Замовлення_{orderId}"
            },
            Objects = {
                new ObjectSettings()
                {
                    HtmlContent = readyOrderHtml,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };

        var converter = new SynchronizedConverter(new PdfTools());
        byte[] pdfBytes = converter.Convert(pdfDoc);

        return pdfBytes;
    }
}