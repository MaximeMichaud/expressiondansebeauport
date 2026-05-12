export class AppVersion {
  current!: AppVersionCurrent
  repository!: AppVersionRepository
  latestRelease?: AppVersionRelease | null
  releases!: AppVersionRelease[]
  isUpToDate!: boolean
  updateError?: string | null
  fetchedAt!: number
}

export class AppVersionCurrent {
  version!: string
  semanticVersion?: string | null
  commitSha?: string | null
  builtAt!: number
}

export class AppVersionRepository {
  owner!: string
  name!: string
  htmlUrl!: string
  releasesUrl!: string
}

export class AppVersionRelease {
  tagName!: string
  name!: string
  body?: string | null
  htmlUrl!: string
  publishedAt!: number
  isPrerelease!: boolean
  isDraft!: boolean
  authorLogin!: string
}
