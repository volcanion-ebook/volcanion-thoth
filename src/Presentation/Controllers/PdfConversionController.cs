using Application.DTOs;
using Application.Interfaces;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfConversionController : ControllerBase
{
    private readonly IPdfConversionService _pdfConversionService;
    private readonly ILogger<PdfConversionController> _logger;

    public PdfConversionController(
        IPdfConversionService pdfConversionService,
        ILogger<PdfConversionController> logger)
    {
        _pdfConversionService = pdfConversionService;
        _logger = logger;
    }

    /// <summary>
    /// Converts a PDF file to EPUB or CBZ format
    /// </summary>
    /// <param name="request">The PDF upload request</param>
    /// <returns>The conversion result with download information</returns>
    [HttpPost("convert")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<ConversionResponseDto>> ConvertPdf([FromForm] PdfUploadDto request)
    {
        try
        {
            if (request.PdfFile == null || request.PdfFile.Length == 0)
            {
                return BadRequest(new ConversionResponseDto
                {
                    Success = false,
                    ErrorMessage = "Please provide a valid PDF file"
                });
            }

            // Validate file type
            if (!request.PdfFile.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase) &&
                !request.PdfFile.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new ConversionResponseDto
                {
                    Success = false,
                    ErrorMessage = "Only PDF files are supported"
                });
            }

            // Check file size (limit to 50MB)
            const long maxFileSize = 50 * 1024 * 1024;
            if (request.PdfFile.Length > maxFileSize)
            {
                return BadRequest(new ConversionResponseDto
                {
                    Success = false,
                    ErrorMessage = "File size exceeds maximum limit of 50MB"
                });
            }

            var result = await _pdfConversionService.ConvertPdfAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in PDF conversion endpoint");
            return StatusCode(500, new ConversionResponseDto
            {
                Success = false,
                ErrorMessage = "An unexpected error occurred. Please try again later."
            });
        }
    }

    /// <summary>
    /// Downloads the converted file
    /// </summary>
    /// <param name="fileName">The name of the file to download</param>
    /// <returns>The file content</returns>
    [HttpGet("download/{fileName}")]
    public IActionResult DownloadFile(string fileName)
    {
        try
        {
            // In a real application, you would store converted files and retrieve them
            // For this example, we'll return a not found response
            // You should implement file storage (disk, cloud storage, etc.)
            
            return NotFound(new { message = "File download functionality not implemented in this demo" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file: {FileName}", fileName);
            return StatusCode(500, new { message = "Error downloading file" });
        }
    }

    /// <summary>
    /// Gets supported output formats
    /// </summary>
    /// <returns>List of supported formats</returns>
    [HttpGet("formats")]
    public ActionResult<object> GetSupportedFormats()
    {
        var formats = Enum.GetValues<OutputFormat>()
            .Select(f => new { name = f.ToString(), value = (int)f })
            .ToList();

        return Ok(new { formats });
    }

    /// <summary>
    /// Gets API health status
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet("health")]
    public ActionResult<object> GetHealth()
    {
        return Ok(new 
        { 
            status = "healthy", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}
