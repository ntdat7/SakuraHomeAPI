// Models/Entities/Lookup/VietnamProvince.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;

namespace SakuraHomeAPI.Models.Entities.Lookup
{
    [Table("VietnamProvinces")]
    public class VietnamProvince : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public virtual ICollection<VietnamWard> Wards { get; set; } = new List<VietnamWard>();
    }
}