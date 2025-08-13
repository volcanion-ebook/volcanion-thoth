namespace Domain.ValueObjects;

public class ConversionRequest
{
    public string FileName { get; }
    public byte[] PdfContent { get; }
    public OutputFormat OutputFormat { get; }
    public ConversionOptions Options { get; }

    public ConversionRequest(string fileName, byte[] pdfContent, OutputFormat outputFormat, ConversionOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        
        if (pdfContent == null || pdfContent.Length == 0)
            throw new ArgumentException("PDF content cannot be empty", nameof(pdfContent));

        FileName = fileName;
        PdfContent = pdfContent;
        OutputFormat = outputFormat;
        Options = options ?? new ConversionOptions();
    }
}

public enum OutputFormat
{
    Epub,
    Cbz
}

public class ConversionOptions
{
    public int DPI { get; set; } = 150;
    public string ImageFormat { get; set; } = "PNG";
    public bool OptimizeImages { get; set; } = true;
    public int CompressionLevel { get; set; } = 6;
}
