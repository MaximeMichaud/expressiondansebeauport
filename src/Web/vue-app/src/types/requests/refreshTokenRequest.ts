export interface IRefreshTokenRequest {
  grantType: string
  clientId: string
  clientSecret: string
  refreshToken: string
}