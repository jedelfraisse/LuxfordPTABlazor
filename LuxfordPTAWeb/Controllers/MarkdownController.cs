using Markdig;
using Microsoft.AspNetCore.Mvc;

namespace LuxfordPTAWeb.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarkdownController : ControllerBase
{
    private readonly ILogger<MarkdownController> _logger;

    public MarkdownController(ILogger<MarkdownController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Convert markdown text to HTML
    /// </summary>
    [HttpPost("to-html")]
    public ActionResult<string> ToHtml([FromBody] MarkdownRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request?.Markdown))
            {
                return BadRequest(new { error = "Markdown content is required" });
            }

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var html = Markdown.ToHtml(request.Markdown, pipeline);

            return Ok(new { html });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting markdown to HTML");
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Error processing markdown" });
        }
    }
}

public class MarkdownRequest
{
    public string? Markdown { get; set; }
}
