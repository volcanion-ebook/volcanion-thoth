namespace Domain.Entities;

public class BookPage
{
    public Guid Id { get; private set; }
    public Guid BookId { get; private set; }
    public int PageNumber { get; private set; }
    public byte[] ImageContent { get; private set; }
    public string ImageFormat { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public BookPage(Guid bookId, int pageNumber, byte[] imageContent, string imageFormat)
    {
        Id = Guid.NewGuid();
        BookId = bookId;
        PageNumber = pageNumber;
        ImageContent = imageContent;
        ImageFormat = imageFormat;
        CreatedAt = DateTime.UtcNow;
    }
}
