export interface BackupRecord {
  id: string
  fileName: string
  sizeInBytes: number
  createdAt: string
  type: string
  status: string
  errorMessage?: string
}
