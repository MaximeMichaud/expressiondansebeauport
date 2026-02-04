// eslint-disable-next-line @typescript-eslint/no-empty-interface
import {
  ICreateBookRequest,
  IEditBookRequest,
  IForgotPasswordRequest,
  ILoginRequest,
  IResetPasswordRequest,
  ITwoFactorRequest
} from "@/types/requests"
import {PaginatedResponse, SucceededOrNotResponse} from "@/types/responses"
import {Administrator, Book, Member, User} from "@/types/entities"
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

export interface IBookService {
  getAllBooks(): Promise<Book[]>

  getBook(bookId: string): Promise<Book>

  deleteBook(bookId: string): Promise<SucceededOrNotResponse>

  createBook(request: ICreateBookRequest): Promise<SucceededOrNotResponse>

  editBook(request: IEditBookRequest): Promise<SucceededOrNotResponse>
}

export interface IUserService {
  getCurrentUser(): Promise<User>
}
