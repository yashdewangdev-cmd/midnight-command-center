/**
 * OData query builder utility.
 * Constructs OData-compliant query strings for API calls.
 */

export interface ODataQueryOptions {
  select?: string[];
  expand?: string[];
  filter?: string;
  orderby?: string;
  top?: number;
  skip?: number;
  count?: boolean;
}

export function buildODataQuery(options: ODataQueryOptions): string {
  const params: string[] = [];

  if (options.select?.length) {
    params.push(`$select=${options.select.join(',')}`);
  }

  if (options.expand?.length) {
    params.push(`$expand=${options.expand.join(',')}`);
  }

  if (options.filter) {
    params.push(`$filter=${encodeURIComponent(options.filter)}`);
  }

  if (options.orderby) {
    params.push(`$orderby=${options.orderby}`);
  }

  if (options.top !== undefined) {
    params.push(`$top=${options.top}`);
  }

  if (options.skip !== undefined) {
    params.push(`$skip=${options.skip}`);
  }

  if (options.count) {
    params.push('$count=true');
  }

  return params.length > 0 ? `?${params.join('&')}` : '';
}

/**
 * Builds an OData entity key URL segment.
 * Format: ({key}) — e.g., Users(550e8400-e29b-41d4-a716-446655440000)
 */
export function odataKey(key: string): string {
  return `(${key})`;
}
