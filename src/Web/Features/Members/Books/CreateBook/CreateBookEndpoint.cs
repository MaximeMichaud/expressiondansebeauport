using Application.Interfaces.Services.Books;
using Domain.Common;
using Domain.Entities.Books;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Members.Books.CreateBook;

public class CreateBookEndpoint: Endpoint<CreateBookRequest, SucceededOrNotResponse>
{
    private readonly IMapper _mapper;
    private readonly IBookCreationService _bookCreationService;

    public CreateBookEndpoint(IMapper mapper, IBookCreationService bookCreationService)
    {
        _mapper = mapper;
        _bookCreationService = bookCreationService;
    }

    public override void Configure()
    {
        AllowFileUploads();
        DontCatchExceptions();

        Post("books");
        Roles(Domain.Constants.User.Roles.MEMBER);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CreateBookRequest req, CancellationToken ct)
    {
        var book = _mapper.Map<Book>(req);
        await _bookCreationService.CreateBook(book, req.CardImage);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}