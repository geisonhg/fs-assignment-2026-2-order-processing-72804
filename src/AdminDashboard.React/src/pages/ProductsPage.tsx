import { useState } from 'react'
import { useFetch } from '../hooks/useFetch'
import { api } from '../api/client'

const fmt = (n: number) =>
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(n)

export default function ProductsPage() {
  const { data: products, loading, error } = useFetch(api.getProducts)
  const [search, setSearch] = useState('')

  if (loading) return <div className="spinner-border text-primary mt-4" role="status" />
  if (error) return <div className="alert alert-danger">{error}</div>
  if (!products) return null

  const visible = products.filter(
    (p) => search === '' || p.name.toLowerCase().includes(search.toLowerCase())
  )

  return (
    <>
      <h1 className="mb-4">Products</h1>
      <div className="row mb-3">
        <div className="col-md-4">
          <input
            className="form-control"
            placeholder="Search products…"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
          />
        </div>
      </div>

      <table className="table table-hover">
        <thead>
          <tr>
            <th>ID</th>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
            <th>Stock</th>
          </tr>
        </thead>
        <tbody>
          {visible.map((p) => (
            <tr key={p.productId}>
              <td>{p.productId}</td>
              <td>
                <div>{p.name}</div>
                <small className="text-muted">{p.description}</small>
              </td>
              <td>{p.category}</td>
              <td>{fmt(p.price)}</td>
              <td>
                <span className={`badge ${p.stockQuantity > 10 ? 'bg-success' : p.stockQuantity > 0 ? 'bg-warning text-dark' : 'bg-danger'}`}>
                  {p.stockQuantity > 0 ? p.stockQuantity : 'Out of stock'}
                </span>
              </td>
            </tr>
          ))}
        </tbody>
      </table>

      {visible.length === 0 && <p className="text-muted">No products match.</p>}
    </>
  )
}
