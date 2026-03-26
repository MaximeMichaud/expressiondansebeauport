import {createI18n} from "vue-i18n";
import {defaultLocale, messages} from "@/locales";

const i18n = createI18n({
    legacy: false,
    locale: defaultLocale,
    messages: messages
});

export default i18n;
