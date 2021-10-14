import { createElement } from "./util";
import { renderSimpleContainer, SimpleContainerContentType } from "./simpleContainer";
import { Page } from "./page";
import { RestRequester } from "./restRequester";
import { renderPage } from "./pagination";

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

let offset = 0;
let pageSize = 5;
let total: number;
const requester = new RestRequester<UnprocessedDocument>(UnprocessedDocumentUri);

async function fetchUnprocessedDocuments(pageSize: number, offset: number): Promise<Page<UnprocessedDocument>> {
  const page = await requester.get(
    'DocumentType=\'Regular\'', 
    orderByUpdatedCheckbox?.checked ? 'UpdateTime' : undefined,
    pageSize,
    offset);
  total = page.total;
  return page;
}

function renderUnprocessedDocumentPage(containingElement: HTMLElement, unprocessedDocuments: Page<UnprocessedDocument>) {
  const containerElement = createElement('div', 'unprocessedDocuments');
  const paginationElement = createElement('div', 'paginationInformation');
  const infoElement = createElement('span');
  infoElement.innerText = `Documents ${unprocessedDocuments.offset + 1} - ${unprocessedDocuments.offset + unprocessedDocuments.items.length} of ${unprocessedDocuments.total}`;
  paginationElement.appendChild(infoElement);
  const nextPageButton = createElement('button');
  nextPageButton.innerText = "Next";
  nextPageButton.onclick = async () => {
    if (offset + pageSize < total) {
      offset += pageSize;
      await fetchAndRenderDocuments(documentsElement)
    }
  }
  const prevPageButton = createElement('button');
  prevPageButton.innerText = "Prev";
  prevPageButton.onclick = async () => {
    if (offset - pageSize >= 0) {
      offset -= pageSize;
      await fetchAndRenderDocuments(documentsElement)
    }
  }
  paginationElement.appendChild(prevPageButton);
  paginationElement.appendChild(nextPageButton);
  containerElement.appendChild(paginationElement);

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
  const documents = await fetchUnprocessedDocuments(pageSize,offset);
  containingElement.innerHTML = '';

  renderUnprocessedDocumentPage(containingElement, documents);
}

export function createContentElement(doc: UnprocessedDocument) {
  const contentElement = createElement('div', 'content');
  contentElement.innerHTML = doc.content.content;

  // const imageTags = contentElement.getElementsByTagName('img');

  // for (let i = 0; i < imageTags.length; ++i) {
  //   const image = imageTags.item(i);
    
  //   // TODO: find a more general way to stopp image loading. I don't like how I have to figure out exactly which attributes to cache.
  //   let src: string;
  //   if (image.src) {
  //     src = image.src;
  //     image.src = '#';
  //   }

  //   let srcset: string;
  //   if (image.srcset) {
  //     srcset = image.srcset;
  //     image.srcset = '#';
  //   }

  //   const placeholderElement = createElement('div','imagePlaceholder');
  //   placeholderElement.style.height = '250px';
  //   placeholderElement.style.width = '250px';

  //   const showPlaceholderDiv = createElement('div');
  //   const showPlaceholderButton = createElement('button');
  //   showPlaceholderButton.innerText = "Show Image";
  //   showPlaceholderButton.onclick = (event) => {
  //     //contentElement.replaceChild(image, placeholderElement);
  //     placeholderElement.insertAdjacentElement('afterend', image);
  //     placeholderElement.remove();
  //     if (src) image.src = src;
  //     if (srcset) image.srcset = srcset;
  //   }
  //   showPlaceholderDiv.appendChild(showPlaceholderButton);
  //   placeholderElement.appendChild(showPlaceholderDiv);
  //   const imageLinkText = createElement('div','imagePlaceholderLinkText');
  //   imageLinkText.innerText = srcset;
  //   placeholderElement.appendChild(imageLinkText);

  //   image.insertAdjacentElement('afterend', placeholderElement);
  //   image.remove();
  // }

  return contentElement;
}