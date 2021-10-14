import { Page } from "./page";

export type Render<T> = (container: HTMLElement, item: T) => void;

export function renderPage<T>(containingElement: HTMLElement, page: Page<T>, renderElement: Render<T>) {
  for (const item of page.items) {
    renderElement(containingElement, item);
  }  
}