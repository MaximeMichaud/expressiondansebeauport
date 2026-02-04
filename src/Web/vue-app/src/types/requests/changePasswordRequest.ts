export interface IChangePasswordRequest {
    currentPassword: string
    newPassword: string
    newPasswordConfirmation: string
}