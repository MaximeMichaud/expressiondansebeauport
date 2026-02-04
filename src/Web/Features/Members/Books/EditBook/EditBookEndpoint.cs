using Application.Interfaces.Services.Books;
using Domain.Common;
using Domain.Entities.Books;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IMapper = AutoMapper.IMapper;

namespace Web.Features.Members.Books.EditBook;

public class EditBookEndpoint: Endpoint<EditBookRequest, SucceededOrNotResponse>
{
    private readonly IMapper _mapper;
    private readonly IBookUpdateService _bookUpdateService;

    public EditBookEndpoint(IMapper mapper, IBookUpdateService bookUpdateService)
    {
        _mapper = mapper;
        _bookUpdateService = bookUpdateService;
    }

    public override void Configure()
    {
        AllowFileUploads();
        DontCatchExceptions();

        Put("books/{id}");
        Roles(Domain.Constants.User.Roles.MEMBER);
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(EditBookRequest req, CancellationToken ct)
    {
        var book = _mapper.Map<Book>(req);
        await _bookUpdateService.UpdateBook(book, req.CardImage);
        await Send.OkAsync(new SucceededOrNotResponse(true), ct);
    }
}