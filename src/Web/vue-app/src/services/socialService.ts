import { injectable } from "inversify"
import { ApiService } from "@/services/apiService"
import { SucceededOrNotResponse } from "@/types/responses"
import type { Group, GroupMember, Member, Post, Comment, Conversation, Message } from "@/types/entities"

const API = import.meta.env.VITE_API_BASE_URL

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
    const response = await this._httpClient.get<Group[]>(`${API}/social/groups/mine`)
    return response.data
  }

  async createGroup(name: string, description: string, season: string, inviteCode: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/admin/groups`, { name, description, season, inviteCode: inviteCode || undefined }, this.headersWithJsonContentType())
    return response.data
  }

  async getActiveGroups(): Promise<Group[]> {
    const response = await this._httpClient.get<Group[]>(`${API}/social/groups/active`)
    return response.data
  }

  async getGroupDetails(id: string): Promise<any> {
    const response = await this._httpClient.get(`${API}/social/groups/${id}`)
    return response.data
  }

  async getGroupMembers(groupId: string, page: number = 1): Promise<GroupMember[]> {
    const response = await this._httpClient.get(`${API}/social/groups/${groupId}/members?Page=${page}`)
    return (response.data as any).items || response.data
  }

  async joinGroup(inviteCode: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/groups/join`, { inviteCode }, this.headersWithJsonContentType())
    return response.data
  }

  // === Posts ===
  async getGroupFeed(groupId: string, page: number = 1): Promise<Post[]> {
    const response = await this._httpClient.get<Post[]>(`${API}/social/groups/${groupId}/posts?Page=${page}`)
    return response.data
  }

  async getPost(id: string): Promise<Post> {
    const response = await this._httpClient.get<Post>(`${API}/social/posts/${id}`)
    return response.data
  }

  async createPost(groupId: string, content: string, type: string = 'Text'): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/posts`, { groupId, content, type }, this.headersWithJsonContentType())
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
    const response = await this._httpClient.get<Comment[]>(`${API}/social/posts/${postId}/comments?Page=${page}`)
    return response.data
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

  // === Announcements ===
  async getAnnouncements(page: number = 1): Promise<Post[]> {
    const response = await this._httpClient.get<Post[]>(`${API}/social/announcements?Page=${page}`)
    return response.data
  }

  // === Conversations ===
  async getConversations(page: number = 1): Promise<Conversation[]> {
    const response = await this._httpClient.get<Conversation[]>(`${API}/social/conversations?Page=${page}`)
    return response.data
  }

  async getMessages(conversationId: string, page: number = 1): Promise<Message[]> {
    const response = await this._httpClient.get<Message[]>(`${API}/social/conversations/${conversationId}/messages?Page=${page}`)
    return response.data
  }

  async sendMessage(conversationId: string, content: string): Promise<SucceededOrNotResponse> {
    const response = await this._httpClient.post<SucceededOrNotResponse>(`${API}/social/conversations/${conversationId}/messages`, { content }, this.headersWithJsonContentType())
    return response.data
  }

  async startConversation(otherMemberId: string): Promise<any> {
    const response = await this._httpClient.post(`${API}/social/conversations`, { otherMemberId }, this.headersWithJsonContentType())
    return response.data
  }

  async markAsRead(conversationId: string): Promise<void> {
    await this._httpClient.put(`${API}/social/conversations/${conversationId}/read`)
  }

  async getUnreadCount(): Promise<number> {
    const response = await this._httpClient.get<{ count: number }>(`${API}/social/messages/unread-count`)
    return response.data.count
  }

  // === Members ===
  async getMyProfile(): Promise<Member> {
    const response = await this._httpClient.get<Member>(`${API}/social/members/me`)
    return response.data
  }

  async searchMembers(query: string): Promise<any[]> {
    const response = await this._httpClient.get(`${API}/social/members/search?Query=${encodeURIComponent(query)}`)
    return response.data as any[]
  }

  async getMemberProfile(id: string): Promise<any> {
    const response = await this._httpClient.get(`${API}/social/members/${id}`)
    return response.data
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
  async uploadFile(file: File): Promise<{ succeeded: boolean; url: string; fileName: string; contentType: string; size: number }> {
    const formData = new FormData()
    formData.append('file', file)
    const response = await this._httpClient.post(`${API}/social/upload`, formData, this.headersWithFormDataContentType())
    return response.data
  }
}
