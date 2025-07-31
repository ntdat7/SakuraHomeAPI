using SakuraHomeAPI.Models.Base;
using SakuraHomeAPI.Models.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SakuraHomeAPI.Models.Entities
{
    [Table("SearchLogs")]
    public class SearchLog : LogEntity
    {
        [Required, MaxLength(500)]
        public string SearchTerm { get; set; }

        public int ResultCount { get; set; }

        [MaxLength(1000)]
        public string Filters { get; set; } // JSON string for search filters

        [MaxLength(100)]
        public string SortBy { get; set; }

        public bool HasResults { get; set; }
    }
}