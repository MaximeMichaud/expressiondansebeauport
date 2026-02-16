declare module "vue3-easy-data-table" {
  import { DefineComponent } from "vue";
  const Vue3EasyDataTable: DefineComponent<{}, {}, any>;
  export default Vue3EasyDataTable;

  export type SortType = "asc" | "desc";
  export type FilterComparison = "=" | "!=" | ">" | ">=" | "<" | "<=" | "between" | "in";
  export type Item = Record<string, any>;
  export type FilterOption = {
    field: string;
    comparison: FilterComparison | ((value: any, criteria: string) => boolean);
    criteria: any;
  };
  export type Header = {
    text: string;
    value: string;
    sortable?: boolean;
    fixed?: boolean;
    width?: number;
  };
  export type ServerOptions = {
    page: number;
    rowsPerPage: number;
    sortBy?: string | string[];
    sortType?: SortType | SortType[];
  };
}
