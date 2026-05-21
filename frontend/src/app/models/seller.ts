export interface Seller {
  sellerId: string;
  sellerZipCodePrefix: string;
  sellerCity: string;
  sellerState: string;
}

export interface SellerOrderSummary {
  orderId: string;
  orderStatus: string;
  orderPurchaseTimestamp: string;
  orderEstimatedDeliveryDate: string;
  itemCount: number;
  totalValue: number;
}

export interface SellerOrderItem {
  productId: string;
  product: {
    productName: string;
    productCategoryName: string;
    productCategoryNameEnglish: string | null;
    productPrice: number;
  };
  orderItemQuantity: number;
  price: number;
  freightValue: number;
}

export interface SellerOrderDetail {
  orderId: string;
  orderStatus: string;
  orderPurchaseTimestamp: string;
  orderEstimatedDeliveryDate: string;
  orderItems: SellerOrderItem[];
}

export interface SellerProduct {
  productId: string;
  productName: string;
  productCategoryName: string | null;
  productCategoryNameEnglish: string | null;
  productDescription: string | null;
  productPrice: number;
  productStock: number;
  totalSold: number;
  totalRevenue: number;
  isActive: boolean;
}


export interface MonthlyRevenue {
  month: string;
  revenue: number;
  orderCount: number;
}

export interface SellerAnalytics {
  totalRevenue: number;
  totalOrders: number;
  avgOrderValue: number;
  monthlyRevenue: MonthlyRevenue[];
  statusBreakdown: { status: string; count: number }[];
}
