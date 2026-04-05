import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'
import { StatusBadge } from './DashboardPage'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)

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

  if (loading) return <div className="spinner-border text-primary mt-4" role="status" />
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!orders) return null

  const visible = orders.filter((o) =>
    (filter === 'All' || o.statusDisplay === filter) &&
    (search === '' || o.orderReference.toLowerCase().includes(search.toLowerCase()) ||
      o.customerName.toLowerCase().includes(search.toLowerCase()))
  )

  return (
    <>
      <h1 className="mb-4">Orders</h1>
      <div className="row mb-3 g-2">
        <div className="col-md-4">
          <input
            className="form-control"
            placeholder="Search reference or customer…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
        <div className="col-md-4">
          <select className="form-select" value={filter} onChange={(e) => setFilter(e.target.value)}>
            {ALL_STATUSES.map((s) => <option key={s}>{s}</option>)}
          </select>
        </div>
        <div className="col-md-4 text-end text-muted small pt-2">
          {visible.length} / {orders.length} orders
        </div>
      </div>

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
              <td>#{o.orderId}</td>
              <td>{o.orderReference}</td>
              <td>{o.customerName}</td>
              <td>{fmt(o.totalAmount)}</td>
              <td><StatusBadge status={o.statusDisplay} /></td>
              <td>{new Date(o.createdAt).toLocaleDateString()}</td>
              <td><Link to={`/orders/${o.orderId}`} className="btn btn-sm btn-outline-primary">View</Link></td>
            </tr>
          ))}
        </tbody>
      </table>

      {visible.length === 0 && <p className="text-muted">No orders match.</p>}
    </>
  )
}
