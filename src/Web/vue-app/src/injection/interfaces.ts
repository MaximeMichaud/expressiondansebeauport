 
import {
  IChangePasswordRequest,
  IForgotPasswordRequest,
  ILoginRequest,
  IResetPasswordRequest,
  ITwoFactorRequest
} from "@/types/requests"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Administrator, MediaFile, NavigationMenu, NavigationMenuItem, Page, SiteHealth, SiteSettings, User, Group, GroupMember, Post, Comment, Conversation, Message} from "@/types/entities"
export interface IApiService {
  headersWithJsonContentType(): any

  headersWithFormDataContentType(): any

  buildEmptyBody(): string
}

export interface IAdministratorService {
  getAuthenticated(): Promise<Administrator | undefined>
  updateMe(admin: Administrator): Promise<SucceededOrNotResponse>
}


export interface IAuthenticationService {
  login(request: ILoginRequest): Promise<SucceededOrNotResponse>

  twoFactor(request: ITwoFactorRequest): Promise<SucceededOrNotResponse>

  forgotPassword(request: IForgotPasswordRequest): Promise<SucceededOrNotResponse>

  resetPassword(request: IResetPasswordRequest): Promise<SucceededOrNotResponse>

  changePassword(request: IChangePasswordRequest): Promise<SucceededOrNotResponse>

  logout(): Promise<SucceededOrNotResponse>
}

export interface IUserService {
  getCurrentUser(): Promise<User>
}

export interface IMediaService {
  getAll(pageIndex: number, pageSize: number): Promise<PaginatedResponse<MediaFile>>

  get(id: string): Promise<MediaFile>

  upload(file: File): Promise<MediaFile | null>

  update(id: string, altText: string): Promise<SucceededOrNotResponse>

  delete(id: string): Promise<SucceededOrNotResponse>
}

export interface IPageService {
  getAll(pageIndex: number, pageSize: number, status?: string): Promise<PaginatedResponse<Page>>

  get(id: string): Promise<Page>

  create(page: Page): Promise<SucceededOrNotResponse>

  update(page: Page): Promise<SucceededOrNotResponse>

  delete(id: string): Promise<SucceededOrNotResponse>
}

export interface IMenuService {
  getAll(): Promise<NavigationMenu[]>

  get(id: string): Promise<NavigationMenu>

  create(menu: NavigationMenu): Promise<SucceededOrNotResponse>

  update(menu: NavigationMenu): Promise<SucceededOrNotResponse>

  delete(id: string): Promise<SucceededOrNotResponse>

  addMenuItem(menuId: string, item: NavigationMenuItem): Promise<SucceededOrNotResponse>

  updateMenuItem(menuId: string, item: NavigationMenuItem): Promise<SucceededOrNotResponse>

  deleteMenuItem(menuId: string, itemId: string): Promise<SucceededOrNotResponse>

  reorderMenuItems(menuId: string, items: { id: string; sortOrder: number }[]): Promise<SucceededOrNotResponse>
}

export interface ISiteSettingsService {
  get(): Promise<SiteSettings>

  update(settings: SiteSettings): Promise<SucceededOrNotResponse>

  getPublic(): Promise<SiteSettings>
}

export interface ISiteHealthService {
  get(): Promise<SiteHealth>
}

export interface IImportExportService {
  exportData(): Promise<Blob>

  importData(file: File): Promise<SucceededOrNotResponse>
}

export interface ISocialService {
  register(firstName: string, lastName: string, email: string, password: string): Promise<SucceededOrNotResponse>
  confirmEmail(email: string, code: string): Promise<SucceededOrNotResponse>
  resendCode(email: string): Promise<SucceededOrNotResponse>
  getMyGroups(): Promise<Group[]>
  getActiveGroups(): Promise<Group[]>
  getGroupDetails(id: string): Promise<any>
  getGroupMembers(groupId: string, page?: number): Promise<GroupMember[]>
  joinGroup(inviteCode: string): Promise<SucceededOrNotResponse>
  getGroupFeed(groupId: string, page?: number): Promise<Post[]>
  getPost(id: string): Promise<Post>
  createPost(groupId: string, content: string, type?: string): Promise<SucceededOrNotResponse>
  deletePost(id: string): Promise<SucceededOrNotResponse>
  toggleLike(postId: string): Promise<SucceededOrNotResponse>
  recordView(postId: string): Promise<void>
  pinPost(postId: string, groupId: string): Promise<SucceededOrNotResponse>
  getComments(postId: string, page?: number): Promise<Comment[]>
  addComment(postId: string, content: string): Promise<SucceededOrNotResponse>
  deleteComment(id: string): Promise<SucceededOrNotResponse>
  votePoll(postId: string, pollOptionId: string): Promise<SucceededOrNotResponse>
  getAnnouncements(page?: number): Promise<Post[]>
  getConversations(page?: number): Promise<Conversation[]>
  getMessages(conversationId: string, page?: number): Promise<Message[]>
  sendMessage(conversationId: string, content: string): Promise<SucceededOrNotResponse>
  startConversation(otherMemberId: string): Promise<any>
  markAsRead(conversationId: string): Promise<void>
  getUnreadCount(): Promise<number>
  searchMembers(query: string): Promise<any[]>
  uploadFile(file: File): Promise<{ succeeded: boolean; url: string; fileName: string; contentType: string; size: number }>
}
