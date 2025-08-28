// Tools/AddressImporter.cs - Tạo class này trong project
using Microsoft.EntityFrameworkCore;
using SakuraHomeAPI.Data;
using SakuraHomeAPI.Models.Entities.Lookup;
using SakuraHomeAPI.Tools;
using System.Text;

namespace SakuraHomeAPI.Tools
{
    public class AddressImporter
    {
        private readonly ApplicationDbContext _context;

        public AddressImporter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task ImportFromCsvAsync(string csvFilePath)
        {
            Console.WriteLine("Bắt đầu import dữ liệu địa chỉ...");

            try
            {
                // Thử các encoding khác nhau cho tiếng Việt
                string[] lines = null;
                string detectedEncoding = "";

                var encodingsToTry = new[]
                {
                    Encoding.UTF8,
                    Encoding.GetEncoding("UTF-8"),
                    Encoding.GetEncoding("windows-1252"),
                    Encoding.GetEncoding("iso-8859-1"),
                    Encoding.Default,
                    Encoding.GetEncoding("cp1252")
                };

                // Auto-detect encoding bằng cách thử từng loại
                foreach (var encoding in encodingsToTry)
                {
                    try
                    {
                        var testLines = File.ReadAllLines(csvFilePath, encoding);
                        if (testLines.Length > 0)
                        {
                            // Kiểm tra có ký tự tiếng Việt đúng không
                            var sampleText = string.Join(" ", testLines.Take(10));
                            if (ContainsProperVietnamese(sampleText))
                            {
                                lines = testLines;
                                detectedEncoding = encoding.EncodingName;
                                Console.WriteLine($"✓ Đã detect encoding: {detectedEncoding}");
                                break;
                            }
                        }
                    }
                    catch { continue; }
                }

                // Fallback: đọc as bytes và convert
                if (lines == null)
                {
                    Console.WriteLine("Thử phương pháp auto-detect encoding...");
                    lines = ReadCsvWithAutoDetectEncoding(csvFilePath);
                    detectedEncoding = "Auto-detected";
                }

                if (lines == null || lines.Length == 0)
                {
                    Console.WriteLine("❌ Không thể đọc file CSV hoặc file rỗng!");
                    return;
                }

                // Skip header line nếu có
                var dataLines = lines.Skip(1).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
                Console.WriteLine($"✓ Tìm thấy {dataLines.Count} dòng dữ liệu (encoding: {detectedEncoding})");

                // Dictionary để group theo tỉnh
                var provincesData = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

                // Parse CSV data với xử lý tiếng Việt
                int lineNumber = 2; // Start from 2 (after header)
                foreach (var line in dataLines)
                {
                    try
                    {
                        var (provinceName, wardName) = ParseCsvLine(line);

                        if (string.IsNullOrEmpty(provinceName) || string.IsNullOrEmpty(wardName))
                        {
                            Console.WriteLine($"⚠ Dòng {lineNumber}: Thiếu dữ liệu - '{line}'");
                            lineNumber++;
                            continue;
                        }

                        // Normalize Vietnamese text
                        provinceName = NormalizeVietnameseText(provinceName);
                        wardName = NormalizeVietnameseText(wardName);

                        if (!provincesData.ContainsKey(provinceName))
                        {
                            provincesData[provinceName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        }

                        if (provincesData[provinceName].Add(wardName))
                        {
                            // Chỉ log khi thêm mới thành công (không duplicate)
                            if (lineNumber <= 5) // Log vài dòng đầu để check
                            {
                                Console.WriteLine($"✓ Dòng {lineNumber}: {provinceName} -> {wardName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Lỗi parse dòng {lineNumber}: {ex.Message} - '{line}'");
                    }

                    lineNumber++;
                }

                Console.WriteLine($"Tìm thấy {provincesData.Count} tỉnh thành");

                // Clear existing data
                var existingWards = await _context.VietnamWards.ToListAsync();
                var existingProvinces = await _context.VietnamProvinces.ToListAsync();

                if (existingWards.Any() || existingProvinces.Any())
                {
                    Console.WriteLine("Xóa dữ liệu cũ...");
                    _context.VietnamWards.RemoveRange(existingWards);
                    _context.VietnamProvinces.RemoveRange(existingProvinces);
                    await _context.SaveChangesAsync();
                }

                // Import provinces
                var provinces = new List<VietnamProvince>();
                int displayOrder = 1;

                foreach (var kvp in provincesData.OrderBy(x => x.Key))
                {
                    var province = new VietnamProvince
                    {
                        Name = kvp.Key,
                        DisplayOrder = displayOrder++
                    };
                    provinces.Add(province);
                }

                _context.VietnamProvinces.AddRange(provinces);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Đã import {provinces.Count} tỉnh thành");

                // Import wards
                var allWards = new List<VietnamWard>();

                foreach (var province in provinces)
                {
                    if (provincesData.ContainsKey(province.Name))
                    {
                        var wards = provincesData[province.Name].Select(wardName => new VietnamWard
                        {
                            Name = wardName,
                            ProvinceId = province.Id
                        }).ToList();

                        allWards.AddRange(wards);
                    }
                }

                _context.VietnamWards.AddRange(allWards);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Đã import {allWards.Count} phường/xã");
                Console.WriteLine("Import hoàn thành!");

                // In thống kê
                var stats = await _context.VietnamProvinces
                    .Include(p => p.Wards)
                    .Select(p => new { p.Name, WardCount = p.Wards.Count() })
                    .OrderByDescending(x => x.WardCount)
                    .Take(5)
                    .ToListAsync();

                Console.WriteLine("\n5 tỉnh có nhiều phường/xã nhất:");
                foreach (var stat in stats)
                {
                    Console.WriteLine($"- {stat.Name}: {stat.WardCount} phường/xã");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi khi import: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        /// <summary>
        /// Kiểm tra text có chứa ký tự tiếng Việt đúng không
        /// </summary>
        private bool ContainsProperVietnamese(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            // Các ký tự đặc trưng tiếng Việt
            var vietnameseChars = new[] { 'ă', 'â', 'đ', 'ê', 'ô', 'ơ', 'ư', 'á', 'à', 'ả', 'ã', 'ạ',
                'ắ', 'ằ', 'ẳ', 'ẵ', 'ặ', 'ấ', 'ầ', 'ẩ', 'ẫ', 'ậ', 'é', 'è', 'ẻ', 'ẽ', 'ẹ', 'ế', 'ề',
                'ể', 'ễ', 'ệ', 'í', 'ì', 'ỉ', 'ĩ', 'ị', 'ó', 'ò', 'ỏ', 'õ', 'ọ', 'ố', 'ồ', 'ổ', 'ỗ',
                'ộ', 'ớ', 'ờ', 'ở', 'ỡ', 'ợ', 'ú', 'ù', 'ủ', 'ũ', 'ụ', 'ứ', 'ừ', 'ử', 'ữ', 'ự', 'ý',
                'ỳ', 'ỷ', 'ỹ', 'ỵ' };

            var lowerText = text.ToLower();

            // Kiểm tra có ít nhất 2 ký tự tiếng Việt và không có ký tự lạ
            var vietnameseCount = lowerText.Count(c => vietnameseChars.Contains(c));
            var hasWeirdChars = lowerText.Any(c => c == '?' && lowerText.Count(x => x == '?') > text.Length * 0.1);

            return vietnameseCount >= 2 && !hasWeirdChars &&
                   (lowerText.Contains("thành phố") || lowerText.Contains("tỉnh") || lowerText.Contains("phường") ||
                    lowerText.Contains("xã") || lowerText.Contains("huyện") || vietnameseCount > 5);
        }

        /// <summary>
        /// Auto-detect encoding và đọc CSV
        /// </summary>
        private string[] ReadCsvWithAutoDetectEncoding(string filePath)
        {
            try
            {
                // Đọc bytes đầu tiên để detect BOM
                var bytes = File.ReadAllBytes(filePath);

                // Check UTF-8 BOM
                if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                {
                    Console.WriteLine("✓ Detected UTF-8 with BOM");
                    return File.ReadAllLines(filePath, new UTF8Encoding(true));
                }

                // Try UTF-8 without BOM
                try
                {
                    var utf8Text = Encoding.UTF8.GetString(bytes);
                    if (ContainsProperVietnamese(utf8Text))
                    {
                        Console.WriteLine("✓ Detected UTF-8 without BOM");
                        return utf8Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                catch { }

                // Try Windows-1252 (commonly used for Vietnamese)
                try
                {
                    var win1252Text = Encoding.GetEncoding("windows-1252").GetString(bytes);
                    if (ContainsProperVietnamese(win1252Text))
                    {
                        Console.WriteLine("✓ Detected Windows-1252");
                        return win1252Text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    }
                }
                catch { }

                // Fallback to default encoding
                Console.WriteLine("⚠ Using default encoding as fallback");
                return File.ReadAllLines(filePath, Encoding.Default);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in auto-detect: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Parse CSV line xử lý trường hợp có dấu phẩy trong tên
        /// </summary>
        private (string provinceName, string wardName) ParseCsvLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return (null, null);

            // Xử lý CSV với quoted fields
            var fields = new List<string>();
            var currentField = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                var c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        // Escaped quote
                        currentField.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        // Toggle quote state
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    // Field separator
                    fields.Add(currentField.ToString().Trim());
                    currentField.Clear();
                }
                else
                {
                    currentField.Append(c);
                }
            }

            // Add last field
            fields.Add(currentField.ToString().Trim());

            if (fields.Count < 2)
            {
                // Fallback to simple split
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    return (parts[0].Trim().Trim('"'), parts[1].Trim().Trim('"'));
                }
                return (null, null);
            }

            return (fields[0].Trim('"'), fields[1].Trim('"'));
        }

        /// <summary>
        /// Normalize Vietnamese text
        /// </summary>
        private string NormalizeVietnameseText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Trim và remove extra spaces
            text = System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"\s+", " ");

            // Remove leading/trailing quotes
            text = text.Trim('"', '\'');

            // Fix common Vietnamese text issues
            text = text.Replace("  ", " ")
                      .Replace(" ,", ",")
                      .Replace(", ", ", ")
                      .Trim();

            return text;
        }
    }
}