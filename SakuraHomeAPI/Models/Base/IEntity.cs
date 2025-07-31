using SakuraHomeAPI.Models.Entities.Catalog;
using System;
using System.Collections.Generic;

namespace SakuraHomeAPI.Models.Base
{
    /// <summary>
    /// Base interface for all entities
    /// </summary>
    public interface IEntity
    {
        int Id { get; set; }
    }

    /// <summary>
    /// Interface for entities that need audit tracking
    /// </summary>
    public interface IAuditable
    {
        DateTime CreatedAt { get; set; }
        DateTime UpdatedAt { get; set; }
        int? CreatedBy { get; set; }
        int? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Interface for entities that support soft delete
    /// </summary>
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
        DateTime? DeletedAt { get; set; }
        int? DeletedBy { get; set; }
    }

    /// <summary>
    /// Interface for entities that can be activated/deactivated
    /// </summary>
    public interface IActivatable
    {
        bool IsActive { get; set; }
    }

    /// <summary>
    /// Interface for entities that support multi-language translations
    /// </summary>
    public interface ITranslatable
    {
        ICollection<Translation> Translations { get; set; }
    }

    /// <summary>
    /// Interface for entities that need URL-friendly slugs
    /// </summary>
    public interface ISlugifiable
    {
        string Slug { get; set; }
    }

    /// <summary>
    /// Interface for entities that need SEO metadata
    /// </summary>
    public interface ISeoFriendly
    {
        string MetaTitle { get; set; }
        string MetaDescription { get; set; }
        string MetaKeywords { get; set; }
    }

    /// <summary>
    /// Interface for entities that have display order
    /// </summary>
    public interface IOrderable
    {
        int DisplayOrder { get; set; }
    }

    /// <summary>
    /// Interface for entities that can be featured
    /// </summary>
    public interface IFeaturable
    {
        bool IsFeatured { get; set; }
    }
}