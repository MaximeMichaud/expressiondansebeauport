import { Guid } from "@/types/guid";

declare module "@vue/reactivity" {
  export interface RefUnwrapBailTypes {
    guid: Guid;
  }
}
