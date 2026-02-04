using Application.Exceptions.Books;
using Domain.Entities.Books;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Slugify;

namespace Infrastructure.Repositories.Books;

public class BookRepository : IBookRepository
{
    private readonly ISlugHelper _slugHelper;
    private readonly GarneauTemplateDbContext _context;

    public BookRepository(GarneauTemplateDbContext context, ISlugHelper slugHelper)
    {
        _context = context;
        _slugHelper = slugHelper;
    }

    public List<Book> GetAll()
    {
        return _context.Books.AsNoTracking().ToList();
    }

    public Book FindById(Guid id)
    {
        var book = _context.Books
            .AsNoTracking()
            .FirstOrDefault(x => x.Id == id);
        if (book == null)
            throw new BookNotFoundException($"Could not find book with id {id}.");
        return book;
    }

    public async Task CreateBook(Book book)
    {
        if (_context.Books.Any(x => x.Isbn.Trim() == book.Isbn.Trim()))
            throw new BookWithIsbnAlreadyExistsException($"A book with isbn {book.Isbn} already exists.");
        GenerateSlug(book);
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateBook(Book book)
    {
        if (_context.Books.Any(x => x.Isbn == book.Isbn.Trim() && x.Id != book.Id))
            throw new BookWithIsbnAlreadyExistsException($"Another book with isbn {book.Isbn} already exists.");

        if (string.IsNullOrWhiteSpace(book.Slug))
            GenerateSlug(book);

        _context.Books.Update(book);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteBookWithId(Guid id)
    {
        var book = _context.Books.FirstOrDefault(x => x.Id == id);
        if (book == null)
            throw new BookNotFoundException($"Could not find book with id {id}.");

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
    }

    private void GenerateSlug(Book book)
    {
        var slug = book.NameFr;
        var slugs = _context.Books.AsNoTracking().Where(x => x.Slug == slug).ToList();
        if (slugs.Count != 0)
            slug = $"{slug}-{slug.Length + 1}";
        book.SetSlug(_slugHelper.GenerateSlug(slug).Replace(".", ""));
    }
}