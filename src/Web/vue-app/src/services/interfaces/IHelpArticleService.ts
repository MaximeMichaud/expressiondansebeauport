// Ré-export pour respecter le pattern services/interfaces/<Name>.
// L'interface canonique vit dans @/injection/interfaces aux côtés des autres
// interfaces de service de l'application (IPageService, IMenuService, etc.).
export type {IHelpArticleService} from "@/injection/interfaces"
