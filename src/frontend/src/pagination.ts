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
  infoElement.innerText = `${page.offset + 1} - ${page.offset + page.items.length} of ${page.total}`;

  const prevPageButton = createSubElement(paginationElement, 'button');
  prevPageButton.innerText = "Prev";
  prevPageButton.onclick = async () => {
    if (paginationInfo.offset - paginationInfo.pageSize >= 0) {
      paginationInfo.offset -= paginationInfo.pageSize;
      await renderPage();
    }
  }

  const pagesElement = createSubElement(paginationElement, 'span', 'paginationPages');

  let currentPageIndex = (paginationInfo.offset / paginationInfo.pageSize);  // TODO: handle non-integer here?

  let maxPageIndex = Math.floor(paginationInfo.total / paginationInfo.pageSize);
  if (paginationInfo.total % paginationInfo.pageSize === 0) {
    maxPageIndex -= 1;
  }
  if (maxPageIndex < 0) {
    maxPageIndex = 0;
  }

  let firstPageIndex = currentPageIndex - 5;
  let lastPageIndex = currentPageIndex + 5;
  if (firstPageIndex < 0) {
    lastPageIndex -= firstPageIndex;
    firstPageIndex = 0;
  }
  if (lastPageIndex > maxPageIndex) {
    firstPageIndex -= (lastPageIndex - maxPageIndex);
    lastPageIndex = maxPageIndex;
  }

  if (firstPageIndex < 0) firstPageIndex = 0;
  if (lastPageIndex > maxPageIndex) lastPageIndex = maxPageIndex;

  for (let currPageIndex = firstPageIndex; currPageIndex <= lastPageIndex; ++currPageIndex) {
    let pageLinkElement: HTMLElement;
    if (currPageIndex != currentPageIndex) {
      pageLinkElement = createSubElement(pagesElement, 'a', 'paginationPagesLink');
      pageLinkElement.innerText = (currPageIndex+1).toString();
      pageLinkElement.setAttribute('href','#');
      pageLinkElement.onclick = async () => {
        paginationInfo.offset = currPageIndex * paginationInfo.pageSize;
        await renderPage();
      }
    } else {
      pageLinkElement = createSubElement(pagesElement, 'span', 'paginationPagesLink', 'paginationPagesLinkCurrent');
      pageLinkElement.innerText = (currPageIndex+1).toString();
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