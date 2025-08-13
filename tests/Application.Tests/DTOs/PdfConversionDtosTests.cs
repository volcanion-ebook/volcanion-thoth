using Application.DTOs;
using Domain.ValueObjects;
using Xunit;

namespace Application.Tests.DTOs;

public class PdfUploadDtoTests
{
    /// <summary>
    /// Kiểm tra PdfUploadDto có giá trị mặc định đúng khi khởi tạo
    /// Kết quả mong muốn: OutputFormat=Epub, các properties khác null
    /// </summary>
    [Fact]
    public void PdfUploadDto_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var dto = new PdfUploadDto();

        // Assert
        Assert.Equal(OutputFormat.Epub, dto.OutputFormat);
        Assert.Null(dto.PdfFile);
        Assert.Null(dto.Title);
        Assert.Null(dto.Options);
    }

    /// <summary>
    /// Kiểm tra việc set các properties của PdfUploadDto
    /// Kết quả mong muốn: Tất cả properties được lưu đúng giá trị
    /// </summary>
    [Fact]
    public void PdfUploadDto_ShouldAllowSettingProperties()
    {
        // Arrange
        var dto = new PdfUploadDto();
        var title = "Test Book";
        var outputFormat = OutputFormat.Cbz;
        var options = new ConversionOptionsDto { DPI = 300 };

        // Act
        dto.Title = title;
        dto.OutputFormat = outputFormat;
        dto.Options = options;

        // Assert
        Assert.Equal(title, dto.Title);
        Assert.Equal(outputFormat, dto.OutputFormat);
        Assert.Equal(options, dto.Options);
    }
}

public class ConversionOptionsDtoTests
{
    /// <summary>
    /// Kiểm tra ConversionOptionsDto có giá trị mặc định đúng khi khởi tạo
    /// Kết quả mong muốn: DPI=150, ImageFormat=PNG, OptimizeImages=true, CompressionLevel=6
    /// </summary>
    [Fact]
    public void ConversionOptionsDto_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var dto = new ConversionOptionsDto();

        // Assert
        Assert.Equal(150, dto.DPI);
        Assert.Equal("PNG", dto.ImageFormat);
        Assert.True(dto.OptimizeImages);
        Assert.Equal(6, dto.CompressionLevel);
    }

    /// <summary>
    /// Kiểm tra việc set DPI với các giá trị hợp lệ
    /// Kết quả mong muốn: DPI được lưu chính xác với giá trị đầu vào
    /// </summary>
    [Theory]
    [InlineData(72)]
    [InlineData(150)]
    [InlineData(300)]
    [InlineData(600)]
    public void DPI_ShouldBeSettable(int dpi)
    {
        // Arrange
        var dto = new ConversionOptionsDto();

        // Act
        dto.DPI = dpi;

        // Assert
        Assert.Equal(dpi, dto.DPI);
    }

    /// <summary>
    /// Kiểm tra việc set ImageFormat với các định dạng hợp lệ
    /// Kết quả mong muốn: ImageFormat được lưu chính xác với giá trị đầu vào
    /// </summary>
    [Theory]
    [InlineData("PNG")]
    [InlineData("JPEG")]
    [InlineData("JPG")]
    public void ImageFormat_ShouldBeSettable(string format)
    {
        // Arrange
        var dto = new ConversionOptionsDto();

        // Act
        dto.ImageFormat = format;

        // Assert
        Assert.Equal(format, dto.ImageFormat);
    }

    /// <summary>
    /// Kiểm tra việc set OptimizeImages với các giá trị boolean
    /// Kết quả mong muốn: OptimizeImages được lưu chính xác với giá trị đầu vào
    /// </summary>
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OptimizeImages_ShouldBeSettable(bool optimize)
    {
        // Arrange
        var dto = new ConversionOptionsDto();

        // Act
        dto.OptimizeImages = optimize;

        // Assert
        Assert.Equal(optimize, dto.OptimizeImages);
    }

    /// <summary>
    /// Kiểm tra việc set CompressionLevel với các giá trị hợp lệ
    /// Kết quả mong muốn: CompressionLevel được lưu chính xác với giá trị đầu vào
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    [InlineData(6)]
    [InlineData(9)]
    public void CompressionLevel_ShouldBeSettable(int level)
    {
        // Arrange
        var dto = new ConversionOptionsDto();

        // Act
        dto.CompressionLevel = level;

        // Assert
        Assert.Equal(level, dto.CompressionLevel);
    }
}

public class ConversionResponseDtoTests
{
    /// <summary>
    /// Kiểm tra ConversionResponseDto có giá trị mặc định đúng khi khởi tạo
    /// Kết quả mong muốn: Success=false, TotalPages=0, Format=empty, các properties khác null
    /// </summary>
    [Fact]
    public void ConversionResponseDto_ShouldHaveCorrectDefaultValues()
    {
        // Act
        var dto = new ConversionResponseDto();

        // Assert
        Assert.False(dto.Success);
        Assert.Null(dto.DownloadUrl);
        Assert.Null(dto.FileName);
        Assert.Null(dto.ErrorMessage);
        Assert.Equal(0, dto.TotalPages);
        Assert.Equal(string.Empty, dto.Format);
    }

    /// <summary>
    /// Kiểm tra việc set tất cả properties của ConversionResponseDto
    /// Kết quả mong muốn: Tất cả properties được lưu chính xác với giá trị đầu vào
    /// </summary>
    [Fact]
    public void ConversionResponseDto_ShouldAllowSettingAllProperties()
    {
        // Arrange
        var dto = new ConversionResponseDto();
        var success = true;
        var downloadUrl = "https://example.com/download/123";
        var fileName = "test.epub";
        var errorMessage = "No error";
        var totalPages = 15;
        var format = "Epub";

        // Act
        dto.Success = success;
        dto.DownloadUrl = downloadUrl;
        dto.FileName = fileName;
        dto.ErrorMessage = errorMessage;
        dto.TotalPages = totalPages;
        dto.Format = format;

        // Assert
        Assert.Equal(success, dto.Success);
        Assert.Equal(downloadUrl, dto.DownloadUrl);
        Assert.Equal(fileName, dto.FileName);
        Assert.Equal(errorMessage, dto.ErrorMessage);
        Assert.Equal(totalPages, dto.TotalPages);
        Assert.Equal(format, dto.Format);
    }

    /// <summary>
    /// Kiểm tra ConversionResponseDto chấp nhận giá trị null cho các properties
    /// Kết quả mong muốn: Các nullable properties có thể được set null
    /// </summary>
    [Fact]
    public void ConversionResponseDto_ShouldAllowNullValues()
    {
        // Arrange
        var dto = new ConversionResponseDto();

        // Act
        dto.DownloadUrl = null;
        dto.FileName = null;
        dto.ErrorMessage = null;

        // Assert
        Assert.Null(dto.DownloadUrl);
        Assert.Null(dto.FileName);
        Assert.Null(dto.ErrorMessage);
    }
}
