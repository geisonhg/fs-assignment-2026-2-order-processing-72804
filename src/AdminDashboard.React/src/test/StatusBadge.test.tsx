import { render, screen } from '@testing-library/react'
import { StatusBadge } from '../pages/DashboardPage'

describe('StatusBadge', () => {
  it('shows success badge for Delivered', () => {
    render(<StatusBadge status="Delivered" />)
    expect(screen.getByText('Delivered')).toHaveClass('text-bg-success')
  })

  it('shows danger badge for Failed', () => {
    render(<StatusBadge status="Failed" />)
    expect(screen.getByText('Failed')).toHaveClass('text-bg-danger')
  })

  it('shows warning badge for in-progress status', () => {
    render(<StatusBadge status="PaymentProcessing" />)
    expect(screen.getByText('PaymentProcessing')).toHaveClass('text-bg-warning')
  })

  it('shows secondary badge for Pending', () => {
    render(<StatusBadge status="Pending" />)
    expect(screen.getByText('Pending')).toHaveClass('text-bg-secondary')
  })
})
