import { Link } from 'react-router-dom'
import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'
import { StatusBadge } from './DashboardPage'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' }).format(n)

export default function FailedOrdersPage() {
  const { data: orders, loading, error } = useFetch(() => api.getOrdersByStatus('Failed'))

  if (loading) return (
    <div className="d-flex flex-column align-items-center gap-2 py-5 text-muted">
      <div className="spinner-border text-danger" role="status" />
      <span style={{ fontSize: '0.9rem' }}>Loading failed orders…</span>
    </div>
  )
  if (error) return <div className="alert alert-danger">{error}</div>

  return (
    <>
      <div className="page-header d-flex align-items-center gap-3">
        <div>
          <h1 className="mb-0 text-danger">&#9888; Failed Orders</h1>
          <p className="text-muted mb-0 mt-1" style={{ fontSize: '0.9rem' }}>
            Orders that could not be completed
          </p>
        </div>
        {orders && orders.length > 0 && (
          <span className="ms-auto badge bg-danger" style={{ fontSize: '1rem', padding: '0.5em 0.9em' }}>
            {orders.length} failed
          </span>
        )}
      </div>

      {!orders || orders.length === 0 ? (
        <div className="alert alert-success d-flex align-items-center gap-2">
          <span>&#9989;</span>
          <span><strong>All clear.</strong> No failed orders at the moment.</span>
        </div>
      ) : (
        <table className="table table-hover">
          <thead>
            <tr>
              <th>Order #</th>
              <th>Reference</th>
              <th>Customer</th>
              <th>Total</th>
              <th>Status</th>
              <th>Date</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {orders.map((o) => (
              <tr key={o.orderId}>
                <td className="fw-bold">#{o.orderId}</td>
                <td><code style={{ fontSize: '0.78rem' }}>{o.orderReference}</code></td>
                <td>{o.customerName}</td>
                <td className="fw-bold">{fmt(o.totalAmount)}</td>
                <td><StatusBadge status={o.statusDisplay} /></td>
                <td className="text-muted">{new Date(o.createdAt).toLocaleDateString('en-IE')}</td>
                <td>
                  <Link to={`/orders/${o.orderId}`} className="btn btn-sm btn-outline-danger">
                    View
                  </Link>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  )
}
