 
import {
  IChangePasswordRequest,
  IForgotPasswordRequest,
  ILoginRequest,
  IResetPasswordRequest,
  ITwoFactorRequest
} from "@/types/requests"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Administrator, BackupRecord, MediaFile, NavigationMenu, NavigationMenuItem, Page, SiteHealth, SiteSettings, User} from "@/types/entities"
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

export interface IBackupService {
  getAll(): Promise<BackupRecord[]>

  create(): Promise<BackupRecord | null>

  download(fileName: string): Promise<Blob>

  deleteBackup(id: string): Promise<SucceededOrNotResponse>

  restore(fileName: string): Promise<SucceededOrNotResponse>
}
