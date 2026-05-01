using Web.Features.Common;

namespace Tests.Web.Features.Common;

public class PaginateRequestTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenInvalidPageIndex_WhenGetNormalizedPageIndex_ThenReturnFirstPage(int pageIndex)
    {
        // Arrange
        var request = new PaginateRequest { PageIndex = pageIndex };

        // Act
        var normalizedPageIndex = request.NormalizedPageIndex;

        // Assert
        normalizedPageIndex.ShouldBe(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GivenInvalidPageSize_WhenGetNormalizedPageSize_ThenReturnDefaultPageSize(int pageSize)
    {
        // Arrange
        var request = new PaginateRequest { PageSize = pageSize };

        // Act
        var normalizedPageSize = request.NormalizedPageSize;

        // Assert
        normalizedPageSize.ShouldBe(10);
    }

    [Fact]
    public void GivenValidPagination_WhenGetNormalizedValues_ThenReturnOriginalValues()
    {
        // Arrange
        var request = new PaginateRequest { PageIndex = 3, PageSize = 25 };

        // Act & assert
        request.NormalizedPageIndex.ShouldBe(3);
        request.NormalizedPageSize.ShouldBe(25);
    }
}
