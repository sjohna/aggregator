import "./styles.css";

const UnprocessedDocumentsUri = 'api/UnprocessedDocument';

const uri = 'https://localhost:44365/' + UnprocessedDocumentsUri;

class UnprocessedDocumentAuthor {
  name: string;
  context: string;
  uri: string;
}

class UnprocessedDocument {
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

let unprocessedDocuments : UnprocessedDocument[];

fetchUnprocessedDocuments();

async function fetchUnprocessedDocuments() {
  try {
    const unprocessedDocumentsResponse = await (await fetch(uri)).json();
    unprocessedDocuments = unprocessedDocumentsResponse;
    renderDocuments();
  } catch (error) {
    console.log(error);
  }
}

function renderDocuments() {
  let body = document.getElementsByTagName('body')[0];

  const containerElement = createElement('div', 'unprocessedDocuments');

  for (const unprocessedDocument of unprocessedDocuments) {
    if (unprocessedDocument.documentType !== 'Regular') {
      continue;
    }

    const documentElement = createElement('div', 'unprocessedDocument');
    containerElement.appendChild(documentElement);

    const titleElement = createElement('h2', 'title');
    titleElement.innerHTML = unprocessedDocument.content.title;
    documentElement.appendChild(titleElement);

    const dateAndAuthorElement = createElement('div', 'dateAndAuthor');
    dateAndAuthorElement.innerText = `${unprocessedDocument.updateTime} ${unprocessedDocument?.authors[0]?.name}`
    documentElement.appendChild(dateAndAuthorElement);

    documentElement.appendChild(createContentElement(unprocessedDocument));
  }

  body.appendChild(containerElement);
}

function createContentElement(doc: UnprocessedDocument) {
  const contentElement = createElement('div', 'content');
  contentElement.innerHTML = doc.content.content;

  const imageTags = contentElement.getElementsByTagName('img');

  for (let i = 0; i < imageTags.length; ++i) {
    const image = imageTags.item(i);
    
    // TODO: find a more general way to stopp image loading. I don't like how I have to figure out exactly which attributes to cache.
    let src: string;
    if (image.src) {
      src = image.src;
      image.src = '#';
    }

    let srcset: string;
    if (image.srcset) {
      srcset = image.srcset;
      image.srcset = '#';
    }


    const placeholderElement = createElement('div','imagePlaceholder');
    placeholderElement.style.height = '250px';
    placeholderElement.style.width = '250px';

    const showPlaceholderDiv = createElement('div');
    const showPlaceholderButton = createElement('button');
    showPlaceholderButton.innerText = "Show Image";
    showPlaceholderButton.onclick = (event) => {
      //contentElement.replaceChild(image, placeholderElement);
      placeholderElement.insertAdjacentElement('afterend', image);
      placeholderElement.remove();
      if (src) image.src = src;
      if (srcset) image.srcset = srcset;
    }
    showPlaceholderDiv.appendChild(showPlaceholderButton);
    placeholderElement.appendChild(showPlaceholderDiv);
    const imageLinkText = createElement('div','imagePlaceholderLinkText');
    imageLinkText.innerText = srcset;
    placeholderElement.appendChild(imageLinkText);

    image.insertAdjacentElement('afterend', placeholderElement);
    image.remove();
  }

  return contentElement;
}

function createElement(tag: string, className: string = null): HTMLElement {
  const element = document.createElement(tag);
  if (className) element.classList.add(className);
  return element;
}