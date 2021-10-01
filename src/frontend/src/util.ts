export function createElement(tag: string, ...classNames: string[]): HTMLElement {
  const element = document.createElement(tag);
  for (const className of classNames) {
    if (className) element.classList.add(className);
  }
  return element;
}