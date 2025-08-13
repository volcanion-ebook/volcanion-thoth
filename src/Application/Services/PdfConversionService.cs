using Application.DTOs;
using Application.Interfaces;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Application.Services;

public class PdfConversionService : IPdfConversionService
{
    private readonly IPdfProcessingService _pdfProcessingService;
    private readonly ILogger<PdfConversionService> _logger;

    public PdfConversionService(IPdfProcessingService pdfProcessingService, ILogger<PdfConversionService> logger)
    {
        _pdfProcessingService = pdfProcessingService;
        _logger = logger;
    }

    public async Task<ConversionResponseDto> ConvertPdfAsync(PdfUploadDto uploadDto)
    {
        try
        {
            _logger.LogInformation("Starting PDF conversion for file: {FileName}", uploadDto.PdfFile.FileName);

            // Read PDF content
            using var memoryStream = new MemoryStream();
            await uploadDto.PdfFile.CopyToAsync(memoryStream);
            var pdfContent = memoryStream.ToArray();

            // Map DTOs to domain objects
            var options = MapToConversionOptions(uploadDto.Options);
            var title = uploadDto.Title ?? Path.GetFileNameWithoutExtension(uploadDto.PdfFile.FileName);
            
            var conversionRequest = new ConversionRequest(
                uploadDto.PdfFile.FileName,
                pdfContent,
                uploadDto.OutputFormat,
                options
            );

            // Process conversion
            var result = await _pdfProcessingService.ConvertPdfToEbookAsync(conversionRequest);

            // Map result to response DTO
            return MapToResponseDto(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during PDF conversion");
            return new ConversionResponseDto
            {
                Success = false,
                ErrorMessage = "An error occurred during conversion. Please try again."
            };
        }
    }

    public List<string> GetSupportedFormats()
    {
        return new List<string> { "EPUB", "CBZ" };
    }

    private static ConversionOptions MapToConversionOptions(ConversionOptionsDto? optionsDto)
    {
        if (optionsDto == null)
            return new ConversionOptions();

        return new ConversionOptions
        {
            DPI = optionsDto.DPI,
            ImageFormat = optionsDto.ImageFormat,
            OptimizeImages = optionsDto.OptimizeImages,
            CompressionLevel = optionsDto.CompressionLevel
        };
    }

    private static ConversionResponseDto MapToResponseDto(ConversionResult? result)
    {
        if (result == null)
        {
            return new ConversionResponseDto
            {
                Success = false,
                ErrorMessage = "Conversion failed"
            };
        }

        return new ConversionResponseDto
        {
            Success = result.Success,
            FileName = result.OutputFileName,
            ErrorMessage = result.ErrorMessage,
            TotalPages = result.TotalPages,
            Format = result.Format.ToString()
        };
    }
}
