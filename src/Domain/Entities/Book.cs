namespace Domain.Entities;

public class Book
{
    public Guid Id { get; private set; }
    public string OriginalFileName { get; private set; }
    public string Title { get; private set; }
    public byte[] PdfContent { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? ProcessedAt { get; private set; }
    public BookStatus Status { get; private set; }
    public List<BookPage> Pages { get; private set; }

    public Book(string originalFileName, string title, byte[] pdfContent)
    {
        Id = Guid.NewGuid();
        OriginalFileName = originalFileName;
        Title = title;
        PdfContent = pdfContent;
        CreatedAt = DateTime.UtcNow;
        Status = BookStatus.Pending;
        Pages = new List<BookPage>();
    }

    public void MarkAsProcessed()
    {
        Status = BookStatus.Processed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void AddPage(BookPage page)
    {
        Pages.Add(page);
    }
}

public enum BookStatus
{
    Pending,
    Processing,
    Processed,
    Failed
}
