// Navigation Types

import { StackScreenProps, StackNavigationProp } from '@react-navigation/stack';
import { BottomTabScreenProps, BottomTabNavigationProp } from '@react-navigation/bottom-tabs';
import { CompositeScreenProps, CompositeNavigationProp } from '@react-navigation/native';

// Root Stack Navigator Types - All screens accessible via navigation
export type RootStackParamList = {
  Login: undefined;
  OTPVerification: { phone: string; userType: string };
  LanguageSelection: undefined;
  ChangePassword: undefined;
  ForgotPassword: { email?: string };
  GuardRegistration: undefined;
  SupervisorRegistration: undefined;
  GuardMain: undefined;
  SupervisorMain: undefined;
  
  // Guard screens
  Documents: undefined;
  AttendanceHistory: undefined;
  Notifications: undefined;
  Support: undefined;
  CheckIn: undefined;
  CheckOut: undefined;
  AssignedDuties: undefined;
  PatrolTracking: undefined;
  EmergencySOS: undefined;
  VisitorManagement: undefined;
  VehicleLog: undefined;
  LeaveRequest: undefined;
  Salary: undefined;
  Training: undefined;
  Announcements: { manageMode?: boolean } | undefined;
  CreateAnnouncement: undefined;
  EditAnnouncement: { id: string };
  QRScanner: undefined;
  ShiftHandover: undefined;
  DailyJournal: undefined;
  KeyManagement: undefined;
  OvertimeRequest: undefined;
  
  // Supervisor screens
  SiteWiseGuardList: undefined;
  AttendanceApproval: undefined;
  IncidentReview: undefined;
  GuardReplacement: undefined;
  AnalyticsDashboard: undefined;
  RosterManagement: undefined;
  SiteManagement: undefined;
  GuardMap: undefined;
  PayrollManagement: undefined;
  TrainingAssignment: undefined;
  ComplianceDashboard: undefined;
  ClientCommunication: undefined;
  AssetManagement: undefined;
  VisitorAnalytics: undefined;
  GuardPerformance: undefined;
  
  // Common screens
  EditProfile: undefined;
  NotificationSettings: undefined;
  FAQ: undefined;
  LiveChat: undefined;
  Feedback: undefined;
  ReportIssue: undefined;
  Settings: undefined;
  About: undefined;
  Privacy: undefined;
  Terms: undefined;
  AddVisitor: undefined;
};

// Bottom Tab Navigator Types for Guard
export type GuardTabParamList = {
  GuardDashboard: undefined;
  TodayShift: undefined;
  IncidentReporting: undefined;
  Profile: undefined;
  SiteNavigation: undefined;
};

// Bottom Tab Navigator Types for Supervisor
export type SupervisorTabParamList = {
  SupervisorDashboard: undefined;
  LiveAttendance: undefined;
  SupervisorProfile: undefined;
};

// Combined navigation type for Guard screens that need access to both Tab and Stack
export type GuardTabNavigationProp = CompositeNavigationProp<
  BottomTabNavigationProp<GuardTabParamList>,
  StackNavigationProp<RootStackParamList>
>;

// Combined navigation type for Supervisor screens that need access to both Tab and Stack
export type SupervisorTabNavigationProp = CompositeNavigationProp<
  BottomTabNavigationProp<SupervisorTabParamList>,
  StackNavigationProp<RootStackParamList>
>;

// Screen Props Types - Stack Screens
export type LoginScreenProps = StackScreenProps<RootStackParamList, 'Login'>;
export type OTPVerificationScreenProps = StackScreenProps<RootStackParamList, 'OTPVerification'>;
export type LanguageSelectionScreenProps = StackScreenProps<RootStackParamList, 'LanguageSelection'>;
export type ChangePasswordScreenProps = StackScreenProps<RootStackParamList, 'ChangePassword'>;
export type ForgotPasswordScreenProps = StackScreenProps<RootStackParamList, 'ForgotPassword'>;
export type GuardRegistrationScreenProps = StackScreenProps<RootStackParamList, 'GuardRegistration'>;
export type SupervisorRegistrationScreenProps = StackScreenProps<RootStackParamList, 'SupervisorRegistration'>;

// Guard Tab Screen Props - with composite navigation for stack access
export type GuardDashboardScreenProps = CompositeScreenProps<
  BottomTabScreenProps<GuardTabParamList, 'GuardDashboard'>,
  StackScreenProps<RootStackParamList>
>;

export type TodayShiftScreenProps = CompositeScreenProps<
  BottomTabScreenProps<GuardTabParamList, 'TodayShift'>,
  StackScreenProps<RootStackParamList>
>;

export type IncidentReportingScreenProps = CompositeScreenProps<
  BottomTabScreenProps<GuardTabParamList, 'IncidentReporting'>,
  StackScreenProps<RootStackParamList>
>;

export type ProfileScreenProps = CompositeScreenProps<
  BottomTabScreenProps<GuardTabParamList, 'Profile'>,
  StackScreenProps<RootStackParamList>
>;

export type SiteNavigationScreenProps = CompositeScreenProps<
  BottomTabScreenProps<GuardTabParamList, 'SiteNavigation'>,
  StackScreenProps<RootStackParamList>
>;

// Supervisor Tab Screen Props - with composite navigation for stack access
export type SupervisorDashboardScreenProps = CompositeScreenProps<
  BottomTabScreenProps<SupervisorTabParamList, 'SupervisorDashboard'>,
  StackScreenProps<RootStackParamList>
>;

export type LiveAttendanceScreenProps = CompositeScreenProps<
  BottomTabScreenProps<SupervisorTabParamList, 'LiveAttendance'>,
  StackScreenProps<RootStackParamList>
>;

export type SupervisorProfileScreenProps = CompositeScreenProps<
  BottomTabScreenProps<SupervisorTabParamList, 'SupervisorProfile'>,
  StackScreenProps<RootStackParamList>
>;

// Stack navigation props for individual screens
export type DocumentsScreenProps = StackScreenProps<RootStackParamList, 'Documents'>;
export type AttendanceHistoryScreenProps = StackScreenProps<RootStackParamList, 'AttendanceHistory'>;
export type NotificationsScreenProps = StackScreenProps<RootStackParamList, 'Notifications'>;
export type SupportScreenProps = StackScreenProps<RootStackParamList, 'Support'>;
export type CheckInScreenProps = StackScreenProps<RootStackParamList, 'CheckIn'>;
export type CheckOutScreenProps = StackScreenProps<RootStackParamList, 'CheckOut'>;
export type AssignedDutiesScreenProps = StackScreenProps<RootStackParamList, 'AssignedDuties'>;
export type PatrolTrackingScreenProps = StackScreenProps<RootStackParamList, 'PatrolTracking'>;
export type EmergencySOSScreenProps = StackScreenProps<RootStackParamList, 'EmergencySOS'>;
export type VisitorManagementScreenProps = StackScreenProps<RootStackParamList, 'VisitorManagement'>;
export type VehicleLogScreenProps = StackScreenProps<RootStackParamList, 'VehicleLog'>;
export type LeaveRequestScreenProps = StackScreenProps<RootStackParamList, 'LeaveRequest'>;
export type SalaryScreenProps = StackScreenProps<RootStackParamList, 'Salary'>;
export type TrainingScreenProps = StackScreenProps<RootStackParamList, 'Training'>;
export type AnnouncementsScreenProps = StackScreenProps<RootStackParamList, 'Announcements'>;

// Guard additional screen props
export type QRScannerScreenProps = StackScreenProps<RootStackParamList, 'QRScanner'>;
export type ShiftHandoverScreenProps = StackScreenProps<RootStackParamList, 'ShiftHandover'>;
export type DailyJournalScreenProps = StackScreenProps<RootStackParamList, 'DailyJournal'>;
export type KeyManagementScreenProps = StackScreenProps<RootStackParamList, 'KeyManagement'>;
export type OvertimeRequestScreenProps = StackScreenProps<RootStackParamList, 'OvertimeRequest'>;
export type AddVisitorScreenProps = StackScreenProps<RootStackParamList, 'AddVisitor'>;

// Supervisor screen props
export type SiteWiseGuardListScreenProps = StackScreenProps<RootStackParamList, 'SiteWiseGuardList'>;
export type AttendanceApprovalScreenProps = StackScreenProps<RootStackParamList, 'AttendanceApproval'>;
export type IncidentReviewScreenProps = StackScreenProps<RootStackParamList, 'IncidentReview'>;
export type GuardReplacementScreenProps = StackScreenProps<RootStackParamList, 'GuardReplacement'>;
export type AnalyticsDashboardScreenProps = StackScreenProps<RootStackParamList, 'AnalyticsDashboard'>;
export type RosterManagementScreenProps = StackScreenProps<RootStackParamList, 'RosterManagement'>;
export type SiteManagementScreenProps = StackScreenProps<RootStackParamList, 'SiteManagement'>;
export type GuardMapScreenProps = StackScreenProps<RootStackParamList, 'GuardMap'>;
export type PayrollManagementScreenProps = StackScreenProps<RootStackParamList, 'PayrollManagement'>;
export type TrainingAssignmentScreenProps = StackScreenProps<RootStackParamList, 'TrainingAssignment'>;
export type ComplianceDashboardScreenProps = StackScreenProps<RootStackParamList, 'ComplianceDashboard'>;
export type AssetManagementScreenProps = StackScreenProps<RootStackParamList, 'AssetManagement'>;
export type VisitorAnalyticsScreenProps = StackScreenProps<RootStackParamList, 'VisitorAnalytics'>;
export type GuardPerformanceScreenProps = StackScreenProps<RootStackParamList, 'GuardPerformance'>;

// Common screen props
export type EditProfileScreenProps = StackScreenProps<RootStackParamList, 'EditProfile'>;
export type NotificationSettingsScreenProps = StackScreenProps<RootStackParamList, 'NotificationSettings'>;
export type FAQScreenProps = StackScreenProps<RootStackParamList, 'FAQ'>;
export type LiveChatScreenProps = StackScreenProps<RootStackParamList, 'LiveChat'>;
export type FeedbackScreenProps = StackScreenProps<RootStackParamList, 'Feedback'>;
export type ReportIssueScreenProps = StackScreenProps<RootStackParamList, 'ReportIssue'>;
export type SettingsScreenProps = StackScreenProps<RootStackParamList, 'Settings'>;
export type AboutScreenProps = StackScreenProps<RootStackParamList, 'About'>;
export type PrivacyScreenProps = StackScreenProps<RootStackParamList, 'Privacy'>;
export type TermsScreenProps = StackScreenProps<RootStackParamList, 'Terms'>;
