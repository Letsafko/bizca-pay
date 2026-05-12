using System;
using Bizca.Sdk.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bizca.Users.Infrastructure.Context.Extensions;

internal static class EntityTypeConfigurationExtensions
{
	private const string VersionColumnName = "version";
	private const string CreatedDateColumnName = "createdOn";
	private const string LastModifiedDateColumnName = "lastModified";

	internal static EntityTypeBuilder<T> AddVersionAsShadowProperty<T>(this EntityTypeBuilder<T> builder) where T : class, IVersionedEntity
	{
		builder.Property(static e => e.Version).IsRowVersion().HasColumnName(VersionColumnName);
		return builder;
	}

	internal static EntityTypeBuilder<T> AddAuditingProperties<T>(this EntityTypeBuilder<T> builder) where T : Entity
	{
		builder.Property(static e => e.CreatedDatetime).HasColumnName(CreatedDateColumnName);
		builder.Property(static e => e.LastModifiedDatetime).HasColumnName(LastModifiedDateColumnName);
		return builder;
	}

	internal static void IgnoreAuditingProperties<T>(this EntityTypeBuilder<T> builder, Action<EntityTypeBuilder<T>>? configureBuilder = null)
		where T : Entity
	{
		builder.Ignore(static x => x.DomainEvents);
		configureBuilder?.Invoke(builder);
	}
}