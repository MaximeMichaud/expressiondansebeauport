using AutoMapper;
using Domain.Common;

namespace Application.Mapping;

public static class MappingProfileExtensions
{
    public static IMappingExpression<TSource, TDestination> IgnoreAuditableEntityProperties<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> map)
    {
        map.ForMember(nameof(AuditableAndSoftDeletableEntity.Created), config => config.Ignore());
        map.ForMember(nameof(AuditableAndSoftDeletableEntity.CreatedBy), config => config.Ignore());
        map.ForMember(nameof(AuditableAndSoftDeletableEntity.LastModified), config => config.Ignore());
        map.ForMember(nameof(AuditableAndSoftDeletableEntity.LastModifiedBy), config => config.Ignore());
        map.ForMember(nameof(AuditableAndSoftDeletableEntity.Deleted), config => config.Ignore());
        map.ForMember(nameof(AuditableAndSoftDeletableEntity.DeletedBy), config => config.Ignore());
        
        map.ForMember(nameof(AuditableEntity.Created), config => config.Ignore());
        map.ForMember(nameof(AuditableEntity.CreatedBy), config => config.Ignore());
        map.ForMember(nameof(AuditableEntity.LastModified), config => config.Ignore());
        map.ForMember(nameof(AuditableEntity.LastModifiedBy), config => config.Ignore());

        return map;
    }
}