import { ApiService } from '@/services/apiService'

const API = import.meta.env.VITE_API_BASE_URL

export interface PushSubscriptionPayload {
  endpoint: string
  p256dh: string
  auth: string
}

export interface MutedGroup {
  groupId: string
}

export interface PushPreferences {
  directMessage: boolean
  announcement: boolean
  groupPost: boolean
  mutedGroups: MutedGroup[]
}

export interface PushPreferencesUpdate {
  directMessage: boolean
  announcement: boolean
  groupPost: boolean
}

export class PushService extends ApiService {
  async getVapidPublicKey(): Promise<string> {
    const { data } = await this._httpClient.get<{ publicKey: string }>(`${API}/social/push/vapid-public-key`)
    return data.publicKey
  }

  async createSubscription(payload: PushSubscriptionPayload): Promise<void> {
    await this._httpClient.post(`${API}/social/push/subscriptions`, payload, this.headersWithJsonContentType())
  }

  async deleteSubscription(endpoint: string): Promise<void> {
    await this._httpClient.delete(`${API}/social/push/subscriptions`, {
      ...this.headersWithJsonContentType(),
      data: { endpoint }
    })
  }

  async getPreferences(): Promise<PushPreferences> {
    const { data } = await this._httpClient.get<PushPreferences>(`${API}/social/push/preferences`)
    return data
  }

  async updatePreferences(prefs: PushPreferencesUpdate): Promise<void> {
    await this._httpClient.put(`${API}/social/push/preferences`, prefs, this.headersWithJsonContentType())
  }

  async updateGroupPreference(groupId: string, enabled: boolean): Promise<void> {
    await this._httpClient.put(`${API}/social/push/preferences/groups/${groupId}`, { groupId, enabled }, this.headersWithJsonContentType())
  }
}
