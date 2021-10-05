import { createElement } from "./util";
import { renderSimpleContainer, SimpleContainerContentType } from "./simpleContainer";

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
    renderSimpleContainer(
      containerElement,
      `${content.context} (${content.type}) ${content.sourceUri ?? 'No Source URI'}`,
      `${content.retrieveTime}`,
      content.content,
      'individualRawContent',
      SimpleContainerContentType.Text
    );
  }

  containingElement.appendChild(containerElement);
}