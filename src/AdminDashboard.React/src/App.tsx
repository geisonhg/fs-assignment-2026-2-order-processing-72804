import { Routes, Route, NavLink } from 'react-router-dom'
import DashboardPage from './pages/DashboardPage'
import OrdersPage from './pages/OrdersPage'
import OrderDetailPage from './pages/OrderDetailPage'
import ProductsPage from './pages/ProductsPage'

export default function App() {
  return (
    <>
      <nav className="navbar navbar-dark bg-dark px-3 mb-4">
        <span className="navbar-brand fw-bold">&#9971; SportsStore Admin</span>
        <div className="d-flex gap-3">
          <NavLink className={({ isActive }) => 'nav-link text-white' + (isActive ? ' fw-bold' : '')} to="/">Dashboard</NavLink>
          <NavLink className={({ isActive }) => 'nav-link text-white' + (isActive ? ' fw-bold' : '')} to="/orders">Orders</NavLink>
          <NavLink className={({ isActive }) => 'nav-link text-white' + (isActive ? ' fw-bold' : '')} to="/products">Products</NavLink>
        </div>
      </nav>
      <main className="container">
        <Routes>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/orders" element={<OrdersPage />} />
          <Route path="/orders/:id" element={<OrderDetailPage />} />
          <Route path="/products" element={<ProductsPage />} />
        </Routes>
      </main>
    </>
  )
}
