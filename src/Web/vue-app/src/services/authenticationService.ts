import {IAuthenticationService} from "@/injection/interfaces";
import {injectable} from "inversify";
import {ApiService} from "./apiService";
import {SucceededOrNotResponse} from "@/types/responses";
import {AxiosError, AxiosResponse} from "axios";
import {ILoginRequest} from "@/types/requests/loginRequest";
import {ITwoFactorRequest} from "@/types/requests/twoFactorRequest";
import {IForgotPasswordRequest} from "@/types/requests/forgotPasswordRequest";
import {IResetPasswordRequest} from "@/types/requests";

@injectable()
export class AuthenticationService extends ApiService implements IAuthenticationService {
  public async login(request: ILoginRequest): Promise<SucceededOrNotResponse> {
    const response = await this
    ._httpClient
    .post<ILoginRequest, AxiosResponse<SucceededOrNotResponse>>(
        `${import.meta.env.VITE_API_BASE_URL}/authentication/login`,
        request,
        this.headersWithJsonContentType())
    .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
      return error.response as AxiosResponse<SucceededOrNotResponse>
    })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }

  public async twoFactor(request: ITwoFactorRequest): Promise<SucceededOrNotResponse> {
    const response = await this
    ._httpClient
    .post<ITwoFactorRequest, AxiosResponse<SucceededOrNotResponse>>(
        `${import.meta.env.VITE_API_BASE_URL}/authentication/two-factor`,
        request,
        this.headersWithJsonContentType())
    .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
      return error.response as AxiosResponse<SucceededOrNotResponse>
    })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }

  public async forgotPassword(request: IForgotPasswordRequest): Promise<SucceededOrNotResponse> {
    const response = await this
    ._httpClient
    .post<IForgotPasswordRequest, AxiosResponse<SucceededOrNotResponse>>(
        `${import.meta.env.VITE_API_BASE_URL}/authentication/forgot-password`,
        request,
        this.headersWithJsonContentType())
    .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
      return error.response as AxiosResponse<SucceededOrNotResponse>
    })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }

  public async resetPassword(request: IResetPasswordRequest): Promise<SucceededOrNotResponse> {
    const response = await this
    ._httpClient
    .post<IResetPasswordRequest, AxiosResponse<SucceededOrNotResponse>>(
        `${import.meta.env.VITE_API_BASE_URL}/authentication/reset-password`,
        request,
        this.headersWithJsonContentType())
    .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
      return error.response as AxiosResponse<SucceededOrNotResponse>
    })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }

  public async logout(): Promise<SucceededOrNotResponse> {
    const response = await this
    ._httpClient
    .get<any, AxiosResponse<SucceededOrNotResponse>>(`${import.meta.env.VITE_API_BASE_URL}/authentication/logout`)
    .catch(function (error: AxiosError): AxiosResponse<SucceededOrNotResponse> {
      return error.response as AxiosResponse<SucceededOrNotResponse>
    })
    const succeededOrNotResponse = response.data as SucceededOrNotResponse
    return new SucceededOrNotResponse(succeededOrNotResponse.succeeded, succeededOrNotResponse.errors)
  }
}