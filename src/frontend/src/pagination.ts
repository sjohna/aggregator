import { Page } from "./page";
import { createElement } from "./util";

export type Render<T> = (containingElement: HTMLElement, item: T) => void;

export function renderPage<T>(containingElement: HTMLElement, page: Page<T>, renderElement: Render<T>) {
  for (const item of page.items) {
    renderElement(containingElement, item);
  }  
}

export function renderPaginationNavigation<T>(containerElement: HTMLElement, paginationInfo: PaginationInfo, page: Page<T>, renderPage: () => Promise<void>) {
  const paginationElement = createElement('div', 'paginationInformation');
  const infoElement = createElement('span');
  infoElement.innerText = `Documents ${page.offset + 1} - ${page.offset + page.items.length} of ${page.total}`;
  paginationElement.appendChild(infoElement);
  const nextPageButton = createElement('button');
  nextPageButton.innerText = "Next";
  nextPageButton.onclick = async () => {
    if (paginationInfo.offset + paginationInfo.pageSize < paginationInfo.total) {
      paginationInfo.offset += paginationInfo.pageSize;
      await renderPage();
    }
  }
  const prevPageButton = createElement('button');
  prevPageButton.innerText = "Prev";
  prevPageButton.onclick = async () => {
    if (paginationInfo.offset - paginationInfo.pageSize >= 0) {
      paginationInfo.offset -= paginationInfo.pageSize;
      await renderPage();
    }
  }
  paginationElement.appendChild(prevPageButton);
  paginationElement.appendChild(nextPageButton);
  containerElement.appendChild(paginationElement);  
}

export class PaginationInfo {
  public offset = 0;
  public pageSize = 5;
  public total?: number;
}