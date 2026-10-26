using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HammadLatifLab.ChatBot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TestsController : ControllerBase
{
    private readonly ITestRepository _testRepository;
    private readonly ILogger<TestsController> _logger;

    public TestsController(ITestRepository testRepository, ILogger<TestsController> logger)
    {
        _testRepository = testRepository;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all verified diagnostic laboratory tests and prices.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TestItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var tests = await _testRepository.GetAllAsync(ct);
        return Ok(tests);
    }

    /// <summary>
    /// Retrieves a specific test by its unique ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TestItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var test = await _testRepository.GetByIdAsync(id, ct);
        return test is null ? NotFound(new { message = $"Test with ID {id} not found." }) : Ok(test);
    }

    /// <summary>
    /// Searches laboratory tests by name, keywords, or Roman Urdu queries.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(IReadOnlyList<TestItem>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search([FromQuery(Name = "q")] string? q, [FromQuery] int top = 5, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(Array.Empty<TestItem>());
        }

        var results = await _testRepository.SearchAsync(q, top, ct);
        return Ok(results);
    }
}
