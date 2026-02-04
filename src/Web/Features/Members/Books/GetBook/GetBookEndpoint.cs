using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Members.Books.GetBook;

public class GetBookEndpoint : Endpoint<GetBookRequest, BookDto>
{
    private readonly IMapper _mapper;
    private readonly IBookRepository _bookRepository;

    public GetBookEndpoint(IMapper mapper, IBookRepository bookRepository)
    {
        _mapper = mapper;
        _bookRepository = bookRepository;
    }

    public override void Configure()
    {
        DontCatchExceptions();

        Get("books/{id}");
        Roles(Domain.Constants.User.Roles.MEMBER);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetBookRequest request, CancellationToken ct)
    {
        var book = _bookRepository.FindById(request.Id);
        await Send.OkAsync(_mapper.Map<BookDto>(book), ct);
    }
}