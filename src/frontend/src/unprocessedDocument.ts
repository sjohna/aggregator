import { createElement } from "./util";
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
  const containerElement = createElement('div', 'unprocessedDocuments');

  renderPaginationNavigation(containerElement, paginationInfo, unprocessedDocuments, () => fetchAndRenderDocuments(documentsElement));

  renderPage(containerElement, unprocessedDocuments, renderUnprocessedDocumentInSimpleContainer);

  containingElement.appendChild(containerElement);
}

export async function renderDocuments(containingElement: HTMLElement) {
  // top row: options
  const headerElement = createElement('div', 'unprocessedDocumentsHeader');
  const refreshButton = createElement('button', 'headerButton');

  refreshButton.innerText = "Refresh";

  refreshButton.onclick = async () => {
    await fetchAndRenderDocuments(documentsElement);
  }

  headerElement.appendChild(refreshButton);

  orderByUpdatedCheckbox = createElement('input', 'unprocessedDocumentsHeaderInput') as HTMLInputElement;
  orderByUpdatedCheckbox.setAttribute('type','checkbox');
  orderByUpdatedCheckbox.setAttribute('id', 'orderByUpdated')
  headerElement.appendChild(orderByUpdatedCheckbox);
  const orderByUpdatedLabel = createElement('label');
  orderByUpdatedLabel.innerText = "Order by Update Time";
  orderByUpdatedLabel.setAttribute('for','orderByUpdated');
  headerElement.appendChild(orderByUpdatedLabel);


  containingElement.appendChild(headerElement);

  documentsElement = createElement('div');
  containingElement.appendChild(documentsElement);

  await fetchAndRenderDocuments(documentsElement);
}

async function fetchAndRenderDocuments(containingElement: HTMLElement) {
  const documents = await fetchUnprocessedDocuments(paginationInfo.pageSize, paginationInfo.offset);
  containingElement.innerHTML = '';

  renderUnprocessedDocumentPage(containingElement, documents);
}