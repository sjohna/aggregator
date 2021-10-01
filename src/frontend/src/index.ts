import "./styles.css";
import { RawContent, renderRawContent } from "./rawContent";
import { renderDocuments, UnprocessedDocument } from "./unprocessedDocument";

const UnprocessedDocumentUri = 'api/UnprocessedDocument';
const RawContentUri = 'api/RawContent';

let unprocessedDocuments : UnprocessedDocument[];
let rawContent: RawContent[];

fetchUnprocessedDocuments();
fetchRawContent();

async function fetchUnprocessedDocuments() {
  try {
    const uri = 'https://localhost:44365/' + UnprocessedDocumentUri;
    unprocessedDocuments = await (await fetch(uri)).json();
    //renderDocuments(document.getElementsByTagName('body')[0], unprocessedDocuments);
  } catch (error) {
    console.log(error);
  }
}

async function fetchRawContent() {
  try {
    const uri = 'https://localhost:44365/' + RawContentUri;
    rawContent = await (await fetch(uri)).json();
    renderRawContent(document.getElementsByTagName('body')[0], rawContent);
  } catch (error) {
    console.log(error);
  }
}