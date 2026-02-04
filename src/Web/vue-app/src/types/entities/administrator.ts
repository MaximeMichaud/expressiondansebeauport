import {IPerson} from "@/types/entities/person";

export class Administrator implements IPerson {
  id?: string
  firstName?: string
  lastName?: string
  fullName?: string
  email?: string
}