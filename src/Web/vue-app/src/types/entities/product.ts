export interface IProduct {
    id?: string
    nameFr?: string
    nameEn?: string
    descriptionFr?: string
    descriptionEn?: string
    price?: number
    cardImage?: File
    savedCardImage?: string
    membersOnly?: boolean
}

export class Product implements IProduct {
    id?: string;
    nameFr?: string
    nameEn?: string
    descriptionFr?: string
    descriptionEn?: string

    price?: number
    cardImage?: File
    savedCardImage?: string
    membersOnly?: boolean
}