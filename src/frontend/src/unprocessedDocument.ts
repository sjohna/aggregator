import { createSubElement } from "./util";
import { renderSimpleContainer, SimpleContainerContentType } from "./simpleContainer";
import { Page } from "./page";
import { RestRequester } from "./restRequester";
import { PaginationInfo, renderPage, renderPaginationNavigation } from "./pagination";

export class UnprocessedDocumentAuthor {
  name: string;
  context: string;
  uri: string;
}

export class UnprocessedDocument {
  id: string;
  uri: string;
  sourceId: string;
  parentDocumentUri?: string;
  retreiveTime: string;
  updateTime: string;
  publishTime: string;
  content: any;
  authors: UnprocessedDocumentAuthor[];
  sourceRawContentId: string;
  documentType: 'Regular' | 'SourceDescription' | 'AuthorDescription';
}

export function renderUnprocessedDocumentInSimpleContainer(containingElement: HTMLElement, unprocessedDocument: UnprocessedDocument) {
  renderSimpleContainer(
    containingElement,
    unprocessedDocument.content.title,
    `${unprocessedDocument.updateTime} ${unprocessedDocument?.authors[0]?.name}`,
    unprocessedDocument?.content?.content,
    'unprocessedDocument',
    SimpleContainerContentType.HTML
  );
}

let documentsElement: HTMLElement;
let orderByUpdatedCheckbox: HTMLInputElement;
const UnprocessedDocumentUri = 'api/UnprocessedDocument';

const paginationInfo = new PaginationInfo();
const requester = new RestRequester<UnprocessedDocument>(UnprocessedDocumentUri);

async function fetchUnprocessedDocuments(pageSize: number, offset: number): Promise<Page<UnprocessedDocument>> {
  const page = await requester.get(
    'DocumentType=\'Regular\'', 
    orderByUpdatedCheckbox?.checked ? 'UpdateTime' : undefined,
    pageSize,
    offset);
    paginationInfo.total = page.total;
  return page;
}

function renderUnprocessedDocumentPage(containingElement: HTMLElement, unprocessedDocuments: Page<UnprocessedDocument>) {
  const containerElement = createSubElement(containingElement, 'div', 'unprocessedDocuments');

  renderPaginationNavigation(containerElement, paginationInfo, unprocessedDocuments, () => fetchAndRenderDocuments(documentsElement));

  renderPage(containerElement, unprocessedDocuments, renderUnprocessedDocumentInSimpleContainer);
}

export async function renderDocuments(containingElement: HTMLElement) {
  const headerElement = createSubElement(containingElement, 'div', 'unprocessedDocumentsHeader');

  const refreshButton = createSubElement(headerElement, 'button', 'headerButton');
  refreshButton.innerText = "Refresh";
  refreshButton.onclick = async () => {
    await fetchAndRenderDocuments(documentsElement);
  }

  orderByUpdatedCheckbox = createSubElement(headerElement, 'input', 'unprocessedDocumentsHeaderInput') as HTMLInputElement;
  orderByUpdatedCheckbox.setAttribute('type','checkbox');
  orderByUpdatedCheckbox.setAttribute('id', 'orderByUpdated')

  const orderByUpdatedLabel = createSubElement(headerElement, 'label');
  orderByUpdatedLabel.innerText = "Order by Update Time";
  orderByUpdatedLabel.setAttribute('for','orderByUpdated');

  documentsElement = createSubElement(containingElement, 'div');

  await fetchAndRenderDocuments(documentsElement);
}

async function fetchAndRenderDocuments(containingElement: HTMLElement) {
  const documents = await fetchUnprocessedDocuments(paginationInfo.pageSize, paginationInfo.offset);
  containingElement.innerHTML = '';

  renderUnprocessedDocumentPage(containingElement, documents);
}