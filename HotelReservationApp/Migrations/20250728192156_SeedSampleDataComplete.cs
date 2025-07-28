using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservationApp.Migrations
{
    /// <inheritdoc />
    public partial class SeedSampleDataComplete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleID", "RoleName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Hotel Manager" },
                    { 3, "Customer" },
                }
            );

            // Cities tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "CityID", "CityName" },
                values: new object[,]
                {
                    { 34, "İstanbul" },
                    { 6, "Ankara" },
                    { 35, "İzmir" },
                    { 7, "Antalya" },
                    { 16, "Bursa" },
                }
            );

            // Users tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[]
                {
                    "UserID",
                    "FullName",
                    "Email",
                    "PasswordHash",
                    "RoleID",
                    "Phone",
                    "DateOfBirth",
                    "Gender",
                    "CreatedDate",
                },
                values: new object[,]
                {
                    {
                        1,
                        "Admin User",
                        "admin@hotel.com",
                        "admin123",
                        1,
                        "555-0001",
                        new DateTime(1990, 1, 1),
                        "Erkek",
                        new DateTime(2024, 1, 1),
                    },
                    {
                        2,
                        "Otel Manager 1",
                        "manager1@hotel.com",
                        "manager123",
                        2,
                        "555-0002",
                        new DateTime(1985, 5, 15),
                        "Kadın",
                        new DateTime(2024, 1, 1),
                    },
                    {
                        3,
                        "Otel Manager 2",
                        "manager2@hotel.com",
                        "manager123",
                        2,
                        "555-0003",
                        new DateTime(1988, 8, 20),
                        "Erkek",
                        new DateTime(2024, 1, 1),
                    },
                    {
                        4,
                        "Customer 1",
                        "customer1@email.com",
                        "customer123",
                        3,
                        "555-0004",
                        new DateTime(1995, 3, 10),
                        "Kadın",
                        new DateTime(2024, 1, 1),
                    },
                    {
                        5,
                        "Customer 2",
                        "customer2@email.com",
                        "customer123",
                        3,
                        "555-0005",
                        new DateTime(1992, 7, 25),
                        "Erkek",
                        new DateTime(2024, 1, 1),
                    },
                }
            );

            // RoomTypes tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "RoomTypes",
                columns: new[] { "RoomTypeID", "TypeName" },
                values: new object[,]
                {
                    { 1, "Single Room" },
                    { 2, "Family Room" },
                    { 3, "Presidential Suite" },
                }
            );

            // Hotels tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "Hotels",
                columns: new[]
                {
                    "HotelID",
                    "Name",
                    "Description",
                    "CityID",
                    "OwnerID",
                    "CreatedDate",
                    "IsActive",
                    "Address",
                    "Phone",
                    "Email",
                    "Website",
                },
                values: new object[,]
                {
                    {
                        1,
                        "Grand Hotel İstanbul",
                        "In the heart of Istanbul",
                        34,
                        2,
                        new DateTime(2024, 1, 1),
                        true,
                        "Sultanahmet Meydanı No:1",
                        "0212-555-0001",
                        "info@grandhotel.com",
                        "www.grandhotel.com",
                    },
                    {
                        2,
                        "Ankara Plaza Hotel",
                        "In the center of Ankara",
                        6,
                        3,
                        new DateTime(2024, 1, 1),
                        true,
                        "Kızılay Meydanı No:5",
                        "0312-555-0002",
                        "info@ankaraplaza.com",
                        "www.ankaraplaza.com",
                    },
                    {
                        3,
                        "İzmir Beach Resort",
                        "With a magnificent view of the Izmir coast",
                        35,
                        2,
                        new DateTime(2024, 1, 1),
                        true,
                        "Alsancak Sahil No:10",
                        "0232-555-0003",
                        "info@izmirresort.com",
                        "www.izmirresort.com",
                    },
                }
            );

            // Rooms tablosuna veri ekle (RoomNumber dahil)
            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[]
                {
                    "RoomID",
                    "HotelID",
                    "RoomTypeID",
                    "RoomNumber",
                    "PricePerNight",
                    "Capacity",
                },
                values: new object[,]
                {
                    { 1, 1, 1, "101", 500.00m, 1 },
                    { 2, 1, 2, "201", 800.00m, 4 },
                    { 3, 1, 3, "301", 2000.00m, 2 },
                    { 4, 2, 1, "101", 400.00m, 1 },
                    { 5, 2, 2, "201", 600.00m, 4 },
                    { 6, 3, 1, "101", 600.00m, 1 },
                    { 7, 3, 2, "201", 900.00m, 4 },
                    { 8, 3, 3, "301", 2500.00m, 2 },
                }
            );

            // HotelAmenities tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "HotelAmenities",
                columns: new[] { "AmenityID", "AmenityName", "Icon" },
                values: new object[,]
                {
                    { 1, "Air Conditioner", "fa-wind" },
                    { 2, "Bathtub", "fa-bath" },
                    { 3, "Shower", "fa-shower" },
                    { 4, "Television", "fa-tv" },
                    { 5, "WiFi", "fa-wifi" },
                    { 6, "Telephone", "fa-phone" },
                    { 7, "Mini Bar", "fa-glass-martini" },
                    { 8, "Kitchen", "fa-utensils" },
                }
            );

            // HotelAmenityMapping tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "HotelAmenityMapping",
                columns: new[] { "HotelID", "AmenityID" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 8 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 2, 5 },
                    { 2, 6 },
                    { 3, 1 },
                    { 3, 2 },
                    { 3, 3 },
                    { 3, 4 },
                    { 3, 5 },
                    { 3, 6 },
                    { 3, 7 },
                }
            );

            // Reservations tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[]
                {
                    "ReservationID",
                    "UserID",
                    "RoomID",
                    "CheckInDate",
                    "CheckOutDate",
                    "Status",
                    "CreatedDate",
                    "UpdatedDate",
                    "TotalAmount",
                    "GuestCount",
                    "SpecialRequests",
                    "CancellationDate",
                },
                values: new object[,]
                {
                    {
                        1,
                        4,
                        1,
                        new DateTime(2024, 8, 1),
                        new DateTime(2024, 8, 3),
                        "Unconfirmed",
                        new DateTime(2024, 7, 15),
                        null,
                        1000.00m,
                        1,
                        "Erken check-in istiyorum",
                        null,
                    },
                    {
                        2,
                        5,
                        3,
                        new DateTime(2024, 8, 5),
                        new DateTime(2024, 8, 7),
                        "Confirmed",
                        new DateTime(2024, 7, 20),
                        null,
                        4000.00m,
                        2,
                        "Romantik sürpriz hazırlayın",
                        new DateTime(2024, 7, 21),
                    },
                    {
                        3,
                        4,
                        5,
                        new DateTime(2024, 8, 10),
                        new DateTime(2024, 8, 12),
                        "Confirmed",
                        new DateTime(2024, 7, 25),
                        new DateTime(2024, 7, 26),
                        1200.00m,
                        4,
                        "Çocuk yatağı ekleyin",
                        null,
                    },
                }
            );

            // Reviews tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "Reviews",
                columns: new[]
                {
                    "ReviewID",
                    "UserID",
                    "HotelID",
                    "Rating",
                    "Comment",
                    "ReviewDate",
                },
                values: new object[,]
                {
                    {
                        1,
                        4,
                        1,
                        5,
                        "Great hotel, very clean and the staff is very attentive!",
                        new DateTime(2024, 8, 4),
                    },
                    { 2, 5, 1, 4, "Nice hotel but a bit expensive", new DateTime(2024, 8, 8) },
                    {
                        3,
                        4,
                        2,
                        5,
                        "The best hotel I've stayed at in Ankara",
                        new DateTime(2024, 8, 13),
                    },
                }
            );

            // Payments tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "Payments",
                columns: new[]
                {
                    "PaymentID",
                    "ReservationID",
                    "Amount",
                    "PaymentDate",
                    "PaymentMethod",
                },
                values: new object[,]
                {
                    { 1, 1, 1000.00m, new DateTime(2024, 7, 15), "Credit Card" },
                    { 2, 2, 4000.00m, new DateTime(2024, 7, 20), "Cash" },
                    { 3, 3, 1200.00m, new DateTime(2024, 7, 25), "Bank Transfer" },
                }
            );

            // HotelImages tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "HotelImages",
                columns: new[] { "ImageID", "HotelID", "ImageUrl" },
                values: new object[,]
                {
                    { 1, 1, "/img/carousel-1.jpg" },
                    { 2, 2, "/img/carousel-2.jpg" },
                    { 3, 3, "/img/about-3.jpg" },
                }
            );

            // RoomImages tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "RoomImages",
                columns: new[] { "ImageID", "RoomID", "ImageUrl" },
                values: new object[,]
                {
                    { 1, 1, "/img/room-1.jpg" },
                    { 2, 2, "/img/room-2.jpg" },
                    { 3, 3, "/img/room-3.jpg" },
                    { 4, 4, "/img/roomImg1.jpg" },
                    { 5, 5, "/img/roomImg2.jpg" },
                    { 6, 6, "/img/roomImg3.jpg" },
                    { 7, 7, "/img/room-1.jpg" },
                    { 8, 8, "/img/room-2.jpg" },
                }
            );

            // RoomAvailability tablosuna veri ekle
            migrationBuilder.InsertData(
                table: "RoomAvailabilities",
                columns: new[] { "RoomID", "Date", "IsAvailable", "Price" },
                values: new object[,]
                {
                    { 1, new DateTime(2024, 8, 1), true, 500.00m },
                    { 1, new DateTime(2024, 8, 2), true, 500.00m },
                    { 1, new DateTime(2024, 8, 3), false, 500.00m },
                    { 2, new DateTime(2024, 8, 1), true, 800.00m },
                    { 2, new DateTime(2024, 8, 2), true, 800.00m },
                    { 2, new DateTime(2024, 8, 3), false, 800.00m },
                    { 3, new DateTime(2024, 8, 1), true, 2000.00m },
                    { 3, new DateTime(2024, 8, 2), true, 2000.00m },
                    { 3, new DateTime(2024, 8, 3), true, 2000.00m },
                }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Verileri sil (ters sırayla)
            migrationBuilder.DeleteData(
                table: "Payments",
                keyColumn: "PaymentID",
                keyValues: new object[] { 1, 2, 3 }
            );
            migrationBuilder.DeleteData(
                table: "Reviews",
                keyColumn: "ReviewID",
                keyValues: new object[] { 1, 2, 3 }
            );
            migrationBuilder.DeleteData(
                table: "Reservations",
                keyColumn: "ReservationID",
                keyValues: new object[] { 1, 2, 3 }
            );

            migrationBuilder.DeleteData(
                table: "HotelAmenityMapping",
                keyColumns: new[] { "HotelID", "AmenityID" },
                keyValues: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 1, 3 },
                    { 1, 4 },
                    { 1, 5 },
                    { 1, 6 },
                    { 1, 7 },
                    { 1, 8 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 },
                    { 2, 5 },
                    { 2, 6 },
                    { 3, 1 },
                    { 3, 2 },
                    { 3, 3 },
                    { 3, 4 },
                    { 3, 5 },
                    { 3, 6 },
                    { 3, 7 },
                }
            );

            migrationBuilder.DeleteData(
                table: "HotelAmenities",
                keyColumn: "AmenityID",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8 }
            );
            migrationBuilder.DeleteData(
                table: "Rooms",
                keyColumn: "RoomID",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8 }
            );
            migrationBuilder.DeleteData(
                table: "Hotels",
                keyColumn: "HotelID",
                keyValues: new object[] { 1, 2, 3 }
            );
            migrationBuilder.DeleteData(
                table: "RoomTypes",
                keyColumn: "RoomTypeID",
                keyValues: new object[] { 1, 2, 3 }
            );
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "UserID",
                keyValues: new object[] { 1, 2, 3, 4, 5 }
            );
            migrationBuilder.DeleteData(
                table: "Cities",
                keyColumn: "CityID",
                keyValues: new object[] { 34, 6, 35, 7, 16 }
            );
            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "RoleID",
                keyValues: new object[] { 1, 2, 3 }
            );
            migrationBuilder.DeleteData(
                table: "HotelImages",
                keyColumn: "ImageID",
                keyValues: new object[] { 1, 2, 3 }
            );
            migrationBuilder.DeleteData(
                table: "RoomImages",
                keyColumn: "ImageID",
                keyValues: new object[] { 1, 2, 3, 4, 5, 6, 7, 8 }
            );
            migrationBuilder.DeleteData(
                table: "RoomAvailabilities",
                keyColumns: new[] { "RoomID", "Date" },
                keyValues: new object[,]
                {
                    { 1, new DateTime(2024, 8, 1) },
                    { 1, new DateTime(2024, 8, 2) },
                    { 1, new DateTime(2024, 8, 3) },
                    { 2, new DateTime(2024, 8, 1) },
                    { 2, new DateTime(2024, 8, 2) },
                    { 2, new DateTime(2024, 8, 3) },
                    { 3, new DateTime(2024, 8, 1) },
                    { 3, new DateTime(2024, 8, 2) },
                    { 3, new DateTime(2024, 8, 3) },
                }
            );
        }
    }
}
