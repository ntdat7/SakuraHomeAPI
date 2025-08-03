using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs.Common
{
    /// <summary>
    /// Pagination request parameters
    /// </summary>
    public class PaginationRequestDto
    {
        private int _page = 1;
        private int _pageSize = 12;

        [Range(1, int.MaxValue, ErrorMessage = "Page must be greater than 0")]
        public int Page 
        { 
            get => _page; 
            set => _page = Math.Max(1, value); 
        }

        [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100")]
        public int PageSize 
        { 
            get => _pageSize; 
            set => _pageSize = Math.Min(100, Math.Max(1, value)); 
        }
    }

    /// <summary>
    /// Pagination response information
    /// </summary>
    public class PaginationResponseDto
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
        public bool HasNext { get; set; }
        public bool HasPrevious { get; set; }
        public int? NextPage => HasNext ? CurrentPage + 1 : null;
        public int? PreviousPage => HasPrevious ? CurrentPage - 1 : null;
    }

    /// <summary>
    /// Generic paginated response
    /// </summary>
    public class PagedResponseDto<T>
    {
        public IEnumerable<T> Data { get; set; } = new List<T>();
        public PaginationResponseDto Pagination { get; set; } = new();
        public bool Success { get; set; } = true;
        public string? Message { get; set; }
    }
}