import {createI18n} from "vue3-i18n";
import {defaultLocale, messages} from "@/locales";

const i18n = createI18n({
    locale: defaultLocale,
    messages: messages
});

export default i18n;
