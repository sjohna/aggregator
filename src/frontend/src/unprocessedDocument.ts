import { createElement } from "./util";
import { renderSimpleContainer, SimpleContainerContentType } from "./simpleContainer";

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

let documentsElement: HTMLElement;
let orderByUpdatedCheckbox: HTMLInputElement;
const UnprocessedDocumentUri = 'api/UnprocessedDocument';

async function fetchUnprocessedDocuments(): Promise<UnprocessedDocument[]> {
  let uri = 'https://localhost:44365/' + UnprocessedDocumentUri;
  if (orderByUpdatedCheckbox?.checked === true) {
    uri += '?sort=UpdateTime';
  }
  return await (await fetch(uri)).json();
}

function renderUnprocessedDocumentPage(containingElement: HTMLElement, unprocessedDocuments: UnprocessedDocument[]) {
  const containerElement = createElement('div', 'unprocessedDocuments');

  for (const unprocessedDocument of unprocessedDocuments) {
    if (unprocessedDocument.documentType !== 'Regular') {
      continue;
    }

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
  const documents = await fetchUnprocessedDocuments();
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