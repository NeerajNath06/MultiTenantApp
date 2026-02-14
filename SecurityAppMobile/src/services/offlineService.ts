import AsyncStorage from '@react-native-async-storage/async-storage';

export interface OfflineQueueItem {
  id: string;
  endpoint: string;
  method: 'POST' | 'PUT' | 'PATCH' | 'DELETE';
  data: any;
  timestamp: number;
  retries: number;
}

class OfflineService {
  private readonly QUEUE_KEY = '@offline_queue';
  private readonly MAX_RETRIES = 3;
  private readonly SYNC_INTERVAL = 30000; // 30 seconds
  private syncInterval: NodeJS.Timeout | null = null;

  /**
   * Initialize offline service and start syncing
   */
  async initialize(): Promise<void> {
    await this.startSync();
  }

  /**
   * Add request to offline queue
   */
  async addToQueue(endpoint: string, method: 'POST' | 'PUT' | 'PATCH' | 'DELETE', data: any): Promise<string> {
    try {
      const queue = await this.getQueue();
      const item: OfflineQueueItem = {
        id: `${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
        endpoint,
        method,
        data,
        timestamp: Date.now(),
        retries: 0,
      };

      queue.push(item);
      await this.saveQueue(queue);
      return item.id;
    } catch (error) {
      console.error('Error adding to offline queue:', error);
      throw error;
    }
  }

  /**
   * Get offline queue
   */
  async getQueue(): Promise<OfflineQueueItem[]> {
    try {
      const queueJson = await AsyncStorage.getItem(this.QUEUE_KEY);
      return queueJson ? JSON.parse(queueJson) : [];
    } catch (error) {
      console.error('Error getting offline queue:', error);
      return [];
    }
  }

  /**
   * Save offline queue
   */
  private async saveQueue(queue: OfflineQueueItem[]): Promise<void> {
    try {
      await AsyncStorage.setItem(this.QUEUE_KEY, JSON.stringify(queue));
    } catch (error) {
      console.error('Error saving offline queue:', error);
    }
  }

  /**
   * Remove item from queue
   */
  async removeFromQueue(itemId: string): Promise<void> {
    try {
      const queue = await this.getQueue();
      const filtered = queue.filter(item => item.id !== itemId);
      await this.saveQueue(filtered);
    } catch (error) {
      console.error('Error removing from queue:', error);
    }
  }

  /**
   * Clear offline queue
   */
  async clearQueue(): Promise<void> {
    try {
      await AsyncStorage.removeItem(this.QUEUE_KEY);
    } catch (error) {
      console.error('Error clearing queue:', error);
    }
  }

  /**
   * Process offline queue
   */
  async processQueue(baseUrl: string, getAuthToken: () => Promise<string | null>): Promise<void> {
    try {
      const queue = await this.getQueue();
      if (queue.length === 0) return;

      const token = await getAuthToken();
      if (!token) {
        console.log('No auth token, skipping queue processing');
        return;
      }

      const processed: string[] = [];
      
      for (const item of queue) {
        try {
          const response = await fetch(`${baseUrl}${item.endpoint}`, {
            method: item.method,
            headers: {
              'Content-Type': 'application/json',
              'Authorization': `Bearer ${token}`,
            },
            body: JSON.stringify(item.data),
          });

          if (response.ok) {
            processed.push(item.id);
          } else if (item.retries >= this.MAX_RETRIES) {
            // Max retries reached, remove from queue
            processed.push(item.id);
            console.warn(`Max retries reached for ${item.endpoint}, removing from queue`);
          } else {
            // Increment retry count
            item.retries += 1;
          }
        } catch (error) {
          console.error(`Error processing queue item ${item.id}:`, error);
          if (item.retries >= this.MAX_RETRIES) {
            processed.push(item.id);
          } else {
            item.retries += 1;
          }
        }
      }

      // Remove processed items
      if (processed.length > 0) {
        const remaining = queue.filter(item => !processed.includes(item.id));
        await this.saveQueue(remaining);
      } else {
        // Update retry counts
        await this.saveQueue(queue);
      }
    } catch (error) {
      console.error('Error processing offline queue:', error);
    }
  }

  /**
   * Start automatic sync
   */
  startSync(): void {
    if (this.syncInterval) {
      clearInterval(this.syncInterval);
    }

    // Sync will be triggered manually when network is available
    // This is just a placeholder for future implementation
  }

  /**
   * Stop automatic sync
   */
  stopSync(): void {
    if (this.syncInterval) {
      clearInterval(this.syncInterval);
      this.syncInterval = null;
    }
  }

  /**
   * Check if device is online
   */
  async isOnline(): Promise<boolean> {
    try {
      // Simple network check
      const response = await fetch('https://www.google.com', {
        method: 'HEAD',
        mode: 'no-cors',
        cache: 'no-cache',
      });
      return true;
    } catch {
      return false;
    }
  }

  /**
   * Get queue size
   */
  async getQueueSize(): Promise<number> {
    const queue = await this.getQueue();
    return queue.length;
  }
}

export const offlineService = new OfflineService();

