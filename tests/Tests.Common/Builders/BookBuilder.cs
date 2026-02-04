using Domain.Common;
using Domain.Entities.Books;
using Slugify;

namespace Tests.Common.Builders;

public class BookBuilder
{
    private const string NAME_FR = "Guide de la réglementation en copropriété";
    private const string NAME_EN = "Guide to condominium regulations";
    private const string DESCRIPTION_FR = "On vise, en COPROpriété, à assurer aux occupant un milieu de vie paisible.";

    private const string DESCRIPTION_EN =
        "We aim, in co-ownership, to provide occupants with a peaceful living environment.";

    private const string ISBN = "978-2-89689-559-5";
    private const string AUTHOR = "Christine Gagnon, Yves Papineau";
    private const string EDITOR = "Wilson & Lafleur";
    private const string CARD_IMAGE = "www.google.com";
    private const int YEAR_OF_PUBLICATION = 2023;
    private const int NUMBER_OF_PAGES = 346;

    private TranslatableString? Name { get; set; }
    private TranslatableString? Description { get; set; }
    private string? Isbn { get; set; }
    private string? Author { get; set; }
    private string? Editor { get; set; }
    private string? CardImage { get; set; }
    private int? YearOfPublication { get; set; }
    private int? NumberOfPages { get; set; }

    public BookBuilder WithName(string fr, string en)
    {
        Name = new TranslatableString(fr, en);
        return this;
    }

    public BookBuilder WithDescriptions(string fr, string en)
    {
        Description = new TranslatableString(fr, en);
        return this;
    }

    public BookBuilder WithIsbn(string? isbn)
    {
        Isbn = isbn;
        return this;
    }

    public BookBuilder WithAuthor(string author)
    {
        Author = author;
        return this;
    }

    public BookBuilder WithEditor(string editor)
    {
        Editor = editor;
        return this;
    }

    public BookBuilder WithCardImage(string cardImageUrl)
    {
        CardImage = cardImageUrl;
        return this;
    }

    public BookBuilder WithYearOfPublication(int yearOfPublication)
    {
        YearOfPublication = yearOfPublication;
        return this;
    }

    public BookBuilder WithNumberOfPages(int numberOfPages)
    {
        NumberOfPages = numberOfPages;
        return this;
    }

    public Book Build()
    {
        var book = new Book();
        var name = new TranslatableString(NAME_FR, NAME_EN);
        book.SetDescription(Description ?? new TranslatableString(DESCRIPTION_FR, DESCRIPTION_EN));
        book.SetIsbn(Isbn ?? ISBN);
        book.SetAuthor(Author ?? AUTHOR);
        book.SetEditor(Editor ?? EDITOR);
        book.SetCardImageUrl(CardImage ?? CARD_IMAGE);
        book.SetYearOfPublication(YearOfPublication ?? YEAR_OF_PUBLICATION);
        book.SetNumberOfPages(NumberOfPages ?? NUMBER_OF_PAGES);
        book.SetName(Name ?? name);
        var slug = $"{book.NameFr}-{DateTime.Now.Millisecond}";
        book.SetSlug(new SlugHelper().GenerateSlug(slug).Replace(".", ""));
        return book;
    }
}