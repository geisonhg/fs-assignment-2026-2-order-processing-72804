import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'
import { StatusBadge } from './DashboardPage'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)

export default function FailedOrdersPage() {
  const { data: orders, loading, error } = useFetch(() => api.getOrdersByStatus('Failed'))

  if (loading) return <div className="spinner-border text-danger mt-4" role="status" />
  if (error) return <div className="alert alert-danger">{error}</div>

  return (
    <>
      <h1 className="mb-4 text-danger">Failed Orders</h1>
      {!orders || orders.length === 0 ? (
        <div className="alert alert-success">No failed orders.</div>
      ) : (
        <table className="table table-hover table-sm">
          <thead className="table-dark">
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
                <td>#{o.orderId}</td>
                <td>{o.orderReference}</td>
                <td>{o.customerName}</td>
                <td>{fmt(o.totalAmount)}</td>
                <td><StatusBadge status={o.statusDisplay} /></td>
                <td>{new Date(o.createdAt).toLocaleDateString()}</td>
                <td><a href={`/orders/${o.orderId}`} className="btn btn-sm btn-outline-secondary">View</a></td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  )
}
