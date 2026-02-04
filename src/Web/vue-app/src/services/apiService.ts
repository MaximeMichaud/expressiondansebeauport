import axios, {type AxiosInstance, type AxiosResponse, type InternalAxiosRequestConfig} from "axios";
import {inject, injectable} from "inversify";
import {useApiStore} from "@/stores/apiStore";

import "@/extensions/date.extensions";
import Cookies from "universal-cookie";
import {TYPES} from "@/injection/types";
import {IApiService} from "@/injection/interfaces";
import {SucceededOrNotResponse} from "@/types/responses";

@injectable()
export class ApiService implements IApiService {
  _httpClient: AxiosInstance;

  constructor(@inject(TYPES.AxiosInstance) httpClient: AxiosInstance) {
    this._httpClient = httpClient;
    /*
        Attaching the accessToken to the request header. The access token is stored in cookies by LoginEndpoint or TwoFactorEndpoint
        AccessToken and RefreshToken rotation is handled in by RefreshTokenEndpoint
    */
    this._httpClient.interceptors.request.use(
        async (config) => {
          // Checking if the AccessToken has expired. If it is expired, we call the backend to get the current valid token
          // otherwise we do nothing since the bearer is already set with the valid token.
          if (!this.getAccessToken() && this.getRefreshToken()) {
            await this.refreshToken(config, false);
          } else if (this.getAccessToken()) {
            const bearer = `Bearer ${this.getAccessToken()}`;
            config.headers.Authorization = bearer;
            this._httpClient.defaults.headers.common['Authorization'] = bearer;
          }
          return config;
        },
        (error) => {
          return Promise.reject(error);
        }
    );

    this._httpClient.interceptors.response.use(
        (response) => {
          return response;
        },
        async (error) => {
          const originalRequest = error.config;

          if (error.request.status === 401 && originalRequest._retry) {
            return Promise.reject(error);
          }

          if (error.request.status == 401) {
            originalRequest._retry = true;
            console.log('request status 401 so refreshing token')
            return await this.refreshToken(
                originalRequest,
                true
            );
          }
          return Promise.reject(error);
        }
    );
  }

  private getAccessToken() {
    return new Cookies().get("accessToken");
  }

  private getRefreshToken() {
    return new Cookies().get("refreshToken");
  }

  private async refreshToken(
      config: InternalAxiosRequestConfig<any>,
      retryRequest: boolean
  ) {
    try {
      return await axios
          .get(
              `${import.meta.env.VITE_API_BASE_URL}/authentication/refresh-token`
          )
          .then((response: AxiosResponse<SucceededOrNotResponse>) => {
            if (!response.data) return;

            const succeededOrNotResponse = response.data;
            if (!succeededOrNotResponse.succeeded) {
              // refresh token is expired
              this.logoutUserAndRedirectToHomePage();
              return;
            }

            const bearer = `Bearer ${this.getAccessToken()}`;
            config.headers.Authorization = bearer;
            this._httpClient.defaults.headers.common['Authorization'] = bearer;

            if (retryRequest) {
              return this._httpClient(config)
            }
          })
    } catch (error) {
      this.logoutUserAndRedirectToHomePage()
      return Promise.reject(error);
    }
  }

  private logoutUserAndRedirectToHomePage() {
    const apiStore = useApiStore();
    apiStore.setNeedToLogout(true);
    // A watcher is set up in the component LogoutPopup, it will handle the rest.
  }

  public headersWithJsonContentType() {
    return {
      headers: {
        "Content-Type": 'application/json',
      },
    };
  }

  public headersWithFormDataContentType() {
    return {
      headers: {
        "Content-Type": '"multipart/form-data"',
      },
    };
  }

  public buildEmptyBody(): string {
    return '{}'
  }
}