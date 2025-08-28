using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.DTOs;

namespace SakuraHomeAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AddressController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<AddressController> _logger;

        public AddressController(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<AddressController> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tất cả tỉnh thành Việt Nam
        /// </summary>
        /// <returns>Danh sách các tỉnh thành</returns>
        [HttpGet("provinces")]
        public async Task<ActionResult<AddressDataResponse<ProvinceDto>>> GetProvinces()
        {
            try
            {
                const string cacheKey = "vietnam_provinces";

                // Kiểm tra cache trước
                if (!_cache.TryGetValue(cacheKey, out List<ProvinceDto> provinces))
                {
                    provinces = await _context.VietnamProvinces
                        .Select(p => new ProvinceDto
                        {
                            Id = p.Id,
                            Name = p.Name,
                            WardCount = p.Wards.Count()
                        })
                        .OrderBy(p => p.Name)
                        .ToListAsync();

                    // Cache 24 giờ (dữ liệu ít thay đổi)
                    _cache.Set(cacheKey, provinces, TimeSpan.FromHours(24));
                    _logger.LogInformation("Loaded {Count} provinces from database and cached", provinces.Count);
                }
                else
                {
                    _logger.LogInformation("Loaded {Count} provinces from cache", provinces.Count);
                }

                return Ok(new AddressDataResponse<ProvinceDto>
                {
                    Success = true,
                    Message = "Lấy danh sách tỉnh thành thành công",
                    Data = provinces,
                    Total = provinces.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting provinces");
                return StatusCode(500, new AddressDataResponse<ProvinceDto>
                {
                    Success = false,
                    Message = "Có lỗi khi lấy danh sách tỉnh thành",
                    Data = new List<ProvinceDto>(),
                    Total = 0
                });
            }
        }

        /// <summary>
        /// Lấy danh sách phường/xã theo tỉnh
        /// </summary>
        /// <param name="provinceId">ID của tỉnh</param>
        /// <returns>Danh sách các phường/xã</returns>
        [HttpGet("provinces/{provinceId}/wards")]
        public async Task<ActionResult<AddressDataResponse<WardDto>>> GetWardsByProvince(int provinceId)
        {
            try
            {
                if (provinceId <= 0)
                {
                    return BadRequest(new AddressDataResponse<WardDto>
                    {
                        Success = false,
                        Message = "ID tỉnh không hợp lệ",
                        Data = new List<WardDto>(),
                        Total = 0
                    });
                }

                string cacheKey = $"vietnam_wards_province_{provinceId}";

                // Kiểm tra cache trước
                if (!_cache.TryGetValue(cacheKey, out List<WardDto> wards))
                {
                    // Kiểm tra tỉnh có tồn tại không
                    var province = await _context.VietnamProvinces
                        .FirstOrDefaultAsync(p => p.Id == provinceId);

                    if (province == null)
                    {
                        return NotFound(new AddressDataResponse<WardDto>
                        {
                            Success = false,
                            Message = "Không tìm thấy tỉnh thành",
                            Data = new List<WardDto>(),
                            Total = 0
                        });
                    }

                    wards = await _context.VietnamWards
                        .Where(w => w.ProvinceId == provinceId)
                        .Select(w => new WardDto
                        {
                            Id = w.Id,
                            Name = w.Name,
                            ProvinceId = w.ProvinceId,
                            ProvinceName = w.Province.Name
                        })
                        .OrderBy(w => w.Name)
                        .ToListAsync();

                    // Cache 24 giờ
                    _cache.Set(cacheKey, wards, TimeSpan.FromHours(24));
                    _logger.LogInformation("Loaded {Count} wards for province {ProvinceId} from database and cached",
                        wards.Count, provinceId);
                }
                else
                {
                    _logger.LogInformation("Loaded {Count} wards for province {ProvinceId} from cache",
                        wards.Count, provinceId);
                }

                return Ok(new AddressDataResponse<WardDto>
                {
                    Success = true,
                    Message = $"Lấy danh sách phường/xã thành công",
                    Data = wards,
                    Total = wards.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting wards for province {ProvinceId}", provinceId);
                return StatusCode(500, new AddressDataResponse<WardDto>
                {
                    Success = false,
                    Message = "Có lỗi khi lấy danh sách phường/xã",
                    Data = new List<WardDto>(),
                    Total = 0
                });
            }
        }

        /// <summary>
        /// Tìm kiếm địa chỉ theo tên
        /// </summary>
        /// <param name="query">Từ khóa tìm kiếm</param>
        /// <param name="limit">Giới hạn kết quả (mặc định 20)</param>
        /// <returns>Danh sách kết quả tìm kiếm</returns>
        [HttpGet("search")]
        public async Task<ActionResult> SearchAddress(
            [FromQuery] string query,
            [FromQuery] int limit = 20)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                {
                    return BadRequest(new
                    {
                        Success = false,
                        Message = "Từ khóa tìm kiếm phải có ít nhất 2 ký tự",
                        Data = new { Provinces = new List<ProvinceDto>(), Wards = new List<WardDto>() }
                    });
                }

                var normalizedQuery = query.Trim().ToLower();

                // Tìm provinces
                var provinces = await _context.VietnamProvinces
                    .Where(p => p.Name.ToLower().Contains(normalizedQuery))
                    .Select(p => new ProvinceDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        WardCount = p.Wards.Count()
                    })
                    .Take(Math.Min(limit / 2, 10))
                    .ToListAsync();

                // Tìm wards
                var wards = await _context.VietnamWards
                    .Include(w => w.Province)
                    .Where(w => w.Name.ToLower().Contains(normalizedQuery))
                    .Select(w => new WardDto
                    {
                        Id = w.Id,
                        Name = w.Name,
                        ProvinceId = w.ProvinceId,
                        ProvinceName = w.Province.Name
                    })
                    .Take(limit)
                    .ToListAsync();

                return Ok(new
                {
                    Success = true,
                    Message = $"Tìm thấy {provinces.Count} tỉnh thành và {wards.Count} phường/xã",
                    Data = new
                    {
                        Provinces = provinces,
                        Wards = wards
                    },
                    Total = provinces.Count + wards.Count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching addresses with query: {Query}", query);
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Có lỗi khi tìm kiếm địa chỉ",
                    Data = new { Provinces = new List<ProvinceDto>(), Wards = new List<WardDto>() }
                });
            }
        }

        /// <summary>
        /// Lấy thống kê về dữ liệu địa chỉ
        /// </summary>
        /// <returns>Thông tin thống kê</returns>
        [HttpGet("stats")]
        public async Task<ActionResult> GetAddressStats()
        {
            try
            {
                const string cacheKey = "vietnam_address_stats";

                if (!_cache.TryGetValue(cacheKey, out object stats))
                {
                    var totalProvinces = await _context.VietnamProvinces.CountAsync();
                    var totalWards = await _context.VietnamWards.CountAsync();

                    var topProvinces = await _context.VietnamProvinces
                        .Include(p => p.Wards)
                        .Select(p => new { p.Name, WardCount = p.Wards.Count() })
                        .OrderByDescending(x => x.WardCount)
                        .Take(5)
                        .ToListAsync();

                    stats = new
                    {
                        TotalProvinces = totalProvinces,
                        TotalWards = totalWards,
                        TopProvincesWithMostWards = topProvinces,
                        LastUpdated = DateTime.UtcNow
                    };

                    // Cache 6 giờ
                    _cache.Set(cacheKey, stats, TimeSpan.FromHours(6));
                }

                return Ok(new
                {
                    Success = true,
                    Message = "Thống kê dữ liệu địa chỉ",
                    Data = stats
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting address statistics");
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "Có lỗi khi lấy thống kê"
                });
            }
        }
    }
}