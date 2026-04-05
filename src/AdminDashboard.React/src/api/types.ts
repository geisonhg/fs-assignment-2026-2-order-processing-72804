export interface OrderSummary {
  orderId: number
  orderReference: string
  customerName: string
  statusDisplay: string
  totalAmount: number
  createdAt: string
}

export interface OrderDetail {
  orderId: number
  orderReference: string
  customerId: number
  customerName: string
  customerEmail: string
  statusDisplay: string
  totalAmount: number
  createdAt: string
  updatedAt: string
  shippingName: string
  shippingLine1: string
  shippingLine2?: string
  shippingCity: string
  shippingCountry: string
  giftWrap: boolean
  correlationId: string
  items: OrderItem[]
  inventoryResult?: InventoryResult
  paymentResult?: PaymentResult
  shipmentResult?: ShipmentResult
}

export interface OrderItem {
  productId: number
  productName: string
  quantity: number
  unitPrice: number
  subtotal: number
}

export interface InventoryResult {
  success: boolean
  failureReason?: string
  processedAt: string
}

export interface PaymentResult {
  success: boolean
  transactionReference?: string
  failureReason?: string
  amount: number
  processedAt: string
}

export interface ShipmentResult {
  success: boolean
  trackingReference?: string
  estimatedDispatchDate?: string
  failureReason?: string
  createdAt: string
}

export interface Product {
  productId: number
  name: string
  description: string
  price: number
  category: string
  stockQuantity: number
}
