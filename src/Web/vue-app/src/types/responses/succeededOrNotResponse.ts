import i18n from "@/i18n";
import '@/extensions/string.extensions';
import {Error} from "./error";

export class SucceededOrNotResponse {
    errors: Error[] = [];
    succeeded: boolean;

    constructor(succeeded: boolean, errors?: Error[]) {
        this.succeeded = succeeded
        if (errors)
            this.errors = errors
    }
    
    getErrorMessages(translationLocation: string, fallbackKey?: string): string[] {
        const errorMessages: string[] = [];
        this.errors.forEach(error => {
            const errorKey = error.errorType.makeFirstLetterLowercase()
            const errorMessage = i18n.t(`${translationLocation}.${errorKey}`)
            if (fallbackKey)
                errorMessages.push(errorMessage ? errorMessage : i18n.t(fallbackKey))
            else
                errorMessages.push(errorMessage ? errorMessage : i18n.t('validation.errorOccured'))
        })
        return errorMessages
    }
}
