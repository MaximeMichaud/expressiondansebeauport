import {createI18n} from "vue3-i18n";
import {defaultLocale, Locales, messages} from "@/locales";
import Cookies from "universal-cookie";

function getDefaultLocale(): string {
    // First, check if a cookie exists
    const language = new Cookies().get<string>('lang')
    if (language != undefined && language.length > 0)
        return language.toLowerCase().includes('en') ? Locales.EN : Locales.FR

    return defaultLocale
}

const i18n = createI18n({
    locale: getDefaultLocale(),
    messages: messages
});

export default i18n;
