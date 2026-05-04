import type { Component } from "vue"
import IconFacebook from "vue-material-design-icons/Facebook.vue"
import IconInstagram from "vue-material-design-icons/Instagram.vue"
import IconYoutube from "vue-material-design-icons/Youtube.vue"
import IconTwitter from "vue-material-design-icons/Twitter.vue"
import IconLinkedin from "vue-material-design-icons/Linkedin.vue"
import IconPinterest from "vue-material-design-icons/Pinterest.vue"
import IconWeb from "vue-material-design-icons/Web.vue"
import IconTiktok from "@/components/icons/TiktokIcon.vue"

export const socialLinkPlatforms = [
  { value: "facebook", label: "Facebook" },
  { value: "instagram", label: "Instagram" },
  { value: "youtube", label: "YouTube" },
  { value: "tiktok", label: "TikTok" },
  { value: "twitter", label: "X / Twitter" },
  { value: "linkedin", label: "LinkedIn" },
  { value: "pinterest", label: "Pinterest" },
]

export function getSocialIcon(platform?: string): Component {
  switch (normalizeSocialPlatform(platform)) {
    case "facebook": return IconFacebook
    case "instagram": return IconInstagram
    case "youtube": return IconYoutube
    case "tiktok": return IconTiktok
    case "twitter": return IconTwitter
    case "linkedin": return IconLinkedin
    case "pinterest": return IconPinterest
    default: return IconWeb
  }
}

export function getSocialLabel(platform?: string) {
  const normalizedPlatform = normalizeSocialPlatform(platform)
  return socialLinkPlatforms.find(p => p.value === normalizedPlatform)?.label || platform || ""
}

function normalizeSocialPlatform(platform?: string) {
  return platform?.trim().toLowerCase()
}
