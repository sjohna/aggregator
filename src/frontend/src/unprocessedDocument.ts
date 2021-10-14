import { createElement } from "./util";
import { renderSimpleContainer, SimpleContainerContentType } from "./simpleContainer";
import { Page } from "./page";
import { RestRequester } from "./restRequester";

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

const UnprocessedDocumentUri = 'api/UnprocessedDocument';

export class PaginatedContainer {
  public offset = 0;
  public pageSize = 5;
  public total: number;
  public orderByUpdatedCheckbox: HTMLInputElement;
  private requester = new RestRequester<UnprocessedDocument>(UnprocessedDocumentUri);
  public filter = 'DocumentType=\'Regular\'';

  constructor() { }

  async fetch(): Promise<Page<UnprocessedDocument>> {
    const page = await this.requester.get(
      this.filter, 
      this.orderByUpdatedCheckbox?.checked ? 'UpdateTime' : undefined,
      this.pageSize,
      this.offset);
    this.total = page.total;
    return page;
  }

  renderPage(containingElement: HTMLElement, unprocessedDocuments: Page<UnprocessedDocument>) {
    const containerElement = createElement('div', 'unprocessedDocuments');
    const paginationElement = createElement('div', 'paginationInformation');
    const infoElement = createElement('span');
    infoElement.innerText = `Documents ${unprocessedDocuments.offset + 1} - ${unprocessedDocuments.offset + unprocessedDocuments.items.length} of ${unprocessedDocuments.total}`;
    paginationElement.appendChild(infoElement);
    const nextPageButton = createElement('button');
    nextPageButton.innerText = "Next";
    nextPageButton.onclick = async () => {
      if (this.offset + this.pageSize < this.total) {
        this.offset += this.pageSize;
        await this.fetchAndRenderDocuments(documentsElement)
      }
    }
    const prevPageButton = createElement('button');
    prevPageButton.innerText = "Prev";
    prevPageButton.onclick = async () => {
      if (this.offset - this.pageSize >= 0) {
        this.offset -= this.pageSize;
        await this.fetchAndRenderDocuments(documentsElement)
      }
    }
    paginationElement.appendChild(prevPageButton);
    paginationElement.appendChild(nextPageButton);
    containerElement.appendChild(paginationElement);
  
    for (const unprocessedDocument of unprocessedDocuments.items) {
      renderSimpleContainer(
        containerElement,
        unprocessedDocument.content.title,
        `${unprocessedDocument.updateTime} ${unprocessedDocument?.authors[0]?.name}`,
        unprocessedDocument?.content?.content,
        'unprocessedDocument',
        SimpleContainerContentType.HTML
      );
    }
  
    containingElement.appendChild(containerElement);
  }

  async renderDocuments(containingElement: HTMLElement) {
    // top row: options
    const headerElement = createElement('div', 'unprocessedDocumentsHeader');
    const refreshButton = createElement('button', 'headerButton');
  
    refreshButton.innerText = "Refresh";
  
    refreshButton.onclick = async () => {
      await this.fetchAndRenderDocuments(documentsElement);
    }
  
    headerElement.appendChild(refreshButton);
  
    this.orderByUpdatedCheckbox = createElement('input', 'unprocessedDocumentsHeaderInput') as HTMLInputElement;
    this.orderByUpdatedCheckbox.setAttribute('type','checkbox');
    this.orderByUpdatedCheckbox.setAttribute('id', 'orderByUpdated')
    headerElement.appendChild(this.orderByUpdatedCheckbox);
    const orderByUpdatedLabel = createElement('label');
    orderByUpdatedLabel.innerText = "Order by Update Time";
    orderByUpdatedLabel.setAttribute('for','orderByUpdated');
    headerElement.appendChild(orderByUpdatedLabel);
  
  
    containingElement.appendChild(headerElement);
  
    documentsElement = createElement('div');
    containingElement.appendChild(documentsElement);
  
    await this.fetchAndRenderDocuments(documentsElement);
  }
  
  async fetchAndRenderDocuments(containingElement: HTMLElement) {
    const documents = await this.fetch();
    containingElement.innerHTML = '';
  
    this.renderPage(containingElement, documents);
  }
  
  createContentElement(doc: UnprocessedDocument) {
    const contentElement = createElement('div', 'content');
    contentElement.innerHTML = doc.content.content;
  
    return contentElement;
  }
}

let documentsElement: HTMLElement;