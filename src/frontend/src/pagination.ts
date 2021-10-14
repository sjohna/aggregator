import { Page } from "./page";
import { createSubElement } from "./util";

export type Render<T> = (containingElement: HTMLElement, item: T) => void;

export function renderPage<T>(containingElement: HTMLElement, page: Page<T>, renderElement: Render<T>) {
  for (const item of page.items) {
    renderElement(containingElement, item);
  }  
}

export function renderPaginationNavigation<T>(containerElement: HTMLElement, paginationInfo: PaginationInfo, page: Page<T>, renderPage: () => Promise<void>) {
  const paginationElement = createSubElement(containerElement, 'div', 'paginationInformation');
  const infoElement = createSubElement(paginationElement, 'span');
  infoElement.innerText = `Documents ${page.offset + 1} - ${page.offset + page.items.length} of ${page.total}`;
  const prevPageButton = createSubElement(paginationElement, 'button');
  prevPageButton.innerText = "Prev";
  prevPageButton.onclick = async () => {
    if (paginationInfo.offset - paginationInfo.pageSize >= 0) {
      paginationInfo.offset -= paginationInfo.pageSize;
      await renderPage();
    }
  }
  const nextPageButton = createSubElement(paginationElement, 'button');
  nextPageButton.innerText = "Next";
  nextPageButton.onclick = async () => {
    if (paginationInfo.offset + paginationInfo.pageSize < paginationInfo.total) {
      paginationInfo.offset += paginationInfo.pageSize;
      await renderPage();
    }
  }

}

export class PaginationInfo {
  public offset = 0;
  public pageSize = 5;
  public total?: number;
}