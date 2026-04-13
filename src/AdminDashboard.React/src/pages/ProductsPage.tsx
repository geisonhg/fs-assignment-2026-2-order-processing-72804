import { useState } from 'react'
import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-IE', { style: 'currency', currency: 'EUR' }).format(n)

export default function ProductsPage() {
  const { data: products, loading, error } = useFetch(api.getProducts)
  const [search, setSearch] = useState('')

  if (loading) return (
    <div className="d-flex flex-column align-items-center gap-2 py-5 text-muted">
      <div className="spinner-border text-primary" role="status" />
      <span style={{ fontSize: '0.9rem' }}>Loading products…</span>
    </div>
  )
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!products) return null

  const visible = products.filter(
    (p) => search === '' || p.name.toLowerCase().includes(search.toLowerCase())
      || p.category.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <>
      <div className="page-header">
        <h1 className="mb-0">Products</h1>
        <p className="text-muted mb-0 mt-1" style={{ fontSize: '0.9rem' }}>
          {visible.length} of {products.length} products
        </p>
      </div>

      <div className="filter-bar">
        <input
          className="form-control"
          style={{ maxWidth: 300 }}
          placeholder="&#128269; Search by name or category…"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
      </div>

      {visible.length === 0 ? (
        <p className="text-muted">No products match.</p>
      ) : (
        <table className="table table-hover">
          <thead>
            <tr>
              <th>#</th>
              <th>Product</th>
              <th>Category</th>
              <th>Price</th>
              <th>Stock</th>
            </tr>
          </thead>
          <tbody>
            {visible.map((p) => (
              <tr key={p.productId}>
                <td className="text-muted">{p.productId}</td>
                <td>
                  <div className="fw-bold">{p.name}</div>
                  <small className="text-muted">{p.description}</small>
                </td>
                <td>
                  <span className="badge rounded-pill" style={{ background: 'var(--brand-light)', color: 'var(--brand-primary)', fontSize: '0.72rem' }}>
                    {p.category}
                  </span>
                </td>
                <td className="fw-bold">{fmt(p.price)}</td>
                <td>
                  {p.stockQuantity === 0 ? (
                    <span className="badge bg-danger">Out of stock</span>
                  ) : p.stockQuantity <= 10 ? (
                    <span className="badge bg-warning text-dark">Low — {p.stockQuantity}</span>
                  ) : (
                    <span className="badge bg-success">{p.stockQuantity} in stock</span>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </>
  )
}
