using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class ConversionResultTests
{
    /// <summary>
    /// Kiểm tra tạo kết quả thành công với đầy đủ thông tin
    /// Kết quả mong muốn: ConversionResult có Success=true và các thông tin đúng
    /// </summary>
    [Fact]
    public void CreateSuccess_ShouldReturnSuccessfulResult()
    {
        // Arrange
        var outputContent = new byte[] { 1, 2, 3, 4, 5 };
        var outputFileName = "test.epub";
        var format = OutputFormat.Epub;
        var totalPages = 10;

        // Act
        var result = ConversionResult.CreateSuccess(outputContent, outputFileName, format, totalPages);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(outputContent, result.OutputContent);
        Assert.Equal(outputFileName, result.OutputFileName);
        Assert.Equal(format, result.Format);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(totalPages, result.TotalPages);
    }

    /// <summary>
    /// Kiểm tra tạo kết quả thất bại với thông báo lỗi
    /// Kết quả mong muốn: ConversionResult có Success=false và ErrorMessage đúng
    /// </summary>
    [Fact]
    public void CreateFailure_ShouldReturnFailedResult()
    {
        // Arrange
        var errorMessage = "Conversion failed due to invalid PDF";

        // Act
        var result = ConversionResult.CreateFailure(errorMessage);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.OutputContent);
        Assert.Equal(string.Empty, result.OutputFileName);
        Assert.Equal(OutputFormat.Epub, result.Format); // Default value
        Assert.Equal(errorMessage, result.ErrorMessage);
        Assert.Equal(0, result.TotalPages);
    }

    /// <summary>
    /// Kiểm tra tạo kết quả thành công với nội dung rỗng
    /// Kết quả mong muốn: ConversionResult chấp nhận content rỗng và Success=true
    /// </summary>
    [Fact]
    public void CreateSuccess_WithEmptyContent_ShouldSucceed()
    {
        // Arrange
        var outputContent = Array.Empty<byte>();
        var outputFileName = "empty.epub";
        var format = OutputFormat.Cbz;
        var totalPages = 0;

        // Act
        var result = ConversionResult.CreateSuccess(outputContent, outputFileName, format, totalPages);

        // Assert
        Assert.True(result.Success);
        Assert.Equal(outputContent, result.OutputContent);
        Assert.Empty(result.OutputContent);
        Assert.Equal(outputFileName, result.OutputFileName);
        Assert.Equal(format, result.Format);
        Assert.Null(result.ErrorMessage);
        Assert.Equal(totalPages, result.TotalPages);
    }

    /// <summary>
    /// Kiểm tra tạo kết quả thành công với các định dạng khác nhau
    /// Kết quả mong muốn: Format được lưu chính xác theo đầu vào
    /// </summary>
    [Theory]
    [InlineData(OutputFormat.Epub)]
    [InlineData(OutputFormat.Cbz)]
    public void CreateSuccess_WithDifferentFormats_ShouldRetainFormat(OutputFormat format)
    {
        // Arrange
        var outputContent = new byte[] { 1, 2, 3 };
        var outputFileName = $"test.{format.ToString().ToLower()}";
        var totalPages = 5;

        // Act
        var result = ConversionResult.CreateSuccess(outputContent, outputFileName, format, totalPages);

        // Assert
        Assert.Equal(format, result.Format);
    }

    /// <summary>
    /// Kiểm tra tạo kết quả thất bại với các thông báo lỗi khác nhau
    /// Kết quả mong muốn: ErrorMessage được lưu chính xác theo đầu vào
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("Simple error")]
    [InlineData("Complex error with detailed information about what went wrong")]
    public void CreateFailure_WithDifferentErrorMessages_ShouldRetainMessage(string errorMessage)
    {
        // Act
        var result = ConversionResult.CreateFailure(errorMessage);

        // Assert
        Assert.Equal(errorMessage, result.ErrorMessage);
    }

    /// <summary>
    /// Kiểm tra tạo kết quả thành công với số trang khác nhau
    /// Kết quả mong muốn: TotalPages được lưu chính xác theo đầu vào
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(1000)]
    public void CreateSuccess_WithDifferentPageCounts_ShouldRetainCount(int totalPages)
    {
        // Arrange
        var outputContent = new byte[] { 1, 2, 3 };
        var outputFileName = "test.epub";
        var format = OutputFormat.Epub;

        // Act
        var result = ConversionResult.CreateSuccess(outputContent, outputFileName, format, totalPages);

        // Assert
        Assert.Equal(totalPages, result.TotalPages);
    }

    /// <summary>
    /// Kiểm tra tạo kết quả thành công với tên file khác nhau
    /// Kết quả mong muốn: OutputFileName được lưu chính xác theo đầu vào
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("test")]
    [InlineData("very-long-filename-with-special-characters-123.epub")]
    public void CreateSuccess_WithDifferentFileNames_ShouldRetainFileName(string fileName)
    {
        // Arrange
        var outputContent = new byte[] { 1, 2, 3 };
        var format = OutputFormat.Epub;
        var totalPages = 5;

        // Act
        var result = ConversionResult.CreateSuccess(outputContent, fileName, format, totalPages);

        // Assert
        Assert.Equal(fileName, result.OutputFileName);
    }
}
