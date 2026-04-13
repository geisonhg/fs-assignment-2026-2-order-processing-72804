import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' }).format(n)

export default function DashboardPage() {
  const { data: summary, loading, error } = useFetch(api.getDashboardSummary)

  if (loading) return (
    <div className="d-flex flex-column align-items-center gap-2 py-5 text-muted">
      <div className="spinner-border text-primary" role="status" />
      <span style={{ fontSize: '0.9rem' }}>Loading dashboard…</span>
    </div>
  )
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!summary) return null

  return (
    <>
      <div className="page-header">
        <h1 className="mb-0">Dashboard</h1>
        <p className="text-muted mb-0 mt-1" style={{ fontSize: '0.9rem' }}>Order platform overview</p>
      </div>

      <div className="row g-3 mb-4">
        <StatCard label="Total Orders"   value={String(summary.totalOrders)}    color="primary" />
        <StatCard label="Revenue"        value={fmt(summary.totalRevenue)}       color="success" />
        <StatCard label="In Progress"    value={String(summary.pendingOrders)}   color="warning" />
        <StatCard label="Failed"         value={String(summary.failedOrders)}    color="danger" />
        <StatCard label="Completed"      value={String(summary.completedOrders)} color="info" />
        <StatCard label="Cancelled"      value={String(summary.cancelledOrders)} color="secondary" />
      </div>

      <div className="row g-4">
        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">Orders by Status</div>
            <div className="card-body p-0">
              <table className="table mb-0">
                <thead>
                  <tr><th>Status</th><th>Count</th></tr>
                </thead>
                <tbody>
                  {summary.byStatus.map((s) => (
                    <tr key={s.status}>
                      <td><StatusBadge status={s.status} /></td>
                      <td className="fw-bold">{s.count}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div className="col-md-6">
          <div className="card h-100">
            <div className="card-header">Revenue — Last 7 Days</div>
            <div className="card-body p-0">
              <table className="table mb-0">
                <thead>
                  <tr><th>Date</th><th>Orders</th><th>Revenue</th></tr>
                </thead>
                <tbody>
                  {summary.revenueLastSevenDays.map((d) => (
                    <tr key={d.date}>
                      <td>{d.date}</td>
                      <td>{d.orderCount}</td>
                      <td className="fw-bold">{fmt(d.revenue)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </>
  )
}

function StatCard({ label, value, color }: { label: string; value: string; color: string }) {
  return (
    <div className="col-sm-6 col-md-2">
      <div className={`card stat-card text-bg-${color}`}>
        <div className="card-body">
          <div className="stat-value">{value}</div>
          <div className="stat-label">{label}</div>
        </div>
      </div>
    </div>
  )
}

export function StatusBadge({ status }: { status: string }) {
  const color =
    ['Completed', 'InventoryConfirmed'].includes(status) ? 'success' :
    ['Failed', 'Cancelled', 'InventoryFailed', 'PaymentFailed'].includes(status) ? 'danger' :
    ['Cart', 'Submitted'].includes(status) ? 'secondary' :
    ['PaymentApproved', 'ShippingCreated'].includes(status) ? 'info' :
    'warning'
  return <span className={`badge text-bg-${color}`}>{status}</span>
}
