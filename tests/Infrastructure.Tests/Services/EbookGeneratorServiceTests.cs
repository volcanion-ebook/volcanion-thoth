using Domain.Services;
using Domain.ValueObjects;
using Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Infrastructure.Tests.Services;

public class EbookGeneratorServiceTests
{
    private readonly Mock<ILogger<EbookGeneratorService>> _mockLogger;
    private readonly EbookGeneratorService _service;

    public EbookGeneratorServiceTests()
    {
        _mockLogger = new Mock<ILogger<EbookGeneratorService>>();
        _service = new EbookGeneratorService(_mockLogger.Object);
    }

    [Fact]
    public async Task GenerateEpubAsync_WithValidInput_ShouldReturnEpubContent()
    {
        // Arrange
        var title = "Test Book";
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 },
            new byte[] { 6, 7, 8, 9, 10 }
        };
        var options = new ConversionOptions { CompressionLevel = 6 };

        // Act
        var result = await _service.GenerateEpubAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // EPUB files start with PK (ZIP signature)
        Assert.Equal(0x50, result[0]); // 'P'
        Assert.Equal(0x4B, result[1]); // 'K'
    }

    [Fact]
    public async Task GenerateCbzAsync_WithValidInput_ShouldReturnCbzContent()
    {
        // Arrange
        var title = "Test Comic";
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 },
            new byte[] { 6, 7, 8, 9, 10 },
            new byte[] { 11, 12, 13, 14, 15 }
        };
        var options = new ConversionOptions { CompressionLevel = 9 };

        // Act
        var result = await _service.GenerateCbzAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // CBZ files are ZIP files, so they start with PK
        Assert.Equal(0x50, result[0]); // 'P'
        Assert.Equal(0x4B, result[1]); // 'K'
    }

    [Fact]
    public async Task GenerateEpubAsync_WithEmptyPageList_ShouldStillGenerateValidEpub()
    {
        // Arrange
        var title = "Empty Book";
        var pageImages = new List<byte[]>();
        var options = new ConversionOptions();

        // Act
        var result = await _service.GenerateEpubAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should still be a valid ZIP file
        Assert.Equal(0x50, result[0]); // 'P'
        Assert.Equal(0x4B, result[1]); // 'K'
    }

    [Fact]
    public async Task GenerateCbzAsync_WithEmptyPageList_ShouldStillGenerateValidCbz()
    {
        // Arrange
        var title = "Empty Comic";
        var pageImages = new List<byte[]>();
        var options = new ConversionOptions();

        // Act
        var result = await _service.GenerateCbzAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        
        // Should still be a valid ZIP file
        Assert.Equal(0x50, result[0]); // 'P'
        Assert.Equal(0x4B, result[1]); // 'K'
    }

    [Fact]
    public async Task GenerateEpubAsync_WithSinglePage_ShouldGenerateCorrectly()
    {
        // Arrange
        var title = "Single Page Book";
        var pageImages = new List<byte[]>
        {
            new byte[] { 255, 216, 255, 224 } // JPEG header
        };
        var options = new ConversionOptions { ImageFormat = "JPEG" };

        // Act
        var result = await _service.GenerateEpubAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 100); // Should be substantial content
    }

    [Fact]
    public async Task GenerateCbzAsync_WithSinglePage_ShouldGenerateCorrectly()
    {
        // Arrange
        var title = "Single Page Comic";
        var pageImages = new List<byte[]>
        {
            new byte[] { 137, 80, 78, 71 } // PNG header
        };
        var options = new ConversionOptions { ImageFormat = "PNG" };

        // Act
        var result = await _service.GenerateCbzAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(result.Length > 50); // Should contain the image data plus ZIP overhead
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(9)]
    public async Task GenerateEpubAsync_WithDifferentCompressionLevels_ShouldSucceed(int compressionLevel)
    {
        // Arrange
        var title = "Compression Test";
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 }
        };
        var options = new ConversionOptions { CompressionLevel = compressionLevel };

        // Act
        var result = await _service.GenerateEpubAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(9)]
    public async Task GenerateCbzAsync_WithDifferentCompressionLevels_ShouldSucceed(int compressionLevel)
    {
        // Arrange
        var title = "Compression Test";
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 }
        };
        var options = new ConversionOptions { CompressionLevel = compressionLevel };

        // Act
        var result = await _service.GenerateCbzAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("PNG")]
    [InlineData("JPEG")]
    [InlineData("JPG")]
    public async Task GenerateEpubAsync_WithDifferentImageFormats_ShouldSucceed(string imageFormat)
    {
        // Arrange
        var title = "Format Test";
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 }
        };
        var options = new ConversionOptions { ImageFormat = imageFormat };

        // Act
        var result = await _service.GenerateEpubAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Theory]
    [InlineData("PNG")]
    [InlineData("JPEG")]
    [InlineData("JPG")]
    public async Task GenerateCbzAsync_WithDifferentImageFormats_ShouldSucceed(string imageFormat)
    {
        // Arrange
        var title = "Format Test";
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 }
        };
        var options = new ConversionOptions { ImageFormat = imageFormat };

        // Act
        var result = await _service.GenerateCbzAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateEpubAsync_WithLargeTitle_ShouldHandleCorrectly()
    {
        // Arrange
        var title = new string('A', 1000); // Very long title
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 }
        };
        var options = new ConversionOptions();

        // Act
        var result = await _service.GenerateEpubAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GenerateCbzAsync_WithSpecialCharactersInTitle_ShouldHandleCorrectly()
    {
        // Arrange
        var title = "Test & Title <with> \"special\" 'characters' ñáéíóú";
        var pageImages = new List<byte[]>
        {
            new byte[] { 1, 2, 3, 4, 5 }
        };
        var options = new ConversionOptions();

        // Act
        var result = await _service.GenerateCbzAsync(title, pageImages, options);

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }
}
