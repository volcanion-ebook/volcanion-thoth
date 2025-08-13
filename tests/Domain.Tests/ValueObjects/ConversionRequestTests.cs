using Domain.ValueObjects;
using Xunit;

namespace Domain.Tests.ValueObjects;

public class ConversionRequestTests
{
    /// <summary>
    /// Kiểm tra khởi tạo ConversionRequest với tất cả tham số hợp lệ
    /// Kết quả mong muốn: Tất cả properties được set đúng giá trị đầu vào
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var fileName = "test.pdf";
        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };
        var outputFormat = OutputFormat.Epub;
        var options = new ConversionOptions { DPI = 300 };

        // Act
        var request = new ConversionRequest(fileName, pdfContent, outputFormat, options);

        // Assert
        Assert.Equal(fileName, request.FileName);
        Assert.Equal(pdfContent, request.PdfContent);
        Assert.Equal(outputFormat, request.OutputFormat);
        Assert.Equal(options, request.Options);
    }

    /// <summary>
    /// Kiểm tra khởi tạo ConversionRequest mà không truyền options
    /// Kết quả mong muốn: Options sử dụng giá trị mặc định
    /// </summary>
    [Fact]
    public void Constructor_WithNullOptions_ShouldUseDefaultOptions()
    {
        // Arrange
        var fileName = "test.pdf";
        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };
        var outputFormat = OutputFormat.Cbz;

        // Act
        var request = new ConversionRequest(fileName, pdfContent, outputFormat);

        // Assert
        Assert.NotNull(request.Options);
        Assert.Equal(150, request.Options.DPI);
        Assert.Equal("PNG", request.Options.ImageFormat);
        Assert.True(request.Options.OptimizeImages);
        Assert.Equal(6, request.Options.CompressionLevel);
    }

    /// <summary>
    /// Kiểm tra constructor với tên file không hợp lệ (rỗng hoặc whitespace)
    /// Kết quả mong muốn: Ném ArgumentException với message phù hợp
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidFileName_ShouldThrowArgumentException(string fileName)
    {
        // Arrange
        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };
        var outputFormat = OutputFormat.Epub;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new ConversionRequest(fileName, pdfContent, outputFormat));
        Assert.Equal("File name cannot be empty (Parameter 'fileName')", exception.Message);
    }

    /// <summary>
    /// Kiểm tra constructor với tên file null
    /// Kết quả mong muốn: Ném ArgumentException với message phù hợp
    /// </summary>
    [Fact]
    public void Constructor_WithNullFileName_ShouldThrowArgumentException()
    {
        // Arrange
        string fileName = null!;
        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };
        var outputFormat = OutputFormat.Epub;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new ConversionRequest(fileName, pdfContent, outputFormat));
        Assert.Equal("File name cannot be empty (Parameter 'fileName')", exception.Message);
    }

    /// <summary>
    /// Kiểm tra constructor với PDF content null
    /// Kết quả mong muốn: Ném ArgumentException với message phù hợp
    /// </summary>
    [Fact]
    public void Constructor_WithNullPdfContent_ShouldThrowArgumentException()
    {
        // Arrange
        var fileName = "test.pdf";
        byte[] pdfContent = null!;
        var outputFormat = OutputFormat.Epub;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new ConversionRequest(fileName, pdfContent, outputFormat));
        Assert.Equal("PDF content cannot be empty (Parameter 'pdfContent')", exception.Message);
    }

    /// <summary>
    /// Kiểm tra constructor với PDF content rỗng
    /// Kết quả mong muốn: Ném ArgumentException với message phù hợp
    /// </summary>
    [Fact]
    public void Constructor_WithEmptyPdfContent_ShouldThrowArgumentException()
    {
        // Arrange
        var fileName = "test.pdf";
        var pdfContent = Array.Empty<byte>();
        var outputFormat = OutputFormat.Epub;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new ConversionRequest(fileName, pdfContent, outputFormat));
        Assert.Equal("PDF content cannot be empty (Parameter 'pdfContent')", exception.Message);
    }

    /// <summary>
    /// Kiểm tra constructor với các định dạng output hợp lệ
    /// Kết quả mong muốn: OutputFormat được set đúng giá trị
    /// </summary>
    [Theory]
    [InlineData(OutputFormat.Epub)]
    [InlineData(OutputFormat.Cbz)]
    public void Constructor_WithValidOutputFormat_ShouldSetFormat(OutputFormat format)
    {
        // Arrange
        var fileName = "test.pdf";
        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var request = new ConversionRequest(fileName, pdfContent, format);

        // Assert
        Assert.Equal(format, request.OutputFormat);
    }
}

public class ConversionOptionsTests
{
    /// <summary>
    /// Kiểm tra constructor mặc định của ConversionOptions
    /// Kết quả mong muốn: Tất cả properties có giá trị mặc định đúng
    /// </summary>
    [Fact]
    public void DefaultConstructor_ShouldSetDefaultValues()
    {
        // Act
        var options = new ConversionOptions();

        // Assert
        Assert.Equal(150, options.DPI);
        Assert.Equal("PNG", options.ImageFormat);
        Assert.True(options.OptimizeImages);
        Assert.Equal(6, options.CompressionLevel);
    }

    /// <summary>
    /// Kiểm tra việc set giá trị DPI với các giá trị hợp lệ
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
        var options = new ConversionOptions();

        // Act
        options.DPI = dpi;

        // Assert
        Assert.Equal(dpi, options.DPI);
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
        var options = new ConversionOptions();

        // Act
        options.ImageFormat = format;

        // Assert
        Assert.Equal(format, options.ImageFormat);
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
        var options = new ConversionOptions();

        // Act
        options.OptimizeImages = optimize;

        // Assert
        Assert.Equal(optimize, options.OptimizeImages);
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
        var options = new ConversionOptions();

        // Act
        options.CompressionLevel = level;

        // Assert
        Assert.Equal(level, options.CompressionLevel);
    }
}

public class OutputFormatTests
{
    /// <summary>
    /// Kiểm tra các giá trị enum OutputFormat có hợp lệ
    /// Kết quả mong muốn: Tất cả values được định nghĩa đúng trong enum
    /// </summary>
    [Theory]
    [InlineData(OutputFormat.Epub)]
    [InlineData(OutputFormat.Cbz)]
    public void OutputFormat_ShouldHaveCorrectValues(OutputFormat format)
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(OutputFormat), format));
    }

    /// <summary>
    /// Kiểm tra số lượng values trong enum OutputFormat
    /// Kết quả mong muốn: Có đúng 2 giá trị Epub và Cbz
    /// </summary>
    [Fact]
    public void OutputFormat_ShouldHaveExpectedCount()
    {
        // Act
        var values = Enum.GetValues<OutputFormat>();

        // Assert
        Assert.Equal(2, values.Length);
    }
}
