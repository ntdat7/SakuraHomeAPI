using SakuraHomeAPI.Models.Enums;

namespace SakuraHomeAPI.DTOs.Products.Components
{
    /// <summary>
    /// Product attribute information
    /// </summary>
    public class ProductAttributeDto
    {
        public int AttributeId { get; set; }
        public string AttributeName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string? DisplayValue { get; set; }
        public AttributeType Type { get; set; }
        public string? Unit { get; set; }
    }
}