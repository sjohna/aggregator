import "./styles.css";
import { renderDocuments, UnprocessedDocument } from "./unprocessedDocument";

const UnprocessedDocumentsUri = 'api/UnprocessedDocument';

const uri = 'https://localhost:44365/' + UnprocessedDocumentsUri;

let unprocessedDocuments : UnprocessedDocument[];

fetchUnprocessedDocuments();

async function fetchUnprocessedDocuments() {
  try {
    const unprocessedDocumentsResponse = await (await fetch(uri)).json();
    unprocessedDocuments = unprocessedDocumentsResponse;
    renderDocuments(document.getElementsByTagName('body')[0], unprocessedDocuments);
  } catch (error) {
    console.log(error);
  }
}