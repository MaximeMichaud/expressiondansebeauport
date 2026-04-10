 
import {
  IChangePasswordRequest,
  IForgotPasswordRequest,
  ILoginRequest,
  IResetPasswordRequest,
  ITwoFactorRequest
} from "@/types/requests"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Administrator, BackupRecord, FooterPartner, MediaFile, NavigationMenu, NavigationMenuItem, Page, PageRevision, PageRevisionListItem, SiteHealth, SiteSettings, SocialLink, User, Group, GroupMember, Post, Comment, Conversation, Message} from "@/types/entities"
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

  duplicate(id: string): Promise<Page | null>

  getRevisions(pageId: string): Promise<PageRevisionListItem[]>

  getRevision(pageId: string, revisionId: string): Promise<PageRevision>

  restoreRevision(pageId: string, revisionId: string): Promise<Page | null>

  autosave(pageId: string, data: Partial<Page>): Promise<{ savedAt: string } | null>

  createPreview(pageId: string): Promise<{ token: string; previewUrl: string } | null>
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

  addSocialLink(link: SocialLink): Promise<SocialLink | null>

  updateSocialLink(link: SocialLink): Promise<SucceededOrNotResponse>

  deleteSocialLink(id: string): Promise<SucceededOrNotResponse>

  addFooterPartner(partner: FooterPartner): Promise<FooterPartner | null>

  updateFooterPartner(partner: FooterPartner): Promise<SucceededOrNotResponse>

  deleteFooterPartner(id: string): Promise<SucceededOrNotResponse>
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
  createGroup(name: string, description: string, season: string, inviteCode: string): Promise<SucceededOrNotResponse>
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
  createPoll(groupId: string, question: string, options: string[], allowMultipleAnswers: boolean): Promise<SucceededOrNotResponse>
  getAnnouncements(page?: number): Promise<Post[]>
  createAnnouncement(content: string): Promise<SucceededOrNotResponse>
  getConversations(page?: number): Promise<Conversation[]>
  getMessages(conversationId: string, page?: number): Promise<Message[]>
  sendMessage(conversationId: string, content: string): Promise<SucceededOrNotResponse>
  deleteMessage(messageId: string): Promise<SucceededOrNotResponse>
  startConversation(otherMemberId: string): Promise<any>
  markAsRead(conversationId: string): Promise<void>
  getUnreadCount(): Promise<number>
  searchMembers(query: string): Promise<any[]>
  getMemberProfile(id: string): Promise<any>
  getMyProfile(): Promise<any>
  updateMyProfile(firstName: string, lastName: string, email: string): Promise<SucceededOrNotResponse>
  deleteMember(id: string): Promise<SucceededOrNotResponse>
  promoteMember(id: string): Promise<SucceededOrNotResponse>
  demoteMember(id: string): Promise<SucceededOrNotResponse>
  uploadFile(file: File): Promise<{ succeeded: boolean; url: string; fileName: string; contentType: string; size: number }>
}

export interface IBackupService {
  getAll(): Promise<BackupRecord[]>

  create(): Promise<BackupRecord | null>

  download(fileName: string): Promise<Blob>

  deleteBackup(id: string): Promise<SucceededOrNotResponse>

  restore(fileName: string): Promise<SucceededOrNotResponse>

  checkStatus(): Promise<boolean>
}
