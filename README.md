# ABC Pharmacy - Medicine Tracker

A Single Page Application to track medicines and sales, built for the
Publicis Sapient coding assessment.

## Stack

- **Backend:** ASP.NET Core 8 Web API (C#)
- **Frontend:** Vanilla JavaScript SPA (served from `wwwroot`, same origin as the API)
- **Storage:** JSON files on disk (`MedicineTracker.Api/Data/medicines.json`, `sales.json`) — no database, as required.

## Features

- View all medicines in a grid (all attributes except Notes).
- **Red** row highlight: medicine expires in fewer than 30 days.
- **Yellow** row highlight: quantity in stock is fewer than 10 units.
- Add new medicine via a modal form.
- Search medicines by name (debounced, live).
- Record a sale for a medicine: validates stock, decrements quantity, and
  logs a sale record (medicine, quantity, unit price, total, timestamp)
  visible in the "Recent Sales" table below the grid.
- Delete a medicine.

## Project layout

```
MedicineTracker/
└── MedicineTracker.Api/
    ├── Controllers/
    │   ├── MedicinesController.cs   # GET/POST/PUT/DELETE api/medicines
    │   └── SalesController.cs       # GET/POST api/sales
    ├── Models/
    │   ├── Medicine.cs
    │   └── SaleRecord.cs
    ├── Services/
    │   ├── MedicineService.cs       # JSON-file repository (thread-safe)
    │   └── SaleService.cs           # records sales, decrements stock
    ├── Data/
    │   ├── medicines.json           # seed data (5 sample medicines)
    │   └── sales.json               # starts empty
    ├── wwwroot/                     # the SPA
    │   ├── index.html
    │   ├── css/styles.css
    │   └── js/app.js
    └── Program.cs
```

## How to run

Requires the [.NET 8 SDK](https://dotnet.microsoft.com/download).

```bash
cd MedicineTracker/MedicineTracker.Api
dotnet restore
dotnet run
```

The console will print the listening URL (e.g. `http://localhost:5252`).
Open it in a browser — the SPA is served directly from the API, so there's
nothing else to start.

Swagger UI (for exploring the API directly) is available at `/swagger` in
the Development environment.

## API summary

| Method | Route                         | Description                          |
|--------|--------------------------------|---------------------------------------|
| GET    | `/api/medicines?search=xyz`   | List medicines, optional name search |
| GET    | `/api/medicines/{id}`         | Get one medicine                     |
| POST   | `/api/medicines`              | Add a medicine                       |
| PUT    | `/api/medicines/{id}`         | Update a medicine                    |
| DELETE | `/api/medicines/{id}`         | Delete a medicine                    |
| GET    | `/api/sales`                  | List all sale records                |
| POST   | `/api/sales`                  | Record a sale `{ medicineId, quantity }` |

## Design notes

- Storage is a simple JSON-file repository behind an interface
  (`IMedicineService` / `ISaleService`), registered as singletons so an
  in-process lock protects concurrent writes to the same file. This keeps
  the solution dependency-free while still being swappable for a real
  database later (just implement the interface against EF Core, for
  example).
- Selling a medicine goes through `SaleService.RecordSale`, which
  decrements stock via `MedicineService.DecrementStock` (guards against
  over-selling) and only then appends the sale record — so the two files
  stay consistent even if a sale is rejected midway.
- The color-coding logic mirrors on both server data (dates/quantities are
  the source of truth) and client (`app.js` computes `row-expiring` /
  `row-low-stock` classes from the returned data), keeping the API
  response format simple (no server-computed "isExpiring" flags to keep in
  sync).
