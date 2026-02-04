using Microsoft.EntityFrameworkCore;

namespace Web.Features.Members.Books.CreateBook;

public class CreateBookRequest
{
    public string NameFr { get; set; } = default!;
    public string NameEn { get; set; } = default!;
    public string DescriptionFr { get; set; } = default!;
    public string DescriptionEn { get; set; } = default!;
    public string Isbn { get; set; } = default!;
    public string Author { get; set; } = default!;
    public string Editor { get; set; } = default!;
    public int YearOfPublication { get; set; }
    public int NumberOfPages { get; set; }
    public IFormFile CardImage { get; set; } = default!;
    [Precision(18, 2)]
    public decimal Price { get; set; }
}