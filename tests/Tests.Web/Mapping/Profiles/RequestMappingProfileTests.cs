using AutoMapper;
using Domain.Common;
using Tests.Common.Mapping;
using Tests.Web.TestCollections;
using Web.Dtos;
using Web.Mapping.Profiles;

namespace Tests.Web.Mapping.Profiles;

[Collection(WebTestCollection.NAME)]
public class RequestMappingProfileTests
{
    private const string NameFr = "Guide de la réglementation en copropriété";
    private const string NameEn = "Guide to condominium regulations";

    private readonly IMapper _mapper = new MapperBuilder()
        .WithProfile<RequestMappingProfile>()
        .Build();

    [Fact]
    public void GivenTranslatableStringDto_WhenMap_ThenReturnTranslatableStringMappedCorrectly()
    {
        // Arrange
        var translatableStringDto = new TranslatableStringDto
        {
            Fr = NameFr,
            En = NameEn
        };

        // Act
        var translatableString = _mapper.Map<TranslatableString>(translatableStringDto);

        // Assert
        translatableString.Fr.ShouldBe(NameFr);
        translatableString.En.ShouldBe(NameEn);
    }

    [Fact]
    public void GivenTranslatableString_WhenMap_ThenReturnTranslatableStringDtoMappedCorrectly()
    {
        // Arrange
        var translatableString = new TranslatableString
        {
            Fr = NameFr,
            En = NameEn
        };

        // Act
        var translatableStringDto = _mapper.Map<TranslatableStringDto>(translatableString);

        // Assert
        translatableStringDto.Fr.ShouldBe(NameFr);
        translatableStringDto.En.ShouldBe(NameEn);
    }
}
