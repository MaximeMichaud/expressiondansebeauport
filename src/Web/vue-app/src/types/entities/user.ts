import {Role} from "@/types/enums";

export class User {
  id?: string
  fullName?: string
  email?: string
  phoneNumber?: string
  phoneExtension?: number
  roles: Role[] = []
}
