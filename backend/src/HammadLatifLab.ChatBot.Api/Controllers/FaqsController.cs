using HammadLatifLab.ChatBot.Application.DTOs;
using HammadLatifLab.ChatBot.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HammadLatifLab.ChatBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class FaqsController : ControllerBase
{
    private readonly IFaqService _faqService;

    public FaqsController(IFaqService faqService)
    {
        _faqService = faqService;
    }

    /// <summary>
    /// Retrieves all verified diagnostic FAQs from the repository.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FaqDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var faqs = await _faqService.GetAllFaqsAsync(ct);
        return Ok(faqs);
    }

    /// <summary>
    /// Retrieves a specific FAQ by its unique ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(FaqDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var faq = await _faqService.GetFaqByIdAsync(id, ct);
        return faq is null ? NotFound(new { message = $"FAQ with ID {id} not found." }) : Ok(faq);
    }

    /// <summary>
    /// Searches the FAQ repository using keywords in English or Roman Urdu.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<FaqDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery] string query, [FromQuery] int top = 5, CancellationToken ct = default)
    {
        var results = await _faqService.SearchFaqsAsync(query, top, ct);
        return Ok(results);
    }
}
