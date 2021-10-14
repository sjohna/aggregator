import { createSubElement } from "./util";

export enum SimpleContainerContentType {
  Text,
  HTML
}

export function renderSimpleContainer(containerElement: HTMLElement, title: string, tagline: string, content: string, parentClass: string, contentType: SimpleContainerContentType) {
  const simpleContainerElement = createSubElement(containerElement, 'div', 'simpleContainer', parentClass);

  const sourceUriElement = createSubElement(simpleContainerElement, 'h2','simpleContainerHeader');
  sourceUriElement.innerText = title;

  const taglineElement = createSubElement(simpleContainerElement, 'div', 'simpleContainerTagline');
  taglineElement.innerText = tagline;

  const contentElement = createSubElement(simpleContainerElement, 'div','simpleContainerContent');
  if (contentType === SimpleContainerContentType.Text) {
    contentElement.innerText = content;
  } else if (contentType === SimpleContainerContentType.HTML) {
    contentElement.innerHTML = content;
  }
}