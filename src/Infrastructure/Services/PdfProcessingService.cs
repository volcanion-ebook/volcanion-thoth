using Domain.Services;
using Domain.ValueObjects;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace Infrastructure.Services;

public class PdfProcessingService : IPdfProcessingService
{
    private readonly IEbookGeneratorService _ebookGeneratorService;
    private readonly ILogger<PdfProcessingService> _logger;

    public PdfProcessingService(IEbookGeneratorService ebookGeneratorService, ILogger<PdfProcessingService> logger)
    {
        _ebookGeneratorService = ebookGeneratorService;
        _logger = logger;
    }

    public async Task<ConversionResult> ConvertPdfToEbookAsync(ConversionRequest request)
    {
        try
        {
            _logger.LogInformation("Starting PDF conversion for file: {FileName}", request.FileName);

            // Extract pages as images
            var pageImages = await ExtractPagesAsImagesAsync(request.PdfContent, request.Options);
            
            if (pageImages.Count == 0)
            {
                return ConversionResult.CreateFailure("No pages found in PDF file");
            }

            var title = Path.GetFileNameWithoutExtension(request.FileName);
            byte[] outputContent;
            string outputFileName;

            // Generate ebook based on format
            switch (request.OutputFormat)
            {
                case OutputFormat.Epub:
                    outputContent = await _ebookGeneratorService.GenerateEpubAsync(title, pageImages, request.Options);
                    outputFileName = $"{title}.epub";
                    break;
                
                case OutputFormat.Cbz:
                    outputContent = await _ebookGeneratorService.GenerateCbzAsync(title, pageImages, request.Options);
                    outputFileName = $"{title}.cbz";
                    break;
                
                default:
                    return ConversionResult.CreateFailure($"Unsupported output format: {request.OutputFormat}");
            }

            _logger.LogInformation("Successfully converted PDF to {Format} with {PageCount} pages", 
                request.OutputFormat, pageImages.Count);

            return ConversionResult.CreateSuccess(outputContent, outputFileName, request.OutputFormat, pageImages.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting PDF to ebook");
            return ConversionResult.CreateFailure($"Conversion failed: {ex.Message}");
        }
    }

    public async Task<List<byte[]>> ExtractPagesAsImagesAsync(byte[] pdfContent, ConversionOptions options)
    {
        var pageImages = new List<byte[]>();

        try
        {
            using var memoryStream = new MemoryStream(pdfContent);
            using var pdfReader = new PdfReader(memoryStream);
            using var pdfDocument = new PdfDocument(pdfReader);

            int numberOfPages = pdfDocument.GetNumberOfPages();
            _logger.LogInformation("Extracting {PageCount} pages from PDF", numberOfPages);

            for (int pageNumber = 1; pageNumber <= numberOfPages; pageNumber++)
            {
                try
                {
                    var pageImage = await ExtractPageAsImageAsync(pdfDocument, pageNumber, options);
                    if (pageImage != null)
                    {
                        pageImages.Add(pageImage);
                        _logger.LogDebug("Extracted page {PageNumber} as image", pageNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to extract page {PageNumber}", pageNumber);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting pages from PDF");
            throw;
        }

        return pageImages;
    }

    private async Task<byte[]?> ExtractPageAsImageAsync(PdfDocument pdfDocument, int pageNumber, ConversionOptions options)
    {
        // Note: iText7 community version doesn't have direct PDF to image conversion
        // This is a simplified implementation that extracts text and creates a basic image
        // For production use, consider using libraries like PDFtoPrint or Ghostscript

        try
        {
            var page = pdfDocument.GetPage(pageNumber);
            var pageSize = page.GetPageSize();
            
            // Create a bitmap with the specified DPI
            var width = (int)(pageSize.GetWidth() * options.DPI / 72);
            var height = (int)(pageSize.GetHeight() * options.DPI / 72);
            
            using var bitmap = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(bitmap);
            
            // Fill with white background
            graphics.Clear(Color.White);
            
            // Extract text content for basic rendering
            var strategy = new SimpleTextExtractionStrategy();
            var text = PdfTextExtractor.GetTextFromPage(page, strategy);
            
            // Basic text rendering (this is a simplified approach)
            if (!string.IsNullOrEmpty(text))
            {
                using var font = new Font("Arial", 12);
                using var brush = new SolidBrush(Color.Black);
                
                var rect = new RectangleF(10, 10, width - 20, height - 20);
                graphics.DrawString(text, font, brush, rect);
            }
            
            // Convert to byte array
            using var stream = new MemoryStream();
            var format = options.ImageFormat.ToUpper() switch
            {
                "PNG" => ImageFormat.Png,
                "JPEG" => ImageFormat.Jpeg,
                "JPG" => ImageFormat.Jpeg,
                _ => ImageFormat.Png
            };
            
            bitmap.Save(stream, format);
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting page {PageNumber} as image", pageNumber);
            return null;
        }
    }
}
