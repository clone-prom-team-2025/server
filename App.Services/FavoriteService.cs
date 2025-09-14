using App.Core.Constants;
using App.Core.DTOs.Favorite;
using App.Core.Exceptions;
using App.Core.Interfaces;
using App.Core.Models.Favorite;
using AutoMapper;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;

namespace App.Services;

public class FavoriteService(
    IStoreRepository storeRepository,
    IFavoriteProductRepository favoriteProductRepository,
    ILogger<FavoriteService> logger,
    IMapper mapper,
    IUserRepository userRepository,
    IProductRepository productRepository,
    IFavoriteSellerRepository favoriteSellerRepository) : IFavoriteService
{
    private readonly IFavoriteProductRepository _favoriteProductRepository = favoriteProductRepository;
    private readonly IFavoriteSellerRepository _favoriteSellerRepository = favoriteSellerRepository;
    private readonly ILogger<FavoriteService> _logger = logger;
    private readonly IMapper _mapper = mapper;
    private readonly IProductRepository _productRepository = productRepository;
    private readonly IStoreRepository _storeRepository = storeRepository;
    private readonly IUserRepository _userRepository = userRepository;

    public async Task CreateFavoriteProductCollection(FavoriteProductCreateDto dto)
    {
        using (_logger.BeginScope("CreateFavoriteProductCollection"))
        {
            _logger.LogInformation("CreateFavoriteProductCollection called");
            await _favoriteProductRepository.CreateAsync(_mapper.Map<FavoriteProduct>(dto));
            _logger.LogInformation("CreateFavoriteProductCollection successfully");
        }
    }

    public async Task<IEnumerable<FavoriteProductDto>?> GetFavoriteProductAllByUserIdAsync(string userId)
    {
        using (_logger.BeginScope("GetFavoriteProductAllByUserIdAsync"))
        {
            _logger.LogInformation("GetFavoriteProductAllByUserIdAsync called");
            return _mapper.Map<IEnumerable<FavoriteProductDto>?>(
                await _favoriteProductRepository.GetAllByUserIdAsync(ObjectId.Parse(userId)));
        }
    }

    public async Task UpdateFavoriteProductCollectionName(string id, string name, string userId)
    {
        using (_logger.BeginScope("UpdateFavoriteProductCollectionName: Id={id}, Name={name}, UserId={userId}", id,
                   name, userId))
        {
            _logger.LogInformation(
                "UpdateFavoriteProductCollectionName called with Id={id}, Name={name}, UserId={userId}", id, name,
                userId);
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorite = await _favoriteProductRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite product collection");
                throw new AccessDeniedException("It's not your favorite product collection");
            }

            favorite.Name = name;
            var result = await _favoriteProductRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite product failed");
                throw new InvalidOperationException("Update favorite product failed");
            }

            _logger.LogInformation("Update favorite product successfully");
        }
    }

    public async Task AddToFavoriteProductCollection(string id, string userId, string productId)
    {
        using (_logger.BeginScope("AddToFavoriteProductCollection"))
        {
            _logger.LogInformation("AddToFavoriteProductCollection");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorite = await _favoriteProductRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
            if (product == null)
            {
                _logger.LogInformation("Product not found");
                throw new KeyNotFoundException("Product not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite product collection");
                throw new AccessDeniedException("It's not your favorite product collection");
            }

            if (favorite.Products.Contains(ObjectId.Parse(productId)))
            {
                _logger.LogInformation("Product already in this favorite product collection");
                throw new InvalidOperationException("Product already in this favorite product collection");
            }

            favorite.Products.Add(ObjectId.Parse(productId));

            var result = await _favoriteProductRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite product failed");
                throw new InvalidOperationException("Update favorite product failed");
            }

            _logger.LogInformation("Update favorite product successfully");
        }
    }

    public async Task AddToFavoriteProductCollectionByName(string name, string userId, string productId)
    {
        using (_logger.BeginScope("AddToFavoriteProductByNameCollection"))
        {
            _logger.LogInformation("AddToFavoriteProductByNameCollection");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites = await _favoriteProductRepository.GetByNameAsync(name);
            if (favorites == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            var favorite = favorites.FirstOrDefault(fav => fav.UserId.ToString() == userId);
            if (favorite == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
            if (product == null)
            {
                _logger.LogInformation("Product not found");
                throw new KeyNotFoundException("Product not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite product collection");
                throw new AccessDeniedException("It's not your favorite product collection");
            }

            if (favorite.Products.Contains(ObjectId.Parse(productId)))
            {
                _logger.LogInformation("Product already in this favorite product collection");
                throw new InvalidOperationException("Product already in this favorite product collection");
            }

            favorite.Products.Add(ObjectId.Parse(productId));

            var result = await _favoriteProductRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite product failed");
                throw new InvalidOperationException("Update favorite product failed");
            }

            _logger.LogInformation("Update favorite product successfully");
        }
    }

    public async Task AddToFavoriteProductCollectionToDefault(string userId, string productId)
    {
        using (_logger.BeginScope("AddToFavoriteProductCollectionToDefault"))
        {
            _logger.LogInformation("AddToFavoriteProductCollectionToDefault");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites =
                await _favoriteProductRepository.GetByNameAsync(DefaultFavoriteNames.DefaultProductCollectionName);
            if (favorites == null || favorites.FirstOrDefault(fav => fav.UserId.ToString() == userId) == null)
                await _favoriteProductRepository.CreateAsync(new FavoriteProduct(ObjectId.Parse(userId),
                    DefaultFavoriteNames.DefaultProductCollectionName));
            favorites = await _favoriteProductRepository.GetByNameAsync(DefaultFavoriteNames
                .DefaultProductCollectionName);
            if (favorites == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            var favorite = favorites.FirstOrDefault(fav => fav.UserId.ToString() == userId);
            if (favorite == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
            if (product == null)
            {
                _logger.LogInformation("Product not found");
                throw new KeyNotFoundException("Product not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite product collection");
                throw new AccessDeniedException("It's not your favorite product collection");
            }

            if (favorite.Products.Contains(ObjectId.Parse(productId)))
            {
                _logger.LogInformation("Product already in this favorite product collection");
                throw new InvalidOperationException("Product already in this favorite product collection");
            }

            favorite.Products.Add(ObjectId.Parse(productId));

            var result = await _favoriteProductRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite product failed");
                throw new InvalidOperationException("Update favorite product failed");
            }

            _logger.LogInformation("Update favorite product successfully");
        }
    }

    public async Task RemoveFromFavoriteProductCollection(string id, string userId, string productId)
    {
        using (_logger.BeginScope("RemoveFromFavoriteProductCollection"))
        {
            _logger.LogInformation("RemoveFromFavoriteProductCollection");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorite = await _favoriteProductRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            var product = await _productRepository.GetByIdAsync(ObjectId.Parse(productId));
            if (product == null)
            {
                _logger.LogInformation("Product not found");
                throw new KeyNotFoundException("Product not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite product collection");
                throw new AccessDeniedException("It's not your favorite product collection");
            }

            if (!favorite.Products.Contains(ObjectId.Parse(productId)))
            {
                _logger.LogInformation("Product not in this favorite product collection");
                throw new InvalidOperationException("Product not in this favorite product collection");
            }

            favorite.Products.Remove(ObjectId.Parse(productId));

            var result = await _favoriteProductRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Remove favorite product failed");
                throw new InvalidOperationException("Remove favorite product failed");
            }

            _logger.LogInformation("Remove favorite product successfully");
        }
    }

    public async Task CreateEmptyFavoriteProductCollection(string name, string userId)
    {
        using (_logger.BeginScope("CreateEmptyFavoriteProductCollection"))
        {
            _logger.LogInformation("CreateEmptyFavoriteProductCollection");
            var parsedUserId = ObjectId.Parse(userId);
            var user = await _userRepository.GetUserByIdAsync(parsedUserId);

            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites = await _favoriteProductRepository.GetByNameAsync(name);
            if (favorites != null && favorites.Any(f => f.UserId == parsedUserId))
            {
                _logger.LogInformation("Favorite product collection already exist");
                throw new InvalidOperationException("Favorite product collection already exist");
            }

            await _favoriteProductRepository.CreateAsync(new FavoriteProduct(parsedUserId, name));
            _logger.LogInformation("Create favorite product successfully");
        }
    }

    public async Task CreateDefaultFavoriteProductCollectionIfNotExist(string userId)
    {
        using (_logger.BeginScope("CreateDefaultFavoriteProductCollectionIfNotExist"))
        {
            _logger.LogInformation("CreateDefaultFavoriteProductCollectionIfNotExist");
            var parsedUserId = ObjectId.Parse(userId);
            var user = await _userRepository.GetUserByIdAsync(parsedUserId);

            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites =
                await _favoriteProductRepository.GetByNameAsync(DefaultFavoriteNames.DefaultProductCollectionName);
            if (favorites != null && favorites.Any(f => f.UserId == parsedUserId))
                _logger.LogInformation("Favorite product collection already exist");

            await _favoriteProductRepository.CreateAsync(new FavoriteProduct(parsedUserId,
                DefaultFavoriteNames.DefaultProductCollectionName));
            _logger.LogInformation("Create favorite product collection successfully");
        }
    }

    public async Task DeleteFavoriteProductCollection(string id, string userId)
    {
        using (_logger.BeginScope("DeleteFavoriteProductCollection"))
        {
            _logger.LogInformation("DeleteFavoriteProductCollection");
            var favorite = await _favoriteProductRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite product not found");
                throw new KeyNotFoundException("Favorite product not found");
            }

            if (favorite.Name == DefaultFavoriteNames.DefaultProductCollectionName)
            {
                _logger.LogInformation("You can't delete default collection");
                throw new AccessDeniedException("You can't delete default collection");
            }

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite product collection");
                throw new AccessDeniedException("It's not your favorite product collection");
            }

            var result = await _favoriteProductRepository.DeleteAsync(favorite.Id);
            if (!result)
            {
                _logger.LogInformation("Delete favorite product failed");
                throw new InvalidOperationException("Delete favorite product failed");
            }

            _logger.LogInformation("Delete favorite product successfully");
        }
    }

    public async Task CreateFavoriteSellerCollection(FavoriteSellerCreateDto dto)
    {
        using (_logger.BeginScope("CreateFavoriteSellerCollection"))
        {
            _logger.LogInformation("CreateFavoriteSellerCollection called");
            await _favoriteSellerRepository.CreateAsync(_mapper.Map<FavoriteSeller>(dto));
            _logger.LogInformation("CreateFavoriteSellerCollection successfully");
        }
    }

    public async Task<IEnumerable<FavoriteSellerDto>> GetFavoriteSellerAllByUserIdAsync(string userId)
    {
        using (_logger.BeginScope("GetFavoriteSellerAllByUserIdAsync"))
        {
            _logger.LogInformation("GetFavoriteSellerAllByUserIdAsync");
            await _favoriteSellerRepository.GetAllByUserIdAsync(ObjectId.Parse(userId));
            _logger.LogInformation("GetFavoriteSellerAllByUserIdAsync successfully");
            return _mapper.Map<IEnumerable<FavoriteSellerDto>>(await _favoriteSellerRepository.GetAllAsync());
        }
    }

    public async Task UpdateFavoriteSellerCollectionName(string id, string name, string userId)
    {
        using (_logger.BeginScope("UpdateFavoriteSellerCollectionName: Id={id}, Name={name}, UserId={userId}", id, name,
                   userId))
        {
            _logger.LogInformation(
                "UpdateFavoriteSellerCollectionName called with Id={id}, Name={name}, UserId={userId}", id, name,
                userId);
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorite = await _favoriteSellerRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite seller collection");
                throw new AccessDeniedException("It's not your favorite seller collection");
            }

            favorite.Name = name;
            var result = await _favoriteSellerRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite seller failed");
                throw new InvalidOperationException("Update favorite seller failed");
            }

            _logger.LogInformation("Update favorite seller successfully");
        }
    }

    public async Task AddToFavoriteSellerCollection(string id, string userId, string productId)
    {
        using (_logger.BeginScope("AddToFavoriteSellerCollection"))
        {
            _logger.LogInformation("AddToFavoriteSellerCollection");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorite = await _favoriteSellerRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            var seller = await _storeRepository.GetStoreById(ObjectId.Parse(productId));
            if (seller == null)
            {
                _logger.LogInformation("Seller not found");
                throw new KeyNotFoundException("Seller not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite seller collection");
                throw new AccessDeniedException("It's not your favorite seller collection");
            }

            if (favorite.Sellers.Contains(ObjectId.Parse(productId)))
            {
                _logger.LogInformation("Product already in this favorite seller collection");
                throw new InvalidOperationException("Product already in this favorite seller collection");
            }

            favorite.Sellers.Add(ObjectId.Parse(productId));

            var result = await _favoriteSellerRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite seller failed");
                throw new InvalidOperationException("Update favorite seller failed");
            }

            _logger.LogInformation("Update favorite seller successfully");
        }
    }

    public async Task AddToFavoriteSellerCollectionByName(string name, string userId, string sellerId)
    {
        using (_logger.BeginScope("AddToFavoriteSellerCollectionByName"))
        {
            _logger.LogInformation("AddToFavoriteSellerCollectionByName");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites = await _favoriteSellerRepository.GetByNameAsync(name);
            if (favorites == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            var favorite = favorites.FirstOrDefault(fav => fav.UserId.ToString() == userId);
            if (favorite == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            var seller = await _storeRepository.GetStoreById(ObjectId.Parse(sellerId));
            if (seller == null)
            {
                _logger.LogInformation("Seller not found");
                throw new KeyNotFoundException("Seller not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite seller collection");
                throw new AccessDeniedException("It's not your favorite seller collection");
            }

            if (favorite.Sellers.Contains(ObjectId.Parse(sellerId)))
            {
                _logger.LogInformation("Seller already in this favorite seller collection");
                throw new InvalidOperationException("Seller already in this favorite seller collection");
            }

            favorite.Sellers.Add(ObjectId.Parse(sellerId));

            var result = await _favoriteSellerRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite seller failed");
                throw new InvalidOperationException("Update favorite seller failed");
            }

            _logger.LogInformation("Update favorite seller successfully");
        }
    }

    public async Task AddToFavoriteSellerCollectionToDefault(string userId, string sellerId)
    {
        using (_logger.BeginScope("AddToFavoriteSellerCollectionToDefault"))
        {
            _logger.LogInformation("AddToFavoriteSellerCollectionToDefault");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites =
                await _favoriteSellerRepository.GetByNameAsync(DefaultFavoriteNames.DefaultSellerCollectionName);
            if (favorites == null || favorites.FirstOrDefault(fav => fav.UserId.ToString() == userId) == null)
                await _favoriteSellerRepository.CreateAsync(new FavoriteSeller(ObjectId.Parse(userId),
                    DefaultFavoriteNames.DefaultSellerCollectionName));
            favorites = await _favoriteSellerRepository.GetByNameAsync(DefaultFavoriteNames
                .DefaultSellerCollectionName);
            if (favorites == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            var favorite = favorites.FirstOrDefault(fav => fav.UserId.ToString() == userId);
            if (favorite == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            var seller = await _storeRepository.GetStoreById(ObjectId.Parse(sellerId));
            if (seller == null)
            {
                _logger.LogInformation("Seller not found");
                throw new KeyNotFoundException("Seller not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite seller collection");
                throw new AccessDeniedException("It's not your favorite seller collection");
            }

            if (favorite.Sellers.Contains(ObjectId.Parse(sellerId)))
            {
                _logger.LogInformation("Seller already in this favorite seller collection");
                throw new InvalidOperationException("Seller already in this favorite seller collection");
            }

            favorite.Sellers.Add(ObjectId.Parse(sellerId));

            var result = await _favoriteSellerRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Update favorite seller failed");
                throw new InvalidOperationException("Update favorite seller failed");
            }

            _logger.LogInformation("Update favorite seller successfully");
        }
    }

    public async Task RemoveFromFavoriteSellerCollection(string id, string userId, string productId)
    {
        using (_logger.BeginScope("RemoveFromFavoriteSellerCollection"))
        {
            _logger.LogInformation("RemoveFromFavoriteSellerCollection");
            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorite = await _favoriteSellerRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            var store = await _storeRepository.GetStoreById(ObjectId.Parse(productId));
            if (store == null)
            {
                _logger.LogInformation("Seller not found");
                throw new KeyNotFoundException("Seller not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite seller collection");
                throw new AccessDeniedException("It's not your favorite seller collection");
            }

            if (!favorite.Sellers.Contains(ObjectId.Parse(productId)))
            {
                _logger.LogInformation("Product not in this favorite seller collection");
                throw new InvalidOperationException("Product not in this favorite seller collection");
            }

            favorite.Sellers.Remove(ObjectId.Parse(productId));

            var result = await _favoriteSellerRepository.UpdateAsync(favorite);
            if (!result)
            {
                _logger.LogInformation("Remove favorite seller failed");
                throw new InvalidOperationException("Remove favorite seller failed");
            }

            _logger.LogInformation("Remove favorite seller successfully");
        }
    }

    public async Task CreateEmptyFavoriteSellerCollection(string name, string userId)
    {
        using (_logger.BeginScope("CreateEmptyFavoriteSellerCollection"))
        {
            _logger.LogInformation("CreateEmptyFavoriteSellerCollection");
            var parsedUserId = ObjectId.Parse(userId);
            var user = await _userRepository.GetUserByIdAsync(parsedUserId);

            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites = await _favoriteSellerRepository.GetByNameAsync(name);
            if (favorites != null && favorites.Any(f => f.UserId == parsedUserId))
            {
                _logger.LogInformation("Favorite seller collection already exist");
                throw new InvalidOperationException("Favorite seller collection already exist");
            }

            await _favoriteSellerRepository.CreateAsync(new FavoriteSeller(parsedUserId, name));
            _logger.LogInformation("Create favorite seller successfully");
        }
    }

    public async Task CreateDefaultFavoriteSellerCollectionIfNotExist(string userId)
    {
        using (_logger.BeginScope("CreateDefaultFavoriteSellerCollectionIfNotExist"))
        {
            _logger.LogInformation("CreateDefaultFavoriteSellerCollectionIfNotExist");
            var parsedUserId = ObjectId.Parse(userId);
            var user = await _userRepository.GetUserByIdAsync(parsedUserId);

            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            var favorites =
                await _favoriteSellerRepository.GetByNameAsync(DefaultFavoriteNames.DefaultSellerCollectionName);
            if (favorites != null && favorites.Any(f => f.UserId == parsedUserId))
                _logger.LogInformation("Favorite seller collection already exist");

            await _favoriteSellerRepository.CreateAsync(new FavoriteSeller(parsedUserId,
                DefaultFavoriteNames.DefaultSellerCollectionName));
            _logger.LogInformation("Create favorite seller collection successfully");
        }
    }

    public async Task DeleteFavoriteSellerCollection(string id, string userId)
    {
        using (_logger.BeginScope("DeleteFavoriteSellerCollection"))
        {
            _logger.LogInformation("DeleteFavoriteSellerCollection");
            var favorite = await _favoriteSellerRepository.GetAsync(ObjectId.Parse(id));
            if (favorite == null)
            {
                _logger.LogInformation("Favorite seller not found");
                throw new KeyNotFoundException("Favorite seller not found");
            }

            if (favorite.Name == DefaultFavoriteNames.DefaultSellerCollectionName)
            {
                _logger.LogInformation("You can't delete default collection");
                throw new AccessDeniedException("You can't delete default collection");
            }

            var user = await _userRepository.GetUserByIdAsync(ObjectId.Parse(userId));
            if (user == null)
            {
                _logger.LogInformation("User not found");
                throw new KeyNotFoundException("User not found");
            }

            if (favorite.UserId.ToString() != userId)
            {
                _logger.LogInformation("It's not your favorite seller collection");
                throw new AccessDeniedException("It's not your favorite seller collection");
            }

            var result = await _favoriteSellerRepository.DeleteAsync(favorite.Id);
            if (!result)
            {
                _logger.LogInformation("Delete favorite seller failed");
                throw new InvalidOperationException("Delete favorite seller failed");
            }

            _logger.LogInformation("Delete favorite seller successfully");
        }
    }

    public async Task CreateFavoriteSellerCollection(FavoriteProductCreateDto dto)
    {
        using (_logger.BeginScope("CreateFavoriteSellerCollection"))
        {
            _logger.LogInformation("CreateFavoriteSellerCollection called");
            await _favoriteSellerRepository.CreateAsync(_mapper.Map<FavoriteSeller>(dto));
            _logger.LogInformation("CreateFavoriteSellerCollection successfully");
        }
    }
}