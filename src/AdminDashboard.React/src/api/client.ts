import type { DashboardSummary, OrderDetail, OrderSummary, Product } from './types'

const BASE = '/api'

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE}${path}`)
  if (!res.ok) throw new Error(`${res.status} ${res.statusText}`)
  return res.json() as Promise<T>
}

export const api = {
  getOrders: () => get<OrderSummary[]>('/orders'),
  getOrdersByStatus: (status: string) => get<OrderSummary[]>(`/orders/by-status/${status}`),
  getOrder: (id: number) => get<OrderDetail>(`/orders/${id}`),
  getProducts: () => get<Product[]>('/products'),
  getDashboardSummary: () => get<DashboardSummary>('/orders/summary'),
}
