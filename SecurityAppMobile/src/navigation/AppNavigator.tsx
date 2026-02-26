import React from 'react';
import { NavigationContainer } from '@react-navigation/native';
import { createStackNavigator } from '@react-navigation/stack';
import { createBottomTabNavigator } from '@react-navigation/bottom-tabs';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS } from '../constants/theme';

// Common screens
import LoginScreen from '../screens/common/LoginScreen';
import OTPVerificationScreen from '../screens/common/OTPVerificationScreen';
import LanguageSelectionScreen from '../screens/common/LanguageSelectionScreen';
import ChangePasswordScreen from '../screens/common/ChangePasswordScreen';
import EditProfileScreen from '../screens/common/EditProfileScreen';
import NotificationSettingsScreen from '../screens/common/NotificationSettingsScreen';
import FAQScreen from '../screens/common/FAQScreen';
import LiveChatScreen from '../screens/common/LiveChatScreen';
import FeedbackScreen from '../screens/common/FeedbackScreen';
import ReportIssueScreen from '../screens/common/ReportIssueScreen';
import SettingsScreen from '../screens/common/SettingsScreen';
import AboutScreen from '../screens/common/AboutScreen';
import PrivacyScreen from '../screens/common/PrivacyScreen';
import TermsScreen from '../screens/common/TermsScreen';

// Guard screens
import DashboardScreen from '../screens/guard/DashboardScreen';
import CheckInScreen from '../screens/guard/CheckInScreen';
import CheckOutScreen from '../screens/guard/CheckOutScreen';
import ProfileScreen from '../screens/guard/ProfileScreen';
import TodayShiftScreen from '../screens/guard/TodayShiftScreen';
import IncidentReportingScreen from '../screens/guard/IncidentReportingScreen';
import SiteNavigationScreen from '../screens/guard/SiteNavigationScreen';
import DocumentsScreen from '../screens/guard/DocumentsScreen';
import AttendanceHistoryScreen from '../screens/guard/AttendanceHistoryScreen';
import NotificationsScreen from '../screens/guard/NotificationsScreen';
import SupportScreen from '../screens/guard/SupportScreen';
import AssignedDutiesScreen from '../screens/guard/AssignedDutiesScreen';
import PatrolTrackingScreen from '../screens/guard/PatrolTrackingScreen';
import EmergencySOSScreen from '../screens/guard/EmergencySOSScreen';
import VisitorManagementScreen from '../screens/guard/VisitorManagementScreen';
import VehicleLogScreen from '../screens/guard/VehicleLogScreen';
import AddVehicleEntryScreen from '../screens/guard/AddVehicleEntryScreen';
import LeaveRequestScreen from '../screens/guard/LeaveRequestScreen';
import SalaryScreen from '../screens/guard/SalaryScreen';
import TrainingScreen from '../screens/guard/TrainingScreen';
import AnnouncementsScreen from '../screens/guard/AnnouncementsScreen';
import QRScannerScreen from '../screens/guard/QRScannerScreen';
import ShiftHandoverScreen from '../screens/guard/ShiftHandoverScreen';
import DailyJournalScreen from '../screens/guard/DailyJournalScreen';
import KeyManagementScreen from '../screens/guard/KeyManagementScreen';
import OvertimeRequestScreen from '../screens/guard/OvertimeRequestScreen';
import AddVisitorScreen from '../screens/guard/AddVisitorScreen';

// Supervisor screens
import SupervisorDashboardScreen from '../screens/supervisor/SupervisorDashboardScreen';
import SupervisorProfileScreen from '../screens/supervisor/SupervisorProfileScreen';
import LiveAttendanceScreen from '../screens/supervisor/LiveAttendanceScreen';
import SiteWiseGuardListScreen from '../screens/supervisor/SiteWiseGuardListScreen';
import AttendanceApprovalScreen from '../screens/supervisor/AttendanceApprovalScreen';
import IncidentReviewScreen from '../screens/supervisor/IncidentReviewScreen';
import GuardReplacementScreen from '../screens/supervisor/GuardReplacementScreen';
import AnalyticsDashboardScreen from '../screens/supervisor/AnalyticsDashboardScreen';
import RosterManagementScreen from '../screens/supervisor/RosterManagementScreen';
import GuardPerformanceScreen from '../screens/supervisor/GuardPerformanceScreen';
import PayrollManagementScreen from '../screens/supervisor/PayrollManagementScreen';
import SiteManagementScreen from '../screens/supervisor/SiteManagementScreen';
import TrainingAssignmentScreen from '../screens/supervisor/TrainingAssignmentScreen';
import ComplianceDashboardScreen from '../screens/supervisor/ComplianceDashboardScreen';
import GuardMapScreen from '../screens/supervisor/GuardMapScreen';
import VisitorAnalyticsScreen from '../screens/supervisor/VisitorAnalyticsScreen';
import SiteVehicleLogScreen from '../screens/supervisor/SiteVehicleLogScreen';
import SiteVehicleLogDetailScreen from '../screens/supervisor/SiteVehicleLogDetailScreen';
import AssetManagementScreen from '../screens/supervisor/AssetManagementScreen';
import CreateAnnouncementScreen from '../screens/supervisor/CreateAnnouncementScreen';
import EditAnnouncementScreen from '../screens/supervisor/EditAnnouncementScreen';

import { RootStackParamList, GuardTabParamList, SupervisorTabParamList } from '../types/navigation';

const Stack = createStackNavigator<RootStackParamList>();
const GuardTab = createBottomTabNavigator<GuardTabParamList>();
const SupervisorTab = createBottomTabNavigator<SupervisorTabParamList>();

// Guard Tab Navigator
const GuardTabNavigator = () => {
  return (
    <GuardTab.Navigator
      screenOptions={({ route }) => ({
        tabBarIcon: ({ focused, color, size }) => {
          let iconName: keyof typeof MaterialCommunityIcons.glyphMap;

          if (route.name === 'GuardDashboard') {
            iconName = focused ? 'home' : 'home-outline';
          } else if (route.name === 'TodayShift') {
            iconName = focused ? 'clock' : 'clock-outline';
          } else if (route.name === 'IncidentReporting') {
            iconName = focused ? 'alert' : 'alert-outline';
          } else if (route.name === 'Profile') {
            iconName = focused ? 'account' : 'account-outline';
          } else if (route.name === 'SiteNavigation') {
            iconName = focused ? 'map-marker' : 'map-marker-outline';
          } else {
            iconName = 'home-outline';
          }

          return <MaterialCommunityIcons name={iconName} size={size} color={color} />;
        },
        tabBarActiveTintColor: COLORS.primary,
        tabBarInactiveTintColor: COLORS.gray400,
        headerShown: false,
        tabBarStyle: {
          backgroundColor: COLORS.white,
          borderTopWidth: 0,
          elevation: 8,
          shadowColor: COLORS.black,
          shadowOffset: { width: 0, height: -2 },
          shadowOpacity: 0.1,
          shadowRadius: 8,
          height: 70,
          paddingBottom: 10,
          paddingTop: 10,
        },
        tabBarLabelStyle: {
          fontSize: 11,
          fontWeight: '500',
        },
      })}
    >
      <GuardTab.Screen 
        name="GuardDashboard" 
        component={DashboardScreen}
        options={{ tabBarLabel: 'Home' }}
      />
      <GuardTab.Screen 
        name="TodayShift" 
        component={TodayShiftScreen}
        options={{ tabBarLabel: 'Shift' }}
      />
      <GuardTab.Screen 
        name="IncidentReporting" 
        component={IncidentReportingScreen}
        options={{ tabBarLabel: 'Report' }}
      />
      <GuardTab.Screen 
        name="SiteNavigation" 
        component={SiteNavigationScreen}
        options={{ tabBarLabel: 'Map' }}
      />
      <GuardTab.Screen 
        name="Profile" 
        component={ProfileScreen}
        options={{ tabBarLabel: 'Profile' }}
      />
    </GuardTab.Navigator>
  );
};

// Supervisor Tab Navigator
const SupervisorTabNavigator = () => {
  return (
    <SupervisorTab.Navigator
      screenOptions={({ route }) => ({
        tabBarIcon: ({ focused, color, size }) => {
          let iconName: keyof typeof MaterialCommunityIcons.glyphMap;

          if (route.name === 'SupervisorDashboard') {
            iconName = focused ? 'view-dashboard' : 'view-dashboard-outline';
          } else if (route.name === 'LiveAttendance') {
            iconName = focused ? 'account-group' : 'account-group-outline';
          } else if (route.name === 'SupervisorProfile') {
            iconName = focused ? 'account' : 'account-outline';
          } else {
            iconName = 'view-dashboard-outline';
          }

          return <MaterialCommunityIcons name={iconName} size={size} color={color} />;
        },
        tabBarActiveTintColor: COLORS.primary,
        tabBarInactiveTintColor: COLORS.gray400,
        headerShown: false,
        tabBarStyle: {
          backgroundColor: COLORS.white,
          borderTopWidth: 0,
          elevation: 8,
          shadowColor: COLORS.black,
          shadowOffset: { width: 0, height: -2 },
          shadowOpacity: 0.1,
          shadowRadius: 8,
          height: 70,
          paddingBottom: 10,
          paddingTop: 10,
        },
        tabBarLabelStyle: {
          fontSize: 11,
          fontWeight: '500',
        },
      })}
    >
      <SupervisorTab.Screen 
        name="SupervisorDashboard" 
        component={SupervisorDashboardScreen}
        options={{ tabBarLabel: 'Dashboard' }}
      />
      <SupervisorTab.Screen 
        name="LiveAttendance" 
        component={LiveAttendanceScreen}
        options={{ tabBarLabel: 'Attendance' }}
      />
      <SupervisorTab.Screen 
        name="SupervisorProfile" 
        component={SupervisorProfileScreen}
        options={{ tabBarLabel: 'Profile' }}
      />
    </SupervisorTab.Navigator>
  );
};

const AppNavigator: React.FC = () => {
  return (
    <NavigationContainer>
      <Stack.Navigator 
        initialRouteName="Login" 
        screenOptions={{ 
          headerShown: false,
          cardStyle: { backgroundColor: COLORS.background },
        }}
      >
        {/* Auth Screens */}
        <Stack.Screen name="Login" component={LoginScreen} />
        <Stack.Screen name="OTPVerification" component={OTPVerificationScreen} />
        <Stack.Screen name="LanguageSelection" component={LanguageSelectionScreen} />
        <Stack.Screen name="ChangePassword" component={ChangePasswordScreen} />
        
        {/* Main App Navigation */}
        <Stack.Screen name="GuardMain" component={GuardTabNavigator} />
        <Stack.Screen name="SupervisorMain" component={SupervisorTabNavigator} />
        
        {/* Guard Stack Screens */}
        <Stack.Screen name="Documents" component={DocumentsScreen} />
        <Stack.Screen name="AttendanceHistory" component={AttendanceHistoryScreen} />
        <Stack.Screen name="Notifications" component={NotificationsScreen} />
        <Stack.Screen name="Support" component={SupportScreen} />
        <Stack.Screen name="CheckIn" component={CheckInScreen} />
        <Stack.Screen name="CheckOut" component={CheckOutScreen} />
        <Stack.Screen name="AssignedDuties" component={AssignedDutiesScreen} />
        <Stack.Screen name="PatrolTracking" component={PatrolTrackingScreen} />
        <Stack.Screen name="EmergencySOS" component={EmergencySOSScreen} />
        <Stack.Screen name="VisitorManagement" component={VisitorManagementScreen} />
        <Stack.Screen name="VehicleLog" component={VehicleLogScreen} />
        <Stack.Screen name="AddVehicleEntry" component={AddVehicleEntryScreen} />
        <Stack.Screen name="LeaveRequest" component={LeaveRequestScreen} />
        <Stack.Screen name="Salary" component={SalaryScreen} />
        <Stack.Screen name="Training" component={TrainingScreen} />
        <Stack.Screen name="Announcements" component={AnnouncementsScreen} />
        <Stack.Screen name="CreateAnnouncement" component={CreateAnnouncementScreen} />
        <Stack.Screen name="EditAnnouncement" component={EditAnnouncementScreen} />
        <Stack.Screen name="QRScanner" component={QRScannerScreen} />
        <Stack.Screen name="ShiftHandover" component={ShiftHandoverScreen} />
        <Stack.Screen name="DailyJournal" component={DailyJournalScreen} />
        <Stack.Screen name="KeyManagement" component={KeyManagementScreen} />
        <Stack.Screen name="OvertimeRequest" component={OvertimeRequestScreen} />
        <Stack.Screen name="AddVisitor" component={AddVisitorScreen} />
        
        {/* Supervisor Stack Screens */}
        <Stack.Screen name="SiteWiseGuardList" component={SiteWiseGuardListScreen} />
        <Stack.Screen name="AttendanceApproval" component={AttendanceApprovalScreen} />
        <Stack.Screen name="IncidentReview" component={IncidentReviewScreen} />
        <Stack.Screen name="GuardReplacement" component={GuardReplacementScreen} />
        <Stack.Screen name="AnalyticsDashboard" component={AnalyticsDashboardScreen} />
        <Stack.Screen name="RosterManagement" component={RosterManagementScreen} />
        <Stack.Screen name="GuardPerformance" component={GuardPerformanceScreen} />
        <Stack.Screen name="PayrollManagement" component={PayrollManagementScreen} />
        <Stack.Screen name="SiteManagement" component={SiteManagementScreen} />
        <Stack.Screen name="TrainingAssignment" component={TrainingAssignmentScreen} />
        <Stack.Screen name="ComplianceDashboard" component={ComplianceDashboardScreen} />
        <Stack.Screen name="GuardMap" component={GuardMapScreen} />
        <Stack.Screen name="VisitorAnalytics" component={VisitorAnalyticsScreen} />
        <Stack.Screen name="SiteVehicleLog" component={SiteVehicleLogScreen} />
        <Stack.Screen name="SiteVehicleLogDetail" component={SiteVehicleLogDetailScreen} />
        <Stack.Screen name="AssetManagement" component={AssetManagementScreen} />
        
        {/* Common/Shared Screens */}
        <Stack.Screen name="EditProfile" component={EditProfileScreen} />
        <Stack.Screen name="NotificationSettings" component={NotificationSettingsScreen} />
        <Stack.Screen name="FAQ" component={FAQScreen} />
        <Stack.Screen name="LiveChat" component={LiveChatScreen} />
        <Stack.Screen name="Feedback" component={FeedbackScreen} />
        <Stack.Screen name="ReportIssue" component={ReportIssueScreen} />
        <Stack.Screen name="Settings" component={SettingsScreen} />
        <Stack.Screen name="About" component={AboutScreen} />
        <Stack.Screen name="Privacy" component={PrivacyScreen} />
        <Stack.Screen name="Terms" component={TermsScreen} />
      </Stack.Navigator>
    </NavigationContainer>
  );
};

export default AppNavigator;
