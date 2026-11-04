using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using AjkTransportSystem.API.Controllers;
using AjkTransportSystem.API.Data;
using AjkTransportSystem.API.Models;

namespace AjkTransportSystem.Tests
{
    public class SqlServerIntegrationTests
    {
        private const string TestDbConnectionString = "Server=localhost\\SQLEXPRESS;Database=AjkTransport_TestDb;Trusted_Connection=True;TrustServerCertificate=True;";

        private AppDbContext CreateSqlServerDbContext(int? tenantId = null, bool isSuperAdmin = false)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(TestDbConnectionString)
                .Options;

            return new AppDbContext(options)
            {
                TenantId = tenantId,
                IsSuperAdmin = isSuperAdmin
            };
        }

        private PassengerController CreatePassengerController(AppDbContext context, int userId)
        {
            var controller = new PassengerController(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, "Passenger")
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        private async Task CleanDatabaseAsync()
        {
            using var context = CreateSqlServerDbContext(isSuperAdmin: true);
            await context.Database.ExecuteSqlRawAsync(@"
                DELETE FROM Bookings;
                DELETE FROM Complaints;
                DELETE FROM Advisories;
                DELETE FROM Documents;
                DELETE FROM Notifications;
                DELETE FROM UserSessions;
                DELETE FROM AuditLogs;
                DELETE FROM Feedbacks;
                DELETE FROM Fares;
                DELETE FROM RouteStops;
                DELETE FROM Timings;
                DELETE FROM Trips;
                DELETE FROM Vehicles;
                DELETE FROM Routes;
                DELETE FROM Users;
                DELETE FROM Organizations;
            ");
        }

        [Fact]
        public async Task Concurrency_SimultaneousRequestsForSameSeat_OnlyOneSucceedsAndBookedSeatsAccurate()
        {
            await CleanDatabaseAsync();

            int passenger1Id;
            int passenger2Id;
            int tripId;

            using (var seedContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var org = new Organization
                {
                    OrganizationName = "Kashmir Express",
                    OrganizationCode = "KEX",
                    Location = "Muzaffarabad",
                    District = "Muzaffarabad",
                    Status = "Active"
                };
                seedContext.Organizations.Add(org);
                await seedContext.SaveChangesAsync();

                var user1 = new User { Name = "Tariq Khan", Email = "tariq@test.com", Role = "Passenger", PasswordHash = "hash1", OrganizationId = org.OrganizationId };
                var user2 = new User { Name = "Zubair Ahmed", Email = "zubair@test.com", Role = "Passenger", PasswordHash = "hash2", OrganizationId = org.OrganizationId };
                var vehicle = new Vehicle { OrganizationId = org.OrganizationId, VehicleNumber = "KEX-901", VehicleType = "Coaster", Capacity = 20, Status = "Active" };
                var route = new AjkTransportSystem.API.Models.Route { OrganizationId = org.OrganizationId, RouteName = "Muzaffarabad - Rawalpindi", StartPoint = "Muzaffarabad", EndPoint = "Rawalpindi", Distance = 135.0m };
                
                seedContext.Users.AddRange(user1, user2);
                seedContext.Vehicles.Add(vehicle);
                seedContext.Routes.Add(route);
                await seedContext.SaveChangesAsync();

                var fare = new Fare { RouteId = route.RouteId, Amount = 750.0m, PassengerType = "Standard" };
                var trip = new Trip
                {
                    OrganizationId = org.OrganizationId,
                    VehicleId = vehicle.VehicleId,
                    RouteId = route.RouteId,
                    TravelDate = DateTime.UtcNow.Date.AddDays(1),
                    DepartureTime = new TimeSpan(9, 0, 0),
                    Status = "Scheduled",
                    BookedSeats = 0
                };

                seedContext.Fares.Add(fare);
                seedContext.Trips.Add(trip);
                await seedContext.SaveChangesAsync();

                passenger1Id = user1.UserId;
                passenger2Id = user2.UserId;
                tripId = trip.TripId;
            }

            // Dispatch two simultaneous booking requests for seat "05"
            var task1 = Task.Run(async () =>
            {
                using var db1 = CreateSqlServerDbContext();
                var controller1 = CreatePassengerController(db1, passenger1Id);
                return await controller1.BookTrip(new PassengerController.CreateBookingDto
                {
                    TripId = tripId,
                    PassengerName = "Tariq Khan",
                    PassengerPhone = "03001234567",
                    PassengerCNIC = "82201-1234567-1",
                    SelectedSeats = new List<string> { "05" }
                });
            });

            var task2 = Task.Run(async () =>
            {
                using var db2 = CreateSqlServerDbContext();
                var controller2 = CreatePassengerController(db2, passenger2Id);
                return await controller2.BookTrip(new PassengerController.CreateBookingDto
                {
                    TripId = tripId,
                    PassengerName = "Zubair Ahmed",
                    PassengerPhone = "03007654321",
                    PassengerCNIC = "82201-7654321-2",
                    SelectedSeats = new List<string> { "05" }
                });
            });

            var results = await Task.WhenAll(task1, task2);

            int successCount = results.Count(r => r is OkObjectResult);
            int conflictCount = results.Count(r => r is BadRequestObjectResult);

            if (successCount != 1)
            {
                var errors = string.Join(" | ", results.Select(r => r is ObjectResult o ? $"{o.GetType().Name}: {System.Text.Json.JsonSerializer.Serialize(o.Value)}" : r.GetType().Name));
                throw new Exception($"Concurrency test failed! Results: {errors}");
            }

            Assert.Equal(1, successCount);
            Assert.Equal(1, conflictCount);

            var badRequest = (BadRequestObjectResult)results.First(r => r is BadRequestObjectResult);
            Assert.Contains("already booked", badRequest.Value!.ToString(), StringComparison.OrdinalIgnoreCase);

            using (var verifyContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var confirmedBookings = await verifyContext.Bookings
                    .Where(b => b.TripId == tripId && b.Status == "Confirmed")
                    .ToListAsync();
                Assert.Single(confirmedBookings);
                Assert.Equal("05", confirmedBookings[0].SeatNumber);

                var dbTrip = await verifyContext.Trips.FindAsync(tripId);
                Assert.NotNull(dbTrip);
                Assert.Equal(1, dbTrip.BookedSeats);
            }
        }

        [Fact]
        public async Task Concurrency_NearCapacityCeiling_CapacityNeverExceeded()
        {
            await CleanDatabaseAsync();

            int passenger1Id;
            int passenger2Id;
            int tripId;

            using (var seedContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var org = new Organization
                {
                    OrganizationName = "Valley Transit",
                    OrganizationCode = "VT",
                    Location = "Rawalakot",
                    District = "Poonch",
                    Status = "Active"
                };
                seedContext.Organizations.Add(org);
                await seedContext.SaveChangesAsync();

                var user1 = new User { Name = "User One", Email = "u1@test.com", Role = "Passenger", PasswordHash = "hash1", OrganizationId = org.OrganizationId };
                var user2 = new User { Name = "User Two", Email = "u2@test.com", Role = "Passenger", PasswordHash = "hash2", OrganizationId = org.OrganizationId };
                var vehicle = new Vehicle { OrganizationId = org.OrganizationId, VehicleNumber = "VT-Mini", VehicleType = "MiniVan", Capacity = 2, Status = "Active" };
                var route = new AjkTransportSystem.API.Models.Route { OrganizationId = org.OrganizationId, RouteName = "Rawalakot - Bagh", StartPoint = "Rawalakot", EndPoint = "Bagh", Distance = 45.0m };
                
                seedContext.Users.AddRange(user1, user2);
                seedContext.Vehicles.Add(vehicle);
                seedContext.Routes.Add(route);
                await seedContext.SaveChangesAsync();

                var fare = new Fare { RouteId = route.RouteId, Amount = 300.0m, PassengerType = "Standard" };
                var trip = new Trip
                {
                    OrganizationId = org.OrganizationId,
                    VehicleId = vehicle.VehicleId,
                    RouteId = route.RouteId,
                    TravelDate = DateTime.UtcNow.Date.AddDays(1),
                    DepartureTime = new TimeSpan(10, 0, 0),
                    Status = "Scheduled",
                    BookedSeats = 1 // 1 of 2 seats already booked
                };

                seedContext.Fares.Add(fare);
                seedContext.Trips.Add(trip);
                await seedContext.SaveChangesAsync();

                var existingBooking = new Booking
                {
                    UserId = user1.UserId,
                    RouteId = route.RouteId,
                    TripId = trip.TripId,
                    OrganizationId = org.OrganizationId,
                    PassengerName = "Prior Passenger",
                    PassengerPhone = "03120000000",
                    PassengerCNIC = "82201-0000000-0",
                    Seats = 1,
                    SeatNumber = "01",
                    TotalFare = 300.0m,
                    TravelDate = DateTime.UtcNow.Date.AddDays(1),
                    BookingDate = DateTime.UtcNow,
                    Status = "Confirmed"
                };

                seedContext.Bookings.Add(existingBooking);
                await seedContext.SaveChangesAsync();

                passenger1Id = user1.UserId;
                passenger2Id = user2.UserId;
                tripId = trip.TripId;
            }

            var task1 = Task.Run(async () =>
            {
                using var db1 = CreateSqlServerDbContext();
                var controller1 = CreatePassengerController(db1, passenger1Id);
                return await controller1.BookTrip(new PassengerController.CreateBookingDto
                {
                    TripId = tripId,
                    PassengerName = "User One",
                    PassengerPhone = "03001111111",
                    PassengerCNIC = "82201-1111111-1",
                    SelectedSeats = new List<string> { "02" }
                });
            });

            var task2 = Task.Run(async () =>
            {
                using var db2 = CreateSqlServerDbContext();
                var controller2 = CreatePassengerController(db2, passenger2Id);
                return await controller2.BookTrip(new PassengerController.CreateBookingDto
                {
                    TripId = tripId,
                    PassengerName = "User Two",
                    PassengerPhone = "03002222222",
                    PassengerCNIC = "82201-2222222-2",
                    SelectedSeats = new List<string> { "03" }
                });
            });

            var results = await Task.WhenAll(task1, task2);

            int successCount = results.Count(r => r is OkObjectResult);
            int rejectedCount = results.Count(r => r is BadRequestObjectResult);

            Assert.Equal(1, successCount);
            Assert.Equal(1, rejectedCount);

            using (var verifyContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var dbTrip = await verifyContext.Trips.FindAsync(tripId);
                Assert.NotNull(dbTrip);
                Assert.Equal(2, dbTrip.BookedSeats); // Max capacity of 2 never exceeded
                
                var totalConfirmedBookings = await verifyContext.Bookings
                    .Where(b => b.TripId == tripId && b.Status == "Confirmed")
                    .SumAsync(b => b.Seats);
                Assert.Equal(2, totalConfirmedBookings);
            }
        }

        [Fact]
        public async Task Concurrency_CancellationRestoresSeatCapacityExactlyOnce()
        {
            await CleanDatabaseAsync();

            int passengerId;
            int tripId;
            int bookingId;

            using (var seedContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var org = new Organization
                {
                    OrganizationName = "Poonch Express",
                    OrganizationCode = "PEX",
                    Location = "Rawalakot",
                    District = "Poonch",
                    Status = "Active"
                };
                seedContext.Organizations.Add(org);
                await seedContext.SaveChangesAsync();

                var user = new User { Name = "Ahmad Shah", Email = "ahmad@test.com", Role = "Passenger", PasswordHash = "hash", OrganizationId = org.OrganizationId };
                var vehicle = new Vehicle { OrganizationId = org.OrganizationId, VehicleNumber = "PEX-101", VehicleType = "Van", Capacity = 15, Status = "Active" };
                var route = new AjkTransportSystem.API.Models.Route { OrganizationId = org.OrganizationId, RouteName = "Rawalakot - Islamabad", StartPoint = "Rawalakot", EndPoint = "Islamabad", Distance = 120.0m };
                
                seedContext.Users.Add(user);
                seedContext.Vehicles.Add(vehicle);
                seedContext.Routes.Add(route);
                await seedContext.SaveChangesAsync();

                var trip = new Trip
                {
                    OrganizationId = org.OrganizationId,
                    VehicleId = vehicle.VehicleId,
                    RouteId = route.RouteId,
                    TravelDate = DateTime.UtcNow.Date.AddDays(1),
                    DepartureTime = new TimeSpan(11, 0, 0),
                    Status = "Scheduled",
                    BookedSeats = 2
                };
                seedContext.Trips.Add(trip);
                await seedContext.SaveChangesAsync();

                var booking = new Booking
                {
                    UserId = user.UserId,
                    RouteId = route.RouteId,
                    TripId = trip.TripId,
                    OrganizationId = org.OrganizationId,
                    PassengerName = "Ahmad Shah",
                    PassengerPhone = "03003333333",
                    PassengerCNIC = "82201-3333333-3",
                    Seats = 2,
                    SeatNumber = "01,02",
                    TotalFare = 1000.0m,
                    TravelDate = DateTime.UtcNow.Date.AddDays(1),
                    BookingDate = DateTime.UtcNow,
                    Status = "Confirmed"
                };

                seedContext.Bookings.Add(booking);
                await seedContext.SaveChangesAsync();

                passengerId = user.UserId;
                tripId = trip.TripId;
                bookingId = booking.BookingId;
            }

            var cancelTask1 = Task.Run(async () =>
            {
                using var db1 = CreateSqlServerDbContext();
                var controller1 = CreatePassengerController(db1, passengerId);
                return await controller1.CancelBooking(bookingId);
            });

            var cancelTask2 = Task.Run(async () =>
            {
                using var db2 = CreateSqlServerDbContext();
                var controller2 = CreatePassengerController(db2, passengerId);
                return await controller2.CancelBooking(bookingId);
            });

            var cancelResults = await Task.WhenAll(cancelTask1, cancelTask2);

            int successCount = cancelResults.Count(r => r is OkObjectResult);
            int duplicateCount = cancelResults.Count(r => r is BadRequestObjectResult);

            Assert.Equal(1, successCount);
            Assert.Equal(1, duplicateCount);

            var badRequest = (BadRequestObjectResult)cancelResults.First(r => r is BadRequestObjectResult);
            Assert.Contains("already cancelled", badRequest.Value!.ToString(), StringComparison.OrdinalIgnoreCase);

            using (var verifyContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var dbTrip = await verifyContext.Trips.FindAsync(tripId);
                Assert.NotNull(dbTrip);
                Assert.Equal(0, dbTrip.BookedSeats);

                var dbBooking = await verifyContext.Bookings.FindAsync(bookingId);
                Assert.NotNull(dbBooking);
                Assert.Equal("Cancelled", dbBooking.Status);
            }
        }

        [Fact]
        public async Task Security_OrganizationIsolation_TransportAdminCannotQueryAnotherOrganization()
        {
            await CleanDatabaseAsync();

            int orgAId;
            int orgBId;
            int vehicleBId;

            using (var seedContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var orgA = new Organization
                {
                    OrganizationName = "Org Alpha",
                    OrganizationCode = "ALP",
                    Location = "Muzaffarabad",
                    District = "Muzaffarabad",
                    Status = "Active"
                };
                var orgB = new Organization
                {
                    OrganizationName = "Org Beta",
                    OrganizationCode = "BET",
                    Location = "Mirpur",
                    District = "Mirpur",
                    Status = "Active"
                };
                seedContext.Organizations.AddRange(orgA, orgB);
                await seedContext.SaveChangesAsync();

                var vehicleA = new Vehicle { OrganizationId = orgA.OrganizationId, VehicleNumber = "ALP-001", VehicleType = "Bus", Status = "Active" };
                var vehicleB = new Vehicle { OrganizationId = orgB.OrganizationId, VehicleNumber = "BET-001", VehicleType = "Bus", Status = "Active" };

                seedContext.Vehicles.AddRange(vehicleA, vehicleB);
                await seedContext.SaveChangesAsync();

                orgAId = orgA.OrganizationId;
                orgBId = orgB.OrganizationId;
                vehicleBId = vehicleB.VehicleId;
            }

            using (var adminAContext = CreateSqlServerDbContext(tenantId: orgAId, isSuperAdmin: false))
            {
                var vehicles = await adminAContext.Vehicles.ToListAsync();
                Assert.Single(vehicles);
                Assert.Equal(orgAId, vehicles[0].OrganizationId);
                Assert.Equal("ALP-001", vehicles[0].VehicleNumber);

                var forbiddenVehicle = await adminAContext.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == vehicleBId);
                Assert.Null(forbiddenVehicle);
            }
        }

        [Fact]
        public async Task Security_PassengerOwnership_PassengerCannotCancelOtherPassengersBooking()
        {
            await CleanDatabaseAsync();

            int attackerPassengerId;
            int bookingId;

            using (var seedContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var org = new Organization
                {
                    OrganizationName = "Neelum Transport",
                    OrganizationCode = "NT",
                    Location = "Athmuqam",
                    District = "Neelum",
                    Status = "Active"
                };
                seedContext.Organizations.Add(org);
                await seedContext.SaveChangesAsync();

                var victim = new User { Name = "Victim User", Email = "victim@test.com", Role = "Passenger", PasswordHash = "hash", OrganizationId = org.OrganizationId };
                var attacker = new User { Name = "Attacker User", Email = "attacker@test.com", Role = "Passenger", PasswordHash = "hash", OrganizationId = org.OrganizationId };
                var route = new AjkTransportSystem.API.Models.Route { OrganizationId = org.OrganizationId, RouteName = "Muzaffarabad - Athmuqam", StartPoint = "Muzaffarabad", EndPoint = "Athmuqam", Distance = 84.0m };
                
                seedContext.Users.AddRange(victim, attacker);
                seedContext.Routes.Add(route);
                await seedContext.SaveChangesAsync();

                var booking = new Booking
                {
                    UserId = victim.UserId,
                    RouteId = route.RouteId,
                    OrganizationId = org.OrganizationId,
                    PassengerName = "Victim User",
                    PassengerPhone = "03009999999",
                    PassengerCNIC = "82201-9999999-9",
                    Seats = 1,
                    SeatNumber = "04",
                    TotalFare = 500.0m,
                    TravelDate = DateTime.UtcNow.Date.AddDays(1),
                    BookingDate = DateTime.UtcNow,
                    Status = "Confirmed"
                };

                seedContext.Bookings.Add(booking);
                await seedContext.SaveChangesAsync();

                attackerPassengerId = attacker.UserId;
                bookingId = booking.BookingId;
            }

            using (var attackerContext = CreateSqlServerDbContext())
            {
                var controller = CreatePassengerController(attackerContext, attackerPassengerId);
                var result = await controller.CancelBooking(bookingId);

                Assert.IsType<NotFoundObjectResult>(result);
            }

            using (var verifyContext = CreateSqlServerDbContext(isSuperAdmin: true))
            {
                var dbBooking = await verifyContext.Bookings.FindAsync(bookingId);
                Assert.NotNull(dbBooking);
                Assert.Equal("Confirmed", dbBooking.Status);
            }
        }
    }
}
