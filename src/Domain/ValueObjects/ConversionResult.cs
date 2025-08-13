namespace Domain.ValueObjects;

public class ConversionResult
{
    public bool Success { get; }
    public byte[]? OutputContent { get; }
    public string OutputFileName { get; }
    public OutputFormat Format { get; }
    public string? ErrorMessage { get; }
    public int TotalPages { get; }

    private ConversionResult(bool success, byte[]? outputContent, string outputFileName, OutputFormat format, string? errorMessage, int totalPages)
    {
        Success = success;
        OutputContent = outputContent;
        OutputFileName = outputFileName;
        Format = format;
        ErrorMessage = errorMessage;
        TotalPages = totalPages;
    }

    public static ConversionResult CreateSuccess(byte[] outputContent, string outputFileName, OutputFormat format, int totalPages)
    {
        return new ConversionResult(true, outputContent, outputFileName, format, null, totalPages);
    }

    public static ConversionResult CreateFailure(string errorMessage)
    {
        return new ConversionResult(false, null, string.Empty, OutputFormat.Epub, errorMessage, 0);
    }
}
