using App.Core.DTOs.Product.Review;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace App.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductReviewController : ControllerBase
{
    private readonly IProductReviewService _productReviewService;

    public ProductReviewController(IProductReviewService productReviewService)
    {
        _productReviewService = productReviewService;
    }

    [HttpPost("create")]
    public async Task<IActionResult> CreateReview([FromBody] ProductReviewCreateDto review)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _productReviewService.CreateReviewAsync(review);
        return Ok();
    }

    [HttpPost("{reviewId}/comment")]
    public async Task<IActionResult> AddComment(string reviewId, [FromBody] ProductReviewCommentCreateDto comment)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _productReviewService.AddCommentToReviewAsync(reviewId, comment);
        return Ok();
    }

    [HttpPost("{reviewId}/comment/{commentId}/reaction")]
    public async Task<IActionResult> AddReaction(string reviewId, string commentId, [FromBody] ProductReviewCommentReactionDto reaction)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        await _productReviewService.AddReactionToCommentAsync(reviewId, commentId, reaction);
        return Ok();
    }

    [HttpGet("{reviewId}")]
    public async Task<IActionResult> GetReviewById(string reviewId)
    {
        var review = await _productReviewService.GetReviewByIdAsync(reviewId);
        return review is null ? NotFound() : Ok(review);
    }

    [HttpGet("product/{productId}")]
    public async Task<IActionResult> GetReviewByProductId(string productId)
    {
        var review = await _productReviewService.GetReviewByProductIdAsync(productId);
        return review is null ? NotFound() : Ok(review);
    }

    [HttpGet("seller/{sellerId}")]
    public async Task<IActionResult> GetReviewsBySellerId(string sellerId)
    {
        var reviews = await _productReviewService.GetReviewsBySellerIdAsync(sellerId);
        return Ok(reviews ?? []);
    }

    [HttpGet("{reviewId}/comments")]
    public async Task<IActionResult> GetComments(string reviewId)
    {
        var comments = await _productReviewService.GetCommentsByReviewIdAsync(reviewId);
        return Ok(comments ?? []);
    }

    [HttpPut("{reviewId}/comment")]
    public async Task<IActionResult> UpdateComment(string reviewId, [FromBody] ProductReviewCommentDto comment)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _productReviewService.UpdateCommentInReviewAsync(reviewId, comment);
        return result ? Ok() : NotFound();
    }

    [HttpPut("{reviewId}")]
    public async Task<IActionResult> UpdateReview(string reviewId, [FromBody] ProductReviewDto review)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var result = await _productReviewService.UpdateReviewAsync(review);
        return result ? Ok() : NotFound();
    }

    [HttpDelete("{reviewId}")]
    public async Task<IActionResult> DeleteReview(string reviewId)
    {
        var result = await _productReviewService.DeleteReviewAsync(reviewId);
        return result ? Ok() : NotFound();
    }

    [HttpDelete("{reviewId}/comment/{commentId}")]
    public async Task<IActionResult> DeleteComment(string reviewId, string commentId)
    {
        var result = await _productReviewService.DeleteCommentFromReviewAsync(reviewId, commentId);
        return result ? Ok() : NotFound();
    }
}
