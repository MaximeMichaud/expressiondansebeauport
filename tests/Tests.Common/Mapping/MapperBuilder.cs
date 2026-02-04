using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tests.Common.Mapping;

public class MapperBuilder
{
    private readonly List<Type> _profiles = new();

    public MapperBuilder WithProfile<T>() where T : Profile
    {
        _profiles.Add(typeof(T));
        return this;
    }

    public IMapper Build()
    {
        var configExpression = new MapperConfigurationExpression();
        foreach (var type in _profiles)
            configExpression.AddProfile(type);

        var config = new MapperConfiguration(configExpression, NullLoggerFactory.Instance);
        return config.CreateMapper();
    }
}