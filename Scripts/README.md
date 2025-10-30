# 🌸 Sakura Home API - Database Setup Guide

## 📋 Tổng Quan

Dự án này **KHÔNG** sử dụng seed data trong code nữa để giữ cho source code clean và dễ chia sẻ. Thay vào đó, chúng ta sử dụng SQL scripts để setup database.

## 🚀 Hướng Dẫn Setup Database

### **Bước 1: Clone Repository**
```bash
git clone <repository-url>
cd SakuraHomeAPI
```

### **Bước 2: Cấu Hình Connection String**
Cập nhật connection string trong `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=SakuraHomeDB;Trusted_Connection=true;TrustServerCertificate=true;"
  }
}
```

### **Bước 3: Chạy Migrations**
```bash
dotnet ef database update
```

### **Bước 4: Import Seed Data**
Có 2 cách để import data:

#### **Option A: Import toàn bộ (Khuyến nghị)**
```sql
-- Chạy script này trong SQL Server Management Studio
Scripts/SeedData/01-Complete-SeedData.sql
```

#### **Option B: Import từng phần**
```sql
-- Chạy theo thứ tự:
Scripts/SeedData/02-System-Settings.sql
Scripts/SeedData/03-Categories-Brands.sql
Scripts/SeedData/04-Products.sql
Scripts/SeedData/05-Additional-Data.sql
```

### **Bước 5: Verify Setup**
```bash
dotnet run
```
Truy cập: `https://localhost:8080` để kiểm tra API

## 📁 Cấu Trúc Scripts

```
Scripts/
├── SeedData/
│   ├── 00-Clean-All-SeedData.sql      # Xóa toàn bộ seed data
│   ├── 01-Complete-SeedData.sql       # Import toàn bộ seed data
│   ├── 02-System-Settings.sql         # Chỉ system settings
│   ├── 03-Categories-Brands.sql       # Categories và brands
│   ├── 04-Products.sql                # Sample products
│   └── 05-Additional-Data.sql         # Payment methods, etc.
└── README.md                          # File này
```

## 🔄 Tái Cấu Hình Database

Nếu cần reset database:

### **1. Xóa toàn bộ data:**
```sql
Scripts/SeedData/00-Clean-All-SeedData.sql
```

### **2. Import lại data:**
```sql
Scripts/SeedData/01-Complete-SeedData.sql
```

## 💡 Lợi Ích Của Cách Làm Này

### ✅ **Ưu điểm:**
- **Source code clean:** Không có seed data cồng kềnh trong code
- **Flexible:** Team có thể chọn import data gì
- **Fast startup:** Application start nhanh hơn
- **Easy sharing:** Dễ chia sẻ source với team
- **Production ready:** Production không cần seed data

### 🎯 **Use Cases:**
- **Development:** Import full seed data để test
- **Staging:** Import minimal data
- **Production:** Không import seed data

## 🛠️ Development Mode

Nếu muốn enable seed data trong development:

1. Mở file `SeedData.cs`
2. Uncomment dòng này trong `ApplicationDbContext.cs`:
```csharp
// DatabaseSeeder.EnableSeedDataForDevelopment(builder);
```

## ⚠️ Lưu Ý Quan Trọng

### **Cho Team Members:**
1. **KHÔNG** commit changes trong `SeedData.cs` 
2. **LUÔN** sử dụng SQL scripts để setup data
3. **KIỂM TRA** connection string trước khi chạy
4. **BACKUP** database trước khi chạy clean script

### **Khi Chia Sẻ Dự Án:**
1. Chỉ chia sẻ source code (không cần backup database)
2. Team member tự setup theo hướng dẫn này
3. Mỗi người có database riêng, clean và independent

## 🐛 Troubleshooting

### **Lỗi Migration:**
```bash
dotnet ef database drop --force
dotnet ef database update
```

### **Lỗi Connection:**
- Kiểm tra SQL Server running
- Verify connection string
- Check firewall settings

### **Lỗi Import Data:**
- Check foreign key constraints
- Verify table structure
- Run clean script trước

## 📞 Hỗ Trợ

Nếu gặp vấn đề:
1. Check logs trong `logs/` folder
2. Xem Swagger UI tại `https://localhost:8080`
3. Contact team lead

## 🔐 Production Deployment

Khi deploy production:
1. **KHÔNG** chạy seed data scripts
2. **KHÔNG** enable development seed data
3. Chỉ chạy migrations: `dotnet ef database update`
4. Import production data qua admin panel hoặc separate scripts

---

**Happy Coding! 🌸**