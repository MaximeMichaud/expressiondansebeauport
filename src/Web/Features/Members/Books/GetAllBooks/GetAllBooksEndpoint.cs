using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Members.Books.GetAllBooks;

public class GetAllBooksEndpoint: EndpointWithoutRequest<List<BookDto>>
{
    private readonly IMapper _mapper;
    private readonly IBookRepository _bookRepository;

    public GetAllBooksEndpoint(IMapper mapper, IBookRepository bookRepository)
    {
        _mapper = mapper;
        _bookRepository = bookRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Get("books");
        Roles(Domain.Constants.User.Roles.MEMBER);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var books = _bookRepository.GetAll();
        await Send.OkAsync(_mapper.Map<List<BookDto>>(books), ct);
    }
}