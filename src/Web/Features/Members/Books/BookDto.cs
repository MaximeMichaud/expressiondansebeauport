namespace Web.Features.Members.Books;

public class BookDto
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public string NameFr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string DescriptionFr { get; set; } = null!;
    public string DescriptionEn { get; set; } = null!;
    public decimal Price { get; set; } = 0!;
    public string Isbn { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Editor { get; set; } = null!;
    public int YearOfPublication { get; set; }
    public int NumberOfPages { get; set; }
    public string CardImage { get; set; } = null!;
    public string Slug { get; set; } = null!;
}