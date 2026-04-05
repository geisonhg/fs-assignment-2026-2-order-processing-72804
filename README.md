# SportsStore Order Processing Platform

**Full Stack Development — Semester 2, Assignment 2**

A distributed, event-driven order processing system built on .NET 10 and React 18, demonstrating CQRS, async messaging with RabbitMQ, and containerised deployment with Docker Compose.

---

## Architecture Overview

```
┌─────────────────────┐     ┌─────────────────────┐
│  CustomerPortal     │     │  AdminDashboard      │
│  (Blazor Server)    │     │  (React + Vite)      │
│  :5001              │     │  :5200               │
└────────┬────────────┘     └──────────┬───────────┘
         │ HTTP                        │ HTTP
         ▼                             ▼
┌─────────────────────────────────────────────────┐
│              OrderManagement.API                │
│  ASP.NET Core 10 · CQRS/MediatR · AutoMapper   │
│  :5100                                          │
└──────┬──────────────────────────┬───────────────┘
       │ Publish                  │ Consume results
       ▼                          │
┌─────────────────┐               │
│   RabbitMQ      │◄──────────────┘
│   :5672/:15672  │
└──┬──────┬───────┘
   │      │
   ▼      ▼
┌──────┐ ┌──────────┐ ┌──────────┐
│Inven-│ │ Payment  │ │Shipping  │
│tory  │ │ Service  │ │ Service  │
│Svc   │ │          │ │          │
└──────┘ └──────────┘ └──────────┘
   │                        │
   └──────────┬─────────────┘
              ▼
      ┌───────────────┐
      │  SQL Server   │
      │  (shared DB)  │
      └───────────────┘
```

### Order Status Flow

```
Cart → Submitted → InventoryPending → InventoryConfirmed
                                    ↘ InventoryFailed → Failed
                   InventoryConfirmed → PaymentPending → PaymentApproved
                                                      ↘ PaymentFailed → Failed
                   PaymentApproved → ShippingPending → ShippingCreated → Completed
                                                                       ↘ Failed
```

---

## Projects

| Project | Type | Port | Description |
|---|---|---|---|
| `Shared.Contracts` | Class Library | — | DTOs, enums, message types |
| `OrderManagement.API` | ASP.NET Core API | 5100 | CQRS orchestrator, state manager |
| `Inventory.Service` | Worker Service | — | Checks stock availability |
| `Payment.Service` | Worker Service | — | Processes payments (simulated) |
| `Shipping.Service` | Worker Service | — | Creates shipment records |
| `CustomerPortal.Blazor` | Blazor Server | 5001 | Customer shopping + order tracking |
| `AdminDashboard.React` | React + Vite | 5200 | Admin order management |

---

## Getting Started

### Prerequisites
- Docker & Docker Compose
- .NET 10 SDK (for local development only)
- Node.js 22 (for React development only)

### Run with Docker Compose

```bash
git clone https://github.com/geisonhg/fs-assignment-2026-2-order-processing-72804.git
cd SportsStore.OrderPlatform
docker compose up --build
```

| Service | URL |
|---|---|
| Order API (Swagger) | http://localhost:5100/swagger |
| Customer Portal | http://localhost:5001 |
| Admin Dashboard | http://localhost:5200 |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

### Local Development

```bash
# Terminal 1 — infrastructure
docker compose up rabbitmq sqlserver

# Terminal 2 — API
cd src/OrderManagement.API
dotnet run

# Terminal 3 — Customer Portal
cd src/CustomerPortal.Blazor
dotnet run

# Terminal 4 — Admin Dashboard
cd src/AdminDashboard.React
npm install && npm run dev

# Terminal 5 — Workers (each in its own terminal)
cd src/Inventory.Service && dotnet run
cd src/Payment.Service && dotnet run
cd src/Shipping.Service && dotnet run
```

---

## Key Design Decisions

### CQRS with MediatR
Commands mutate state; queries read it. The API is the single source of truth for order status — workers are stateless processors that consume and publish messages without touching order state directly.

### RabbitMQ Queues

| Queue | Direction |
|---|---|
| `order-submitted` | API → Inventory |
| `inventory-result` | Inventory → API |
| `payment-requested` | API → Payment |
| `payment-result` | Payment → API |
| `shipping-requested` | API → Shipping |
| `shipping-result` | Shipping → API |

### Structured Logging
Serilog enriches every log entry with `OrderId`, `CustomerId`, and `CorrelationId` using `LogContext.PushProperty`, enabling end-to-end request tracing across services.

---

## Tests

### .NET (xUnit + Moq + FluentAssertions)
```bash
dotnet test tests/OrderManagement.Tests/
```

Covers: checkout command, inventory result handling, cancel order, get-by-id query, payment processor.

### React (Vitest + Testing Library)
```bash
cd src/AdminDashboard.React && npm test
```

Covers: StatusBadge component rendering across all order statuses.

---

## CI/CD

GitHub Actions (`.github/workflows/ci.yml`) runs on every push to `main`:

1. **.NET Build & Test** — restore, build, run xUnit tests
2. **React Build & Test** — npm ci, build, vitest
3. **Docker Build Smoke Test** — builds all 6 images to verify Dockerfiles compile
