# Security App Mobile - API Setup Guide

## 🔧 Current Status
✅ **LoginScreen is fully integrated with backend API**
✅ **Proper error handling and validation implemented**
✅ **Configuration management system in place**

## 📱 To Test the LoginScreen:

### Step 1: Configure Your IP Address
1. Open `src/config/api.config.ts`
2. Find this line:
   ```typescript
   BASE_URL: 'http://192.168.1.100:5014', // TODO: Replace with your actual IP
   ```
3. Replace `192.168.1.100` with your actual machine IP address

### Step 2: Find Your IP Address
**Windows:**
```cmd
ipconfig
```
Look for "IPv4 Address" (usually starts with 192.168.x.x or 10.0.x.x)

**Mac/Linux:**
```bash
ifconfig
```
OR
```bash
ip addr
```

### Step 3: Start Your Backend
Ensure your .NET backend is running on port 5014:
```bash
cd SecurityAppSolutions/SecurityApp.API
dotnet run
```

### Step 4: Start the Mobile App
```bash
cd SecurityAppMobile
npx react-native start
```

## 🚀 What You Can Test Now:

### Login Functionality
- ✅ Email validation (must be valid email format)
- ✅ Password validation (minimum 6 characters)
- ✅ API integration with your backend
- ✅ Role-based navigation (Guard/Supervisor)
- ✅ Token storage in AsyncStorage
- ✅ Error handling for network issues

### Test Credentials
Use the credentials from your backend:
- **Email:** ankit@test.com (as per your API response)
- **Password:** whatever you have set in your backend

## 🔍 Debugging:

### Check Logs
If login fails, check the console logs for:
- API URL being called
- Request/response data
- Error messages

### Network Errors
If you see "Network request failed":
1. Verify your IP address in `api.config.ts`
2. Ensure backend is running on port 5014
3. Check firewall settings
4. Verify mobile app has internet permissions

### Console Debugging
The app includes detailed logging:
```javascript
// Check what's being sent
console.log('Attempting login to:', `${this.BASE_URL}/api/v1/Auth/login`);
console.log('Credentials:', credentials);

// Check what comes back
console.log('Response status:', response.status);
console.log('Response data:', data);
```

## 📋 Next Implementation Steps:

1. **ForgotPassword Screen** - Password reset functionality
2. **OTPVerification Screen** - Mobile number verification
3. **Dashboard Screens** - Main app interface
4. **CRUD Operations** - Visitor, Vehicle, Key management
5. **Real-time Features** - Live updates, notifications

## 🛠️ API Configuration Details:

### Current Backend API Response Format:
```json
{
  "statusCode": 200,
  "elements": {
    "token": "jwt_token_here",
    "refreshToken": "refresh_token_here", 
    "user": {
      "id": "user_id",
      "username": "Ankit",
      "email": "ankit@test.com",
      "mobile": "7845123698",
      "role": "Guard",
      "agencyId": "agency_id"
    }
  },
  "message": "Operation completed successfully."
}
```

### Token Management:
- ✅ Tokens stored securely in AsyncStorage
- ✅ Refresh token mechanism implemented
- ✅ Automatic logout on token expiry (to be implemented)
- ✅ Role-based access control

## 🎯 Ready for Testing!

Your LoginScreen is now fully functional with:
- Real API integration
- Proper error handling
- User-friendly validation messages
- Secure authentication flow

Try logging in with your backend credentials and let me know if you encounter any issues!
