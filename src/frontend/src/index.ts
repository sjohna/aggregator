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
  documentType: number;
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
    const documentElement = createElement('div', 'unprocessedDocument');
    containerElement.appendChild(documentElement);

    const titleElement = createElement('h2', 'title');
    titleElement.innerHTML = unprocessedDocument.content.title;
    documentElement.appendChild(titleElement);

    const dateAndAuthorElement = createElement('div', 'dateAndAuthor');
    dateAndAuthorElement.innerText = `${unprocessedDocument.updateTime} ${unprocessedDocument?.authors[0]?.name}`
    documentElement.appendChild(dateAndAuthorElement);

    const contentElement = createElement('div', 'content');
    contentElement.innerHTML = unprocessedDocument.content.content;
    documentElement.appendChild(contentElement);
  }

  body.appendChild(containerElement);
}

function createElement(tag: string, className: string): HTMLElement {
  const element = document.createElement(tag);
  element.classList.add(className);
  return element;
}