import {Rule, RuleArray, RuleBoolean} from "@/validation/rules";

export interface Status {
  valid: boolean;
  message?: string;
}

export function validate(value: string, rules: Rule[]): Status {
  for (const rule of rules) {
    const result = rule(value);
    if (!result.valid) {
      return result;
    }
  }

  return {
    valid: true,
  };
}

export function validateBoolean(value: boolean, rules: RuleBoolean[]): Status {
  for (const rule of rules) {
    const result = rule(value);
    if (!result.valid) {
      return result;
    }
  }

  return {
    valid: true,
  };
}

export function validateArray(value: any[], rules: RuleArray[]): Status {
  for (const rule of rules) {
    const result = rule(value);
    if (!result.valid) {
      return result;
    }
  }

  return {
    valid: true,
  };
}