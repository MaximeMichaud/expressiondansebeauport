export enum Locales {
    EN = 'en',
    FR = 'fr',
}

export const LOCALES = [
    { value: Locales.EN, caption: 'English' },
    { value: Locales.FR, caption: 'Français' }
]

import en from "./en.json";
import fr from "./fr.json";

export const messages = {
    [Locales.EN]: en,
    [Locales.FR]: fr
};

export const defaultLocale = Locales.EN;