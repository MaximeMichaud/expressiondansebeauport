namespace Web.Features.Members.Books;

public class BookDto
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public string NameFr { get; set; } = default!;
    public string NameEn { get; set; } = default!;
    public string DescriptionFr { get; set; } = default!;
    public string DescriptionEn { get; set; } = default!;
    public decimal Price { get; set; } = default!;
    public string Isbn { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Editor { get; set; } = default!;
    public int YearOfPublication { get; set; }
    public int NumberOfPages { get; set; }
    public string CardImage { get; set; } = default!;
    public string Slug { get; set; } = default!;
}