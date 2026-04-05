import { useCallback } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'
import { StatusBadge } from './DashboardPage'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const orderId = Number(id)
  const fetcher = useCallback(() => api.getOrder(orderId), [orderId])
  const { data: order, loading, error } = useFetch(fetcher)

  if (loading) return <div className="spinner-border text-primary mt-4" role="status" />
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!order) return null

  return (
    <>
      <div className="d-flex align-items-center gap-3 mb-4">
        <Link to="/orders" className="btn btn-outline-secondary btn-sm">&larr; Orders</Link>
        <h1 className="mb-0">Order #{order.orderId}</h1>
        <StatusBadge status={order.statusDisplay} />
      </div>

      <div className="row g-3 mb-4">
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">Customer</div>
            <div className="card-body">
              <dl className="row mb-0">
                <dt className="col-4">Name</dt><dd className="col-8">{order.customerName}</dd>
                <dt className="col-4">Email</dt><dd className="col-8">{order.customerEmail}</dd>
                <dt className="col-4">Customer ID</dt><dd className="col-8">{order.customerId}</dd>
              </dl>
            </div>
          </div>
        </div>
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">Shipping</div>
            <div className="card-body">
              <p className="mb-1">{order.shippingName}</p>
              <p className="mb-1">{order.shippingLine1}{order.shippingLine2 ? `, ${order.shippingLine2}` : ''}</p>
              <p className="mb-1">{order.shippingCity}, {order.shippingCountry}</p>
              {order.giftWrap && <span className="badge bg-info">Gift wrap</span>}
              {order.shipmentResult?.trackingReference && (
                <p className="mt-2 mb-0">Tracking: <code>{order.shipmentResult.trackingReference}</code></p>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="card mb-4">
        <div className="card-header">Items</div>
        <table className="table table-sm mb-0">
          <thead>
            <tr><th>Product</th><th>Qty</th><th>Unit Price</th><th>Subtotal</th></tr>
          </thead>
          <tbody>
            {order.items.map((item) => (
              <tr key={item.productId}>
                <td>{item.productName}</td>
                <td>{item.quantity}</td>
                <td>{fmt(item.unitPrice)}</td>
                <td>{fmt(item.subtotal)}</td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr className="fw-bold">
              <td colSpan={3}>Total</td>
              <td>{fmt(order.totalAmount)}</td>
            </tr>
          </tfoot>
        </table>
      </div>

      <h5>Processing Timeline</h5>
      <div className="list-group mb-4">
        <TimelineItem
          label="Order Submitted"
          at={order.createdAt}
          success
        />
        {order.inventoryResult && (
          <TimelineItem
            label={order.inventoryResult.success ? 'Inventory confirmed' : 'Inventory failed'}
            at={order.inventoryResult.processedAt}
            success={order.inventoryResult.success}
            note={order.inventoryResult.failureReason}
          />
        )}
        {order.paymentResult && (
          <TimelineItem
            label={order.paymentResult.success ? `Payment approved (${order.paymentResult.transactionReference})` : 'Payment failed'}
            at={order.paymentResult.processedAt}
            success={order.paymentResult.success}
            note={order.paymentResult.failureReason}
          />
        )}
        {order.shipmentResult && (
          <TimelineItem
            label={order.shipmentResult.success ? `Shipment created — ${order.shipmentResult.trackingReference}` : 'Shipment failed'}
            at={order.shipmentResult.createdAt}
            success={order.shipmentResult.success}
            note={order.shipmentResult.failureReason}
          />
        )}
      </div>

      <p className="text-muted small">
        Correlation ID: <code>{order.correlationId}</code> &bull;
        Last updated: {new Date(order.updatedAt).toLocaleString()}
      </p>
    </>
  )
}

function TimelineItem({
  label, at, success, note,
}: {
  label: string; at: string; success: boolean; note?: string
}) {
  return (
    <div className={`list-group-item list-group-item-${success ? 'success' : 'danger'}`}>
      <div className="d-flex justify-content-between">
        <strong>{label}</strong>
        <small>{new Date(at).toLocaleString()}</small>
      </div>
      {note && <small className="text-muted">{note}</small>}
    </div>
  )
}
