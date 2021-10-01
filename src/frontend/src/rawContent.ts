export class RawContent {
  id: string;
  retrieveTime: string;
  content: string;
  context: string;
  type: string;
  sourceUri?: string;
}

export function renderRawContent(containingElement: HTMLElement, rawContent: RawContent[]) {
  const containerElement = createElement('div', 'rawContent');

  for (const content of rawContent) {
    const rawContentElement = createElement('div', 'individualRawContent');
    containerElement.appendChild(rawContentElement);

    const sourceUriElement = createElement('h2','rawContentSourceUri');
    sourceUriElement.innerText = `${content.context} (${content.type}) ${content.sourceUri ?? 'No Source URI'}`;
    rawContentElement.appendChild(sourceUriElement);

    const dateAndAuthorElement = createElement('div', 'rawContentRetrieveTime');
    dateAndAuthorElement.innerText = `${content.retrieveTime}`
    rawContentElement.appendChild(dateAndAuthorElement);

    const contentElement = createElement('div','rawContentContent');
    contentElement.innerText = content.content;
    rawContentElement.appendChild(contentElement);
  }

  containingElement.appendChild(containerElement);
}

function createElement(tag: string, className: string = null): HTMLElement {
  const element = document.createElement(tag);
  if (className) element.classList.add(className);
  return element;
}