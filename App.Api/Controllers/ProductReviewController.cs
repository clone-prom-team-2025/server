using System.Security.Claims;
using App.Core.Constants;
using App.Core.DTOs.Product.Review;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class ProductReviewController : ControllerBase
{
    private readonly ILogger<ProductReviewController> _logger;
    private readonly IProductReviewService _productReviewService;

    public ProductReviewController(IProductReviewService productReviewService, ILogger<ProductReviewController> logger)
    {
        _productReviewService = productReviewService;
        _logger = logger;
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddCommentByMyId(string productId, int rating, string comment)
    {
        try
        {
            using (_logger.BeginScope("AddCommentByMyId action"))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogError("UserId claim is missing");
                    return BadRequest();
                }

                _logger.LogInformation("AddCommentByMyId called with UserId={UserId}, Rating={rating}, Comment={comment}",
                    userIdClaim, rating, comment);

                await _productReviewService.AddCommentToReviewByProductId(productId,
                    new ProductReviewCommentCreateDto(rating, userIdClaim, comment));

                _logger.LogInformation("AddCommentByMyId completed for UserId={UserId}", userIdClaim);
                return Ok();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<IActionResult> AddComment(string userId, string productId, int rating, string comment)
    {
        try
        {
            using (_logger.BeginScope("AddComment action"))
            {
                _logger.LogInformation("AddComment called with UserId={UserId}, Rating={rating}, Comment={comment}", userId,
                    rating, comment);

                await _productReviewService.AddCommentToReviewByProductId(productId,
                    new ProductReviewCommentCreateDto(rating, userId, comment));

                _logger.LogInformation("AddComment completed for UserId={UserId}", userId);
                return Ok();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> RemoveCommentByMyId(string productId)
    {
        try
        {
            using (_logger.BeginScope("RemoveCommentByMyId action"))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogError("UserId claim is missing");
                    return BadRequest();
                }

                _logger.LogInformation("RemoveCommentByMyId called with UserId={UserId}, ProductId={productId}",
                    userIdClaim, productId);

                await _productReviewService.RemoveCommentFromReviewByProductId(productId, userIdClaim);

                _logger.LogInformation("RemoveCommentByMyId completed for UserId={UserId}, ProductId={productId}",
                    userIdClaim, productId);
                return Ok();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> RemoveComment(string productId, string userId)
    {
        try
        {
            using (_logger.BeginScope("RemoveComment action"))
            {
                _logger.LogInformation("RemoveComment called with UserId={UserId}, ProductId={productId}", userId,
                    productId);

                await _productReviewService.RemoveCommentFromReviewByProductId(productId, userId);

                _logger.LogInformation("RemoveComment completed for UserId={UserId}, ProductId={productId}", userId,
                    productId);
                return Ok();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReviews(string productId)
    {
        try
        {
            using (_logger.BeginScope("GetAllReviews action"))
            {
                _logger.LogInformation("GetAllReviews called with ProductId={ProductId}", productId);

                var reviews = await _productReviewService.GetReviewByProductId(productId);
                if (reviews == null)
                {
                    _logger.LogWarning("GetAllReviews returned null for ProductId={ProductId}", productId);
                    return NotFound();
                }

                _logger.LogInformation("GetAllReviews completed for ProductId={ProductId}", productId);
                return Ok(reviews);
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet("{reviewId}")]
    public async Task<IActionResult> GetReview(string reviewId)
    {
        try
        {
            using (_logger.BeginScope("GetReview action"))
            {
                _logger.LogInformation("GetReview called with ReviewId={ReviewId}", reviewId);

                var review = await _productReviewService.GetReviewById(reviewId);
                if (review == null)
                {
                    _logger.LogWarning("GetReview not found for ReviewId={ReviewId}", reviewId);
                    return NotFound();
                }

                _logger.LogInformation("GetReview completed for ReviewId={ReviewId}", reviewId);
                return Ok(review);
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SetReactionByMyId(string productId, string commentUserId, bool reaction)
    {
        try
        {
            using (_logger.BeginScope("SetReaction action"))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogError("UserId claim is missing");
                    return BadRequest();
                }

                _logger.LogInformation(
                    "SetReaction called with ProductId={productId}, CommentUserId={CommentUserId}, ReactionUserId={ReactionUserId}, Reaction={Reaction}",
                    productId, commentUserId, userIdClaim, reaction);

                await _productReviewService.SetReactionToReviewComment(productId, commentUserId, userIdClaim, reaction);

                _logger.LogInformation("SetReaction completed for ProductId={productId}, ReactionUserId={ReactionUserId}",
                    productId, userIdClaim);
                return Ok();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<IActionResult> DeleteReactionByMyId(string productId, string commentUserId)
    {
        try
        {
            using (_logger.BeginScope("DeleteReaction action"))
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    _logger.LogError("UserId claim is missing");
                    return BadRequest();
                }

                _logger.LogInformation(
                    "DeleteReaction called with ProductId={productId}, CommentUserId={CommentUserId}, ReactionUserId={ReactionUserId}",
                    productId, commentUserId, userIdClaim);

                await _productReviewService.DeleteReactionToReviewComment(productId, commentUserId, userIdClaim);


                _logger.LogInformation(
                    "DeleteReaction completed for ProductId={productId}, ReactionUserId={ReactionUserId}", productId,
                    userIdClaim);
                return Ok();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllComments(string productId)
    {
        try
        {
            using (_logger.BeginScope("GetAllComments action"))
            {
                _logger.LogInformation("GetAllComments called with ProductId={ProductId}", productId);

                var comments = await _productReviewService.GetAllCommentsByProductId(productId);
                if (comments == null || !comments.Any())
                {
                    _logger.LogWarning("GetAllComments returned empty for ProductId={ProductId}", productId);
                    return NotFound();
                }

                _logger.LogInformation("GetAllComments completed for ProductId={ProductId}", productId);
                return Ok(comments);
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> ClearReviews(string productId)
    {
        try
        {
            using (_logger.BeginScope("ClearReviews action"))
            {
                _logger.LogInformation("ClearReviews called with ProductId={ProductId}", productId);

                await _productReviewService.ClearAllReviewsByProductId(productId);

                _logger.LogInformation("ClearReviews completed for ProductId={ProductId}", productId);
                return Ok();
            }
        }
        catch (Exception e)
        {
            return BadRequest(e.Message);
        }
    }
}