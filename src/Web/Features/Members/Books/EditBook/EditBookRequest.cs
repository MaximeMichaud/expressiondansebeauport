using Microsoft.EntityFrameworkCore;

namespace Web.Features.Members.Books.EditBook;

public class EditBookRequest
{
    public Guid Id { get; set; }
    public string NameFr { get; set; } = null!;
    public string NameEn { get; set; } = null!;
    public string DescriptionFr { get; set; } = null!;
    public string DescriptionEn { get; set; } = null!;
    public string Isbn { get; set; } = null!;
    public string Author { get; set; } = null!;
    public string Editor { get; set; } = null!;
    public int YearOfPublication { get; set; }
    public int NumberOfPages { get; set; }
    public IFormFile? CardImage { get; set; }
    [Precision(18, 2)]
    public decimal Price { get; set; }
}