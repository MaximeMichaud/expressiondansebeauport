<template>
  <footer class="public-footer">
    <div class="public-footer__inner">
      <div class="public-footer__col public-footer__col--brand">
        <LogoEdb class="public-footer__logo" />
        <p class="public-footer__description">{{ t('public.footer.description') }}</p>
      </div>

      <div class="public-footer__col public-footer__col--contact">
        <h3 class="public-footer__heading">{{ t('public.footer.address') }}</h3>
        <p>15, rue de la Promenade-des-Soeurs</p>
        <p>Beauport, QC G1C 0G3</p>

        <h3 class="public-footer__heading">{{ t('public.footer.phone') }}</h3>
        <p><a href="tel:4186601086">418-660-1086</a></p>

        <h3 class="public-footer__heading">{{ t('public.footer.email') }}</h3>
        <p><a href="mailto:info@expressiondansebeauport.com">info@expressiondansebeauport.com</a></p>
      </div>

      <div class="public-footer__col public-footer__col--social">
        <h3 class="public-footer__heading">{{ t('public.footer.followUs') }}</h3>
        <div class="public-footer__social-links">
          <a href="https://www.facebook.com/expressiondansebeauport" target="_blank" rel="noopener noreferrer" aria-label="Facebook">
            <IconFacebook fill-color="#ffffff" :size="22" />
          </a>
          <a href="https://www.instagram.com/expressiondansebeauport" target="_blank" rel="noopener noreferrer" aria-label="Instagram">
            <IconInstagram fill-color="#ffffff" :size="22" />
          </a>
        </div>

        <LangSwitcher class="public-footer__lang-switcher" />
      </div>
    </div>

    <div class="public-footer__copyright">
      <p>&copy; {{ currentYear }} {{ t('public.footer.copyright') }}</p>
    </div>
  </footer>
</template>

<script lang="ts" setup>
import { useI18n } from "vue3-i18n";
import LangSwitcher from "@/components/layouts/items/LangSwitcher.vue";
import LogoEdb from "@/assets/icons/logo__edb.svg";
import IconFacebook from "vue-material-design-icons/Facebook.vue";
import IconInstagram from "vue-material-design-icons/Instagram.vue";

const { t } = useI18n();
const currentYear = new Date().getFullYear();
</script>
