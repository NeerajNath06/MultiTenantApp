import AsyncStorage from '@react-native-async-storage/async-storage';

export interface CacheItem<T> {
  data: T;
  timestamp: number;
  expiry: number;
}

class CacheService {
  private readonly CACHE_PREFIX = '@cache_';
  private readonly DEFAULT_TTL = 5 * 60 * 1000; // 5 minutes

  /**
   * Set cache item
   */
  async set<T>(key: string, data: T, ttl: number = this.DEFAULT_TTL): Promise<void> {
    try {
      const item: CacheItem<T> = {
        data,
        timestamp: Date.now(),
        expiry: Date.now() + ttl,
      };
      await AsyncStorage.setItem(`${this.CACHE_PREFIX}${key}`, JSON.stringify(item));
    } catch (error) {
      console.error(`Error setting cache for key ${key}:`, error);
    }
  }

  /**
   * Get cache item
   */
  async get<T>(key: string): Promise<T | null> {
    try {
      const itemJson = await AsyncStorage.getItem(`${this.CACHE_PREFIX}${key}`);
      if (!itemJson) return null;

      const item: CacheItem<T> = JSON.parse(itemJson);

      // Check if expired
      if (Date.now() > item.expiry) {
        await this.remove(key);
        return null;
      }

      return item.data;
    } catch (error) {
      console.error(`Error getting cache for key ${key}:`, error);
      return null;
    }
  }

  /**
   * Remove cache item
   */
  async remove(key: string): Promise<void> {
    try {
      await AsyncStorage.removeItem(`${this.CACHE_PREFIX}${key}`);
    } catch (error) {
      console.error(`Error removing cache for key ${key}:`, error);
    }
  }

  /**
   * Clear all cache
   */
  async clear(): Promise<void> {
    try {
      const keys = await AsyncStorage.getAllKeys();
      const cacheKeys = keys.filter(key => key.startsWith(this.CACHE_PREFIX));
      await AsyncStorage.multiRemove(cacheKeys);
    } catch (error) {
      console.error('Error clearing cache:', error);
    }
  }

  /**
   * Clear expired cache items
   */
  async clearExpired(): Promise<void> {
    try {
      const keys = await AsyncStorage.getAllKeys();
      const cacheKeys = keys.filter(key => key.startsWith(this.CACHE_PREFIX));
      
      for (const key of cacheKeys) {
        const itemJson = await AsyncStorage.getItem(key);
        if (itemJson) {
          const item: CacheItem<any> = JSON.parse(itemJson);
          if (Date.now() > item.expiry) {
            await AsyncStorage.removeItem(key);
          }
        }
      }
    } catch (error) {
      console.error('Error clearing expired cache:', error);
    }
  }

  /**
   * Check if cache exists and is valid
   */
  async exists(key: string): Promise<boolean> {
    try {
      const itemJson = await AsyncStorage.getItem(`${this.CACHE_PREFIX}${key}`);
      if (!itemJson) return false;

      const item: CacheItem<any> = JSON.parse(itemJson);
      return Date.now() <= item.expiry;
    } catch {
      return false;
    }
  }

  /**
   * Get cache with fallback function
   */
  async getOrSet<T>(
    key: string,
    fetchFn: () => Promise<T>,
    ttl?: number
  ): Promise<T> {
    // Try to get from cache first
    const cached = await this.get<T>(key);
    if (cached !== null) {
      return cached;
    }

    // Fetch fresh data
    const data = await fetchFn();
    
    // Cache it
    await this.set(key, data, ttl);

    return data;
  }

  /**
   * Invalidate cache by pattern
   */
  async invalidatePattern(pattern: string): Promise<void> {
    try {
      const keys = await AsyncStorage.getAllKeys();
      const cacheKeys = keys.filter(key => 
        key.startsWith(this.CACHE_PREFIX) && key.includes(pattern)
      );
      await AsyncStorage.multiRemove(cacheKeys);
    } catch (error) {
      console.error('Error invalidating cache pattern:', error);
    }
  }
}

export const cacheService = new CacheService();

