import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)

export default function DashboardPage() {
  const { data: summary, loading, error } = useFetch(api.getDashboardSummary)

  if (loading) return <div className="spinner-border text-primary mt-4" role="status" />
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!summary) return null

  return (
    <>
      <h1 className="mb-4">Dashboard</h1>
      <div className="row g-3 mb-4">
        <StatCard label="Total Orders" value={String(summary.totalOrders)} color="primary" />
        <StatCard label="Total Revenue" value={fmt(summary.totalRevenue)} color="success" />
        <StatCard label="In Progress" value={String(summary.pendingOrders)} color="warning" />
        <StatCard label="Failed" value={String(summary.failedOrders)} color="danger" />
        <StatCard label="Completed" value={String(summary.completedOrders)} color="info" />
        <StatCard label="Cancelled" value={String(summary.cancelledOrders)} color="secondary" />
      </div>

      <div className="row g-3">
        <div className="col-md-6">
          <h5>Orders by Status</h5>
          <table className="table table-sm table-bordered">
            <thead className="table-dark">
              <tr><th>Status</th><th>Count</th></tr>
            </thead>
            <tbody>
              {summary.byStatus.map((s) => (
                <tr key={s.status}>
                  <td><StatusBadge status={s.status} /></td>
                  <td>{s.count}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="col-md-6">
          <h5>Revenue — Last 7 Days</h5>
          <table className="table table-sm table-bordered">
            <thead className="table-dark">
              <tr><th>Date</th><th>Orders</th><th>Revenue</th></tr>
            </thead>
            <tbody>
              {summary.revenueLastSevenDays.map((d) => (
                <tr key={d.date}>
                  <td>{d.date}</td>
                  <td>{d.orderCount}</td>
                  <td>{fmt(d.revenue)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </>
  )
}

function StatCard({ label, value, color }: { label: string; value: string; color: string }) {
  return (
    <div className="col-sm-6 col-md-2">
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
    ['Completed', 'InventoryConfirmed'].includes(status) ? 'success' :
    ['Failed', 'Cancelled'].includes(status) ? 'danger' :
    ['Cart', 'Submitted'].includes(status) ? 'secondary' :
    'warning'
  return <span className={`badge text-bg-${color}`}>{status}</span>
}
