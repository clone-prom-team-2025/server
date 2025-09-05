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
    public async Task<ActionResult> AddCommentByMyId(string productId, int rating, string comment)
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

            var result = await _productReviewService.AddCommentToReviewByProductId(productId,
                new ProductReviewCommentCreateDto(rating, userIdClaim, comment));
            if (!result)
            {
                _logger.LogWarning("AddCommentByMyId failed for UserId={UserId}", userIdClaim);
                return BadRequest();
            }

            _logger.LogInformation("AddCommentByMyId completed for UserId={UserId}", userIdClaim);
            return Ok();
        }
    }

    [Authorize(Roles = RoleNames.Admin)]
    [HttpPost]
    public async Task<ActionResult> AddComment(string userId, string productId, int rating, string comment)
    {
        using (_logger.BeginScope("AddComment action"))
        {
            _logger.LogInformation("AddComment called with UserId={UserId}, Rating={rating}, Comment={comment}", userId,
                rating, comment);

            var result = await _productReviewService.AddCommentToReviewByProductId(productId,
                new ProductReviewCommentCreateDto(rating, userId, comment));
            if (!result)
            {
                _logger.LogWarning("AddComment failed for UserId={UserId}", userId);
                return BadRequest();
            }

            _logger.LogInformation("AddComment completed for UserId={UserId}", userId);
            return Ok();
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> RemoveCommentByMyId(string productId)
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

            var result = await _productReviewService.RemoveCommentFromReviewByProductId(productId, userIdClaim);
            if (!result)
            {
                _logger.LogWarning("RemoveCommentByMyId failed for UserId={UserId}, ProductId={productId}", userIdClaim,
                    productId);
                return NotFound();
            }

            _logger.LogInformation("RemoveCommentByMyId completed for UserId={UserId}, ProductId={productId}",
                userIdClaim, productId);
            return Ok();
        }
    }

    [HttpDelete]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> RemoveComment(string productId, string userId)
    {
        using (_logger.BeginScope("RemoveComment action"))
        {
            _logger.LogInformation("RemoveComment called with UserId={UserId}, ProductId={productId}", userId,
                productId);

            var result = await _productReviewService.RemoveCommentFromReviewByProductId(productId, userId);
            if (!result)
            {
                _logger.LogWarning("RemoveComment failed for UserId={UserId}, ProductId={productId}", userId,
                    productId);
                return NotFound();
            }

            _logger.LogInformation("RemoveComment completed for UserId={UserId}, ProductId={productId}", userId,
                productId);
            return Ok();
        }
    }

    [HttpGet]
    public async Task<ActionResult<ProductReviewDto>> GetAllReviews(string productId)
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

    [HttpGet("{reviewId}")]
    public async Task<ActionResult<ProductReviewDto>> GetReview(string reviewId)
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

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> SetReactionByMyId(string productId, string commentUserId, bool reaction)
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

            var result =
                await _productReviewService.SetReactionToReviewComment(productId, commentUserId, userIdClaim, reaction);
            if (!result)
            {
                _logger.LogWarning("SetReaction failed for ProductId={productId}, ReactionUserId={ReactionUserId}",
                    productId, userIdClaim);
                return BadRequest();
            }

            _logger.LogInformation("SetReaction completed for ProductId={productId}, ReactionUserId={ReactionUserId}",
                productId, userIdClaim);
            return Ok();
        }
    }

    [HttpDelete]
    [Authorize]
    public async Task<ActionResult> DeleteReactionByMyId(string productId, string commentUserId)
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

            var result =
                await _productReviewService.DeleteReactionToReviewComment(productId, commentUserId, userIdClaim);
            if (!result)
            {
                _logger.LogWarning("DeleteReaction failed for ProductId={productId}, ReactionUserId={ReactionUserId}",
                    productId, userIdClaim);
                return BadRequest();
            }

            _logger.LogInformation(
                "DeleteReaction completed for ProductId={productId}, ReactionUserId={ReactionUserId}", productId,
                userIdClaim);
            return Ok();
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductReviewCommentDto>>> GetAllComments(string productId)
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

    [HttpPost]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult> ClearReviews(string productId)
    {
        using (_logger.BeginScope("ClearReviews action"))
        {
            _logger.LogInformation("ClearReviews called with ProductId={ProductId}", productId);

            var result = await _productReviewService.ClearAllReviewsByProductId(productId);
            if (!result)
            {
                _logger.LogWarning("ClearReviews failed for ProductId={ProductId}", productId);
                return BadRequest();
            }

            _logger.LogInformation("ClearReviews completed for ProductId={ProductId}", productId);
            return Ok();
        }
    }
}