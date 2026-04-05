import { render, screen } from '@testing-library/react'
import { StatusBadge } from '../pages/DashboardPage'

describe('StatusBadge', () => {
  it('shows success badge for Completed', () => {
    render(<StatusBadge status="Completed" />)
    expect(screen.getByText('Completed')).toHaveClass('text-bg-success')
  })

  it('shows success badge for InventoryConfirmed', () => {
    render(<StatusBadge status="InventoryConfirmed" />)
    expect(screen.getByText('InventoryConfirmed')).toHaveClass('text-bg-success')
  })

  it('shows danger badge for Failed', () => {
    render(<StatusBadge status="Failed" />)
    expect(screen.getByText('Failed')).toHaveClass('text-bg-danger')
  })

  it('shows danger badge for Cancelled', () => {
    render(<StatusBadge status="Cancelled" />)
    expect(screen.getByText('Cancelled')).toHaveClass('text-bg-danger')
  })

  it('shows warning badge for in-progress status', () => {
    render(<StatusBadge status="PaymentPending" />)
    expect(screen.getByText('PaymentPending')).toHaveClass('text-bg-warning')
  })

  it('shows warning badge for ShippingPending', () => {
    render(<StatusBadge status="ShippingPending" />)
    expect(screen.getByText('ShippingPending')).toHaveClass('text-bg-warning')
  })

  it('shows secondary badge for Submitted', () => {
    render(<StatusBadge status="Submitted" />)
    expect(screen.getByText('Submitted')).toHaveClass('text-bg-secondary')
  })
})
