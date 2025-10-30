using System.ComponentModel.DataAnnotations;

namespace SakuraHomeAPI.DTOs
{
    public class ProvinceDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int WardCount { get; set; }
    }

    public class WardDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
    }

    public class AddressDataResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<T> Data { get; set; }
        public int Total { get; set; }
    }
}