import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, TextInput } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

interface FAQItem {
  id: string;
  category: string;
  question: string;
  answer: string;
}

const faqData: FAQItem[] = [
  { id: '1', category: 'Check-In/Out', question: 'How do I check in for my shift?', answer: 'Go to the Dashboard and tap on "Check In". Make sure your location services are enabled and take a selfie when prompted. Your check-in will be recorded with timestamp and GPS coordinates.' },
  { id: '2', category: 'Check-In/Out', question: 'What if I forget to check out?', answer: 'If you forget to check out, contact your supervisor immediately. They can manually adjust your attendance record. Repeated missed check-outs may affect your attendance score.' },
  { id: '3', category: 'Attendance', question: 'How is my attendance calculated?', answer: 'Your attendance is calculated based on your check-in and check-out times compared to your scheduled shift. Late arrivals and early departures are tracked separately.' },
  { id: '4', category: 'Attendance', question: 'Can I request leave through the app?', answer: 'Yes, go to Profile > Attendance History and tap on "Request Leave". Fill in the details and submit. Your supervisor will be notified and can approve or reject the request.' },
  { id: '5', category: 'Incidents', question: 'How do I report an incident?', answer: 'Tap on "Report" in the bottom navigation. Select the incident type, provide a description, and attach photos or videos if needed. For emergencies, use the Emergency button first.' },
  { id: '6', category: 'Incidents', question: 'What types of incidents can I report?', answer: 'You can report: Security breaches, Medical emergencies, Fire/Safety hazards, Suspicious activities, Property damage, Theft, Trespassing, and General observations.' },
  { id: '7', category: 'Documents', question: 'How do I upload my documents?', answer: 'Go to Profile > Documents > Upload. Select the document type (ID, License, Certificate) and choose a file from your gallery or take a photo. Documents are reviewed within 24-48 hours.' },
  { id: '8', category: 'Documents', question: 'What documents are required?', answer: 'Required documents include: Government ID proof, Security guard license, Police verification certificate, Medical fitness certificate, and any training certificates.' },
  { id: '9', category: 'Technical', question: 'The app is not loading properly', answer: 'Try these steps: 1) Check your internet connection, 2) Close and reopen the app, 3) Clear app cache in settings, 4) Update to the latest version, 5) Restart your phone.' },
  { id: '10', category: 'Technical', question: 'Location is not being detected', answer: 'Ensure location services are enabled for this app. Go to your phone Settings > Apps > Security App > Permissions > Location > Allow. Also check if GPS is turned on.' },
  { id: '11', category: 'Account', question: 'How do I change my password?', answer: 'Go to Profile > Change Password. Enter your current password and then your new password twice. Password must be at least 6 characters with letters and numbers.' },
  { id: '12', category: 'Account', question: 'How do I update my contact information?', answer: 'Go to Profile > Edit Profile. You can update your phone number, email, address, and emergency contact. Some fields like Employee ID cannot be changed.' },
];

const categories = ['All', 'Check-In/Out', 'Attendance', 'Incidents', 'Documents', 'Technical', 'Account'];

function FAQScreen({ navigation }: any) {
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedCategory, setSelectedCategory] = useState('All');
  const [expandedId, setExpandedId] = useState<string | null>(null);

  const filteredFAQs = faqData.filter(faq => {
    const matchesSearch = faq.question.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          faq.answer.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = selectedCategory === 'All' || faq.category === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  const toggleExpand = (id: string) => {
    setExpandedId(expandedId === id ? null : id);
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>FAQ</Text>
        <View style={styles.placeholder} />
      </View>

      <View style={styles.searchContainer}>
        <MaterialCommunityIcons name="magnify" size={20} color={COLORS.gray400} />
        <TextInput
          style={styles.searchInput}
          placeholder="Search questions..."
          value={searchQuery}
          onChangeText={setSearchQuery}
          placeholderTextColor={COLORS.gray400}
        />
        {searchQuery.length > 0 && (
          <TouchableOpacity onPress={() => setSearchQuery('')}>
            <MaterialCommunityIcons name="close-circle" size={20} color={COLORS.gray400} />
          </TouchableOpacity>
        )}
      </View>

      <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.categoryScroll} contentContainerStyle={styles.categoryContainer}>
        {categories.map((category) => (
          <TouchableOpacity
            key={category}
            style={[styles.categoryChip, selectedCategory === category && styles.categoryChipActive]}
            onPress={() => setSelectedCategory(category)}
          >
            <Text style={[styles.categoryText, selectedCategory === category && styles.categoryTextActive]}>
              {category}
            </Text>
          </TouchableOpacity>
        ))}
      </ScrollView>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {filteredFAQs.length === 0 ? (
          <View style={styles.emptyState}>
            <MaterialCommunityIcons name="help-circle-outline" size={64} color={COLORS.gray300} />
            <Text style={styles.emptyText}>No questions found</Text>
            <Text style={styles.emptySubtext}>Try adjusting your search or category</Text>
          </View>
        ) : (
          filteredFAQs.map((faq) => (
            <TouchableOpacity
              key={faq.id}
              style={styles.faqCard}
              onPress={() => toggleExpand(faq.id)}
              activeOpacity={0.7}
            >
              <View style={styles.faqHeader}>
                <View style={styles.faqQuestion}>
                  <View style={styles.categoryBadge}>
                    <Text style={styles.categoryBadgeText}>{faq.category}</Text>
                  </View>
                  <Text style={styles.questionText}>{faq.question}</Text>
                </View>
                <MaterialCommunityIcons
                  name={expandedId === faq.id ? 'chevron-up' : 'chevron-down'}
                  size={24}
                  color={COLORS.gray400}
                />
              </View>
              {expandedId === faq.id && (
                <View style={styles.answerContainer}>
                  <Text style={styles.answerText}>{faq.answer}</Text>
                </View>
              )}
            </TouchableOpacity>
          ))
        )}

        {/* Contact Support */}
        <View style={styles.contactCard}>
          <MaterialCommunityIcons name="headset" size={40} color={COLORS.primary} />
          <Text style={styles.contactTitle}>Still have questions?</Text>
          <Text style={styles.contactText}>Our support team is here to help</Text>
          <TouchableOpacity style={styles.contactButton} onPress={() => navigation.navigate('Support')}>
            <Text style={styles.contactButtonText}>Contact Support</Text>
          </TouchableOpacity>
        </View>
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  placeholder: { width: 40 },
  searchContainer: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, marginHorizontal: SIZES.md, marginTop: SIZES.md, paddingHorizontal: SIZES.md, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  searchInput: { flex: 1, height: 44, marginLeft: SIZES.sm, fontSize: FONTS.body, color: COLORS.textPrimary },
  categoryScroll: { maxHeight: 50, marginTop: SIZES.md },
  categoryContainer: { paddingHorizontal: SIZES.md, gap: SIZES.sm },
  categoryChip: { paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.white, borderRadius: SIZES.radiusFull, ...SHADOWS.small },
  categoryChipActive: { backgroundColor: COLORS.primary },
  categoryText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, fontWeight: '500' },
  categoryTextActive: { color: COLORS.white },
  content: { padding: SIZES.md },
  faqCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, marginBottom: SIZES.sm, ...SHADOWS.small, overflow: 'hidden' },
  faqHeader: { flexDirection: 'row', alignItems: 'center', padding: SIZES.md },
  faqQuestion: { flex: 1 },
  categoryBadge: { backgroundColor: COLORS.primary + '15', paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm, alignSelf: 'flex-start', marginBottom: SIZES.xs },
  categoryBadgeText: { fontSize: FONTS.tiny, color: COLORS.primary, fontWeight: '600' },
  questionText: { fontSize: FONTS.body, fontWeight: '500', color: COLORS.textPrimary },
  answerContainer: { paddingHorizontal: SIZES.md, paddingBottom: SIZES.md, paddingTop: SIZES.xs, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  answerText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 20 },
  emptyState: { alignItems: 'center', justifyContent: 'center', paddingVertical: SIZES.xxl },
  emptyText: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.md },
  emptySubtext: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: SIZES.xs },
  contactCard: { backgroundColor: COLORS.primary + '10', borderRadius: SIZES.radiusMd, padding: SIZES.lg, alignItems: 'center', marginTop: SIZES.md, marginBottom: SIZES.xxl },
  contactTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary, marginTop: SIZES.sm },
  contactText: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, marginTop: SIZES.xs },
  contactButton: { backgroundColor: COLORS.primary, paddingHorizontal: SIZES.lg, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd, marginTop: SIZES.md },
  contactButtonText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white },
});

export default FAQScreen;
