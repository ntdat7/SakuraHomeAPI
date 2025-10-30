# 🚀 Quick Start Guide - Sakura Home API

## ⚡ TL;DR - Setup Nhanh cho Team

### **1. Clone & Setup**
```bash
git clone <repository-url>
cd SakuraHomeAPI
dotnet restore
```

### **2. Cấu hình Database**
Sửa `appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=SakuraHomeDB;Trusted_Connection=true;"
}
```

### **3. Tạo Database**
```bash
dotnet ef database update
```

### **4. Import Data (Chọn 1 trong 2)**

#### Option A: Full Data (Khuyến nghị)
```sql
-- Chạy trong SSMS
Scripts/SeedData/01-Complete-SeedData.sql
```

#### Option B: Minimal Data
```sql
-- Chạy trong SSMS  
Scripts/SeedData/02-Minimal-Setup.sql
```

### **5. Chạy Application**
```bash
dotnet run
# Hoặc F5 trong Visual Studio
```

### **6. Test API**
- Swagger: `https://localhost:8080`
- Test files: `Tests/` folder

---

## 🔧 Troubleshooting

**Lỗi database connection:**
- Kiểm tra SQL Server đã chạy
- Verify connection string

**Lỗi migration:**
```bash
dotnet ef database drop
dotnet ef database update
```

**Cần reset data:**
```sql
Scripts/SeedData/00-Clean-All-SeedData.sql  -- Xóa data
Scripts/SeedData/01-Complete-SeedData.sql   -- Import lại
```

---

## 📝 Lưu Ý Quan Trọng

- ✅ Source code KHÔNG có seed data
- ✅ Mỗi dev có database riêng
- ✅ Dùng SQL scripts để import data
- ❌ KHÔNG commit thay đổi trong SeedData.cs

**Có vấn đề? Check README.md đầy đủ!**