import {Guid} from "@/types";

export interface IResetPasswordRequest {
  userId: Guid,
  token: string,
  password: string
  passwordConfirmation: string
}