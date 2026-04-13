import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'
import { StatusBadge } from './DashboardPage'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' }).format(n)

const ALL_STATUSES = [
  'All', 'Cart', 'Submitted',
  'InventoryPending', 'InventoryConfirmed', 'InventoryFailed',
  'PaymentPending', 'PaymentApproved', 'PaymentFailed',
  'ShippingPending', 'ShippingCreated',
  'Completed', 'Failed', 'Cancelled',
]

export default function OrdersPage() {
  const { data: orders, loading, error } = useFetch(api.getOrders)
  const [filter, setFilter] = useState('All')
  const [search, setSearch] = useState('')

  if (loading) return (
    <div className="d-flex flex-column align-items-center gap-2 py-5 text-muted">
      <div className="spinner-border text-primary" role="status" />
      <span style={{ fontSize: '0.9rem' }}>Loading orders…</span>
    </div>
  )
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!orders) return null

  const visible = orders.filter((o) =>
    (filter === 'All' || o.statusDisplay === filter) &&
    (search === '' || o.orderReference.toLowerCase().includes(search.toLowerCase()) ||
      o.customerName.toLowerCase().includes(search.toLowerCase()))
  )

  return (
    <>
      <div className="page-header">
        <h1 className="mb-0">Orders</h1>
        <p className="text-muted mb-0 mt-1" style={{ fontSize: '0.9rem' }}>
          Showing <strong>{visible.length}</strong> of <strong>{orders.length}</strong> orders
        </p>
      </div>

      <div className="filter-bar d-flex flex-wrap gap-2 align-items-center">
        <input
          className="form-control"
          style={{ maxWidth: 260 }}
          placeholder="&#128269; Search reference or customer…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select
          className="form-select"
          style={{ maxWidth: 220 }}
          value={filter}
          onChange={(e) => setFilter(e.target.value)}
        >
          {ALL_STATUSES.map((s) => <option key={s}>{s}</option>)}
        </select>
        {(search || filter !== 'All') && (
          <button
            className="btn btn-outline-secondary btn-sm"
            onClick={() => { setSearch(''); setFilter('All') }}
          >Clear filters</button>
        )}
      </div>

      {visible.length === 0 ? (
        <div className="text-center py-5 text-muted">
          <div style={{ fontSize: '2.5rem', marginBottom: '0.5rem' }}>&#128230;</div>
          <p>No orders match your filters.</p>
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
            {visible.map((o) => (
              <tr key={o.orderId}>
                <td className="fw-bold">#{o.orderId}</td>
                <td><code style={{ fontSize: '0.78rem' }}>{o.orderReference}</code></td>
                <td>{o.customerName}</td>
                <td className="fw-bold">{fmt(o.totalAmount)}</td>
                <td><StatusBadge status={o.statusDisplay} /></td>
                <td className="text-muted">{new Date(o.createdAt).toLocaleDateString('en-IE')}</td>
                <td>
                  <Link to={`/orders/${o.orderId}`} className="btn btn-sm btn-outline-primary">
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
