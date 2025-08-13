using Application.DTOs;
using Application.Interfaces;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Presentation.Controllers;
using Xunit;

namespace Presentation.Tests.Controllers;

public class PdfConversionControllerTests
{
    private readonly Mock<IPdfConversionService> _mockPdfConversionService;
    private readonly Mock<ILogger<PdfConversionController>> _mockLogger;
    private readonly PdfConversionController _controller;

    public PdfConversionControllerTests()
    {
        _mockPdfConversionService = new Mock<IPdfConversionService>();
        _mockLogger = new Mock<ILogger<PdfConversionController>>();
        _controller = new PdfConversionController(_mockPdfConversionService.Object, _mockLogger.Object);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf với request hợp lệ
    /// Kết quả mong muốn: Trả về OkObjectResult với ConversionResponseDto Success=true
    /// </summary>
    [Fact]
    public async Task ConvertPdf_WithValidRequest_ShouldReturnOkResult()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.pdf", "application/pdf", "Test content"u8.ToArray());
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book",
            OutputFormat = OutputFormat.Epub
        };

        var expectedResponse = new ConversionResponseDto
        {
            Success = true,
            FileName = "test.epub",
            TotalPages = 10,
            Format = "Epub"
        };

        _mockPdfConversionService
            .Setup(x => x.ConvertPdfAsync(It.IsAny<PdfUploadDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("test.epub", response.FileName);
        Assert.Equal(10, response.TotalPages);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf với file null
    /// Kết quả mong muốn: Trả về BadRequestObjectResult với error message phù hợp
    /// </summary>
    [Fact]
    public async Task ConvertPdf_WithNullFile_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new PdfUploadDto
        {
            PdfFile = null,
            Title = "Test Book"
        };

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Please provide a valid PDF file", response.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf với file rỗng (empty content)
    /// Kết quả mong muốn: Trả về BadRequestObjectResult với error message phù hợp
    /// </summary>
    [Fact]
    public async Task ConvertPdf_WithEmptyFile_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.pdf", "application/pdf", Array.Empty<byte>());
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book"
        };

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Please provide a valid PDF file", response.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf với content type không phải PDF
    /// Kết quả mong muốn: Trả về BadRequestObjectResult với error message phù hợp
    /// </summary>
    [Theory]
    [InlineData("text/plain")]
    [InlineData("image/jpeg")]
    [InlineData("application/msword")]
    public async Task ConvertPdf_WithInvalidContentType_ShouldReturnBadRequest(string contentType)
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.txt", contentType, "Test content"u8.ToArray());
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book"
        };

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Only PDF files are supported", response.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf với file có extension .pdf nhưng content type khác
    /// Kết quả mong muốn: Chấp nhận file dựa trên extension và trả về OkObjectResult
    /// </summary>
    [Fact]
    public async Task ConvertPdf_WithPdfExtension_ShouldSucceed()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.pdf", "application/octet-stream", "Test content"u8.ToArray());
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book"
        };

        var expectedResponse = new ConversionResponseDto
        {
            Success = true,
            FileName = "test.epub"
        };

        _mockPdfConversionService
            .Setup(x => x.ConvertPdfAsync(It.IsAny<PdfUploadDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(okResult.Value);
        Assert.True(response.Success);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf với file quá lớn (>50MB)
    /// Kết quả mong muốn: Trả về BadRequestObjectResult với error message về file size
    /// </summary>
    [Fact]
    public async Task ConvertPdf_WithOversizedFile_ShouldReturnBadRequest()
    {
        // Arrange
        var largeContent = new byte[51 * 1024 * 1024]; // 51MB
        var mockFile = CreateMockFormFile("large.pdf", "application/pdf", largeContent);
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Large Book"
        };

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("File size exceeds maximum limit of 50MB", response.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf khi service trả về kết quả thất bại
    /// Kết quả mong muốn: Trả về BadRequestObjectResult với error message từ service
    /// </summary>
    [Fact]
    public async Task ConvertPdf_WhenServiceReturnsFailure_ShouldReturnBadRequest()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.pdf", "application/pdf", "Test content"u8.ToArray());
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book"
        };

        var failureResponse = new ConversionResponseDto
        {
            Success = false,
            ErrorMessage = "Processing failed"
        };

        _mockPdfConversionService
            .Setup(x => x.ConvertPdfAsync(It.IsAny<PdfUploadDto>()))
            .ReturnsAsync(failureResponse);

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(badRequestResult.Value);
        Assert.False(response.Success);
        Assert.Equal("Processing failed", response.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf khi có exception không mong muốn
    /// Kết quả mong muốn: Trả về StatusCode 500 với generic error message
    /// </summary>
    [Fact]
    public async Task ConvertPdf_WhenExceptionThrown_ShouldReturnInternalServerError()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.pdf", "application/pdf", "Test content"u8.ToArray());
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book"
        };

        _mockPdfConversionService
            .Setup(x => x.ConvertPdfAsync(It.IsAny<PdfUploadDto>()))
            .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        
        var response = Assert.IsType<ConversionResponseDto>(statusResult.Value);
        Assert.False(response.Success);
        Assert.Equal("An unexpected error occurred. Please try again later.", response.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra API endpoint DownloadFile với file name
    /// Kết quả mong muốn: Trả về NotFoundObjectResult vì chưa implement download logic
    /// </summary>
    [Fact]
    public void DownloadFile_ShouldReturnNotFound()
    {
        // Arrange
        var fileName = "test.epub";

        // Act
        var result = _controller.DownloadFile(fileName);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        var response = notFoundResult.Value;
        Assert.NotNull(response);
    }

    /// <summary>
    /// Kiểm tra API endpoint GetSupportedFormats
    /// Kết quả mong muốn: Trả về OkObjectResult với danh sách formats được hỗ trợ
    /// </summary>
    [Fact]
    public void GetSupportedFormats_ShouldReturnAllFormats()
    {
        // Act
        var result = _controller.GetSupportedFormats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        // Check that it contains formats
        var responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(response);
        Assert.Contains("formats", responseJson);
    }

    /// <summary>
    /// Kiểm tra API endpoint GetHealth để check trạng thái service
    /// Kết quả mong muốn: Trả về OkObjectResult với thông tin healthy status
    /// </summary>
    [Fact]
    public void GetHealth_ShouldReturnHealthyStatus()
    {
        // Act
        var result = _controller.GetHealth();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = okResult.Value;
        Assert.NotNull(response);
        
        var responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(response);
        Assert.Contains("healthy", responseJson);
        Assert.Contains("timestamp", responseJson);
        Assert.Contains("version", responseJson);
    }

    /// <summary>
    /// Kiểm tra API endpoint ConvertPdf với các output format khác nhau
    /// Kết quả mong muốn: Trả về OkObjectResult với format đúng theo request
    /// </summary>
    [Theory]
    [InlineData(OutputFormat.Epub)]
    [InlineData(OutputFormat.Cbz)]
    public async Task ConvertPdf_WithDifferentOutputFormats_ShouldSucceed(OutputFormat format)
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.pdf", "application/pdf", "Test content"u8.ToArray());
        var request = new PdfUploadDto
        {
            PdfFile = mockFile.Object,
            Title = "Test Book",
            OutputFormat = format
        };

        var expectedResponse = new ConversionResponseDto
        {
            Success = true,
            FileName = $"test.{format.ToString().ToLower()}",
            Format = format.ToString()
        };

        _mockPdfConversionService
            .Setup(x => x.ConvertPdfAsync(It.IsAny<PdfUploadDto>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.ConvertPdf(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<ConversionResponseDto>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(format.ToString(), response.Format);
    }

    private static Mock<IFormFile> CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var mockFile = new Mock<IFormFile>();
        var stream = new MemoryStream(content);
        
        mockFile.Setup(f => f.FileName).Returns(fileName);
        mockFile.Setup(f => f.ContentType).Returns(contentType);
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
