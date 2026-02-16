 
import {
  IForgotPasswordRequest,
  ILoginRequest,
  IResetPasswordRequest,
  ITwoFactorRequest
} from "@/types/requests"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Administrator, Member, Page, User} from "@/types/entities"
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

export interface IPageService {
  getAll(): Promise<Page[]>

  getPage(id: string): Promise<Page>

  getPageBySlug(slug: string): Promise<Page>

  updatePage(page: Page): Promise<SucceededOrNotResponse>
}

export interface IUserService {
  getCurrentUser(): Promise<User>
}
