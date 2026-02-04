import {useRoute} from "vue-router";
import {useI18n} from "vue3-i18n";

/**
 * @param parentKey string? if undefined, use current route path as base url
 * @param childKey string? if undefined, return empty.
 * @param parameters expect an array [] containing objects => {name: "", value: ""}
 */
export function getChildPath(parentKey?: string, childKey?: string, parameters: any = null) {
  if (childKey == undefined)
    return '';

  const {t} = useI18n();
  const route = useRoute();

  const parentRoute = parentKey != null ? t(`routes.${parentKey}.fullPath`) : route.path;
  let childRouteSegment = t(`routes.${childKey}.path`);

  if (parameters != null && parameters.length > 0) {
    parameters.forEach((param: any) => {
      childRouteSegment = childRouteSegment.replace(param.name, param.value);
    });
  }

  return `${parentRoute}/${childRouteSegment}`;
}