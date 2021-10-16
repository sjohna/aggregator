import { createSubElement } from "../Util/util";
import { renderSimpleContainer, SimpleContainerContentType } from "../Util/simpleContainer";

export class RawContent {
  id: string;
  retrieveTime: string;
  content: string;
  context: string;
  type: string;
  sourceUri?: string;
}

export function renderRawContent(containingElement: HTMLElement, rawContent: RawContent[]) {
  const containerElement = createSubElement(containingElement, 'div', 'rawContent');

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
}