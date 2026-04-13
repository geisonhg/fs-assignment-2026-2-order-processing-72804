import { useCallback } from 'react'
import { useParams, Link } from 'react-router-dom'
import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'
import { StatusBadge } from './DashboardPage'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' }).format(n)

export default function OrderDetailPage() {
  const { id } = useParams<{ id: string }>()
  const orderId = Number(id)
  const fetcher = useCallback(() => api.getOrder(orderId), [orderId])
  const { data: order, loading, error } = useFetch(fetcher)

  if (loading) return (
    <div className="d-flex flex-column align-items-center gap-2 py-5 text-muted">
      <div className="spinner-border text-primary" role="status" />
      <span style={{ fontSize: '0.9rem' }}>Loading order…</span>
    </div>
  )
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!order) return null

  return (
    <>
      <div className="page-header d-flex align-items-center gap-3">
        <Link to="/orders" className="btn btn-outline-secondary btn-sm">&#8592; Orders</Link>
        <div>
          <h1 className="mb-0">Order #{order.orderId}</h1>
          <p className="text-muted mb-0 mt-1" style={{ fontSize: '0.85rem' }}>
            <code>{order.orderReference}</code>
          </p>
        </div>
        <div className="ms-auto">
          <StatusBadge status={order.statusDisplay} />
        </div>
      </div>

      <div className="row g-3 mb-4">
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">&#128100; Customer</div>
            <div className="card-body">
              <dl className="row mb-0" style={{ fontSize: '0.88rem' }}>
                <dt className="col-4 text-muted fw-normal">Name</dt>
                <dd className="col-8 fw-bold mb-2">{order.customerName}</dd>
                <dt className="col-4 text-muted fw-normal">Email</dt>
                <dd className="col-8 mb-2">{order.customerEmail}</dd>
                <dt className="col-4 text-muted fw-normal">Customer ID</dt>
                <dd className="col-8 mb-0">#{order.customerId}</dd>
              </dl>
            </div>
          </div>
        </div>
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">&#128230; Shipping</div>
            <div className="card-body" style={{ fontSize: '0.88rem' }}>
              <p className="fw-bold mb-1">{order.shippingName}</p>
              <p className="text-muted mb-1">
                {order.shippingLine1}{order.shippingLine2 ? `, ${order.shippingLine2}` : ''}
              </p>
              <p className="text-muted mb-2">{order.shippingCity}, {order.shippingCountry}</p>
              {order.giftWrap && (
                <span className="badge bg-info text-dark me-2">&#127873; Gift wrap</span>
              )}
              {order.shipmentResult?.trackingReference && (
                <div className="mt-2">
                  <span className="text-muted" style={{ fontSize: '0.78rem', textTransform: 'uppercase', fontWeight: 700 }}>Tracking</span>
                  <div><code style={{ fontSize: '0.82rem' }}>{order.shipmentResult.trackingReference}</code></div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      <div className="card mb-4">
        <div className="card-header">&#128179; Order Items</div>
        <table className="table mb-0">
          <thead>
            <tr><th>Product</th><th>Qty</th><th>Unit Price</th><th>Subtotal</th></tr>
          </thead>
          <tbody>
            {order.items.map((item) => (
              <tr key={item.productId}>
                <td className="fw-bold">{item.productName}</td>
                <td>{item.quantity}</td>
                <td className="text-muted">{fmt(item.unitPrice)}</td>
                <td className="fw-bold">{fmt(item.subtotal)}</td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr>
              <td colSpan={3} className="fw-bold text-end">Order Total</td>
              <td className="fw-bold" style={{ color: 'var(--brand-primary)', fontSize: '1rem' }}>
                {fmt(order.totalAmount)}
              </td>
            </tr>
          </tfoot>
        </table>
      </div>

      <div className="card mb-4">
        <div className="card-header">&#128336; Processing Timeline</div>
        <div className="card-body">
          <TimelineItem label="Order Submitted" at={order.createdAt} success />
          {order.inventoryResult && (
            <TimelineItem
              label={order.inventoryResult.success ? '&#9989; Inventory confirmed' : '&#10060; Inventory failed'}
              at={order.inventoryResult.processedAt}
              success={order.inventoryResult.success}
              note={order.inventoryResult.failureReason}
            />
          )}
          {order.paymentResult && (
            <TimelineItem
              label={order.paymentResult.success
                ? `&#9989; Payment approved — ${order.paymentResult.transactionReference}`
                : '&#10060; Payment failed'}
              at={order.paymentResult.processedAt}
              success={order.paymentResult.success}
              note={order.paymentResult.failureReason}
            />
          )}
          {order.shipmentResult && (
            <TimelineItem
              label={order.shipmentResult.success
                ? `&#9989; Shipment created — ${order.shipmentResult.trackingReference}`
                : '&#10060; Shipment failed'}
              at={order.shipmentResult.createdAt}
              success={order.shipmentResult.success}
              note={order.shipmentResult.failureReason}
            />
          )}
        </div>
      </div>

      <p className="text-muted" style={{ fontSize: '0.78rem' }}>
        Correlation ID: <code>{order.correlationId}</code> &bull;{' '}
        Last updated: {new Date(order.updatedAt).toLocaleString('en-IE')}
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
    <div className={`timeline-item ${success ? 'success' : 'danger'}`}>
      <div className="d-flex justify-content-between align-items-start">
        <strong
          style={{ fontSize: '0.88rem' }}
          dangerouslySetInnerHTML={{ __html: label }}
        />
        <small className="text-muted ms-3 text-nowrap">
          {new Date(at).toLocaleString('en-IE')}
        </small>
      </div>
      {note && (
        <div className="text-danger mt-1" style={{ fontSize: '0.8rem' }}>{note}</div>
      )}
    </div>
  )
}
