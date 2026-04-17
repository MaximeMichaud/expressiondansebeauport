<template>
  <footer class="public-footer">
    <div class="public-footer__inner">
      <div class="public-footer__col public-footer__col--brand">
        <LogoEdb class="public-footer__logo" />
        <p v-if="settings.footerDescription" class="public-footer__description">{{ settings.footerDescription }}</p>
        <p v-else class="public-footer__description">{{ t('public.footer.description') }}</p>
        <template v-if="socialLinks.length">
          <h3 class="public-footer__heading">{{ t('public.footer.followUs') }}</h3>
          <div class="public-footer__social-links">
            <a v-for="link in socialLinks" :key="link.id" :href="link.url" target="_blank" rel="noopener noreferrer" :aria-label="link.platform">
              <component :is="getSocialIcon(link.platform)" fill-color="#ffffff" :size="22" />
            </a>
          </div>
        </template>
      </div>

      <div class="public-footer__col public-footer__col--contact">
        <template v-if="settings.footerAddress || settings.footerCity">
          <h3 class="public-footer__heading">{{ t('public.footer.address') }}</h3>
          <p v-if="settings.footerAddress">{{ settings.footerAddress }}</p>
          <p v-if="settings.footerCity">{{ settings.footerCity }}</p>
        </template>

        <template v-if="settings.footerPhone">
          <h3 class="public-footer__heading">{{ t('public.footer.phone') }}</h3>
          <p><a :href="'tel:' + settings.footerPhone.replace(/[^0-9+]/g, '')">{{ settings.footerPhone }}</a></p>
        </template>

        <template v-if="settings.footerEmail">
          <h3 class="public-footer__heading">{{ t('public.footer.email') }}</h3>
          <p><a :href="'mailto:' + settings.footerEmail">{{ settings.footerEmail }}</a></p>
        </template>
      </div>

      <div v-if="footerMenuItems.length" class="public-footer__col public-footer__col--nav">
        <h3 class="public-footer__heading">{{ t('global.quickLinks') }}</h3>
        <ul class="public-footer__nav-links">
          <li v-for="item in footerMenuItems" :key="item.id">
            <a
              v-if="item.url?.startsWith('http')"
              :href="item.url"
              :target="item.target === 'Blank' ? '_blank' : '_self'"
              :rel="item.target === 'Blank' ? 'noopener noreferrer' : undefined">
              {{ item.label }}
            </a>
            <RouterLink v-else :to="item.url || `/${item.pageSlug}`">
              {{ item.label }}
            </RouterLink>
          </li>
        </ul>
      </div>

      <div v-if="footerPartners.length" class="public-footer__col public-footer__col--partners">
        <div class="public-footer__partners-logos">
          <template v-for="partner in footerPartners" :key="partner.id">
            <a v-if="partner.url" :href="partner.url" target="_blank" rel="noopener noreferrer" class="public-footer__partner-link">
              <img :src="partner.mediaUrl" :alt="partner.altText" class="public-footer__partner-logo" />
            </a>
            <div v-else class="public-footer__partner-link">
              <img :src="partner.mediaUrl" :alt="partner.altText" class="public-footer__partner-logo" />
            </div>
          </template>
        </div>
      </div>
    </div>

    <div class="public-footer__copyright">
      <p>&copy; {{ currentYear }} {{ settings.copyrightText || t('public.footer.copyright') }}</p>
      <button class="public-footer__cookies-button" type="button" @click="openCookiePreferences">
        Préférences de cookies
      </button>
      <RouterLink to="/politique-confidentialite" class="public-footer__privacy-link">
        {{ t('public.footer.privacyPolicy') }}
      </RouterLink>
    </div>
  </footer>
</template>

<script lang="ts" setup>
import { ref, onMounted } from "vue";
import { useI18n } from "vue-i18n";
import axios from "axios";
import LogoEdb from "@/assets/icons/logo__edb.svg";
import IconFacebook from "vue-material-design-icons/Facebook.vue";
import IconInstagram from "vue-material-design-icons/Instagram.vue";
import IconYoutube from "vue-material-design-icons/Youtube.vue";
import IconTwitter from "vue-material-design-icons/Twitter.vue";
import IconLinkedin from "vue-material-design-icons/Linkedin.vue";
import IconWeb from "vue-material-design-icons/Web.vue";
import { SiteSettings, SocialLink, FooterPartner, NavigationMenuItem } from "@/types/entities";

const { t } = useI18n();
const currentYear = new Date().getFullYear();
const settings = ref<SiteSettings>(new SiteSettings());
const socialLinks = ref<SocialLink[]>([]);
const footerPartners = ref<FooterPartner[]>([]);
const footerMenuItems = ref<NavigationMenuItem[]>([]);

function getSocialIcon(platform?: string) {
  switch (platform) {
    case "facebook": return IconFacebook;
    case "instagram": return IconInstagram;
    case "youtube": return IconYoutube;
    case "twitter": return IconTwitter;
    case "linkedin": return IconLinkedin;
    default: return IconWeb;
  }
}

function openCookiePreferences() {
  window.dispatchEvent(new Event('open-cookie-preferences'));
}

async function loadSettings() {
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/site-settings`);
    const data = response.data || new SiteSettings();
    settings.value = data;
    socialLinks.value = data.socialLinks || [];
    footerPartners.value = data.footerPartners || [];
  } catch {
    settings.value = new SiteSettings();
  }
}

async function loadFooterMenu() {
  try {
    const response = await axios.get(`${import.meta.env.VITE_API_BASE_URL}/public/menus/Footer`);
    footerMenuItems.value = response.data?.menuItems || [];
  } catch {
    footerMenuItems.value = [];
  }
}

onMounted(() => {
  loadSettings();
  loadFooterMenu();
});
</script>

<style scoped lang="scss">
.public-footer__cookies-button {
  margin-top: 8px;
  padding: 0;
  color: inherit;
  text-decoration: underline;
  cursor: pointer;
  background: transparent;
  border: 0;
}
</style>
