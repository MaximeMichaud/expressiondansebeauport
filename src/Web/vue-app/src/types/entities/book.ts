import {Product} from "@/types/entities/product";

export class Book extends Product {
    isbn?: string;
    author?: string;
    editor?: string;
    yearOfPublication?: number;
    numberOfPages?: number;
}
