using FluentValidation;
using HammadLatifLab.ChatBot.Application.DTOs;
using HammadLatifLab.ChatBot.Application.Interfaces;
using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace HammadLatifLab.ChatBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[EnableRateLimiting("chat_limit")]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IValidator<ChatRequest> _validator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        IValidator<ChatRequest> validator,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _validator = validator;
        _logger = logger;
    }

    /// <summary>
    /// Processes a user query and returns a strictly grounded response based on repository FAQs.
    /// </summary>
    [HttpPost("message")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> PostMessage([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new ValidationProblemDetails(validationResult.ToDictionary()));
        }

        var response = await _chatService.ProcessMessageAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Retrieves recent chat audit logs from the chat repository.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(IReadOnlyList<ChatLogEntry>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory([FromServices] IChatLogRepository chatLogRepo, [FromQuery] int count = 20, CancellationToken ct = default)
    {
        var logs = await chatLogRepo.GetRecentAsync(count, ct);
        return Ok(logs);
    }
}
