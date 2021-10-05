import { createElement } from "./util";

export enum SimpleContainerContentType {
  Text,
  HTML
}

export function renderSimpleContainer(containerElement: HTMLElement, title: string, tagline: string, content: string, parentClass: string, contentType: SimpleContainerContentType) {
  const simpleContainerElement = createElement('div', 'simpleContainer', parentClass);
  containerElement.appendChild(simpleContainerElement);

  const sourceUriElement = createElement('h2','simpleContainerHeader');
  sourceUriElement.innerText = title;
  simpleContainerElement.appendChild(sourceUriElement);

  const dateAndAuthorElement = createElement('div', 'simpleContainerTagline');
  dateAndAuthorElement.innerText = tagline;
  simpleContainerElement.appendChild(dateAndAuthorElement);

  const contentElement = createElement('div','simpleContainerContent');
  if (contentType === SimpleContainerContentType.Text) {
    contentElement.innerText = content;
  } else if (contentType === SimpleContainerContentType.HTML) {
    contentElement.innerHTML = content;
  }

  simpleContainerElement.appendChild(contentElement);
}