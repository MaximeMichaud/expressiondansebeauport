import { injectable } from "inversify"
import { ApiService } from "@/services/apiService"
import { SucceededOrNotResponse } from "@/types/responses"
import type { Group, GroupMember, Member, Post, Comment, Conversation, Message } from "@/types/entities"

const API = import.meta.env.VITE_API_BASE_URL

// Normalize PascalCase keys from C# backend to camelCase
function toCamel(obj: any): any {
  if (Array.isArray(obj)) return obj.map(toCamel)
  if (obj !== null && typeof obj === 'object') {
    return Object.keys(obj).reduce((acc: any, key) => {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1)
      acc[camelKey] = toCamel(obj[key])
      return acc
    }, {})
  }
  return obj
}

@injectable()
export class SocialService extends ApiService {
  // === Member Registration ===
  async register(firstName: string, lastName: string, email: string, password: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/members/register`, { firstName, lastName, email, password }, this.headersWithJsonContentType())
    return response.data
  }

  async confirmEmail(email: string, code: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/members/confirm`, { email, code }, this.headersWithJsonContentType())
    return response.data
  }

  async resendCode(email: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/members/resend-code`, { email }, this.headersWithJsonContentType())
    return response.data
  }

  // === Groups ===
  async getMyGroups(): Promise<Group[]> {
    const response = await this._httpClient.get(`${API}/social/groups/mine`)
    return toCamel(response.data)
  }

  async createGroup(name: string, description: string, season: string, inviteCode: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/admin/groups`, { name, description, season, inviteCode: inviteCode || undefined }, this.headersWithJsonContentType())
    return response.data
  }

  async getActiveGroups(): Promise<Group[]> {
    const response = await this._httpClient.get(`${API}/social/groups/active`)
    return toCamel(response.data)
  }

  async getGroupDetails(id: string): Promise<any> {
    const response = await this._httpClient.get(`${API}/social/groups/${id}`)
    return toCamel(response.data)
  }

  async getGroupMembers(groupId: string, page: number = 1): Promise<GroupMember[]> {
    const response = await this._httpClient.get(`${API}/social/groups/${groupId}/members?Page=${page}`)
    const data = toCamel(response.data)
    return data.items || data
  }

  async joinGroup(inviteCode: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/groups/join`, { inviteCode }, this.headersWithJsonContentType())
    return response.data
  }

  // === Posts ===
  async getGroupFeed(groupId: string, page: number = 1): Promise<Post[]> {
    const response = await this._httpClient.get(`${API}/social/groups/${groupId}/posts?Page=${page}`)
    return toCamel(response.data)
  }

  async getPost(id: string): Promise<Post> {
    const response = await this._httpClient.get(`${API}/social/posts/${id}`)
    return toCamel(response.data)
  }

  async createPost(
    groupId: string,
    content: string,
    type: 'Text' | 'Photo' | 'Poll' | 'File' = 'Text',
    media?: Array<{
      displayUrl: string
      thumbnailUrl: string
      originalUrl: string
      contentType: string
      size: number
    }>
  ): Promise<SucceededOrNotResponse> {
    const body: Record<string, unknown> = {
      groupId,
      content,
      type,
      media: media ?? []
    }
    const response = await this._httpClient.post<SucceededOrNotResponse>(
      `${API}/social/posts`,
      body,
      this.headersWithJsonContentType())
    return response.data
  }

  async deletePost(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(`${API}/social/posts/${id}`)
    return response.data
  }

  async toggleLike(postId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/posts/${postId}/like`, {}, this.headersWithJsonContentType())
    return response.data
  }

  async recordView(postId: string): Promise<void> {
    await this._httpClient.post(`${API}/social/posts/${postId}/view`, {}, this.headersWithJsonContentType())
  }

  async pinPost(postId: string, groupId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/social/posts/${postId}/pin`, { groupId }, this.headersWithJsonContentType())
    return response.data
  }

  // === Comments ===
  async getComments(postId: string, page: number = 1): Promise<Comment[]> {
    const response = await this._httpClient.get(`${API}/social/posts/${postId}/comments?Page=${page}`)
    return toCamel(response.data)
  }

  async addComment(postId: string, content: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/posts/${postId}/comments`, { content }, this.headersWithJsonContentType())
    return response.data
  }

  async deleteComment(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(`${API}/social/comments/${id}`)
    return response.data
  }

  // === Polls ===
  async votePoll(postId: string, pollOptionId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/posts/${postId}/poll/vote`, { pollOptionId }, this.headersWithJsonContentType())
    return response.data
  }

  async createPoll(
    groupId: string,
    question: string,
    options: string[],
    allowMultipleAnswers: boolean
  ): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(
      `${API}/social/groups/${groupId}/polls`,
      { groupId, question, options, allowMultipleAnswers },
      this.headersWithJsonContentType()
    )
    return response.data
  }

  // === Announcements ===
  async createAnnouncement(content: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/announcements`, { content }, this.headersWithJsonContentType())
    return response.data
  }

  async getAnnouncements(page: number = 1): Promise<Post[]> {
    const response = await this._httpClient.get(`${API}/social/announcements?Page=${page}`)
    return toCamel(response.data)
  }

  // === Conversations ===
  async getConversations(page: number = 1): Promise<Conversation[]> {
    const response = await this._httpClient.get(`${API}/social/conversations?Page=${page}`)
    return toCamel(response.data)
  }

  async getMessages(conversationId: string, page: number = 1): Promise<Message[]> {
    const response = await this._httpClient.get(`${API}/social/conversations/${conversationId}/messages?Page=${page}`)
    return toCamel(response.data)
  }

  async sendMessage(
    conversationId: string,
    content: string,
    media?: { displayUrl: string; thumbnailUrl: string; originalUrl: string }
  ): Promise<SucceededOrNotResponse> {
    const body: Record<string, unknown> = {
      conversationId,
      content
    }
    if (media) {
      body.mediaUrl = media.displayUrl
      body.mediaThumbnailUrl = media.thumbnailUrl
      body.mediaOriginalUrl = media.originalUrl
    }
    const response = await this._httpClient.post<SucceededOrNotResponse>(
      `${API}/social/conversations/${conversationId}/messages`,
      body,
      this.headersWithJsonContentType())
    return response.data
  }

  async startConversation(otherMemberId: string): Promise<any> {
    const response = await this._httpClient.post(`${API}/social/conversations`, { otherMemberId }, this.headersWithJsonContentType())
    return toCamel(response.data)
  }

  async deleteMessage(messageId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(`${API}/social/messages/${messageId}`)
    return response.data
  }

  async markAsRead(conversationId: string): Promise<void> {
    await this._httpClient.put(`${API}/social/conversations/${conversationId}/read`, null)
  }

  async getUnreadCount(): Promise<number> {
    const response = await this._httpClient.get(`${API}/social/messages/unread-count`)
    const data = toCamel(response.data)
    return data.count || 0
  }

  // === Members ===
  async getMyProfile(): Promise<Member> {
    const response = await this._httpClient.get(`${API}/social/members/me`)
    return toCamel(response.data)
  }

  async updateMyProfile(firstName: string, lastName: string, email: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/social/members/me`, { firstName, lastName, email }, this.headersWithJsonContentType())
    return response.data
  }

  async searchMembers(query: string): Promise<any[]> {
    const response = await this._httpClient.get(`${API}/social/members/search?Query=${encodeURIComponent(query)}`)
    return toCamel(response.data)
  }

  async getMemberProfile(id: string): Promise<any> {
    const response = await this._httpClient.get(`${API}/social/members/${id}`)
    return toCamel(response.data)
  }

  async deleteMember(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(`${API}/admin/members/${id}`)
    return response.data
  }

  async promoteMember(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/admin/members/${id}/promote`, {}, this.headersWithJsonContentType())
    return response.data
  }

  async demoteMember(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/admin/members/${id}/demote`, {}, this.headersWithJsonContentType())
    return response.data
  }

  // === Upload ===
  async uploadFile(file: File): Promise<{
    succeeded: boolean
    // image fields:
    displayUrl?: string
    thumbnailUrl?: string
    originalUrl?: string
    width?: number
    height?: number
    // pdf rétro-compat:
    url?: string
    fileName?: string
    // common:
    contentType?: string
    size?: number
    errors?: Array<{ errorType: string; errorMessage: string }>
  }> {
    const formData = new FormData()
    formData.append('file', file)
    const response = await this._httpClient.post(`${API}/social/upload`, formData, this.headersWithFormDataContentType())
    return response.data
  }
}
