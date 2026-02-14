# Security Guard Mobile App

A React Native (Expo) mobile application for Security Guard Management System with Guard App and Supervisor App.

## 📱 Features

### Common Screens (4)
- ✅ Login Screen - Phone + Password with Guard/Supervisor toggle
- ✅ OTP Verification Screen - 6-digit OTP verification
- ✅ Language Selection Screen - Multi-language support (10 Indian languages)
- ✅ Change Password Screen - Secure password update with strength indicator

### Guard App Screens (10)
- ✅ Dashboard - Overview with quick actions, shift info, stats
- ✅ Today's Shift - Detailed shift information with checkpoints
- ✅ Site Navigation - Navigate to assigned site
- ✅ Check-in (GPS + Photo) - Location & selfie verification
- ✅ Check-out - End shift with verification
- ✅ Assigned Duties - View and track duties
- ✅ Incident Reporting - Report incidents with photos/videos
- ✅ Upload Media - Upload evidence files
- ✅ Profile - Guard profile and settings
- ✅ Documents - View guard documents

### Supervisor App Screens (7)
- ✅ Dashboard - Overview with guard stats, approvals, incidents
- ✅ Site-wise Guard List - View guards by site
- ✅ Live Attendance - Real-time guard tracking
- ✅ Attendance Approval - Approve/reject attendance requests
- ✅ Incident Review - Review and resolve incidents
- ✅ Guard Replacement Request - Request guard swaps
- ✅ Reports - Generate and view reports

## 🎨 Design System

### Color Scheme
- **Primary:** #1E3A8A (Deep Blue)
- **Secondary:** #10B981 (Emerald Green)
- **Accent:** #F59E0B (Amber)
- **Success:** #22C55E
- **Warning:** #EAB308
- **Error:** #EF4444

### Typography
- Headers: 32px, 24px, 20px, 18px
- Body: 16px, 14px
- Caption: 12px, 10px

## 🛠️ Tech Stack

- **React Native** with **Expo**
- **React Navigation** (Stack + Bottom Tabs)
- **Expo Location** - GPS tracking
- **Expo Camera** - Photo capture
- **Expo Image Picker** - Media selection
- **Expo Linear Gradient** - UI gradients
- **Ionicons** - Icon library

## 📁 Project Structure

```
SecurityAppMobile/
├── App.js                    # Main app entry
├── app.json                  # Expo configuration
├── package.json              # Dependencies
├── src/
│   ├── components/
│   │   └── common/
│   │       ├── Button.js     # Custom button component
│   │       ├── Input.js      # Custom input component
│   │       ├── Card.js       # Card component
│   │       └── index.js      # Component exports
│   ├── constants/
│   │   └── theme.js          # Colors, fonts, sizes, shadows
│   ├── navigation/
│   │   └── AppNavigator.js   # Navigation configuration
│   └── screens/
│       ├── common/
│       │   ├── LoginScreen.js
│       │   ├── OTPVerificationScreen.js
│       │   ├── LanguageSelectionScreen.js
│       │   └── ChangePasswordScreen.js
│       ├── guard/
│       │   ├── DashboardScreen.js
│       │   ├── TodayShiftScreen.js
│       │   ├── CheckInScreen.js
│       │   ├── IncidentReportingScreen.js
│       │   └── ProfileScreen.js
│       └── supervisor/
│           ├── SupervisorDashboardScreen.js
│           └── LiveAttendanceScreen.js
└── assets/                   # App icons and images
```

## 🚀 Getting Started

### Prerequisites
- Node.js >= 18
- npm or yarn
- Expo CLI
- Android Studio (for Android) or Xcode (for iOS)

### Installation

```bash
# Navigate to project directory
cd SecurityAppMobile

# Install dependencies (already done)
npm install

# Start the development server
npm start

# Run on Android
npm run android

# Run on iOS
npm run ios

# Run on Web
npm run web
```

### Running on Physical Device

1. Install **Expo Go** app on your phone
2. Run `npm start` in terminal
3. Scan the QR code with Expo Go (Android) or Camera (iOS)

## 📝 Notes

- GPS and Camera permissions are required for Check-in functionality
- The app supports both Guard and Supervisor roles
- Some screens are placeholder implementations (marked as "Under Development")
- API integration pending - currently using mock data

## 🔐 Security Features

- OTP-based authentication
- GPS location verification for attendance
- Photo verification during check-in/check-out
- Secure password requirements (8+ chars, uppercase, numbers, special chars)

## 📄 License

Private - For internal use only.

---

**Developed for Security App Management System**
