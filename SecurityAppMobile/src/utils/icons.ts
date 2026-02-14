// Icon mapping utility - Maps logical icon names to MaterialCommunityIcons
// This provides a single source of truth for all icons used in the app

export const IconNames = {
  // Navigation & Actions
  home: 'home',
  homeOutline: 'home-outline',
  back: 'arrow-left',
  forward: 'arrow-right',
  chevronForward: 'chevron-right',
  chevronBack: 'chevron-left',
  close: 'close',
  menu: 'menu',
  search: 'magnify',
  filter: 'filter-variant',
  refresh: 'refresh',
  
  // User & Profile
  person: 'account',
  personOutline: 'account-outline',
  personCircle: 'account-circle',
  people: 'account-group',
  peopleOutline: 'account-group-outline',
  
  // Time & Calendar
  time: 'clock',
  timeOutline: 'clock-outline',
  calendar: 'calendar',
  calendarOutline: 'calendar-outline',
  
  // Location
  location: 'map-marker',
  locationOutline: 'map-marker-outline',
  navigate: 'navigation',
  business: 'office-building',
  
  // Security & Protection
  shield: 'shield',
  shieldCheckmark: 'shield-check',
  shieldOutline: 'shield-outline',
  lock: 'lock',
  lockOutline: 'lock-outline',
  
  // Communication
  call: 'phone',
  callOutline: 'phone-outline',
  mail: 'email',
  mailOutline: 'email-outline',
  chatbubbles: 'chat',
  notifications: 'bell',
  notificationsOutline: 'bell-outline',
  
  // Documents
  document: 'file-document',
  documentText: 'file-document-outline',
  documents: 'file-multiple',
  folder: 'folder',
  
  // Actions
  checkmark: 'check',
  checkmarkCircle: 'check-circle',
  checkmarkDone: 'check-all',
  add: 'plus',
  addCircle: 'plus-circle',
  remove: 'minus',
  edit: 'pencil',
  delete: 'delete',
  share: 'share-variant',
  download: 'download',
  upload: 'upload',
  cloudUpload: 'cloud-upload',
  print: 'printer',
  copy: 'content-copy',
  
  // Alerts & Status
  warning: 'alert',
  warningOutline: 'alert-outline',
  alertCircle: 'alert-circle',
  alertCircleOutline: 'alert-circle-outline',
  informationCircle: 'information',
  helpCircle: 'help-circle',
  bug: 'bug',
  
  // Login/Logout
  logIn: 'login',
  logOut: 'logout',
  enter: 'location-enter',
  exit: 'location-exit',
  
  // Misc
  settings: 'cog',
  settingsOutline: 'cog-outline',
  star: 'star',
  starOutline: 'star-outline',
  camera: 'camera',
  cameraOutline: 'camera-outline',
  image: 'image',
  images: 'image-multiple',
  eye: 'eye',
  eyeOff: 'eye-off',
  language: 'translate',
  analytics: 'chart-line',
  analyticsOutline: 'chart-line-variant',
  statsChart: 'chart-bar',
  
  // Emergency & Medical
  medkit: 'medical-bag',
  medical: 'hospital-box',
  flame: 'fire',
  
  // Work
  hammer: 'hammer',
  school: 'school',
  idCard: 'card-account-details',
  
  // Archive
  archive: 'archive',
  
  // Status indicators
  play: 'play',
  pause: 'pause',
  stop: 'stop',
  
  // Help
  helpBuoy: 'lifebuoy',
  
  // Arrows
  arrowUp: 'arrow-up',
  arrowDown: 'arrow-down',
  arrowForward: 'arrow-right',
  
  // Ellipsis
  ellipsisHorizontal: 'dots-horizontal',
  ellipsisVertical: 'dots-vertical',
  
  // Close circle
  closeCircle: 'close-circle',
} as const;

export type IconName = keyof typeof IconNames;

// Helper function to get icon name
export const getIconName = (name: IconName): string => {
  return IconNames[name] || 'help-circle';
};
