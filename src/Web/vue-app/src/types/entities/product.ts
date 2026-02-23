export interface IProduct {
    id?: string
    nameFr?: string
    descriptionFr?: string
    price?: number
    cardImage?: File
    savedCardImage?: string
    membersOnly?: boolean
}

export class Product implements IProduct {
    id?: string;
    nameFr?: string
    descriptionFr?: string
    price?: number
    cardImage?: File
    savedCardImage?: string
    membersOnly?: boolean
}
