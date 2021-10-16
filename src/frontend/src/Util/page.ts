export class Page<T> {
    pageSize: number;
    offset: number;
    total?: number;
    items: T[];
}