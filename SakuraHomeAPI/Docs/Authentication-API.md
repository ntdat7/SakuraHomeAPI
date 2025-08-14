# ?? Authentication API Documentation

## T?ng quan

API Authentication c?a SakuraHome cung c?p ??y ?? các ch?c n?ng xác th?c ng??i dùng, bao g?m ??ng ký, ??ng nh?p, qu?n lý token, và b?o m?t tài kho?n.

**Base URL:** `https://localhost:7240/api/auth`

## ?? M?c l?c

- [??ng ký tài kho?n](#??ng-ký-tài-kho?n)
- [??ng nh?p](#??ng-nh?p)
- [Làm m?i token](#làm-m?i-token)
- [??ng xu?t](#??ng-xu?t)
- [Thông tin ng??i dùng hi?n t?i](#thông-tin-ng??i-dùng-hi?n-t?i)
- [??i m?t kh?u](#??i-m?t-kh?u)
- [Quên m?t kh?u](#quên-m?t-kh?u)
- [??t l?i m?t kh?u](#??t-l?i-m?t-kh?u)
- [Xác th?c email](#xác-th?c-email)
- [Ki?m tra tr?ng thái xác th?c](#ki?m-tra-tr?ng-thái-xác-th?c)
- [Thu h?i token](#thu-h?i-token)

---

## ?? ??ng ký tài kho?n

### POST `/register`

T?o tài kho?n ng??i dùng m?i.

#### Request Body

```json
{
  "email": "user@example.com",
  "password": "MySecurePass123!",
  "confirmPassword": "MySecurePass123!",
  "firstName": "Nguy?n",
  "lastName": "V?n A",
  "phoneNumber": "+84901234567"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "??ng ký thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-refresh-token-here",
    "expiresAt": "2024-01-01T13:00:00Z",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "firstName": "Nguy?n",
      "lastName": "V?n A",
      "fullName": "Nguy?n V?n A",
      "phoneNumber": "+84901234567",
      "role": "Customer",
      "tier": "Bronze",
      "status": "Active",
      "emailVerified": false,
      "phoneVerified": false,
      "avatar": null,
      "lastLoginAt": null
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Response Error (400)

```json
{
  "success": false,
  "message": "D? li?u không h?p l?",
  "errors": [
    "Email ?ã t?n t?i",
    "M?t kh?u ph?i có ít nh?t 8 ký t?"
  ],
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Frontend Integration

```javascript
// React/JavaScript example
const register = async (userData) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData)
    });
    
    const result = await response.json();
    
    if (result.success) {
      // L?u token vào localStorage ho?c cookie
      localStorage.setItem('accessToken', result.data.token);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      localStorage.setItem('user', JSON.stringify(result.data.user));
      
      // Redirect ho?c update UI
      window.location.href = '/dashboard';
    } else {
      // Hi?n th? l?i
      showErrors(result.errors || [result.message]);
    }
  } catch (error) {
    console.error('Registration error:', error);
    showError('Có l?i x?y ra trong quá trình ??ng ký');
  }
};
```

---

## ?? ??ng nh?p

### POST `/login`

Xác th?c ng??i dùng và tr? v? access token.

#### Request Body

```json
{
  "email": "user@example.com",
  "password": "MySecurePass123!",
  "rememberMe": true
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "??ng nh?p thành công",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "base64-refresh-token-here",
    "expiresAt": "2024-01-01T13:00:00Z",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "firstName": "Nguy?n",
      "lastName": "V?n A",
      "fullName": "Nguy?n V?n A",
      "phoneNumber": "+84901234567",
      "role": "Customer",
      "tier": "Silver",
      "status": "Active",
      "emailVerified": true,
      "phoneVerified": false,
      "avatar": "https://example.com/avatar.jpg",
      "lastLoginAt": "2024-01-01T11:30:00Z"
    }
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Response Error (401)

```json
{
  "success": false,
  "message": "Email ho?c m?t kh?u không ?úng",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Frontend Integration

```javascript
const login = async (credentials) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(credentials)
    });
    
    const result = await response.json();
    
    if (result.success) {
      // L?u tokens
      localStorage.setItem('accessToken', result.data.token);
      localStorage.setItem('refreshToken', result.data.refreshToken);
      localStorage.setItem('user', JSON.stringify(result.data.user));
      
      // Set axios default header
      axios.defaults.headers.common['Authorization'] = `Bearer ${result.data.token}`;
      
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Login error:', error);
    throw error;
  }
};
```

---

## ?? Làm m?i token

### POST `/refresh-token`

Làm m?i access token khi h?t h?n.

#### Request Body

```json
{
  "refreshToken": "base64-refresh-token-here"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Token ?ã ???c làm m?i",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "new-base64-refresh-token-here",
    "expiresAt": "2024-01-01T14:00:00Z",
    "user": {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "email": "user@example.com",
      "firstName": "Nguy?n",
      "lastName": "V?n A",
      "fullName": "Nguy?n V?n A"
    }
  },
  "timestamp": "2024-01-01T13:00:00Z"
}
```

#### Frontend Integration

```javascript
// Axios interceptor ?? t? ??ng refresh token
axios.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;
      
      try {
        const refreshToken = localStorage.getItem('refreshToken');
        const response = await axios.post('/api/auth/refresh-token', {
          refreshToken: refreshToken
        });
        
        const newToken = response.data.data.token;
        localStorage.setItem('accessToken', newToken);
        localStorage.setItem('refreshToken', response.data.data.refreshToken);
        
        // Retry original request
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return axios(originalRequest);
      } catch (refreshError) {
        // Redirect to login
        localStorage.clear();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      }
    }
    
    return Promise.reject(error);
  }
);
```

---

## ?? ??ng xu?t

### POST `/logout`

??ng xu?t ng??i dùng và vô hi?u hóa refresh token.

**Yêu c?u:** Bearer Token

#### Request Body (Optional)

```json
{
  "refreshToken": "base64-refresh-token-here"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "??ng xu?t thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Frontend Integration

```javascript
const logout = async () => {
  try {
    const refreshToken = localStorage.getItem('refreshToken');
    
    await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify({ refreshToken })
    });
    
    // Clear storage
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    
    // Remove axios header
    delete axios.defaults.headers.common['Authorization'];
    
    // Redirect to login
    window.location.href = '/login';
  } catch (error) {
    console.error('Logout error:', error);
    // Clear storage anyway
    localStorage.clear();
    window.location.href = '/login';
  }
};
```

---

## ?? Thông tin ng??i dùng hi?n t?i

### GET `/me`

L?y thông tin chi ti?t c?a ng??i dùng hi?n t?i.

**Yêu c?u:** Bearer Token

#### Response Success (200)

```json
{
  "success": true,
  "message": "L?y thông tin thành công",
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "email": "user@example.com",
    "firstName": "Nguy?n",
    "lastName": "V?n A",
    "fullName": "Nguy?n V?n A",
    "phoneNumber": "+84901234567",
    "dateOfBirth": "1990-01-01",
    "gender": "Male",
    "role": "Customer",
    "tier": "Silver",
    "status": "Active",
    "emailVerified": true,
    "phoneVerified": false,
    "avatar": "https://example.com/avatar.jpg",
    "bio": "Tôi yêu thích các s?n ph?m Nh?t B?n",
    "lastLoginAt": "2024-01-01T11:30:00Z",
    "createdAt": "2023-06-15T10:00:00Z",
    "updatedAt": "2024-01-01T11:30:00Z"
  },
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Frontend Integration

```javascript
const getCurrentUser = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/me`, {
      headers: {
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      }
    });
    
    const result = await response.json();
    
    if (result.success) {
      return result.data;
    } else {
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Get current user error:', error);
    throw error;
  }
};
```

---

## ?? ??i m?t kh?u

### POST `/change-password`

Thay ??i m?t kh?u c?a ng??i dùng ?ã ??ng nh?p.

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePass456!",
  "confirmPassword": "NewSecurePass456!"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "??i m?t kh?u thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Response Error (400)

```json
{
  "success": false,
  "message": "M?t kh?u hi?n t?i không ?úng",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Frontend Integration

```javascript
const changePassword = async (passwordData) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/change-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${localStorage.getItem('accessToken')}`
      },
      body: JSON.stringify(passwordData)
    });
    
    const result = await response.json();
    
    if (result.success) {
      showSuccess('??i m?t kh?u thành công');
      return true;
    } else {
      showError(result.message);
      return false;
    }
  } catch (error) {
    console.error('Change password error:', error);
    showError('Có l?i x?y ra khi ??i m?t kh?u');
    return false;
  }
};
```

---

## ?? Quên m?t kh?u

### POST `/forgot-password`

G?i email ??t l?i m?t kh?u.

#### Request Body

```json
{
  "email": "user@example.com"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Email ??t l?i m?t kh?u ?ã ???c g?i",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Frontend Integration

```javascript
const forgotPassword = async (email) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/forgot-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email })
    });
    
    const result = await response.json();
    
    if (result.success) {
      showSuccess('Email ??t l?i m?t kh?u ?ã ???c g?i. Vui lòng ki?m tra h?p th?.');
      return true;
    } else {
      showError(result.message);
      return false;
    }
  } catch (error) {
    console.error('Forgot password error:', error);
    showError('Có l?i x?y ra khi g?i email');
    return false;
  }
};
```

---

## ?? ??t l?i m?t kh?u

### POST `/reset-password`

??t l?i m?t kh?u b?ng token t? email.

#### Request Body

```json
{
  "token": "reset-token-from-email",
  "email": "user@example.com",
  "newPassword": "NewSecurePass456!",
  "confirmPassword": "NewSecurePass456!"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "??t l?i m?t kh?u thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Frontend Integration

```javascript
const resetPassword = async (resetData) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/reset-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(resetData)
    });
    
    const result = await response.json();
    
    if (result.success) {
      showSuccess('??t l?i m?t kh?u thành công. Vui lòng ??ng nh?p.');
      window.location.href = '/login';
      return true;
    } else {
      showError(result.message);
      return false;
    }
  } catch (error) {
    console.error('Reset password error:', error);
    showError('Có l?i x?y ra khi ??t l?i m?t kh?u');
    return false;
  }
};
```

---

## ? Xác th?c email

### POST `/verify-email`

Xác th?c email b?ng token t? email.

#### Request Body

```json
{
  "token": "email-verification-token",
  "email": "user@example.com"
}
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Email ?ã ???c xác th?c thành công",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

### POST `/resend-email-verification`

G?i l?i email xác th?c.

#### Request Body

```json
"user@example.com"
```

#### Response Success (200)

```json
{
  "success": true,
  "message": "Email xác th?c ?ã ???c g?i l?i",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

---

## ?? Ki?m tra tr?ng thái xác th?c

### GET `/check`

Ki?m tra xem token hi?n t?i có h?p l? hay không.

**Yêu c?u:** Bearer Token

#### Response Success (200)

```json
{
  "success": true,
  "message": "Token h?p l?",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

#### Response Error (401)

```json
{
  "success": false,
  "message": "Token không h?p l?",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

---

## ?? Thu h?i token

### POST `/revoke-token`

Thu h?i m?t refresh token c? th?.

**Yêu c?u:** Bearer Token

#### Request Body

```json
{
  "refreshToken": "base64-refresh-token-here"
}
```

### POST `/revoke-all-tokens`

Thu h?i t?t c? token c?a ng??i dùng (??ng xu?t kh?i t?t c? thi?t b?).

**Yêu c?u:** Bearer Token

#### Response Success (200)

```json
{
  "success": true,
  "message": "?ã thu h?i t?t c? token",
  "timestamp": "2024-01-01T12:00:00Z"
}
```

---

## ??? Utilities cho Frontend

### Token Management Utility

```javascript
class AuthService {
  constructor() {
    this.baseURL = process.env.REACT_APP_API_URL;
    this.setupAxiosInterceptors();
  }

  // Setup axios interceptors
  setupAxiosInterceptors() {
    // Request interceptor
    axios.interceptors.request.use(
      (config) => {
        const token = this.getAccessToken();
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor
    axios.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;
        
        if (error.response?.status === 401 && !originalRequest._retry) {
          originalRequest._retry = true;
          
          try {
            await this.refreshToken();
            const newToken = this.getAccessToken();
            originalRequest.headers.Authorization = `Bearer ${newToken}`;
            return axios(originalRequest);
          } catch (refreshError) {
            this.logout();
            return Promise.reject(refreshError);
          }
        }
        
        return Promise.reject(error);
      }
    );
  }

  // Get access token
  getAccessToken() {
    return localStorage.getItem('accessToken');
  }

  // Get refresh token
  getRefreshToken() {
    return localStorage.getItem('refreshToken');
  }

  // Get current user
  getCurrentUser() {
    const userStr = localStorage.getItem('user');
    return userStr ? JSON.parse(userStr) : null;
  }

  // Check if user is authenticated
  isAuthenticated() {
    return !!this.getAccessToken();
  }

  // Login
  async login(credentials) {
    const response = await axios.post(`${this.baseURL}/api/auth/login`, credentials);
    const { data } = response.data;
    
    this.setTokens(data.token, data.refreshToken);
    this.setUser(data.user);
    
    return data;
  }

  // Register
  async register(userData) {
    const response = await axios.post(`${this.baseURL}/api/auth/register`, userData);
    const { data } = response.data;
    
    this.setTokens(data.token, data.refreshToken);
    this.setUser(data.user);
    
    return data;
  }

  // Refresh token
  async refreshToken() {
    const refreshToken = this.getRefreshToken();
    if (!refreshToken) {
      throw new Error('No refresh token available');
    }

    const response = await axios.post(`${this.baseURL}/api/auth/refresh-token`, {
      refreshToken
    });
    
    const { data } = response.data;
    this.setTokens(data.token, data.refreshToken);
    
    return data;
  }

  // Logout
  async logout() {
    try {
      const refreshToken = this.getRefreshToken();
      await axios.post(`${this.baseURL}/api/auth/logout`, { refreshToken });
    } catch (error) {
      console.error('Logout error:', error);
    } finally {
      this.clearTokens();
      window.location.href = '/login';
    }
  }

  // Set tokens
  setTokens(accessToken, refreshToken) {
    localStorage.setItem('accessToken', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
  }

  // Set user
  setUser(user) {
    localStorage.setItem('user', JSON.stringify(user));
  }

  // Clear tokens and user
  clearTokens() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    delete axios.defaults.headers.common['Authorization'];
  }
}

// Export singleton instance
export default new AuthService();
```

### React Hook for Authentication

```javascript
import { useState, useEffect, createContext, useContext } from 'react';
import AuthService from './AuthService';

const AuthContext = createContext();

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

export const AuthProvider = ({ children }) => {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const initAuth = async () => {
      try {
        if (AuthService.isAuthenticated()) {
          const userData = AuthService.getCurrentUser();
          setUser(userData);
        }
      } catch (error) {
        console.error('Auth initialization error:', error);
        AuthService.clearTokens();
      } finally {
        setLoading(false);
      }
    };

    initAuth();
  }, []);

  const login = async (credentials) => {
    const data = await AuthService.login(credentials);
    setUser(data.user);
    return data;
  };

  const register = async (userData) => {
    const data = await AuthService.register(userData);
    setUser(data.user);
    return data;
  };

  const logout = async () => {
    await AuthService.logout();
    setUser(null);
  };

  const value = {
    user,
    login,
    register,
    logout,
    loading,
    isAuthenticated: !!user
  };

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};
```

---

## ?? Security Best Practices

### 1. Token Storage
- **Recommendation:** S? d?ng httpOnly cookies cho production
- **Development:** localStorage có th? s? d?ng cho development

### 2. CSRF Protection
- S? d?ng CSRF tokens cho các forms quan tr?ng
- Validate origin headers

### 3. Rate Limiting
- API có rate limiting cho login: 5 requests/minute
- Registration: 3 requests/hour per IP

### 4. Password Requirements
- T?i thi?u 8 ký t?
- Ít nh?t 1 ch? hoa, 1 ch? th??ng, 1 s?, 1 ký t? ??c bi?t
- Không ch?a thông tin cá nhân

### 5. Session Management
- Access token expires: 60 minutes
- Refresh token expires: 7 days
- Automatic token rotation

---

## ?? Error Handling

```javascript
const handleApiError = (error) => {
  if (error.response) {
    // Server responded with error status
    const { status, data } = error.response;
    
    switch (status) {
      case 400:
        showError(data.message || 'D? li?u không h?p l?');
        break;
      case 401:
        showError('Phiên ??ng nh?p ?ã h?t h?n');
        AuthService.logout();
        break;
      case 403:
        showError('B?n không có quy?n th?c hi?n hành ??ng này');
        break;
      case 429:
        showError('Quá nhi?u yêu c?u. Vui lòng th? l?i sau.');
        break;
      case 500:
        showError('L?i server. Vui lòng th? l?i sau.');
        break;
      default:
        showError('Có l?i x?y ra');
    }
  } else if (error.request) {
    // Network error
    showError('Không th? k?t n?i ??n server');
  } else {
    // Other error
    showError('Có l?i x?y ra');
  }
};
```

---

## ?? Testing Endpoints

### S? d?ng cURL

```bash
# Register
curl -X POST "https://localhost:7240/api/auth/register" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!",
    "confirmPassword": "Test123!",
    "firstName": "Test",
    "lastName": "User"
  }'

# Login
curl -X POST "https://localhost:7240/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123!"
  }'

# Get current user
curl -X GET "https://localhost:7240/api/auth/me" \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

### Postman Collection

T?o collection trong Postman v?i:
- Environment variables cho base URL và tokens
- Pre-request scripts ?? auto-set authorization headers
- Tests ?? validate responses

---

Tài li?u này cung c?p ??y ?? thông tin ?? frontend team tích h?p v?i Authentication API c?a SakuraHome. N?u có th?c m?c, vui lòng liên h? team backend.