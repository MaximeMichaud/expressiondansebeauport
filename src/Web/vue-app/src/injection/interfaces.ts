 
import {
  IForgotPasswordRequest,
  ILoginRequest,
  IResetPasswordRequest,
  ITwoFactorRequest
} from "@/types/requests"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Administrator, MediaFile, Member, NavigationMenu, NavigationMenuItem, Page, SiteHealth, SiteSettings, User} from "@/types/entities"
import {Guid} from "@/types";

export interface IApiService {
  headersWithJsonContentType(): any

  headersWithFormDataContentType(): any

  buildEmptyBody(): string
}

export interface IAdministratorService {
  getAuthenticated(): Promise<Administrator | undefined>
}


export interface IAuthenticationService {
  login(request: ILoginRequest): Promise<SucceededOrNotResponse>

  twoFactor(request: ITwoFactorRequest): Promise<SucceededOrNotResponse>

  forgotPassword(request: IForgotPasswordRequest): Promise<SucceededOrNotResponse>

  resetPassword(request: IResetPasswordRequest): Promise<SucceededOrNotResponse>

  logout(): Promise<SucceededOrNotResponse>
}

export interface IMemberService {

  getAuthenticated(): Promise<Member | undefined>

  search(pageIndex: number, pageSize: number, searchValue: string): Promise<PaginatedResponse<Member>>

  getMember(id: string): Promise<Member>

  createMember(member: Member): Promise<SucceededOrNotResponse>

  updateMember(member: Member): Promise<SucceededOrNotResponse>

  deleteMember(id: Guid): Promise<SucceededOrNotResponse>
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

  reorderMenuItems(menuId: string, itemIds: string[]): Promise<SucceededOrNotResponse>
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
