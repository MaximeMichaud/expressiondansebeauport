using Application.Extensions;
using Domain.Constants.User;
using Domain.Repositories;
using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Tests.Common.Builders;
using Tests.Common.Mapping;
using Web.Features.Members.Books.GetAllBooks;
using Web.Mapping.Profiles;

namespace Tests.Web.Features.Admins.Books.GetAllBooks;

public class GetAllBooksEndpointTests
{
    private readonly BookBuilder _bookBuilder;
    private readonly Mock<IBookRepository> _bookRepository;

    private readonly GetAllBooksEndpoint _endPoint;

    public GetAllBooksEndpointTests()
    {
        _bookBuilder = new BookBuilder();
        _bookRepository = new Mock<IBookRepository>();

        _endPoint = Factory.Create<GetAllBooksEndpoint>(
            new MapperBuilder().WithProfile<ResponseMappingProfile>().Build(),
            _bookRepository.Object
        );
    }

    [Fact]
    public void WhenConfigure_ThenConfigureVerbToBeGet()
    {
        // Act
        _endPoint.Configure();

        // Assert
        _endPoint.Definition.Verbs.ShouldContain(Http.GET.ToString());
    }

    [Fact]
    public void WhenConfigure_ThenConfigureRoute()
    {
        // Act
        _endPoint.Configure();

        // Assert
        _endPoint.Definition.Routes.ShouldContain("books");
    }

    [Fact]
    public void WhenConfigure_ThenConfigureAllowedRoles()
    {
        // Act
        _endPoint.Configure();

        // Assert
        _endPoint.Definition.AllowedRoles!.ShouldContain(Roles.MEMBER);
    }

    [Fact]
    public void WhenConfigure_ThenConfigureAuthSchemeToBeCookieAuthenticationScheme()
    {
        // Act
        _endPoint.Configure();

        // Assert
        _endPoint.Definition.AuthSchemeNames!.ShouldContain(JwtBearerDefaults.AuthenticationScheme);
    }

    [Fact]
    public async Task WhenHandleAsync_ThenReturnOkResult()
    {
        // Act
        await _endPoint.HandleAsync(CancellationToken.None);

        // Assert
        _endPoint.HttpContext.Response.StatusCode.ShouldBe(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task WhenHandleAsync_ThenDelegateGettingBookWithIdToBookRepository()
    {
        // Act
        await _endPoint.HandleAsync(CancellationToken.None);

        // Assert
        _bookRepository.Verify(x => x.GetAll());
    }

    [Fact]
    public async Task WhenHandleAsync_ThenReturnBookFoundByRepositoryAsBookDto()
    {
        // Arrange
        var book = _bookBuilder.Build();
        _bookRepository.Setup(x => x.GetAll()).Returns(book.IntoList());

        // Act
        await _endPoint.HandleAsync(CancellationToken.None);

        // Assert
        _endPoint.Response.ShouldContain(x => x.Id == book.Id);
    }
}