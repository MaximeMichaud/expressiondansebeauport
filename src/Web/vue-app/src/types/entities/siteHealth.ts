export class SiteHealth {
  overallStatus?: string
  checks?: HealthCheck[]
}

export class HealthCheck {
  name?: string
  status?: string
  message?: string
  details?: string
}
