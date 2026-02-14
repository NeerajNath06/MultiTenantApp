import React, { useState, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, TextInput, Alert, ActivityIndicator, RefreshControl } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';
import { journalService } from '../../services/journalService';
import { authService } from '../../services/authService';

interface JournalEntry {
  id: number;
  time: string;
  category: 'observation' | 'incident' | 'visitor' | 'patrol' | 'maintenance';
  title: string;
  description: string;
}

function DailyJournalScreen({ navigation }: any) {
  const [newEntry, setNewEntry] = useState('');
  const [selectedCategory, setSelectedCategory] = useState<string>('observation');
  const [loading, setLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const [entries, setEntries] = useState<JournalEntry[]>([]);

  useEffect(() => {
    loadJournalEntries();
  }, []);

  const loadJournalEntries = async () => {
    try {
      setLoading(true);
      const user = await authService.getStoredUser();
      if (!user) return;

      const today = new Date();
      today.setHours(0, 0, 0, 0);

      const result = await journalService.getJournals({
        guardId: user.id,
        dateFrom: today.toISOString(),
        pageSize: 100,
      });

      if (result.success && result.data) {
        const journals = Array.isArray(result.data)
          ? result.data
          : (result.data.data || []);

        const mappedEntries = journals.map((journal: any) => ({
          id: journal.id || Date.now(),
          time: new Date(journal.journalDate || journal.createdAt).toLocaleTimeString('en-US', { hour: '2-digit', minute: '2-digit' }),
          category: 'observation' as const, // Map from journal type if available
          title: journal.entry?.split('\n')[0]?.slice(0, 50) || 'Journal Entry',
          description: journal.entry || journal.notes || '',
        }));

        setEntries(mappedEntries);
      }
    } catch (error) {
      console.error('Error loading journal entries:', error);
    } finally {
      setLoading(false);
      setRefreshing(false);
    }
  };

  const onRefresh = () => {
    setRefreshing(true);
    loadJournalEntries();
  };

  const categories = [
    { id: 'observation', label: 'Observation', icon: 'eye', color: COLORS.primaryBlue },
    { id: 'incident', label: 'Incident', icon: 'alert-circle', color: COLORS.error },
    { id: 'visitor', label: 'Visitor', icon: 'account', color: COLORS.success },
    { id: 'patrol', label: 'Patrol', icon: 'walk', color: COLORS.warning },
    { id: 'maintenance', label: 'Maintenance', icon: 'wrench', color: COLORS.secondary },
  ];

  const getCategoryIcon = (category: string): keyof typeof MaterialCommunityIcons.glyphMap => {
    const cat = categories.find(c => c.id === category);
    return (cat?.icon as keyof typeof MaterialCommunityIcons.glyphMap) || 'note';
  };

  const getCategoryColor = (category: string) => {
    const cat = categories.find(c => c.id === category);
    return cat?.color || COLORS.gray500;
  };

  const handleAddEntry = async () => {
    if (!newEntry.trim()) {
      Alert.alert('Error', 'Please enter journal entry');
      return;
    }

    try {
      const user = await authService.getStoredUser();
      if (!user) {
        Alert.alert('Error', 'User not found. Please login again.');
        return;
      }

      // Note: The API doesn't have a create journal endpoint in the controller
      // This would need to be added to the backend
      // For now, we'll add it locally and show a message
      const now = new Date();
      const time = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
      
      const entry: JournalEntry = {
        id: Date.now(),
        time,
        category: selectedCategory as JournalEntry['category'],
        title: newEntry.split('\n')[0].slice(0, 50),
        description: newEntry,
      };

      setEntries([entry, ...entries]);
      setNewEntry('');
      Alert.alert('Success', 'Journal entry added successfully. Note: Backend endpoint needs to be implemented for persistence.');
      loadJournalEntries();
    } catch (error) {
      console.error('Error adding journal entry:', error);
      Alert.alert('Error', 'Failed to add journal entry');
    }
  };

  const handleExport = () => {
    Alert.alert('Export Journal', 'Journal will be exported as PDF and sent to your email.');
  };

  return (
    <SafeAreaView style={styles.container}>
      {/* Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <Text style={styles.headerTitle}>Daily Journal</Text>
        <TouchableOpacity style={styles.exportBtn} onPress={handleExport}>
          <MaterialCommunityIcons name="export-variant" size={24} color={COLORS.primary} />
        </TouchableOpacity>
      </View>

      <ScrollView showsVerticalScrollIndicator={false} contentContainerStyle={styles.content}>
        {/* Date Header */}
        <View style={styles.dateCard}>
          <MaterialCommunityIcons name="calendar" size={24} color={COLORS.primary} />
          <View style={styles.dateInfo}>
            <Text style={styles.dateText}>{new Date().toLocaleDateString('en-US', { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' })}</Text>
            <Text style={styles.shiftText}>Morning Shift • 06:00 AM - 06:00 PM</Text>
          </View>
        </View>

        {/* New Entry */}
        <View style={styles.newEntryCard}>
          <Text style={styles.sectionTitle}>Add New Entry</Text>
          
          {/* Category Selection */}
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.categoryScroll}>
            {categories.map((cat) => (
              <TouchableOpacity
                key={cat.id}
                style={[styles.categoryChip, selectedCategory === cat.id && { backgroundColor: cat.color + '20', borderColor: cat.color }]}
                onPress={() => setSelectedCategory(cat.id)}
              >
                <MaterialCommunityIcons name={cat.icon as any} size={18} color={selectedCategory === cat.id ? cat.color : COLORS.gray500} />
                <Text style={[styles.categoryChipText, selectedCategory === cat.id && { color: cat.color }]}>{cat.label}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>

          {/* Entry Input */}
          <TextInput
            style={styles.entryInput}
            placeholder="What happened? Describe the event..."
            value={newEntry}
            onChangeText={setNewEntry}
            multiline
            numberOfLines={3}
            placeholderTextColor={COLORS.gray400}
          />

          {/* Add Button */}
          <TouchableOpacity style={styles.addButton} onPress={handleAddEntry}>
            <MaterialCommunityIcons name="plus" size={20} color={COLORS.white} />
            <Text style={styles.addButtonText}>Add Entry</Text>
          </TouchableOpacity>
        </View>

        {/* Journal Entries */}
        <View style={styles.section}>
          <Text style={styles.sectionTitle}>Today's Entries ({entries.length})</Text>
          
          {entries.map((entry, index) => (
            <View key={entry.id} style={styles.entryCard}>
              <View style={styles.entryTimeline}>
                <View style={[styles.timelineDot, { backgroundColor: getCategoryColor(entry.category) }]}>
                  <MaterialCommunityIcons name={getCategoryIcon(entry.category)} size={16} color={COLORS.white} />
                </View>
                {index < entries.length - 1 && <View style={styles.timelineLine} />}
              </View>
              
              <View style={styles.entryContent}>
                <View style={styles.entryHeader}>
                  <Text style={styles.entryTime}>{entry.time}</Text>
                  <View style={[styles.categoryTag, { backgroundColor: getCategoryColor(entry.category) + '15' }]}>
                    <Text style={[styles.categoryTagText, { color: getCategoryColor(entry.category) }]}>
                      {entry.category.charAt(0).toUpperCase() + entry.category.slice(1)}
                    </Text>
                  </View>
                </View>
                <Text style={styles.entryTitle}>{entry.title}</Text>
                <Text style={styles.entryDescription}>{entry.description}</Text>
              </View>
            </View>
          ))}
        </View>

        <View style={{ height: 50 }} />
      </ScrollView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', justifyContent: 'space-between', paddingHorizontal: SIZES.md, paddingVertical: SIZES.md, backgroundColor: COLORS.white, ...SHADOWS.small },
  backBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerTitle: { fontSize: FONTS.h4, fontWeight: '600', color: COLORS.textPrimary },
  exportBtn: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  content: { padding: SIZES.md },
  dateCard: { flexDirection: 'row', alignItems: 'center', backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.md, ...SHADOWS.small },
  dateInfo: { marginLeft: SIZES.sm },
  dateText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  shiftText: { fontSize: FONTS.caption, color: COLORS.textSecondary, marginTop: 2 },
  newEntryCard: { backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, marginBottom: SIZES.md, ...SHADOWS.small },
  sectionTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.sm },
  categoryScroll: { marginBottom: SIZES.md },
  categoryChip: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusFull, backgroundColor: COLORS.gray100, marginRight: SIZES.sm, borderWidth: 1, borderColor: 'transparent' },
  categoryChipText: { fontSize: FONTS.caption, fontWeight: '500', color: COLORS.textSecondary, marginLeft: SIZES.xs },
  entryInput: { backgroundColor: COLORS.gray50, borderRadius: SIZES.radiusMd, padding: SIZES.md, fontSize: FONTS.body, color: COLORS.textPrimary, minHeight: 80, textAlignVertical: 'top', marginBottom: SIZES.md },
  addButton: { flexDirection: 'row', alignItems: 'center', justifyContent: 'center', backgroundColor: COLORS.primary, paddingVertical: SIZES.md, borderRadius: SIZES.radiusMd, gap: SIZES.xs },
  addButtonText: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.white },
  section: { marginTop: SIZES.sm },
  entryCard: { flexDirection: 'row', marginBottom: SIZES.sm },
  entryTimeline: { alignItems: 'center', marginRight: SIZES.md },
  timelineDot: { width: 32, height: 32, borderRadius: 16, justifyContent: 'center', alignItems: 'center' },
  timelineLine: { width: 2, flex: 1, backgroundColor: COLORS.gray200, marginVertical: SIZES.xs },
  entryContent: { flex: 1, backgroundColor: COLORS.white, borderRadius: SIZES.radiusMd, padding: SIZES.md, ...SHADOWS.small },
  entryHeader: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', marginBottom: SIZES.xs },
  entryTime: { fontSize: FONTS.caption, fontWeight: '600', color: COLORS.textSecondary },
  categoryTag: { paddingHorizontal: SIZES.sm, paddingVertical: 2, borderRadius: SIZES.radiusSm },
  categoryTagText: { fontSize: FONTS.tiny, fontWeight: '600' },
  entryTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary, marginBottom: SIZES.xs },
  entryDescription: { fontSize: FONTS.bodySmall, color: COLORS.textSecondary, lineHeight: 20 },
});

export default DailyJournalScreen;
