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
    public class BackendCoreTests
    {
        private AppDbContext CreateInMemoryDbContext(string dbName, int? tenantId = null, bool isSuperAdmin = false)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            var context = new AppDbContext(options)
            {
                TenantId = tenantId,
                IsSuperAdmin = isSuperAdmin
            };

            return context;
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

        [Fact]
        public async Task BookTrip_SuccessfulBooking_DecreasesAvailableSeatsAndConfirmsBooking()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryDbContext(dbName);

            var vehicle = new Vehicle { VehicleId = 1, VehicleNumber = "AJK-101", VehicleType = "Hiace Van", Capacity = 20, Status = "Active" };
            var route = new Route { RouteId = 1, RouteName = "Muzaffarabad to Rawalpindi", StartPoint = "Muzaffarabad", EndPoint = "Rawalpindi" };
            var fare = new Fare { FareId = 1, RouteId = 1, Amount = 650.0m, PassengerType = "Standard" };
            var trip = new Trip
            {
                TripId = 1,
                RouteId = 1,
                VehicleId = 1,
                TravelDate = DateTime.UtcNow.Date,
                DepartureTime = new TimeSpan(8, 0, 0),
                Status = "Scheduled",
                BookedSeats = 0,
                Vehicle = vehicle,
                Route = route
            };

            context.Vehicles.Add(vehicle);
            context.Routes.Add(route);
            context.Fares.Add(fare);
            context.Trips.Add(trip);
            await context.SaveChangesAsync();

            var controller = CreatePassengerController(context, userId: 10);
            var bookingDto = new PassengerController.CreateBookingDto
            {
                TripId = 1,
                PassengerName = "Ahmad Ali",
                PassengerPhone = "03001234567",
                PassengerCNIC = "82203-1234567-1",
                SelectedSeats = new List<string> { "01", "02" }
            };

            var actionResult = await controller.BookTrip(bookingDto);
            var okResult = Assert.IsType<OkObjectResult>(actionResult);

            var updatedTrip = await context.Trips.FindAsync(1);
            Assert.Equal(2, updatedTrip.BookedSeats);

            var booking = await context.Bookings.FirstOrDefaultAsync(b => b.TripId == 1);
            Assert.NotNull(booking);
            Assert.Equal("Confirmed", booking.Status);
            Assert.Equal(2, booking.Seats);
            Assert.Equal("01,02", booking.SeatNumber);
            Assert.Equal(1300.0m, booking.TotalFare);
        }

        [Fact]
        public async Task BookTrip_DuplicateSeatBooking_ReturnsBadRequest()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryDbContext(dbName);

            var vehicle = new Vehicle { VehicleId = 1, VehicleNumber = "AJK-102", VehicleType = "Coaster", Capacity = 20, Status = "Active" };
            var route = new Route { RouteId = 1, RouteName = "Mirpur to Islamabad", StartPoint = "Mirpur", EndPoint = "Islamabad" };
            var fare = new Fare { FareId = 1, RouteId = 1, Amount = 500.0m, PassengerType = "Standard" };
            var trip = new Trip
            {
                TripId = 1,
                RouteId = 1,
                VehicleId = 1,
                TravelDate = DateTime.UtcNow.Date,
                DepartureTime = new TimeSpan(9, 0, 0),
                Status = "Scheduled",
                BookedSeats = 1,
                Vehicle = vehicle,
                Route = route
            };

            var existingBooking = new Booking
            {
                BookingId = 1,
                UserId = 99,
                TripId = 1,
                RouteId = 1,
                PassengerName = "Existing Passenger",
                PassengerPhone = "03111234567",
                PassengerCNIC = "82203-5555555-1",
                Seats = 1,
                SeatNumber = "05",
                TotalFare = 500.0m,
                Status = "Confirmed",
                TravelDate = DateTime.UtcNow.Date,
                BookingDate = DateTime.UtcNow
            };

            context.Vehicles.Add(vehicle);
            context.Routes.Add(route);
            context.Fares.Add(fare);
            context.Trips.Add(trip);
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();

            var controller = CreatePassengerController(context, userId: 12);
            var dto = new PassengerController.CreateBookingDto
            {
                TripId = 1,
                PassengerName = "Second Passenger",
                PassengerPhone = "03221234567",
                PassengerCNIC = "82203-9999999-1",
                SelectedSeats = new List<string> { "05" } // Conflict!
            };

            var actionResult = await controller.BookTrip(dto);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Contains("already booked", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);

            var checkTrip = await context.Trips.FindAsync(1);
            Assert.Equal(1, checkTrip.BookedSeats); // No corruption
        }

        [Fact]
        public async Task BookTrip_ExceedingVehicleCapacity_ReturnsBadRequest()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryDbContext(dbName);

            var vehicle = new Vehicle { VehicleId = 1, VehicleNumber = "AJK-103", VehicleType = "Car", Capacity = 2, Status = "Active" };
            var route = new Route { RouteId = 1, RouteName = "Rawalakot to Kotli", StartPoint = "Rawalakot", EndPoint = "Kotli" };
            var trip = new Trip
            {
                TripId = 1,
                RouteId = 1,
                VehicleId = 1,
                TravelDate = DateTime.UtcNow.Date,
                DepartureTime = new TimeSpan(10, 0, 0),
                Status = "Scheduled",
                BookedSeats = 1,
                Vehicle = vehicle,
                Route = route
            };

            context.Vehicles.Add(vehicle);
            context.Routes.Add(route);
            context.Trips.Add(trip);
            await context.SaveChangesAsync();

            var controller = CreatePassengerController(context, userId: 15);
            var dto = new PassengerController.CreateBookingDto
            {
                TripId = 1,
                PassengerName = "Overbooking Passenger",
                PassengerPhone = "03331234567",
                PassengerCNIC = "82203-8888888-1",
                SelectedSeats = new List<string> { "02", "03" } // 1 existing + 2 new = 3 > capacity (2)
            };

            var actionResult = await controller.BookTrip(dto);
            var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
            Assert.Contains("capacity", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task CancelBooking_RestoresCapacity_AndPreventsRepeatedCancellation()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryDbContext(dbName);

            var trip = new Trip
            {
                TripId = 1,
                RouteId = 1,
                TravelDate = DateTime.UtcNow.Date,
                DepartureTime = new TimeSpan(7, 30, 0),
                Status = "Scheduled",
                BookedSeats = 3
            };

            var booking = new Booking
            {
                BookingId = 100,
                UserId = 20,
                TripId = 1,
                RouteId = 1,
                PassengerName = "Cancelled Passenger",
                PassengerPhone = "03451234567",
                PassengerCNIC = "82203-4444444-1",
                Seats = 2,
                SeatNumber = "03,04",
                TotalFare = 1000.0m,
                Status = "Confirmed",
                TravelDate = DateTime.UtcNow.Date,
                BookingDate = DateTime.UtcNow
            };

            context.Trips.Add(trip);
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var controller = CreatePassengerController(context, userId: 20);

            // First cancellation: succeeds and decrements BookedSeats from 3 to 1
            var firstCancelResult = await controller.CancelBooking(100);
            var okResult = Assert.IsType<OkObjectResult>(firstCancelResult);

            var updatedTrip = await context.Trips.FindAsync(1);
            Assert.Equal(1, updatedTrip.BookedSeats);

            var updatedBooking = await context.Bookings.FindAsync(100);
            Assert.Equal("Cancelled", updatedBooking.Status);

            // Second cancellation: rejected as already cancelled, does not decrement seats below 1
            var secondCancelResult = await controller.CancelBooking(100);
            var badRequest = Assert.IsType<BadRequestObjectResult>(secondCancelResult);
            Assert.Contains("already cancelled", badRequest.Value.ToString(), StringComparison.OrdinalIgnoreCase);

            var tripAfterSecondAttempt = await context.Trips.FindAsync(1);
            Assert.Equal(1, tripAfterSecondAttempt.BookedSeats);
        }

        [Fact]
        public async Task CancelBooking_UnauthorizedUser_ReturnsNotFound()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryDbContext(dbName);

            var booking = new Booking
            {
                BookingId = 200,
                UserId = 30, // Owned by user 30
                PassengerName = "Private Passenger",
                PassengerPhone = "03459999999",
                PassengerCNIC = "82203-7777777-1",
                Seats = 1,
                Status = "Confirmed"
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Attacker is user 40
            var attackerController = CreatePassengerController(context, userId: 40);
            var result = await attackerController.CancelBooking(200);

            Assert.IsType<NotFoundObjectResult>(result);

            // Booking remains intact
            var intactBooking = await context.Bookings.FindAsync(200);
            Assert.Equal("Confirmed", intactBooking.Status);
        }

        [Fact]
        public async Task MultiTenantIsolation_TransportAdminCannotAccessOtherOrgVehicles()
        {
            var dbName = Guid.NewGuid().ToString();

            // Seed vehicles for Org 1 and Org 2
            using (var seedContext = CreateInMemoryDbContext(dbName, tenantId: null, isSuperAdmin: true))
            {
                seedContext.Vehicles.Add(new Vehicle { VehicleId = 1, OrganizationId = 1, VehicleNumber = "AJK-ORG1-01", VehicleType = "Bus", Status = "Active" });
                seedContext.Vehicles.Add(new Vehicle { VehicleId = 2, OrganizationId = 2, VehicleNumber = "AJK-ORG2-01", VehicleType = "Bus", Status = "Active" });
                await seedContext.SaveChangesAsync();
            }

            // Transport Admin scoped to Organization 1 queries vehicles
            using (var tenant1Context = CreateInMemoryDbContext(dbName, tenantId: 1, isSuperAdmin: false))
            {
                var org1Vehicles = await tenant1Context.Vehicles.ToListAsync();
                Assert.Single(org1Vehicles);
                Assert.Equal(1, org1Vehicles[0].OrganizationId);
                Assert.Equal("AJK-ORG1-01", org1Vehicles[0].VehicleNumber);

                // Attempt direct query for Org 2 vehicle
                var org2Vehicle = await tenant1Context.Vehicles.FirstOrDefaultAsync(v => v.VehicleId == 2);
                Assert.Null(org2Vehicle); // Tenant filter isolates and hides Org 2
            }
        }

        [Fact]
        public async Task DriverTripAssignment_DriverOnlyAccessesAssignedTrips()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateInMemoryDbContext(dbName, tenantId: 1, isSuperAdmin: false);

            var tripAssigned = new Trip
            {
                TripId = 10,
                OrganizationId = 1,
                DriverId = 5, // Assigned to Driver 5
                TravelDate = DateTime.UtcNow.Date,
                DepartureTime = new TimeSpan(8, 0, 0),
                Status = "Scheduled"
            };

            var tripUnassigned = new Trip
            {
                TripId = 11,
                OrganizationId = 1,
                DriverId = 6, // Assigned to Driver 6
                TravelDate = DateTime.UtcNow.Date,
                DepartureTime = new TimeSpan(9, 0, 0),
                Status = "Scheduled"
            };

            context.Trips.AddRange(tripAssigned, tripUnassigned);
            await context.SaveChangesAsync();

            // Driver 5 querying assigned trips
            var driver5Trips = await context.Trips.Where(t => t.DriverId == 5).ToListAsync();
            Assert.Single(driver5Trips);
            Assert.Equal(10, driver5Trips[0].TripId);
        }
    }
}
