using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities 
{ 
    [Table("SystemSettings")]
    public class SystemSetting : AuditableEntity
    {
        [Required, MaxLength(100)]
        public string Key { get; set; }

        [Required, MaxLength(2000)]
        public string Value { get; set; }

        [MaxLength(500)]
        public string Description { get; set; }

        public SettingType Type { get; set; } = SettingType.String;

        [MaxLength(100)]
        public string Category { get; set; } = "General";

        public bool IsPublic { get; set; } = true;
    }
}