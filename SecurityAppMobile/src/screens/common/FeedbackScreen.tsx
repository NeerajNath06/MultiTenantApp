import React, { useState } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, Alert } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import Input from '../../components/common/Input';
import Button from '../../components/common/Button';

interface FeedbackCategory {
  id: string;
  label: string;
  icon: keyof typeof MaterialCommunityIcons.glyphMap;
}

const categories: FeedbackCategory[] = [
  { id: 'general', label: 'General Feedback', icon: 'message-text' },
  { id: 'feature', label: 'Feature Request', icon: 'lightbulb' },
  { id: 'bug', label: 'Bug Report', icon: 'bug' },
  { id: 'improvement', label: 'Improvement', icon: 'chart-line' },
  { id: 'praise', label: 'Appreciation', icon: 'heart' },
];

function FeedbackScreen({ navigation }: any) {
  const [loading, setLoading] = useState(false);
  const [rating, setRating] = useState(0);
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const [feedback, setFeedback] = useState('');
  const [email, setEmail] = useState('');

  const handleSubmit = () => {
    if (rating === 0) {
      Alert.alert('Error', 'Please provide a rating');
      return;
    }
    if (!selectedCategory) {
      Alert.alert('Error', 'Please select a feedback category');
      return;
    }
    if (!feedback.trim()) {
      Alert.alert('Error', 'Please write your feedback');
      return;
    }

    setLoading(true);
    setTimeout(() => {
      setLoading(false);
      Alert.alert(
        'Thank You! 🎉',
        'Your feedback has been submitted successfully. We appreciate your input and will use it to improve our app.',
        [{ text: 'OK', onPress: () => navigation.goBack() }]
      );
    }, 1500);
  };

  const renderStars = () => {
    return (
      <View style={styles.starsContainer}>
        {[1, 2, 3, 4, 5].map((star) => (
          <TouchableOpacity key={star} onPress={() => setRating(star)} style={styles.starButton}>
            <MaterialCommunityIcons
              name={star <= rating ? 'star' : 'star-outline'}
              size={40}
              color={star <= rating ? COLORS.warning : COLORS.gray300}
            />
          </TouchableOpacity>
        ))}
      </View>
    );
  };

  const getRatingText = () => {
    switch (rating) {
      case 1: return 'Poor 😞';
      case 2: return 'Fair 😐';
      case 3: return 'Good 🙂';
      case 4: return 'Very Good 😊';
      case 5: return 'Excellent! 🤩';
      default: return 'Tap to rate';
    }
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Send Feedback</Text>
        <View style={styles.placeholder} />
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Rating Section */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>How would you rate your experience?</Text>
          {renderStars()}
          <Text style={styles.ratingText}>{getRatingText()}</Text>
        </View>

        {/* Category Selection */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>What's your feedback about?</Text>
          <View style={styles.categoriesGrid}>
            {categories.map((category) => (
              <TouchableOpacity
                key={category.id}
                style={[styles.categoryCard, selectedCategory === category.id && styles.categoryCardActive]}
                onPress={() => setSelectedCategory(category.id)}
              >
                <View style={[styles.categoryIcon, selectedCategory === category.id && styles.categoryIconActive]}>
                  <MaterialCommunityIcons
                    name={category.icon}
                    size={24}
                    color={selectedCategory === category.id ? COLORS.white : COLORS.primary}
                  />
                </View>
                <Text style={[styles.categoryLabel, selectedCategory === category.id && styles.categoryLabelActive]}>
                  {category.label}
                </Text>
              </TouchableOpacity>
            ))}
          </View>
        </View>

        {/* Feedback Text */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Tell us more</Text>
          <Input
            placeholder="Share your thoughts, suggestions, or experiences..."
            value={feedback}
            onChangeText={setFeedback}
            multiline
            numberOfLines={5}
            style={styles.feedbackInput}
          />
        </View>

        {/* Email (Optional) */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Your email (optional)</Text>
          <Text style={styles.sectionSubtitle}>If you'd like us to follow up on your feedback</Text>
          <Input
            placeholder="Enter your email"
            value={email}
            onChangeText={setEmail}
            keyboardType="email-address"
            icon="email"
          />
        </View>

        {/* Guidelines */}
        <View style={styles.guidelinesCard}>
          <MaterialCommunityIcons name="information" size={24} color={COLORS.info} />
          <View style={styles.guidelinesContent}>
            <Text style={styles.guidelinesTitle}>Feedback Guidelines</Text>
            <Text style={styles.guidelinesText}>• Be specific and detailed{'\n'}• Include steps to reproduce bugs{'\n'}• Suggest improvements clearly{'\n'}• Keep it constructive</Text>
          </View>
        </View>

        <Button
          title="Submit Feedback"
          onPress={handleSubmit}
          loading={loading}
          style={styles.submitButton}
        />
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
  content: { padding: SIZES.md },
  section: { marginBottom: SIZES.lg },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  sectionSubtitle: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginBottom: SIZES.sm, marginTop: -SIZES.xs },
  starsContainer: { flexDirection: 'row', justifyContent: 'center', marginVertical: SIZES.md },
  starButton: { paddingHorizontal: SIZES.xs },
  ratingText: { fontSize: FONTS.body, color: COLORS.textSecondary, textAlign: 'center' },
  categoriesGrid: { flexDirection: 'row', flexWrap: 'wrap', gap: SIZES.sm },
  categoryCard: { width: '31%', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.sm, alignItems: 'center', borderWidth: 2, borderColor: 'transparent', ...SHADOWS.small },
  categoryCardActive: { borderColor: COLORS.primary, backgroundColor: COLORS.primary + '10' },
  categoryIcon: { width: 48, height: 48, borderRadius: 24, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center', marginBottom: SIZES.xs },
  categoryIconActive: { backgroundColor: COLORS.primary },
  categoryLabel: { fontSize: FONTS.caption, color: COLORS.textSecondary, textAlign: 'center' },
  categoryLabelActive: { color: COLORS.primary, fontWeight: '600' },
  feedbackInput: { height: 120 },
  guidelinesCard: { flexDirection: 'row', backgroundColor: COLORS.info + '10', borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.lg },
  guidelinesContent: { flex: 1, marginLeft: SIZES.sm },
  guidelinesTitle: { fontSize: FONTS.bodySmall, fontWeight: '600', color: COLORS.info, marginBottom: SIZES.xs },
  guidelinesText: { fontSize: FONTS.caption, color: COLORS.textSecondary, lineHeight: 18 },
  submitButton: { marginBottom: SIZES.xxl },
});

export default FeedbackScreen;
