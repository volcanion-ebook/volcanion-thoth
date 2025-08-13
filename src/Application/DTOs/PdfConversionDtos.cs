using Domain.ValueObjects;
using Microsoft.AspNetCore.Http;

namespace Application.DTOs;

public class PdfUploadDto
{
    public IFormFile PdfFile { get; set; } = null!;
    public string? Title { get; set; }
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Epub;
    public ConversionOptionsDto? Options { get; set; }
}

public class ConversionOptionsDto
{
    public int DPI { get; set; } = 150;
    public string ImageFormat { get; set; } = "PNG";
    public bool OptimizeImages { get; set; } = true;
    public int CompressionLevel { get; set; } = 6;
}

public class ConversionResponseDto
{
    public bool Success { get; set; }
    public string? DownloadUrl { get; set; }
    public string? FileName { get; set; }
    public string? ErrorMessage { get; set; }
    public int TotalPages { get; set; }
    public string Format { get; set; } = string.Empty;
}
