import BaseApiService from './baseApiService';

export interface DailyJournal {
  id: string;
  guardId: string;
  guardName?: string;
  siteId: string;
  siteName?: string;
  journalDate: string;
  entry: string;
  notes?: string;
}

export interface ShiftHandover {
  id: string;
  fromGuardId: string;
  fromGuardName?: string;
  toGuardId: string;
  toGuardName?: string;
  siteId: string;
  siteName?: string;
  handoverDate: string;
  notes: string;
  acknowledged: boolean;
}

class JournalService extends BaseApiService {
  async getJournals(filters?: {
    guardId?: string;
    siteId?: string;
    dateFrom?: string;
    dateTo?: string;
    pageNumber?: number;
    pageSize?: number;
  }): Promise<{ success: boolean; data?: any; error?: { code: string; message: string } }> {
    return await this.get('/api/v1/Journals', filters);
  }

  async getJournalById(id: string): Promise<{ success: boolean; data?: DailyJournal; error?: { code: string; message: string } }> {
    return await this.get<DailyJournal>(`/api/v1/Journals/${id}`);
  }

  async createJournal(journal: {
    guardId: string;
    siteId: string;
    journalDate: string;
    entry: string;
    notes?: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: DailyJournal; error?: { code: string; message: string } }> {
    return await this.post<DailyJournal>('/api/v1/Journals', journal);
  }

  async createShiftHandover(handover: {
    fromGuardId: string;
    toGuardId: string;
    siteId: string;
    handoverDate: string;
    notes: string;
    agencyId?: string;
  }): Promise<{ success: boolean; data?: ShiftHandover; error?: { code: string; message: string } }> {
    return await this.post<ShiftHandover>('/api/v1/Journals/handover', handover);
  }

  async acknowledgeHandover(id: string): Promise<{ success: boolean; data?: ShiftHandover; error?: { code: string; message: string } }> {
    return await this.patch<ShiftHandover>(`/api/v1/Journals/handover/${id}/acknowledge`, {});
  }
}

export const journalService = new JournalService();
