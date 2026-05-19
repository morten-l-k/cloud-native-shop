export interface CustomerOrderItem {
  productId: string;
  orderItemQuantity: number;
  price: number;
  freightValue: number;
  sellerId: string;
}

export interface CustomerOrder {
  orderId: string;
  customerId: string;
  orderStatus: string;
  orderPurchaseTimestamp: string;
  orderEstimatedDeliveryDate: string;
  orderItems: CustomerOrderItem[];
}

export interface CustomerOrderItemDetail {
  productId: string;
  orderItemQuantity: number;
  price: number;
  freightValue: number;
  product?: { productName: string; productCategoryName: string; };
}

export interface CustomerOrderDetail {
  orderId: string;
  orderStatus: string;
  orderPurchaseTimestamp: string;
  orderEstimatedDeliveryDate: string;
  orderItems: CustomerOrderItemDetail[];
}
