# 🏨 Hotel Reservation App
A modern and secure hotel reservation system built with ASP.NET Core MVC, featuring comprehensive role-based authorization.

## 📸 Screenshots
[Hotel-Reservation-App.pdf](https://github.com/user-attachments/files/21640885/Hotel-Reservation-App.pdf)

### 👥 User Roles
- **Admin**: System administrator - can manage all hotels, rooms, and reservations
- **Hotel Manager**: Hotel manager - can only manage their own hotels
- **Customer**: Guest - can view rooms, make reservations, and add reviews

### 🔐 Security Features
- Role-based Authorization
- Secure Authentication
- Password Hashing
- CSRF Protection

### 🏨 Hotel Management
- Add, edit, and delete hotels
- Manage hotel images
- City-based hotel filtering
- Hotel amenities management

### 🛏️ Room Management
- Room types and capacities
- Room images
- Price management
- Availability status

### 📅 Reservation System
- Online reservation booking
- Reservation history
- Reservation cancellation
- Reservation status tracking

### ⭐ Review System
- Customer reviews
- Rating system
- Review management

## 🛠️ Technologies

- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: Entity Framework Core
- **Frontend**: HTML5, CSS3, JavaScript, Bootstrap
- **Authentication**: ASP.NET Core Identity
- **Authorization**: Role-based Claims

## 📁 Project Structure

```
HotelReservationApp/
├── Controllers/           # MVC Controllers
│   ├── AccountController.cs
│   ├── AdminController.cs
│   ├── HotelController.cs
│   ├── HotelManagerController.cs
│   ├── ReservationController.cs
│   ├── RoomController.cs
│   └── ReviewController.cs
├── Models/               # Data Models
│   ├── User.cs
│   ├── Role.cs
│   ├── Hotel.cs
│   ├── Room.cs
│   ├── Reservation.cs
│   └── Review.cs
├── Views/                # Razor Views
├── Data/                 # Database Context
├── Migrations/           # EF Migrations
└── wwwroot/             # Static Files
```

## 🚀 Installation

### Prerequisites
- .NET 8.0 SDK
- SQL Server (LocalDB supported)
- VS Code

### Steps

1. **Clone the repository**
   ```bash
   git clone https://github.com/[username]/Hotel-Reservation-App.git
   cd Hotel-Reservation-App
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Create database**
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Open in browser**
   ```
   https://localhost:5001
   ```

## 🔧 Configuration

### Database Connection
Update the connection string in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HotelReservationDB;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### Default Users
The application creates these users with seed data on first run:

- **Admin**: admin@hotel.com / admin123
- **Hotel Manager**: manager1@hotel.com / manager123
- **Customer**: customer1@email.com / customer123

## 📱 Usage

### Admin Panel
- Manage all hotels
- Assign user roles
- System-wide reports
- Reservation management

### Hotel Manager Panel
- Manage own hotels
- Add/edit rooms
- Track reservations
- View customer reviews

### Customer Panel
- Search and filter hotels
- Make reservations
- View reservation history
- Add reviews

## 🔒 Security

### Role-based Authorization
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller

[Authorize(Roles = "Hotel Manager")]
public class HotelManagerController : Controller

[Authorize(Roles = "Customer")]
public async Task<IActionResult> MyReservations()
```

### Security Features
- Password hashing with BCrypt
- HTTPS enforcement
- Anti-forgery tokens
- Input validation
- SQL injection protection

## 🧪 Testing

### Role-based Authorization Testing
1. Login with different roles
2. Try accessing unauthorized URLs
3. Should receive 403 Forbidden errors

### Test URLs
- Admin: `/Admin/Hotels`
- Hotel Manager: `/HotelManager/MyHotels`
- Customer: `/Reservation/MyReservations`

## 🤝 Contributing

1. Fork the project
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📞 Contact

- **Developer**: Ceren Mutlu
- **Email**: cerenmutludevelopment@gmail.com
- **LinkedIn**: linkedin.com/in/cerennnmutlu/

---

⭐ If you like this project, don't forget to give it a star!


