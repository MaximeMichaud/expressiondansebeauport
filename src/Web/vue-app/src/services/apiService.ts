import axios, {type AxiosInstance} from "axios";
import {useApiStore} from "@/stores/apiStore";

import "@/extensions/date.extensions";
import Cookies from "universal-cookie";
import {IApiService} from "@/injection/interfaces";
import {SucceededOrNotResponse} from "@/types/responses";

export function setupInterceptors(httpClient: AxiosInstance) {
  httpClient.interceptors.response.use(
      (response) => response,
      async (error) => {
        const originalRequest = error.config;

        if (error.request.status === 401 && originalRequest._retry) {
          return Promise.reject(error);
        }

        if (error.request.status == 401) {
          originalRequest._retry = true;
          // hasSession est un cookie non-HttpOnly écrit alongside les cookies d'auth
          // (HttpOnly), purement signalétique : sa présence indique qu'on a une session
          // potentiellement rafraîchissable. Si absent → visiteur anonyme, on ne tente
          // pas de refresh.
          if (!new Cookies().get("hasSession")) {
            return Promise.reject(error);
          }
          try {
            const response = await axios.get<SucceededOrNotResponse>(
                `${import.meta.env.VITE_API_BASE_URL}/authentication/refresh-token`,
                { withCredentials: true }
            );
            if (!response.data?.succeeded) {
              const apiStore = useApiStore();
              apiStore.setNeedToLogout(true);
              return Promise.reject(error);
            }
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
