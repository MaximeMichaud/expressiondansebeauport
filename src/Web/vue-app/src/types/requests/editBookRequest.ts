export interface IEditBookRequest {
    id?: string
    nameFr?: string
    nameEn?: string
    descriptionFr?: string
    descriptionEn?: string
    price?: number
    cardImage?: File
    isbn?: string
    author?: string
    editor?: string
    yearOfPublication?: number
    numberOfPages?: number
}