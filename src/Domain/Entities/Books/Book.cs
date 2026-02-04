using Domain.Common;

namespace Domain.Entities.Books;

public class Book : AuditableAndSoftDeletableEntity, ISanitizable
{
    public string NameFr { get; private set;} = null!;
    public string NameEn { get; private set;} = null!;
    public string DescriptionFr { get; private set;} = null!;
    public string DescriptionEn { get; private set;} = null!;
    public decimal Price { get; set; }
    public string Isbn { get; private set; } = null!;
    public string Author { get; private set; } = null!;
    public string Editor { get; private set; } = null!;
    public string CardImage { get; private set; } = null!;
    public int YearOfPublication { get; private set; }
    public int NumberOfPages { get; private set; }
    public string Slug { get; private set; } = null!;

    public void SetName(TranslatableString name)
    {
        NameFr = name.Fr;
        NameEn = name.En;
    }

    public void SetDescription(TranslatableString name)
    {
        DescriptionFr = name.Fr;
        DescriptionEn = name.En;
    }

    public void SetPrice(decimal price) => Price = price;
    public void SetIsbn(string isbn) => Isbn = isbn;
    public void SetAuthor(string author) => Author = author;
    public void SetEditor(string editor) => Editor = editor;
    public void SetCardImageUrl(string cardImageUrl) => CardImage = cardImageUrl;
    public void SetYearOfPublication(int yearOfPublication) => YearOfPublication = yearOfPublication;
    public void SetNumberOfPages(int numberOfPages) => NumberOfPages = numberOfPages;
    public void SetSlug(string value) => Slug = value;

    public void SanitizeForSaving()
    {
        Isbn = Isbn.Trim();
        Author = Author.Trim();
        Editor = Editor.Trim();
        NameFr = NameFr.Trim();
        NameEn = NameEn.Trim();
        DescriptionFr = DescriptionFr.Trim();
        DescriptionEn = DescriptionEn.Trim();
    }
}