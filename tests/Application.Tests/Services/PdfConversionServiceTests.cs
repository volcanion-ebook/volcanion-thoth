using Application.DTOs;
using Application.Services;
using Domain.Services;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using Xunit;

namespace Application.Tests.Services;

public class PdfConversionServiceTests
{
    private readonly Mock<IPdfProcessingService> _mockPdfProcessingService;
    private readonly Mock<ILogger<PdfConversionService>> _mockLogger;
    private readonly PdfConversionService _service;

    public PdfConversionServiceTests()
    {
        _mockPdfProcessingService = new Mock<IPdfProcessingService>();
        _mockLogger = new Mock<ILogger<PdfConversionService>>();
        _service = new PdfConversionService(_mockPdfProcessingService.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Kiểm tra chuyển đổi PDF thành công với đầu vào hợp lệ
    /// Kết quả mong muốn: Trả về PdfConversionResponseDto với Success=true
    /// </summary>
    [Fact]
    public async Task ConvertPdfAsync_WithValidInput_ShouldReturnSuccessResponse()
    {
        // Arrange
        var pdfContent = "Test PDF content"u8.ToArray();
        var mockFile = CreateMockFormFile("test.pdf", pdfContent);
        
        var uploadDto = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book",
            OutputFormat = OutputFormat.Epub,
            Options = new ConversionOptionsDto { DPI = 300 }
        };

        var expectedResult = ConversionResult.CreateSuccess(
            new byte[] { 1, 2, 3 }, 
            "test.epub", 
            OutputFormat.Epub, 
            10
        );

        _mockPdfProcessingService
            .Setup(x => x.ConvertPdfToEbookAsync(It.IsAny<ConversionRequest>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ConvertPdfAsync(uploadDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("test.epub", result.FileName);
        Assert.Equal(10, result.TotalPages);
        Assert.Equal("Epub", result.Format);
        Assert.Null(result.ErrorMessage);

        _mockPdfProcessingService.Verify(
            x => x.ConvertPdfToEbookAsync(It.Is<ConversionRequest>(req => 
                req.FileName == "test.pdf" && 
                req.OutputFormat == OutputFormat.Epub &&
                req.Options.DPI == 300
            )), 
            Times.Once
        );
    }

    /// <summary>
    /// Kiểm tra chuyển đổi PDF khi không có title được cung cấp
    /// Kết quả mong muốn: Sử dụng tên file làm title mặc định
    /// </summary>
    [Fact]
    public async Task ConvertPdfAsync_WithNoTitle_ShouldUseFileNameAsTitle()
    {
        // Arrange
        var pdfContent = "Test PDF content"u8.ToArray();
        var mockFile = CreateMockFormFile("my-book.pdf", pdfContent);
        
        var uploadDto = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = null, // No title provided
            OutputFormat = OutputFormat.Cbz
        };

        var expectedResult = ConversionResult.CreateSuccess(
            new byte[] { 1, 2, 3 }, 
            "my-book.cbz", 
            OutputFormat.Cbz, 
            5
        );

        _mockPdfProcessingService
            .Setup(x => x.ConvertPdfToEbookAsync(It.IsAny<ConversionRequest>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ConvertPdfAsync(uploadDto);

        // Assert
        Assert.True(result.Success);
        
        _mockPdfProcessingService.Verify(
            x => x.ConvertPdfToEbookAsync(It.Is<ConversionRequest>(req => 
                req.FileName == "my-book.pdf"
            )), 
            Times.Once
        );
    }

    /// <summary>
    /// Kiểm tra chuyển đổi PDF khi không có options được cung cấp
    /// Kết quả mong muốn: Sử dụng options mặc định cho ConversionRequest
    /// </summary>
    [Fact]
    public async Task ConvertPdfAsync_WithNullOptions_ShouldUseDefaultOptions()
    {
        // Arrange
        var pdfContent = "Test PDF content"u8.ToArray();
        var mockFile = CreateMockFormFile("test.pdf", pdfContent);
        
        var uploadDto = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test",
            OutputFormat = OutputFormat.Epub,
            Options = null // No options provided
        };

        var expectedResult = ConversionResult.CreateSuccess(
            new byte[] { 1, 2, 3 }, 
            "test.epub", 
            OutputFormat.Epub, 
            1
        );

        _mockPdfProcessingService
            .Setup(x => x.ConvertPdfToEbookAsync(It.IsAny<ConversionRequest>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ConvertPdfAsync(uploadDto);

        // Assert
        Assert.True(result.Success);
        
        _mockPdfProcessingService.Verify(
            x => x.ConvertPdfToEbookAsync(It.Is<ConversionRequest>(req => 
                req.Options.DPI == 150 && // Default DPI
                req.Options.ImageFormat == "PNG" && // Default format
                req.Options.OptimizeImages == true && // Default optimization
                req.Options.CompressionLevel == 6 // Default compression
            )), 
            Times.Once
        );
    }

    /// <summary>
    /// Kiểm tra chuyển đổi PDF khi quá trình xử lý thất bại
    /// Kết quả mong muốn: Trả về PdfConversionResponseDto với Success=false và ErrorMessage
    /// </summary>
    [Fact]
    public async Task ConvertPdfAsync_WhenProcessingFails_ShouldReturnFailureResponse()
    {
        // Arrange
        var pdfContent = "Test PDF content"u8.ToArray();
        var mockFile = CreateMockFormFile("test.pdf", pdfContent);
        
        var uploadDto = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book",
            OutputFormat = OutputFormat.Epub
        };

        var expectedResult = ConversionResult.CreateFailure("Processing failed");

        _mockPdfProcessingService
            .Setup(x => x.ConvertPdfToEbookAsync(It.IsAny<ConversionRequest>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ConvertPdfAsync(uploadDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Processing failed", result.ErrorMessage);
        Assert.Equal(0, result.TotalPages);
    }

    /// <summary>
    /// Kiểm tra chuyển đổi PDF khi có exception không mong muốn
    /// Kết quả mong muốn: Trả về PdfConversionResponseDto với generic error message
    /// </summary>
    [Fact]
    public async Task ConvertPdfAsync_WhenExceptionThrown_ShouldReturnGenericError()
    {
        // Arrange
        var pdfContent = "Test PDF content"u8.ToArray();
        var mockFile = CreateMockFormFile("test.pdf", pdfContent);
        
        var uploadDto = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book",
            OutputFormat = OutputFormat.Epub
        };

        _mockPdfProcessingService
            .Setup(x => x.ConvertPdfToEbookAsync(It.IsAny<ConversionRequest>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        // Act
        var result = await _service.ConvertPdfAsync(uploadDto);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("An error occurred during conversion. Please try again.", result.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra chuyển đổi PDF với các định dạng output khác nhau
    /// Kết quả mong muốn: Format string được map đúng từ OutputFormat enum
    /// </summary>
    [Theory]
    [InlineData(OutputFormat.Epub, "Epub")]
    [InlineData(OutputFormat.Cbz, "Cbz")]
    public async Task ConvertPdfAsync_WithDifferentFormats_ShouldMapCorrectly(OutputFormat inputFormat, string expectedFormat)
    {
        // Arrange
        var pdfContent = "Test PDF content"u8.ToArray();
        var mockFile = CreateMockFormFile("test.pdf", pdfContent);
        
        var uploadDto = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book",
            OutputFormat = inputFormat
        };

        var expectedResult = ConversionResult.CreateSuccess(
            new byte[] { 1, 2, 3 }, 
            $"test.{inputFormat.ToString().ToLower()}", 
            inputFormat, 
            5
        );

        _mockPdfProcessingService
            .Setup(x => x.ConvertPdfToEbookAsync(It.IsAny<ConversionRequest>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _service.ConvertPdfAsync(uploadDto);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(expectedFormat, result.Format);
    }

    private static Mock<IFormFile> CreateMockFormFile(string fileName, byte[] content)
    {
        var mockFile = new Mock<IFormFile>();
        var stream = new MemoryStream(content);
        
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.Length).Returns(content.Length);
        mockFile.Setup(f => f.OpenReadStream()).Returns(stream);
        mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
               .Returns((Stream target, CancellationToken token) => 
               {
                   stream.Position = 0;
                   return stream.CopyToAsync(target, token);
               });
        
        return mockFile;
    }
}
