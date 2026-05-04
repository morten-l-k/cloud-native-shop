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
