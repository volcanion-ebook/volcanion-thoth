using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class BookPageTests
{
    /// <summary>
    /// Kiểm tra khởi tạo BookPage entity với các tham số hợp lệ
    /// Kết quả mong muốn: Tất cả properties được set đúng giá trị ban đầu
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var pageNumber = 5;
        var imageContent = new byte[] { 1, 2, 3, 4, 5 };
        var imageFormat = "PNG";

        // Act
        var bookPage = new BookPage(bookId, pageNumber, imageContent, imageFormat);

        // Assert
        Assert.NotEqual(Guid.Empty, bookPage.Id);
        Assert.Equal(bookId, bookPage.BookId);
        Assert.Equal(pageNumber, bookPage.PageNumber);
        Assert.Equal(imageContent, bookPage.ImageContent);
        Assert.Equal(imageFormat, bookPage.ImageFormat);
        Assert.True(bookPage.CreatedAt <= DateTime.UtcNow);
    }

    /// <summary>
    /// Kiểm tra việc tạo ra Id duy nhất cho mỗi BookPage
    /// Kết quả mong muốn: Mỗi BookPage có Id khác nhau dù cùng BookId
    /// </summary>
    [Fact]
    public void Constructor_ShouldGenerateUniqueIds()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var imageContent = new byte[] { 1, 2, 3 };

        // Act
        var page1 = new BookPage(bookId, 1, imageContent, "PNG");
        var page2 = new BookPage(bookId, 2, imageContent, "PNG");

        // Assert
        Assert.NotEqual(page1.Id, page2.Id);
    }

    /// <summary>
    /// Kiểm tra constructor với các số trang hợp lệ
    /// Kết quả mong muốn: PageNumber được lưu chính xác với giá trị đầu vào
    /// </summary>
    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(999)]
    public void Constructor_ShouldAcceptValidPageNumbers(int pageNumber)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var imageContent = new byte[] { 1, 2, 3 };

        // Act
        var bookPage = new BookPage(bookId, pageNumber, imageContent, "PNG");

        // Assert
        Assert.Equal(pageNumber, bookPage.PageNumber);
    }

    /// <summary>
    /// Kiểm tra constructor với các định dạng ảnh hợp lệ
    /// Kết quả mong muốn: ImageFormat được lưu chính xác với giá trị đầu vào
    /// </summary>
    [Theory]
    [InlineData("PNG")]
    [InlineData("JPEG")]
    [InlineData("JPG")]
    [InlineData("GIF")]
    public void Constructor_ShouldAcceptValidImageFormats(string imageFormat)
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var imageContent = new byte[] { 1, 2, 3 };

        // Act
        var bookPage = new BookPage(bookId, 1, imageContent, imageFormat);

        // Assert
        Assert.Equal(imageFormat, bookPage.ImageFormat);
    }

    /// <summary>
    /// Kiểm tra constructor với nội dung ảnh rỗng
    /// Kết quả mong muốn: ImageContent chấp nhận mảng byte rỗng và lưu đúng
    /// </summary>
    [Fact]
    public void Constructor_ShouldAcceptEmptyImageContent()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var imageContent = Array.Empty<byte>();

        // Act
        var bookPage = new BookPage(bookId, 1, imageContent, "PNG");

        // Assert
        Assert.Equal(imageContent, bookPage.ImageContent);
        Assert.Empty(bookPage.ImageContent);
    }
}
