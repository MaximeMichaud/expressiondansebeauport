export enum Locales {
    FR = 'fr',
}

export const LOCALES = [
    { value: Locales.FR, caption: 'Français' }
]

import fr from "./fr.json";

export const messages = {
    [Locales.FR]: fr
};

export const defaultLocale = Locales.FR;