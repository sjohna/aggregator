import "./styles.css";

console.log("Scripts work.");

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

  const containerElement = document.createElement('div');
  containerElement.classList.add('unprocessedDocuments');

  for (const unprocessedDocument of unprocessedDocuments) {
    const documentElement = document.createElement('div');
    documentElement.classList.add('unprocessedDocument');
    containerElement.appendChild(documentElement);

    const titleElement = document.createElement('h2');
    titleElement.classList.add('title');
    titleElement.innerHTML = unprocessedDocument.content.title;
    documentElement.appendChild(titleElement);

    const dateAndAuthorElement = document.createElement('div');
    dateAndAuthorElement.classList.add('dateAndAuthor');
    dateAndAuthorElement.innerText = `${unprocessedDocument.updateTime} ${unprocessedDocument?.authors[0]?.name}`
    documentElement.appendChild(dateAndAuthorElement);

    const contentElement = document.createElement('div');
    contentElement.classList.add('content');
    contentElement.innerHTML = unprocessedDocument.content.content;
    documentElement.appendChild(contentElement);
  }

  body.appendChild(containerElement);
}