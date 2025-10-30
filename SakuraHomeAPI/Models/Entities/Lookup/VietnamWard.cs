// Models/Entities/Lookup/VietnamWard.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SakuraHomeAPI.Models.Base;

namespace SakuraHomeAPI.Models.Entities.Lookup
{
    [Table("VietnamWards")]
    public class VietnamWard : BaseEntity
    {
        [Required, MaxLength(100)]
        public string Name { get; set; }

        public int ProvinceId { get; set; }

        public virtual VietnamProvince Province { get; set; }
    }
}