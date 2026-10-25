# Package Delivery System

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-latest-239120?logo=csharp&logoColor=white)
![EF Core](https://img.shields.io/badge/EF%20Core-10-512BD4?logo=.net&logoColor=white)
![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?logo=mysql&logoColor=white)
![Blazor](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white)
![Hangfire](https://img.shields.io/badge/Hangfire-1.8-00B2A9)
![Status](https://img.shields.io/badge/status-university%20project-yellow)

A parcel-locker delivery system: senders drop packages into a locker box, couriers move them between lockers, recipients collect them. Built with a layered .NET architecture, Entity Framework Core on MySQL, and a Blazor Server interface.

## Architecture

| Project | Responsibility |
|---|---|
| `PackageDeliverySystem.Core` | Domain entities, status transitions and service interfaces. No external dependencies. |
| `PackageDeliverySystem.Infrastructure` | EF Core persistence (MySQL), e-mail notifications, migrations. |
| `PackageDeliverySystem.Cli` | Console runner that seeds demo data and drives the domain against a real database. |
| `PackageDeliverySystem.Web` | Blazor Server application: role-based demo screens, Hangfire dashboard. |

`Core` depends on nothing else, so the domain logic (`Package`, `Box`, `ParcelLocker`, status transitions) can be tested and reasoned about in isolation from persistence and UI concerns.

## Getting Started

### Prerequisites
- .NET 10 SDK
- Docker (for MySQL and Mailpit)

### Setup
```bash
git clone git@github.com:Krzysiek-Mistrz/package_managment_system.git
cd package_managment_system

docker compose up -d

# bash/zsh
export PDS_CONNECTION_STRING="Server=localhost;Port=3306;Database=package_delivery_system;User=package_delivery_system_user;Password=package_delivery_system_password;"
# fish
set -Ux PDS_CONNECTION_STRING "Server=localhost;Port=3306;Database=package_delivery_system;User=package_delivery_system_user;Password=package_delivery_system_password;"

dotnet tool restore
dotnet run --project src/PackageDeliverySystem.Cli    # applies migrations, seeds demo data, runs a sample scenario
dotnet watch --project src/PackageDeliverySystem.Web   # demo UI + Hangfire dashboard at /hangfire
```

### Tests
```bash
dotnet test
```

## Domain Model

The core entity is `Package`, which moves through a fixed status chain enforced by its own methods (`Accept`, `Prepare`, `Ship`, `Deliver`, `Collect`, `Expire`, `Reject`, `Cancel`) rather than by external code setting a property:

```
Ordered -> Accepted -> Preparing -> Shipped -> Delivered -> Collected
```

A `Package` references an origin `Box` (where the sender drops it) and a destination `Box` (where the recipient collects it); each `Box` belongs to a `ParcelLocker`. `Users` holds every person in the system (`Customer`, `Courier`, `Operator`) in a single table, distinguished by a discriminator column, since any of them can be linked to any number of packages.

## Payments

A `Package` must be paid for and physically deposited before an operator can accept it; `Package.Accept()` enforces both as preconditions. Neither requires the sender to have an account: payment only needs an e-mail address, and the deposit is confirmed by a courier rather than self-reported by the sender.

- **Payment** is a separate `Payment` entity (its own `Pending -> Completed/Failed` lifecycle, not just a flag on `Package`), linked 1-to-1 to its package. The real payment provider is never called directly: `IPaymentGateway` is the extension point an implementer swaps in for Stripe, Przelewy24, PayU or any other gateway; `FakePaymentGateway` is a stand-in that always succeeds, used for local demos and tests.
- **Deposit confirmation** is deliberately not something the sender can trigger: `IPackageIntakeService.ConfirmDepositAsync` requires a real courier to be looked up first, matching the real-world flow of a courier opening the box at pickup time. If the courier finds the box empty, `ReportMissingDepositAsync` cancels the order on the spot, fault attributed to the sender. If no courier visits in time at all, a Hangfire recurring job (`release-missed-deposits`) cancels the order automatically once its 15-minute deposit window closes - the same mechanism `expire-packages` uses for collection timeouts.
- **Idempotency**: a payment webhook firing twice is treated as success rather than an error, and the database itself enforces at most one `Payment` row per `Package` via a unique foreign key, so a duplicate payment request can't silently create two.
- **Known gap**: refunding a completed payment when a package is later rejected or cancelled is not implemented - it depends entirely on the real gateway's own refund API, which this project does not have access to. So the developer implementing this framework should take care of that :)

## Prototypes

Three areas were prototyped ahead of the full implementation to validate technical risk before committing to it.

### Background jobs (Hangfire)
Expiring packages that have not been collected within 48 hours cannot depend on someone remembering to check. `PackageDeliverySystem.Web` registers a Hangfire recurring job (`expire-packages`) that calls `IPackageAcceptanceService.ProcessExpiredPackagesAsync()` on a schedule, independent of any HTTP request. The Hangfire dashboard at `/hangfire` shows the job's schedule, run history and lets it be triggered manually.

### Asynchronous notifications (MailKit + SMTP)
Accepting or expiring a package triggers a notification. Rather than blocking the caller on an SMTP round-trip, `BackgroundNotificationService` enqueues the actual send as a separate Hangfire job, which `EmailNotificationService` executes against an SMTP relay using MailKit. In development, [Mailpit](https://github.com/axllent/mailpit) stands in for a real mail server: it accepts SMTP on `localhost:1025` and exposes every received message at `http://localhost:8025`, so the full pipeline (job queue -> job server -> SMTP -> inbox) is observable without sending real e-mail.

### Optimistic concurrency (box assignment)
Two operators must never be able to assign the same locker box to two different packages at the same time. `Box.Version` is configured as an EF Core concurrency token: every update to a `Box` is conditioned on its version matching the value last read, so a losing concurrent write fails with `DbUpdateConcurrencyException` instead of silently overwriting the winner. This is exercised directly against MySQL by `TwoOperators_RacingForTheSameBox_OnlyOneSucceeds` in the test suite, which races two independent `DbContext` instances against the same box and asserts that exactly one of them succeeds.

## Demo UI

`PackageDeliverySystem.Web` also ships a set of role-based screens (Customer, Operator, Courier, Parcel Locker) backed by an in-memory mock data service rather than the real database. It exists to demonstrate the intended user flows and screen layouts for the course deliverable, not as a production front end; the Cli project is the reference path that exercises the real, MySQL-backed domain logic end to end.

## Project Structure

```
src/
  PackageDeliverySystem.Core/            domain entities, enums, service interfaces
  PackageDeliverySystem.Infrastructure/  EF Core DbContext, migrations, repositories, notifications
  PackageDeliverySystem.Cli/             console demo runner
  PackageDeliverySystem.Web/             Blazor Server demo UI + Hangfire
tests/
  PackageDeliverySystem.Tests/           unit tests and the concurrency integration test
docker-compose.yaml                      MySQL + Mailpit for local development
```
