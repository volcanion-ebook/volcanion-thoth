using Domain.ValueObjects;

namespace Domain.Services;

public interface IEbookGeneratorService
{
    Task<byte[]> GenerateEpubAsync(string title, List<byte[]> pageImages, ConversionOptions options);
    Task<byte[]> GenerateCbzAsync(string title, List<byte[]> pageImages, ConversionOptions options);
}
