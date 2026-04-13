import { Routes, Route, NavLink } from 'react-router-dom'
import DashboardPage from './pages/DashboardPage'
import OrdersPage from './pages/OrdersPage'
import OrderDetailPage from './pages/OrderDetailPage'
import ProductsPage from './pages/ProductsPage'
import FailedOrdersPage from './pages/FailedOrdersPage'

export default function App() {
  return (
    <>
      <nav className="navbar ss-navbar px-3 mb-0">
        <span className="navbar-brand">&#9917; SportsStore Admin</span>
        <div className="d-flex gap-1 align-items-center">
          <NavLink
            className={({ isActive }) => 'nav-link' + (isActive ? ' active' : '')}
            to="/"
            end
          >Dashboard</NavLink>
          <NavLink
            className={({ isActive }) => 'nav-link' + (isActive ? ' active' : '')}
            to="/orders"
          >Orders</NavLink>
          <NavLink
            className={({ isActive }) => 'nav-link' + (isActive ? ' active' : '')}
            to="/products"
          >Products</NavLink>
          <NavLink
            className={({ isActive }) => 'nav-link nav-failed' + (isActive ? ' active' : '')}
            to="/failed"
          >&#9888; Failed Orders</NavLink>
        </div>
      </nav>
      <main className="container">
        <Routes>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/orders" element={<OrdersPage />} />
          <Route path="/orders/:id" element={<OrderDetailPage />} />
          <Route path="/products" element={<ProductsPage />} />
          <Route path="/failed" element={<FailedOrdersPage />} />
        </Routes>
      </main>
    </>
  )
}
