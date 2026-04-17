import axios, {type AxiosInstance} from "axios";
import {useApiStore} from "@/stores/apiStore";

import "@/extensions/date.extensions";
import Cookies from "universal-cookie";
import {IApiService} from "@/injection/interfaces";
import {SucceededOrNotResponse} from "@/types/responses";

export function setupInterceptors(httpClient: AxiosInstance) {
  httpClient.interceptors.request.use(
      async (config) => {
        const getAccessToken = () => new Cookies().get("accessToken");
        const getRefreshToken = () => new Cookies().get("refreshToken");

  constructor(@inject(TYPES.AxiosInstance) httpClient?: AxiosInstance) {
    this._httpClient = httpClient ?? axios.create();
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
            httpClient.defaults.headers.common['Authorization'] = bearer;
          } catch {
            const apiStore = useApiStore();
            apiStore.setNeedToLogout(true);
            return Promise.reject(config);
          }
        } else if (getAccessToken()) {
          const bearer = `Bearer ${getAccessToken()}`;
          config.headers.Authorization = bearer;
          httpClient.defaults.headers.common['Authorization'] = bearer;
        }
        return config;
      },
      (error) => Promise.reject(error)
  );

  httpClient.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;

        if (error.request.status === 401 && originalRequest._retry) {
          return Promise.reject(error);
        }

        if (error.request.status == 401) {
          originalRequest._retry = true;
          try {
            const response = await axios.get<SucceededOrNotResponse>(
                `${import.meta.env.VITE_API_BASE_URL}/authentication/refresh-token`
            );
            if (!response.data?.succeeded) {
              const apiStore = useApiStore();
              apiStore.setNeedToLogout(true);
              return Promise.reject(error);
            }
            const bearer = `Bearer ${new Cookies().get("accessToken")}`;
            originalRequest.headers.Authorization = bearer;
            httpClient.defaults.headers.common['Authorization'] = bearer;
            return httpClient(originalRequest);
          } catch (refreshError) {
            const apiStore = useApiStore();
            apiStore.setNeedToLogout(true);
            return Promise.reject(refreshError);
          }
        }
        return Promise.reject(error);
      }
  );
}

export class ApiService implements IApiService {
  _httpClient: AxiosInstance;

  constructor(httpClient: AxiosInstance) {
    this._httpClient = httpClient;
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
        "Content-Type": 'multipart/form-data',
      },
    };
  }

  public buildEmptyBody(): string {
    return '{}'
  }
}
