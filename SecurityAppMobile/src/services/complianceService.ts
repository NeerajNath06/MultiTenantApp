import BaseApiService from './baseApiService';

export interface ComplianceItem {
  id: string;
  title: string;
  category: string;
  status: string;
  dueDate?: string;
  details: string;
  affectedCount: number;
}

export interface ComplianceSummary {
  compliantCount: number;
  warningCount: number;
  nonCompliantCount: number;
  overallScorePercent: number;
  items: ComplianceItem[];
}

class ComplianceService extends BaseApiService {
  async getSummary(supervisorId?: string): Promise<{ success: boolean; data?: ComplianceSummary; error?: { code: string; message: string } }> {
    const params = supervisorId ? { supervisorId } : undefined;
    return await this.get<ComplianceSummary>('/api/v1/Compliance/summary', params);
  }
}

export const complianceService = new ComplianceService();
