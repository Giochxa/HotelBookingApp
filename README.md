# Hotel Booking App

This is a web-based hotel booking application built using ASP.NET Core MVC, Entity Framework Core, and Identity for authentication and authorization. Users can browse hotels, view rooms, filter available options, and make bookings.

---

## Features

- 🏨 Hotel management (Admin/Manager only)
- 🛏️ Room management and availability
- 👤 User management
- 🔐 Role-based authentication (`Admin`, `Manager`, `User`)
- 📅 Booking system with date availability checks
- 🔎 Filter rooms and bookings by:
  - Hotel
  - Room Type
  - Guest Capacity
  - Check-In/Out dates
  - Price range
- 👤 View bookings (Users can see their own; Admin/Managers can view all)

---

## Technologies Used

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core
- Microsoft Identity (for user authentication & authorization)
- Bootstrap 5 for UI
- SQL Server (configured via `appsettings.json`)
- LINQ, Razor Views, ViewModels

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-repo/hotel-booking-app.git
cd hotel-booking-app
```


### 2. Build & Run the App

Open in **Visual Studio** and run the project or use CLI:
In appsettings.json, set your SQL Server connection string:


"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=HotelBookingDb;Trusted_Connection=True;MultipleActiveResultSets=true"
}

- Delete Migrations

```bash
dotnet build
dotnet ef database update
dotnet run
```

### 3. Seeded Users for Login

| Role     | Email                | Password     |
|----------|----------------------|--------------|
| Admin    | `admin@example.com`  | `Admin@123`  |
| Manager  | `manager@example.com`| `Manager@123`|
| User     | `user@example.com`   | `User@123`   |

> You can modify or extend roles in `DataSeeder.cs`.

---


## 🗂️ Project Structure

| Folder	  | Description                                                     |
|-------------|-----------------------------------------------------------------|
| Controllers | MVC logic (HotelController, RoomController, BookingsController) |
| Views 	  | Razor views for UI                                              |
| Models 	  | Entity Framework models                                         |
| DTOs 		  | Form submission data (e.g. `BookingDto`)                        |
| Services 	  | Business logic and data access                                  |
| ViewModels  | Data prepared for the view 										|
| wwwroot 	  | Static assets (images, CSS, etc.) 								|

## Pages Overview

- `/Hotels` – Browse and filter hotels by name, city, country, and rating
- `/Rooms` – Browse and filter rooms by hotel, price, type, guests, check-in/out
- `/Bookings` – Users see their own bookings; Admins/Managers can filter by user, room, hotel, or dates
- `/Users` – User management, visible for admins only
---

## Roles and Access

| Page            			| Admin | Manager | User |
|---------------------------|:-----:|:-------:|:----:|
| View Hotels    			| ✅    | ✅      | ✅   |
| Add/Edit/Delete Hotels 	| ✅    | ✅      | ❌   |
| View Rooms     			| ✅    | ✅      | ✅   |
| Add/Edit/Delete Rooms 	| ✅    | ✅      | ❌   |
| Book Room       			| ✅    | ✅      | ✅   |
| View All Bookings 		| ✅    | ✅      | ❌   |
| View Own Bookings			| ✅    | ✅      | ✅   |
| User management			| ✅    | ❌      | ❌   |

---

## Developer Notes

- TempData is used to show success messages after actions like booking, editing, or deleting.
- Toast notifications show feedback messages at the top right corner.
- Admins can manage all data including hotels, rooms, user bookings and user management.

---

## License

MIT License © 2025 HotelBookingApp
