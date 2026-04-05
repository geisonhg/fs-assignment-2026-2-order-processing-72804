import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)

export default function DashboardPage() {
  const { data: orders, loading, error } = useFetch(api.getOrders)

  if (loading) return <div className="spinner-border text-primary mt-4" role="status" />
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!orders) return null

  const total = orders.length
  const revenue = orders.reduce((s, o) => s + o.totalAmount, 0)
  const pending = orders.filter((o) => !['Delivered', 'Failed', 'Cancelled'].includes(o.statusDisplay)).length
  const failed = orders.filter((o) => o.statusDisplay === 'Failed').length

  return (
    <>
      <h1 className="mb-4">Dashboard</h1>
      <div className="row g-3 mb-4">
        <StatCard label="Total Orders" value={String(total)} color="primary" />
        <StatCard label="Total Revenue" value={fmt(revenue)} color="success" />
        <StatCard label="In Progress" value={String(pending)} color="warning" />
        <StatCard label="Failed" value={String(failed)} color="danger" />
      </div>

      <h5>Recent Orders</h5>
      <table className="table table-hover table-sm">
        <thead>
          <tr>
            <th>Order #</th>
            <th>Reference</th>
            <th>Customer</th>
            <th>Total</th>
            <th>Status</th>
            <th>Date</th>
          </tr>
        </thead>
        <tbody>
          {orders.slice(0, 10).map((o) => (
            <tr key={o.orderId}>
              <td>#{o.orderId}</td>
              <td><a href={`/orders/${o.orderId}`}>{o.orderReference}</a></td>
              <td>{o.customerName}</td>
              <td>{fmt(o.totalAmount)}</td>
              <td><StatusBadge status={o.statusDisplay} /></td>
              <td>{new Date(o.createdAt).toLocaleDateString()}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  )
}

function StatCard({ label, value, color }: { label: string; value: string; color: string }) {
  return (
    <div className="col-sm-6 col-md-3">
      <div className={`card text-bg-${color} shadow-sm`}>
        <div className="card-body">
          <div className="fs-2 fw-bold">{value}</div>
          <div className="small">{label}</div>
        </div>
      </div>
    </div>
  )
}

export function StatusBadge({ status }: { status: string }) {
  const color =
    ['Delivered', 'Confirmed', 'Shipped'].includes(status) ? 'success' :
    ['Failed', 'Cancelled'].includes(status) ? 'danger' :
    ['Pending', 'Submitted'].includes(status) ? 'secondary' :
    'warning'
  return <span className={`badge text-bg-${color}`}>{status}</span>
}
