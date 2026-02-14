import React, { useState, useRef, useEffect } from 'react';
import { View, Text, StyleSheet, ScrollView, TouchableOpacity, TextInput, KeyboardAvoidingView, Platform } from 'react-native';
import { SafeAreaView } from 'react-native-safe-area-context';
import { MaterialCommunityIcons } from '@expo/vector-icons';
import { COLORS, FONTS, SIZES, SHADOWS } from '../../constants/theme';

interface Message {
  id: string;
  text: string;
  sender: 'user' | 'support';
  timestamp: Date;
  status?: 'sent' | 'delivered' | 'read';
}

const quickReplies = [
  'I need help with check-in',
  'Issue with attendance',
  'Document upload problem',
  'App not working properly',
  'Other issue',
];

function LiveChatScreen({ navigation }: any) {
  const scrollViewRef = useRef<ScrollView>(null);
  const [message, setMessage] = useState('');
  const [messages, setMessages] = useState<Message[]>([
    {
      id: '1',
      text: 'Hello! Welcome to Security App Support. How can I help you today?',
      sender: 'support',
      timestamp: new Date(Date.now() - 60000),
    },
  ]);
  const [isTyping, setIsTyping] = useState(false);

  useEffect(() => {
    scrollViewRef.current?.scrollToEnd({ animated: true });
  }, [messages]);

  const sendMessage = (text: string) => {
    if (!text.trim()) return;

    const newMessage: Message = {
      id: Date.now().toString(),
      text: text.trim(),
      sender: 'user',
      timestamp: new Date(),
      status: 'sent',
    };

    setMessages(prev => [...prev, newMessage]);
    setMessage('');

    // Simulate support typing
    setIsTyping(true);
    setTimeout(() => {
      setIsTyping(false);
      const supportResponse: Message = {
        id: (Date.now() + 1).toString(),
        text: getAutoResponse(text),
        sender: 'support',
        timestamp: new Date(),
      };
      setMessages(prev => [...prev, supportResponse]);
    }, 1500);
  };

  const getAutoResponse = (userMessage: string): string => {
    const lowerMessage = userMessage.toLowerCase();
    if (lowerMessage.includes('check-in') || lowerMessage.includes('checkin')) {
      return 'For check-in issues, please ensure your location services are enabled and you are within the designated area. If the problem persists, try restarting the app. Would you like me to connect you with a supervisor?';
    } else if (lowerMessage.includes('attendance')) {
      return 'I can help with attendance queries. Could you please describe the specific issue you are facing? For example, is it about marking attendance, viewing history, or requesting corrections?';
    } else if (lowerMessage.includes('document')) {
      return 'For document uploads, make sure the file is in JPG, PNG, or PDF format and under 5MB. If you are still facing issues, please try clearing the app cache and retry.';
    } else if (lowerMessage.includes('app') || lowerMessage.includes('not working')) {
      return 'I am sorry to hear you are experiencing technical issues. Please try these steps: 1) Check your internet connection, 2) Clear the app cache, 3) Update to the latest version. If the issue continues, please provide more details.';
    } else {
      return 'Thank you for your message. A support agent will respond shortly. In the meantime, you can check our FAQ section for quick answers. Is there anything specific I can help you with?';
    }
  };

  const formatTime = (date: Date): string => {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  };

  return (
    <SafeAreaView style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backButton} onPress={() => navigation.goBack()}>
          <MaterialCommunityIcons name="arrow-left" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
        <View style={styles.headerInfo}>
          <View style={styles.supportAvatar}>
            <MaterialCommunityIcons name="headset" size={24} color={COLORS.white} />
          </View>
          <View>
            <Text style={styles.headerTitle}>Support Chat</Text>
            <View style={styles.onlineStatus}>
              <View style={styles.onlineDot} />
              <Text style={styles.onlineText}>Online</Text>
            </View>
          </View>
        </View>
        <TouchableOpacity style={styles.menuButton}>
          <MaterialCommunityIcons name="dots-vertical" size={24} color={COLORS.textPrimary} />
        </TouchableOpacity>
      </View>

      <KeyboardAvoidingView 
        style={styles.chatContainer} 
        behavior={Platform.OS === 'ios' ? 'padding' : undefined}
        keyboardVerticalOffset={Platform.OS === 'ios' ? 90 : 0}
      >
        <ScrollView 
          ref={scrollViewRef}
          style={styles.messagesContainer}
          contentContainerStyle={styles.messagesContent}
          showsVerticalScrollIndicator={false}
        >
          {messages.map((msg) => (
            <View key={msg.id} style={[styles.messageWrapper, msg.sender === 'user' ? styles.userMessageWrapper : styles.supportMessageWrapper]}>
              {msg.sender === 'support' && (
                <View style={styles.messageAvatar}>
                  <MaterialCommunityIcons name="headset" size={16} color={COLORS.primary} />
                </View>
              )}
              <View style={[styles.messageBubble, msg.sender === 'user' ? styles.userBubble : styles.supportBubble]}>
                <Text style={[styles.messageText, msg.sender === 'user' && styles.userMessageText]}>{msg.text}</Text>
                <View style={styles.messageFooter}>
                  <Text style={[styles.timestamp, msg.sender === 'user' && styles.userTimestamp]}>{formatTime(msg.timestamp)}</Text>
                  {msg.sender === 'user' && msg.status && (
                    <MaterialCommunityIcons 
                      name={msg.status === 'read' ? 'check-all' : msg.status === 'delivered' ? 'check-all' : 'check'} 
                      size={14} 
                      color={msg.status === 'read' ? COLORS.info : COLORS.white + '80'} 
                    />
                  )}
                </View>
              </View>
            </View>
          ))}

          {isTyping && (
            <View style={styles.typingContainer}>
              <View style={styles.messageAvatar}>
                <MaterialCommunityIcons name="headset" size={16} color={COLORS.primary} />
              </View>
              <View style={styles.typingBubble}>
                <View style={styles.typingDots}>
                  <View style={[styles.dot, styles.dot1]} />
                  <View style={[styles.dot, styles.dot2]} />
                  <View style={[styles.dot, styles.dot3]} />
                </View>
              </View>
            </View>
          )}
        </ScrollView>

        {/* Quick Replies */}
        {messages.length === 1 && (
          <ScrollView horizontal showsHorizontalScrollIndicator={false} style={styles.quickRepliesScroll} contentContainerStyle={styles.quickRepliesContainer}>
            {quickReplies.map((reply, index) => (
              <TouchableOpacity key={index} style={styles.quickReplyChip} onPress={() => sendMessage(reply)}>
                <Text style={styles.quickReplyText}>{reply}</Text>
              </TouchableOpacity>
            ))}
          </ScrollView>
        )}

        {/* Input Area */}
        <View style={styles.inputContainer}>
          <TouchableOpacity style={styles.attachButton}>
            <MaterialCommunityIcons name="paperclip" size={24} color={COLORS.gray400} />
          </TouchableOpacity>
          <TextInput
            style={styles.input}
            placeholder="Type a message..."
            value={message}
            onChangeText={setMessage}
            placeholderTextColor={COLORS.gray400}
            multiline
            maxLength={500}
          />
          <TouchableOpacity 
            style={[styles.sendButton, message.trim() && styles.sendButtonActive]} 
            onPress={() => sendMessage(message)}
            disabled={!message.trim()}
          >
            <MaterialCommunityIcons name="send" size={24} color={message.trim() ? COLORS.white : COLORS.gray400} />
          </TouchableOpacity>
        </View>
      </KeyboardAvoidingView>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: COLORS.background },
  header: { flexDirection: 'row', alignItems: 'center', paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, backgroundColor: COLORS.white, ...SHADOWS.small },
  backButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  headerInfo: { flex: 1, flexDirection: 'row', alignItems: 'center', marginLeft: SIZES.sm },
  supportAvatar: { width: 40, height: 40, borderRadius: 20, backgroundColor: COLORS.primary, justifyContent: 'center', alignItems: 'center', marginRight: SIZES.sm },
  headerTitle: { fontSize: FONTS.body, fontWeight: '600', color: COLORS.textPrimary },
  onlineStatus: { flexDirection: 'row', alignItems: 'center' },
  onlineDot: { width: 8, height: 8, borderRadius: 4, backgroundColor: COLORS.success, marginRight: 4 },
  onlineText: { fontSize: FONTS.caption, color: COLORS.success },
  menuButton: { width: 40, height: 40, borderRadius: 20, justifyContent: 'center', alignItems: 'center' },
  chatContainer: { flex: 1 },
  messagesContainer: { flex: 1 },
  messagesContent: { padding: SIZES.md },
  messageWrapper: { marginBottom: SIZES.sm, flexDirection: 'row', alignItems: 'flex-end' },
  userMessageWrapper: { justifyContent: 'flex-end' },
  supportMessageWrapper: { justifyContent: 'flex-start' },
  messageAvatar: { width: 28, height: 28, borderRadius: 14, backgroundColor: COLORS.primary + '15', justifyContent: 'center', alignItems: 'center', marginRight: SIZES.xs },
  messageBubble: { maxWidth: '75%', paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd },
  userBubble: { backgroundColor: COLORS.primary, borderBottomRightRadius: 4 },
  supportBubble: { backgroundColor: COLORS.white, borderBottomLeftRadius: 4, ...SHADOWS.small },
  messageText: { fontSize: FONTS.body, color: COLORS.textPrimary, lineHeight: 20 },
  userMessageText: { color: COLORS.white },
  messageFooter: { flexDirection: 'row', alignItems: 'center', justifyContent: 'flex-end', marginTop: 4, gap: 4 },
  timestamp: { fontSize: FONTS.tiny, color: COLORS.gray500 },
  userTimestamp: { color: COLORS.white + '80' },
  typingContainer: { flexDirection: 'row', alignItems: 'flex-end', marginBottom: SIZES.sm },
  typingBubble: { backgroundColor: COLORS.white, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusMd, ...SHADOWS.small },
  typingDots: { flexDirection: 'row', alignItems: 'center', gap: 4 },
  dot: { width: 8, height: 8, borderRadius: 4, backgroundColor: COLORS.gray400 },
  dot1: { opacity: 0.4 },
  dot2: { opacity: 0.6 },
  dot3: { opacity: 0.8 },
  quickRepliesScroll: { maxHeight: 50, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  quickRepliesContainer: { padding: SIZES.sm, gap: SIZES.sm },
  quickReplyChip: { backgroundColor: COLORS.white, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, borderRadius: SIZES.radiusFull, borderWidth: 1, borderColor: COLORS.primary },
  quickReplyText: { fontSize: FONTS.bodySmall, color: COLORS.primary },
  inputContainer: { flexDirection: 'row', alignItems: 'flex-end', padding: SIZES.sm, backgroundColor: COLORS.white, borderTopWidth: 1, borderTopColor: COLORS.gray100 },
  attachButton: { width: 44, height: 44, justifyContent: 'center', alignItems: 'center' },
  input: { flex: 1, backgroundColor: COLORS.gray100, borderRadius: SIZES.radiusMd, paddingHorizontal: SIZES.md, paddingVertical: SIZES.sm, maxHeight: 100, fontSize: FONTS.body, color: COLORS.textPrimary },
  sendButton: { width: 44, height: 44, borderRadius: 22, backgroundColor: COLORS.gray200, justifyContent: 'center', alignItems: 'center', marginLeft: SIZES.xs },
  sendButtonActive: { backgroundColor: COLORS.primary },
});

export default LiveChatScreen;
