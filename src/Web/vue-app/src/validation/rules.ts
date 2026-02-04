import i18n from "@/i18n";
import {Status} from "@/validation";

export type Rule = (value: string) => Status;
export type RuleBoolean = (value: boolean) => Status;
export type RuleArray = (value: any[]) => Status;

export const isFutureDate: Rule = (value: string): Status => {
  const startDate = new Date(value);
  startDate.setUTCMinutes(startDate.getTimezoneOffset());
  startDate.setHours(0, 0, 0, 0);
  const todayDate = new Date();
  todayDate.setHours(0, 0, 0, 0);
  const result = startDate >= todayDate;

  return {
    valid: result,
    message: result ? undefined : i18n.t("validation.futureDate"),
  };
};

export function minDate(min: string): Rule {
  return function (value: string): Status {
    const result = value > min;

    return {
      valid: result,
      message: result
        ? undefined
        : i18n.t('validation.min').replace("{min}", min.toString())
    };
  };
}

export function min(min: number): Rule {
  return function (value: string): Status {
    const result = Boolean(parseInt(value) >= min);

    return {
      valid: result,
      message: result
        ? undefined
        : i18n.t('validation.min').replace("{min}", min.toString()),
    };
  };
}

export function max(max: number): Rule {
  return function (value: string): Status {
    const result = Boolean(parseInt(value) <= max);

    return {
      valid: result,
      message: result
        ? undefined
        : i18n.t('validation.max').toString().replace("{max}", max.toString()),
    };
  };
}

export const required: Rule = (value?: string): Status => {
  const result = value == undefined ? false : Boolean(value.toString());
  return {
    valid: result,
    message: result ? undefined : i18n.t('validation.empty').toString()
  };
};

export const requiredTextEditor: Rule = (value: string): Status => {
  const result = Boolean(value !== "" && value !== "<p><br></p>");

  return {
    valid: result,
    message: result ? undefined : i18n.t("validation.empty").toString(),
  };
};

export const requiredBoolean: RuleBoolean = (value: boolean): Status => {
  return {
    valid: value,
    message: value ? undefined : i18n.t('validation.checked').toString()
  };
};

export const requiredArray: RuleArray = (array: any[]): Status => {
    const result = array != null && array.length > 0;
    return {
      valid: result,
      message: result ? undefined : i18n.t('validation.empty').toString()
    };
}

export const mustMatchZipCodeFormat: Rule = (value: string): Status => {
  const zipCodeRegex = new RegExp(/^[ABCEGHJ-NPRSTVXY]\d[ABCEGHJ-NPRSTV-Z][ -]?\d[ABCEGHJ-NPRSTV-Z]\d$/im)
  const valid = zipCodeRegex.test(value)
  return {
    valid: valid,
    message: valid ? undefined : i18n.t('validation.zipCode').toString()
  };
};

export const mustMatchPhoneNumberFormat: Rule = (value: string): Status => {
  const phoneNumberRegex = new RegExp(/^[0-9]{3}-[0-9]{3}-[0-9]{4}$/)
  const valid = phoneNumberRegex.test(value)
  return {
    valid: valid,
    message: valid ? undefined : i18n.t('validation.phoneNumber').toString()
  };
};

export const mustMatchEmailFormat: Rule = (value: string): Status => {
  const emailRegex = new RegExp(/^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/)
  const valid = emailRegex.test(value)
  return {
    valid: valid,
    message: valid ? undefined : i18n.t('validation.email').toString()
  };
}