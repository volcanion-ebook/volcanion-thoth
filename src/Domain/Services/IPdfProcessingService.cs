using Domain.ValueObjects;

namespace Domain.Services;

public interface IPdfProcessingService
{
    Task<ConversionResult> ConvertPdfToEbookAsync(ConversionRequest request);
    Task<List<byte[]>> ExtractPagesAsImagesAsync(byte[] pdfContent, ConversionOptions options);
}
