### Required Fields by Role

#### Customer (Role = 1)const customerRequiredFields = [
  'firstName',    // Họ (bắt buộc)
  'lastName',     // Tên (bắt buộc)
  'email',        // Email (bắt buộc, unique)
  'password',     // Mật khẩu (bắt buộc, min 1 ký tự - KHÔNG yêu cầu mạnh khi admin tạo)
  'role'          // Role = 1
];
#### Staff/Admin/SuperAdmin (Role >= 2)const staffRequiredFields = [
  'firstName',        // Họ (bắt buộc)
  'lastName',         // Tên (bắt buộc)
  'email',            // Email (bắt buộc, unique)
  'password',         // Mật khẩu (bắt buộc, min 1 ký tự - KHÔNG yêu cầu mạnh khi admin tạo)
  'role',             // Role >= 2
  'nationalIdCard',   // CCCD (bắt buộc, unique, max 20 ký tự)
  'hireDate',         // Ngày vào làm (bắt buộc, không được trong tương lai)
  'baseSalary'        // Lương cơ bản (bắt buộc, > 0, max 100,000,000)
];
### Client-side Validation Rules
const ValidationRules = {
  // Email validation
  email: {
    required: true,
    pattern: /^[^\s@]+@[^\s@]+\.[^\s@]+$/,
    maxLength: 255,
    message: 'Email không hợp lệ'
  },

  // Password validation - RELAXED for admin creating users
  password: {
    required: true,
    minLength: 1,     // Chỉ cần tối thiểu 1 ký tự
    maxLength: 100,   // Tối đa 100 ký tự
    message: 'Mật khẩu phải có từ 1-100 ký tự'
    // KHÔNG yêu cầu: chữ hoa, chữ thường, số, ký tự đặc biệt
  },

  // Name validation
  firstName: {
    required: true,
    maxLength: 100,
    message: 'Họ không được vượt quá 100 ký tự'
  },

  lastName: {
    required: true,
    maxLength: 100,
    message: 'Tên không được vượt quá 100 ký tự'
  },

  // Phone validation
  phoneNumber: {
    pattern: /^(\+84|0)[0-9]{9,10}$/,
    message: 'Số điện thoại không hợp lệ'
  },

  // CCCD validation
  nationalIdCard: {
    requiredForStaff: true,
    maxLength: 20,
    pattern: /^[0-9]{9,12}$/,
    message: 'CCCD phải là số từ 9-12 chữ số'
  },

  // Salary validation
  baseSalary: {
    requiredForStaff: true,
    min: 0,
    max: 100000000,
    message: 'Lương phải từ 0 đến 100,000,000'
  },

  // Hire date validation
  hireDate: {
    requiredForStaff: true,
    maxDate: new Date(),
    message: 'Ngày vào làm không được trong tương lai'
  }
};
### ⚠️ **LƯU Ý VỀ PASSWORD KHI ADMIN TẠO USER**

**Khi admin tạo user, password KHÔNG có validation mạnh:**
- ✅ **Chấp nhận**: Mật khẩu đơn giản như "0000", "1234", "abcd"
- ✅ **Không yêu cầu**: Chữ hoa, chữ thường, số, ký tự đặc biệt
- ✅ **Tối thiểu**: Chỉ cần 1 ký tự
- ✅ **Tối đa**: 100 ký tự

**Lý do:**
- Admin có toàn quyền quản lý users
- Có thể tạo password tạm thời đơn giản
- User có thể đổi password sau khi đăng nhập

**Ví dụ password hợp lệ khi admin tạo:**"password": "0000"     // ✅ OK
"password": "1234"     // ✅ OK
