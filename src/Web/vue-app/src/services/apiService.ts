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

        if (!getAccessToken() && getRefreshToken()) {
          try {
            const response = await axios.get<SucceededOrNotResponse>(
                `${import.meta.env.VITE_API_BASE_URL}/authentication/refresh-token`
            );
            if (!response.data?.succeeded) {
              const apiStore = useApiStore();
              apiStore.setNeedToLogout(true);
              return config;
            }
            const bearer = `Bearer ${getAccessToken()}`;
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
          // Pas d'accessToken cookie : visiteur anonyme, aucune session à rafraîchir.
          // Le refreshToken est httpOnly donc invisible côté JS, on ne peut pas s'en servir
          // ici comme indicateur. accessToken (non httpOnly) est notre signal de session.
          if (!new Cookies().get("accessToken")) {
            return Promise.reject(error);
          }
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