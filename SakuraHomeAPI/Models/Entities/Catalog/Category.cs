using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Products;
using SakuraHomeAPI.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities.Catalog
{
    /// <summary>
    /// Category entity for product categorization
    /// </summary>
    [Table("Categories")]
    public class Category : ContentEntity, IFeaturable
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [MaxLength(500)]
        public string ImageUrl { get; set; }

        [MaxLength(100)]
        public string Icon { get; set; } // CSS class or icon name

        [MaxLength(7)]
        public string Color { get; set; } // Hex color code

        // Hierarchy
        public int? ParentId { get; set; }
        public int Level { get; set; } = 0; // 0 = root

        public int ChildrenCount { get; set; } = 0;

        // Display settings
        public bool IsFeatured { get; set; } = false;
        public bool ShowInMenu { get; set; } = true;
        public bool ShowInHome { get; set; } = false;

        // Statistics
        public int ProductCount { get; set; } = 0;
        public int TotalProductCount { get; set; } = 0; // Including subcategories

        // Commission settings
        public decimal? CommissionRate { get; set; }

        // Navigation
        public virtual Category Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; } = new List<Category>();
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<CategoryAttribute> CategoryAttributes { get; set; } = new List<CategoryAttribute>();

        #region Computed Properties
        [NotMapped]
        public string FullPath
        {
            get
            {
                var path = new List<string> { Name };
                var current = Parent;
                while (current != null)
                {
                    path.Insert(0, current.Name);
                    current = current.Parent;
                }
                return string.Join(" > ", path);
            }
        }

        [NotMapped]
        public bool IsRoot => ParentId == null;

        [NotMapped]
        public bool HasChildren => Children?.Any() == true;

        [NotMapped]
        public bool HasProducts => Products?.Any() == true;
        #endregion

        #region Methods
        public IEnumerable<Category> GetAllDescendants()
        {
            var descendants = new List<Category>();
            foreach (var child in Children)
            {
                descendants.Add(child);
                descendants.AddRange(child.GetAllDescendants());
            }
            return descendants;
        }

        public IEnumerable<Category> GetBreadcrumb()
        {
            var breadcrumb = new List<Category>();
            var current = this;
            while (current != null)
            {
                breadcrumb.Insert(0, current);
                current = current.Parent;
            }
            return breadcrumb;
        }

        public void UpdateProductCounts()
        {
            ProductCount = Products.Count(p => p.IsActive && !p.IsDeleted);
            TotalProductCount = ProductCount;
            foreach (var child in Children)
            {
                child.UpdateProductCounts();
                TotalProductCount += child.TotalProductCount;
            }
        }

        public void UpdateChildrenCount()
        {
            ChildrenCount = Children.Count;
        }

        public void UpdateLevel()
        {
            Level = Parent?.Level + 1 ?? 0;
        }
        #endregion
    }
}
