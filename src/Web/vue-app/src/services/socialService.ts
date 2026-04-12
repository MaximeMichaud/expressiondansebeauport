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

  async createGroup(name: string, description: string, season: string, inviteCode: string, imageUrl?: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/admin/groups`, { name, description, season, inviteCode: inviteCode || undefined, imageUrl }, this.headersWithJsonContentType())
    return response.data
  }

  async updateGroup(id: string, name: string, description: string, season: string, imageUrl?: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/admin/groups/${id}`, { name, description, season, imageUrl }, this.headersWithJsonContentType())
    return response.data
  }

  async deleteGroup(id: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(`${API}/admin/groups/${id}`)
    return response.data
  }

  async getActiveGroups(): Promise<Group[]> {
    const response = await this._httpClient.get(`${API}/social/groups/active`)
    return toCamel(response.data)
  }

  async leaveGroup(groupId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(`${API}/social/groups/${groupId}/leave`)
    return response.data
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
  async getGroupFeed(groupId: string, page: number = 1): Promise<{ items: Post[]; hasMore: boolean }> {
    const response = await this._httpClient.get(`${API}/social/groups/${groupId}/posts?Page=${page}`)
    const data = toCamel(response.data)
    return { items: data.items, hasMore: data.hasMore }
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
  async getComments(postId: string, page: number = 1): Promise<{ items: Comment[]; hasMore: boolean }> {
    const response = await this._httpClient.get(`${API}/social/posts/${postId}/comments?Page=${page}`)
    const data = toCamel(response.data)
    return { items: data.items, hasMore: data.hasMore }
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

  // === Member profile image ===
  async setMyProfileImage(imageUrl: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(
      `${API}/social/members/me/profile-image`,
      { imageUrl },
      this.headersWithJsonContentType()
    )
    return response.data
  }

  async removeMyProfileImage(): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(
      `${API}/social/members/me/profile-image`
    )
    return response.data
  }

  // === Group image ===
  async setGroupImage(groupId: string, imageUrl: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(
      `${API}/social/groups/${groupId}/image`,
      { groupId, imageUrl },
      this.headersWithJsonContentType()
    )
    return response.data
  }

  async removeGroupImage(groupId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.delete<SucceededOrNotResponse>(
      `${API}/social/groups/${groupId}/image`
    )
    return response.data
  }

  // === Announcements ===
  async createAnnouncement(content: string, media?: Array<{ displayUrl: string; thumbnailUrl: string; originalUrl: string; contentType: string; size: number }>): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/announcements`, { content, media: media ?? [] }, this.headersWithJsonContentType())
    return response.data
  }

  async updateAnnouncement(id: string, content: string, media?: Array<{ displayUrl: string; thumbnailUrl: string; originalUrl: string; contentType: string; size: number }>): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/social/announcements/${id}`, { content, media: media ?? [] }, this.headersWithJsonContentType())
    return response.data
  }

  async getAnnouncements(page: number = 1): Promise<{ items: Post[]; hasMore: boolean }> {
    const response = await this._httpClient.get(`${API}/social/announcements?Page=${page}`)
    const data = toCamel(response.data)
    return { items: data.items, hasMore: data.hasMore }
  }

  // === Conversations ===
  async getConversations(page: number = 1): Promise<{ items: Conversation[]; hasMore: boolean }> {
    const response = await this._httpClient.get(`${API}/social/conversations?Page=${page}`)
    const data = toCamel(response.data)
    return { items: data.items, hasMore: data.hasMore }
  }

  async getMessages(conversationId: string, page: number = 1): Promise<{ items: Message[]; hasMore: boolean }> {
    const response = await this._httpClient.get(`${API}/social/conversations/${conversationId}/messages?Page=${page}`)
    const data = toCamel(response.data)
    return { items: data.items, hasMore: data.hasMore }
  }

  async sendMessage(
    conversationId: string,
    content: string,
    media?: Array<{
      displayUrl: string
      thumbnailUrl: string
      originalUrl: string
      contentType: string
      size: number
    }>
  ): Promise<SucceededOrNotResponse> {
    const body: Record<string, unknown> = {
      conversationId,
      content,
      media: media ?? []
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

  async getUnreadCount(): Promise<{ count: number, joinRequestNotifications?: Array<{ id: string, groupName: string, status: string }> }> {
    const response = await this._httpClient.get(`${API}/social/messages/unread-count`)
    const data = toCamel(response.data)
    return { count: data.count || 0, joinRequestNotifications: data.joinRequestNotifications }
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

  // === Join Requests ===
  async requestJoinGroup(groupId: string): Promise<any> {
    const response = await this._httpClient.post(`${API}/social/groups/${groupId}/join-requests`, {}, this.headersWithJsonContentType())
    return toCamel(response.data)
  }

  async acceptJoinRequest(joinRequestId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/social/join-requests/${joinRequestId}/accept`, {}, this.headersWithJsonContentType())
    return response.data
  }

  async rejectJoinRequest(joinRequestId: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.put<SucceededOrNotResponse>(`${API}/social/join-requests/${joinRequestId}/reject`, {}, this.headersWithJsonContentType())
    return response.data
  }

  async getMyJoinRequest(groupId: string): Promise<any> {
    const response = await this._httpClient.get(`${API}/social/groups/${groupId}/join-requests/mine`)
    return toCamel(response.data)
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
