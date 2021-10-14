import { Page } from "./page";

export class RestRequester<T> {
  public static host = 'https://localhost:44365/';

  constructor(private uri: string) { }

  async get(filter?: string, sort?: string, pageSize?: number, offset?: number): Promise<Page<T>> {
    let requestUri = RestRequester.host + this.uri;

    const parameters = [];
    if (filter) parameters.push(`filter=${filter}`);
    if (sort) parameters.push(`sort=${sort}`);
    if (pageSize) parameters.push(`pageSize=${pageSize}`);
    if (offset) parameters.push(`offset=${offset}`);
    const parametersString = parameters.join('&');

    if (parametersString) requestUri += `?${parametersString}`;

    return await (await fetch(requestUri)).json() as Page<T>; // TODO: validate, somehow, maybe?
  }
}