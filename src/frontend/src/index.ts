import "./styles.css";
import { RawContent, renderRawContent } from "./rawContent";
import { PaginatedContainer, UnprocessedDocument } from "./unprocessedDocument";
import { createElement } from "./util";

const RawContentUri = 'api/RawContent';
const DownloadRawContentUri = 'api/RawContent/Download';

let mainElement = document.getElementById('main');
let headerElement = document.getElementById('header');
let unprocessedDocumentsButton: HTMLElement;
let rawContentButton: HTMLElement;
let uploadRawContentButton: HTMLElement;

let unprocessedDocuments : UnprocessedDocument[];
let rawContent: RawContent[];

let currentSelection = "UnprocessedDocuments";

configureHeader();
fetchRawContent();
new PaginatedContainer().renderDocuments(mainElement);

async function fetchRawContent() {
  try {
    const uri = 'https://localhost:44365/' + RawContentUri;
    rawContent = await (await fetch(uri)).json();
    if (currentSelection === "RawContent") {
      selectionUpdated();
    }
  } catch (error) {
    console.log(error);
  }
}

const contextField = createElement('input', 'uploadContentInputShort') as HTMLInputElement;
const typeField = createElement('input', 'uploadContentInputShort') as HTMLInputElement;
const sourceUriField = createElement('input', 'uploadContentInputLong') as HTMLInputElement;
const contentField = createElement('span', 'uploadContentContent');

function renderUploadRawContent(containingElement: HTMLElement) {
  const containerElement = createElement('div','uploadRawContent');
  
  const downloadButton = createElement('button', 'updateContentDownloadButton');
  downloadButton.innerText = 'Download and Process Source';
  downloadButton.onclick = () => {
    sendDownloadRequest();
  };
  containerElement.appendChild(downloadButton);

  {
    const contextRow = createElement('div', 'uploadContentRow');
    const contextLabel = createElement('span', 'uploadContentLabel');
    contextLabel.innerText = "Context:";
    contextRow.appendChild(contextLabel);
    contextField.setAttribute('type', 'text');
    contextField.setAttribute('name', 'context');
    contextField.value = 'blog';
    contextRow.appendChild(contextField);
    containerElement.appendChild(contextRow);
  }

  {
    const typeRow = createElement('div', 'uploadContentRow');
    const typeLabel = createElement('span', 'uploadContentLabel');
    typeLabel.innerText = "Type:";
    typeRow.appendChild(typeLabel);
    typeField.setAttribute('type', 'text');
    typeField.setAttribute('name', 'type');
    typeField.value = 'atom/xml';
    typeRow.appendChild(typeField);
    containerElement.appendChild(typeRow);
  }

  {
    const sourceUriRow = createElement('div', 'uploadContentRow');
    const sourceUriLabel = createElement('span', 'uploadContentLabel');
    sourceUriLabel.innerText = "Source URI:";
    sourceUriRow.appendChild(sourceUriLabel);
    sourceUriField.setAttribute('type', 'text');
    sourceUriField.setAttribute('name', 'type');
    sourceUriRow.appendChild(sourceUriField);
    // const downloadButton = createElement('button', 'downloadSourceUriButton');
    // downloadButton.onclick = () => downloadSourceUri(sourceUriField, contentField);
    // downloadButton.innerText = "Download";
    // sourceUriRow.appendChild(downloadButton);
    containerElement.appendChild(sourceUriRow);
  }

  {
    const contentRow = createElement('div', 'uploadContentRow','uploadContentRowFlex');
    // const contentLabel = createElement('span', 'uploadContentLabel', 'uploadContentContentLabel');
    // contentLabel.innerText = "Content:";
    // contentRow.appendChild(contentLabel);
    // contentField.innerText = 'some content\nmore content';
    contentRow.appendChild(contentField);
    containerElement.appendChild(contentRow);
  }

  containingElement.appendChild(containerElement);
}

async function downloadSourceUri(sourceUriField: HTMLInputElement, contentField: HTMLElement) {
  const uri = sourceUriField.value;
  const content = await (await fetch(uri)).text();
  contentField.innerText = content;
}

async function sendDownloadRequest() {
  const url = 'https://localhost:44365/' + DownloadRawContentUri;

  var xhr = new XMLHttpRequest();
  xhr.open("POST", url);

  xhr.setRequestHeader("Accept", "application/json");
  xhr.setRequestHeader("Content-Type", "application/json");

  xhr.onreadystatechange = function () {
    if (xhr.readyState === 4) {
        console.log(xhr.status);
        console.log(xhr.responseText);
        const responseData = JSON.parse(this.responseText);
        contentField.innerText = `${xhr.status}\nDocuments Added: ${responseData.unprocessedDocumentAdditions?.length}\nRaw Content Added: ${responseData.rawContentAdditions?.length}`
    }};

  var data = {
    Context: contextField.value,
    Type: typeField.value,
    SourceUri: sourceUriField.value
  };

  xhr.send(JSON.stringify(data));
}

// Header ------------------------------------------------------------

function configureHeader() {
  unprocessedDocumentsButton = createElement('button', 'headerButton', 'headerButtonSelected');
  rawContentButton = createElement('button', 'headerButton', 'headerButtonUnselected');
  uploadRawContentButton = createElement('button', 'headerButton', 'headerButtonUnselected');

  unprocessedDocumentsButton.innerText = "Unprocessed Documents";
  rawContentButton.innerText = "Raw Content";
  uploadRawContentButton.innerHTML = "Upload Content";

  unprocessedDocumentsButton.onclick = () => {
    if (currentSelection !== "UnprocessedDocuments") {
      currentSelection = "UnprocessedDocuments";
      selectionUpdated();
    }
  }

  rawContentButton.onclick = () => {
    if (currentSelection !== "RawContent") {
      currentSelection = "RawContent";
      selectionUpdated();
    }
  }

  uploadRawContentButton.onclick = () => {
    if (currentSelection !== "UploadRawContent") {
      currentSelection = "UploadRawContent";
      selectionUpdated();
    }
  }

  headerElement.appendChild(unprocessedDocumentsButton);
  headerElement.appendChild(rawContentButton);
  headerElement.appendChild(uploadRawContentButton);
}

function selectionUpdated() {
  if (currentSelection === "UnprocessedDocuments") {
    setHeaderButtonSelected(unprocessedDocumentsButton);
    setHeaderButtonUnselected(rawContentButton);
    setHeaderButtonUnselected(uploadRawContentButton);

    mainElement.innerHTML = '';
    new PaginatedContainer().renderDocuments(mainElement);
  } else if (currentSelection === "RawContent") {
    setHeaderButtonSelected(rawContentButton);
    setHeaderButtonUnselected(unprocessedDocumentsButton);
    setHeaderButtonUnselected(uploadRawContentButton);

    mainElement.innerHTML = '';
    renderRawContent(mainElement, rawContent);
  } else if (currentSelection === "UploadRawContent") {
    setHeaderButtonSelected(uploadRawContentButton);
    setHeaderButtonUnselected(rawContentButton);
    setHeaderButtonUnselected(unprocessedDocumentsButton);

    mainElement.innerHTML = '';
    renderUploadRawContent(mainElement);
  }
}

function setHeaderButtonSelected(button: HTMLElement) {
  button.classList.remove('headerButtonUnselected');
  button.classList.add('headerButtonSelected');
}

function setHeaderButtonUnselected(button: HTMLElement) {
  button.classList.remove('headerButtonSelected');
  button.classList.add('headerButtonUnselected');
}