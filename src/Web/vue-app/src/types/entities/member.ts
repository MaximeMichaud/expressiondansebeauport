import {IPerson} from "@/types/entities/person";

export class Member implements IPerson {
  id?: string
  firstName?: string
  lastName?: string
  fullName?: string
  email?: string
  phoneNumber?: string
  phoneExtension?: number
  apartment?: number
  street?: string
  city?: string
  zipCode?: string
  userId?: string
  roles?: string[]
}