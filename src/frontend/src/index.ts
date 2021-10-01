import "./styles.css";
import { RawContent, renderRawContent } from "./rawContent";
import { renderDocuments, UnprocessedDocument } from "./unprocessedDocument";
import { createElement } from "./util";

const UnprocessedDocumentUri = 'api/UnprocessedDocument';
const RawContentUri = 'api/RawContent';

let mainElement = document.getElementById('main');
let headerElement = document.getElementById('header');
let unprocessedDocumentsButton: HTMLElement;
let rawContentButton: HTMLElement;

let unprocessedDocuments : UnprocessedDocument[];
let rawContent: RawContent[];

let currentSelection = "UnprocessedDocuments";

configureHeader();
fetchUnprocessedDocuments();
fetchRawContent();

async function fetchUnprocessedDocuments() {
  try {
    const uri = 'https://localhost:44365/' + UnprocessedDocumentUri;
    unprocessedDocuments = await (await fetch(uri)).json();
    if (currentSelection === "UnprocessedDocuments") {
      selectionUpdated();
    }
  } catch (error) {
    console.log(error);
  }
}

async function fetchRawContent() {
  try {
    const uri = 'https://localhost:44365/' + RawContentUri;
    rawContent = await (await fetch(uri)).json();
    renderRawContent(mainElement, rawContent);
    if (currentSelection === "RawContent") {
      selectionUpdated();
    }
  } catch (error) {
    console.log(error);
  }
}

function configureHeader() {
  unprocessedDocumentsButton = createElement('button', 'headerButton', 'headerButtonSelected');
  rawContentButton = createElement('button', 'headerButton', 'headerButtonUnselected');

  unprocessedDocumentsButton.innerText = "Unprocessed Documents";
  rawContentButton.innerText = "Raw Content";

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

  headerElement.appendChild(unprocessedDocumentsButton);
  headerElement.appendChild(rawContentButton);
}

function selectionUpdated() {
  if (currentSelection === "UnprocessedDocuments") {
    setHeaderButtonSelected(unprocessedDocumentsButton);
    setHeaderButtonUnselected(rawContentButton);

    mainElement.innerHTML = '';
    renderDocuments(mainElement, unprocessedDocuments);
  } else if (currentSelection === "RawContent") {
    setHeaderButtonSelected(rawContentButton);
    setHeaderButtonUnselected(unprocessedDocumentsButton);

    mainElement.innerHTML = '';
    renderRawContent(mainElement, rawContent);
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