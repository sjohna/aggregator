import "./styles.css";
import { RawContent, renderRawContent } from "./rawContent";
import { renderDocuments, UnprocessedDocument } from "./unprocessedDocument";

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
    unprocessedDocumentsButton.classList.remove('headerButtonUnselected');
    unprocessedDocumentsButton.classList.add('headerButtonSelected');
    rawContentButton.classList.remove('headerButtonSelected');
    rawContentButton.classList.add('headerButtonUnselected');

    mainElement.innerHTML = '';
    renderDocuments(mainElement, unprocessedDocuments);
  } else if (currentSelection === "RawContent") {
    unprocessedDocumentsButton.classList.add('headerButtonUnselected');
    unprocessedDocumentsButton.classList.remove('headerButtonSelected');
    rawContentButton.classList.add('headerButtonSelected');
    rawContentButton.classList.remove('headerButtonUnselected');

    mainElement.innerHTML = '';
    renderRawContent(mainElement, rawContent);
  }
}

function createElement(tag: string, ...classNames: string[]): HTMLElement {
  const element = document.createElement(tag);
  for (const className of classNames) {
    if (className) element.classList.add(className);
  }
  return element;
}