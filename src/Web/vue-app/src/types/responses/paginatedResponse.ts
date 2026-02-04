export interface PaginatedResponse<T> {
  totalItems?: number,
  items?: T[],
}