export interface Product {
  id: string;
  name: string;
  price: number;
  imageUrl: string;
}

export interface ProductFilters {
  page?: number;
  minPrice?: number;
  maxPrice?: number;
  category?: string;
  sort?: 'price_asc' | 'price_desc' | '';
}

export interface ProductPage {
  items: Product[];
  page: number;
  pageSize: 10;
  totalCount: number;
  totalPages: number;
}
