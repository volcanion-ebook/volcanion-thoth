using Domain.Services;
using Domain.ValueObjects;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Xml;

namespace Infrastructure.Services;

public class EbookGeneratorService : IEbookGeneratorService
{
    private readonly ILogger<EbookGeneratorService> _logger;

    public EbookGeneratorService(ILogger<EbookGeneratorService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> GenerateEpubAsync(string title, List<byte[]> pageImages, ConversionOptions options)
    {
        _logger.LogInformation("Generating EPUB for {Title} with {PageCount} pages", title, pageImages.Count);

        using var outputStream = new MemoryStream();
        using var zipStream = new ZipOutputStream(outputStream);
        
        zipStream.SetLevel(options.CompressionLevel);

        try
        {
            // Add mimetype file (must be first and uncompressed)
            await AddMimeTypeAsync(zipStream);

            // Add META-INF/container.xml
            await AddContainerXmlAsync(zipStream);

            // Add EPUB content files
            await AddContentOpfAsync(zipStream, title, pageImages.Count);
            await AddTocNcxAsync(zipStream, title, pageImages.Count);
            
            // Add page images and XHTML files
            for (int i = 0; i < pageImages.Count; i++)
            {
                var pageNumber = i + 1;
                var imageFileName = $"images/page_{pageNumber:D3}.{GetImageExtension(options.ImageFormat)}";
                var xhtmlFileName = $"text/page_{pageNumber:D3}.xhtml";

                // Add image
                await AddFileToZipAsync(zipStream, imageFileName, pageImages[i]);

                // Add XHTML page
                var xhtmlContent = GenerateXhtmlPage(title, pageNumber, imageFileName);
                await AddTextFileToZipAsync(zipStream, xhtmlFileName, xhtmlContent);
            }

            zipStream.Finish();
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating EPUB");
            throw;
        }
    }

    public async Task<byte[]> GenerateCbzAsync(string title, List<byte[]> pageImages, ConversionOptions options)
    {
        _logger.LogInformation("Generating CBZ for {Title} with {PageCount} pages", title, pageImages.Count);

        using var outputStream = new MemoryStream();
        using var zipStream = new ZipOutputStream(outputStream);
        
        zipStream.SetLevel(options.CompressionLevel);

        try
        {
            // CBZ is simply a ZIP file with images
            for (int i = 0; i < pageImages.Count; i++)
            {
                var pageNumber = i + 1;
                var fileName = $"{pageNumber:D3}.{GetImageExtension(options.ImageFormat)}";
                await AddFileToZipAsync(zipStream, fileName, pageImages[i]);
            }

            zipStream.Finish();
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating CBZ");
            throw;
        }
    }

    private async Task AddMimeTypeAsync(ZipOutputStream zipStream)
    {
        var entry = new ZipEntry("mimetype")
        {
            CompressionMethod = CompressionMethod.Stored
        };
        
        zipStream.PutNextEntry(entry);
        var mimeTypeBytes = Encoding.UTF8.GetBytes("application/epub+zip");
        await zipStream.WriteAsync(mimeTypeBytes);
        zipStream.CloseEntry();
    }

    private async Task AddContainerXmlAsync(ZipOutputStream zipStream)
    {
        var containerXml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<container version=""1.0"" xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"">
    <rootfiles>
        <rootfile full-path=""OEBPS/content.opf"" media-type=""application/oebps-package+xml""/>
    </rootfiles>
</container>";

        await AddTextFileToZipAsync(zipStream, "META-INF/container.xml", containerXml);
    }

    private async Task AddContentOpfAsync(ZipOutputStream zipStream, string title, int pageCount)
    {
        var opfContent = GenerateContentOpf(title, pageCount);
        await AddTextFileToZipAsync(zipStream, "OEBPS/content.opf", opfContent);
    }

    private async Task AddTocNcxAsync(ZipOutputStream zipStream, string title, int pageCount)
    {
        var ncxContent = GenerateTocNcx(title, pageCount);
        await AddTextFileToZipAsync(zipStream, "OEBPS/toc.ncx", ncxContent);
    }

    private async Task AddFileToZipAsync(ZipOutputStream zipStream, string fileName, byte[] content)
    {
        var entry = new ZipEntry($"OEBPS/{fileName}");
        zipStream.PutNextEntry(entry);
        await zipStream.WriteAsync(content);
        zipStream.CloseEntry();
    }

    private async Task AddTextFileToZipAsync(ZipOutputStream zipStream, string fileName, string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        await AddFileToZipAsync(zipStream, fileName, bytes);
    }

    private string GenerateContentOpf(string title, int pageCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sb.AppendLine(@"<package xmlns=""http://www.idpf.org/2007/opf"" unique-identifier=""BookId"" version=""2.0"">");
        sb.AppendLine("  <metadata>");
        sb.AppendLine($"    <dc:title xmlns:dc=\"http://purl.org/dc/elements/1.1/\">{title}</dc:title>");
        sb.AppendLine("    <dc:creator xmlns:dc=\"http://purl.org/dc/elements/1.1/\">PDF Processor</dc:creator>");
        sb.AppendLine("    <dc:identifier xmlns:dc=\"http://purl.org/dc/elements/1.1/\" id=\"BookId\">urn:uuid:" + Guid.NewGuid() + "</dc:identifier>");
        sb.AppendLine("    <dc:language xmlns:dc=\"http://purl.org/dc/elements/1.1/\">en</dc:language>");
        sb.AppendLine("  </metadata>");
        sb.AppendLine("  <manifest>");
        sb.AppendLine("    <item id=\"ncx\" href=\"toc.ncx\" media-type=\"application/x-dtbncx+xml\"/>");
        
        for (int i = 1; i <= pageCount; i++)
        {
            sb.AppendLine($"    <item id=\"page{i}\" href=\"text/page_{i:D3}.xhtml\" media-type=\"application/xhtml+xml\"/>");
            sb.AppendLine($"    <item id=\"img{i}\" href=\"images/page_{i:D3}.png\" media-type=\"image/png\"/>");
        }
        
        sb.AppendLine("  </manifest>");
        sb.AppendLine("  <spine toc=\"ncx\">");
        
        for (int i = 1; i <= pageCount; i++)
        {
            sb.AppendLine($"    <itemref idref=\"page{i}\"/>");
        }
        
        sb.AppendLine("  </spine>");
        sb.AppendLine("</package>");
        
        return sb.ToString();
    }

    private string GenerateTocNcx(string title, int pageCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
        sb.AppendLine(@"<ncx xmlns=""http://www.daisy.org/z3986/2005/ncx/"" version=""2005-1"">");
        sb.AppendLine("  <head>");
        sb.AppendLine("    <meta name=\"dtb:uid\" content=\"urn:uuid:" + Guid.NewGuid() + "\"/>");
        sb.AppendLine("    <meta name=\"dtb:depth\" content=\"1\"/>");
        sb.AppendLine("    <meta name=\"dtb:totalPageCount\" content=\"0\"/>");
        sb.AppendLine("    <meta name=\"dtb:maxPageNumber\" content=\"0\"/>");
        sb.AppendLine("  </head>");
        sb.AppendLine($"  <docTitle><text>{title}</text></docTitle>");
        sb.AppendLine("  <navMap>");
        
        for (int i = 1; i <= pageCount; i++)
        {
            sb.AppendLine($"    <navPoint id=\"navpoint-{i}\" playOrder=\"{i}\">");
            sb.AppendLine($"      <navLabel><text>Page {i}</text></navLabel>");
            sb.AppendLine($"      <content src=\"text/page_{i:D3}.xhtml\"/>");
            sb.AppendLine("    </navPoint>");
        }
        
        sb.AppendLine("  </navMap>");
        sb.AppendLine("</ncx>");
        
        return sb.ToString();
    }

    private string GenerateXhtmlPage(string title, int pageNumber, string imageFileName)
    {
        return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.1//EN"" ""http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"">
<head>
    <title>{title} - Page {pageNumber}</title>
    <style type=""text/css"">
        body {{ margin: 0; padding: 0; text-align: center; }}
        img {{ max-width: 100%; max-height: 100%; }}
    </style>
</head>
<body>
    <div>
        <img src=""../{imageFileName}"" alt=""Page {pageNumber}"" />
    </div>
</body>
</html>";
    }

    private string GetImageExtension(string imageFormat)
    {
        return imageFormat.ToLower() switch
        {
            "png" => "png",
            "jpeg" => "jpg",
            "jpg" => "jpg",
            _ => "png"
        };
    }
}
