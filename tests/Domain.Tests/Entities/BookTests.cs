using Domain.Entities;
using Xunit;

namespace Domain.Tests.Entities;

public class BookTests
{
    /// <summary>
    /// Kiểm tra khởi tạo Book entity với các tham số hợp lệ
    /// Kết quả mong muốn: Tất cả properties được set đúng giá trị ban đầu
    /// </summary>
    [Fact]
    public void Constructor_ShouldInitializePropertiesCorrectly()
    {
        // Arrange
        var fileName = "test.pdf";
        var title = "Test Book";
        var pdfContent = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var book = new Book(fileName, title, pdfContent);

        // Assert
        Assert.NotEqual(Guid.Empty, book.Id);
        Assert.Equal(fileName, book.OriginalFileName);
        Assert.Equal(title, book.Title);
        Assert.Equal(pdfContent, book.PdfContent);
        Assert.True(book.CreatedAt <= DateTime.UtcNow);
        Assert.Null(book.ProcessedAt);
        Assert.Equal(BookStatus.Pending, book.Status);
        Assert.Empty(book.Pages);
    }

    /// <summary>
    /// Kiểm tra việc đánh dấu Book đã được xử lý
    /// Kết quả mong muốn: Status chuyển thành Processed và ProcessedAt được cập nhật
    /// </summary>
    [Fact]
    public void MarkAsProcessed_ShouldUpdateStatusAndProcessedAt()
    {
        // Arrange
        var book = new Book("test.pdf", "Test Book", new byte[] { 1, 2, 3 });
        var beforeProcessed = DateTime.UtcNow;

        // Act
        book.MarkAsProcessed();

        // Assert
        Assert.Equal(BookStatus.Processed, book.Status);
        Assert.NotNull(book.ProcessedAt);
        Assert.True(book.ProcessedAt >= beforeProcessed);
        Assert.True(book.ProcessedAt <= DateTime.UtcNow);
    }

    /// <summary>
    /// Kiểm tra việc thêm một page vào Book
    /// Kết quả mong muốn: Page được thêm thành công vào collection Pages
    /// </summary>
    [Fact]
    public void AddPage_ShouldAddPageToCollection()
    {
        // Arrange
        var book = new Book("test.pdf", "Test Book", new byte[] { 1, 2, 3 });
        var page = new BookPage(book.Id, 1, new byte[] { 4, 5, 6 }, "PNG");

        // Act
        book.AddPage(page);

        // Assert
        Assert.Single(book.Pages);
        Assert.Contains(page, book.Pages);
    }

    /// <summary>
    /// Kiểm tra việc thêm nhiều pages vào Book
    /// Kết quả mong muốn: Tất cả pages được thêm thành công và đếm đúng số lượng
    /// </summary>
    [Fact]
    public void AddPage_ShouldAddMultiplePages()
    {
        // Arrange
        var book = new Book("test.pdf", "Test Book", new byte[] { 1, 2, 3 });
        var page1 = new BookPage(book.Id, 1, new byte[] { 4, 5, 6 }, "PNG");
        var page2 = new BookPage(book.Id, 2, new byte[] { 7, 8, 9 }, "PNG");

        // Act
        book.AddPage(page1);
        book.AddPage(page2);

        // Assert
        Assert.Equal(2, book.Pages.Count);
        Assert.Contains(page1, book.Pages);
        Assert.Contains(page2, book.Pages);
    }

    /// <summary>
    /// Kiểm tra tất cả các giá trị enum BookStatus có hợp lệ
    /// Kết quả mong muốn: Tất cả values được định nghĩa đúng trong enum
    /// </summary>
    [Theory]
    [InlineData(BookStatus.Pending)]
    [InlineData(BookStatus.Processing)]
    [InlineData(BookStatus.Processed)]
    [InlineData(BookStatus.Failed)]
    public void BookStatus_ShouldHaveCorrectValues(BookStatus status)
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(BookStatus), status));
    }
}
