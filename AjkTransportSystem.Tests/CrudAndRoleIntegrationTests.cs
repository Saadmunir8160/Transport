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
    public class CrudAndRoleIntegrationTests
    {
        private AppDbContext CreateDbContext(string dbName, int? tenantId = null, bool isSuperAdmin = false)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            return new AppDbContext(options)
            {
                TenantId = tenantId,
                IsSuperAdmin = isSuperAdmin
            };
        }

        private TransportAdminController CreateTransportAdminController(AppDbContext context, int orgId)
        {
            var controller = new TransportAdminController(context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "99"),
                new Claim(ClaimTypes.Role, "TransportAdmin"),
                new Claim("OrganizationId", orgId.ToString())
            }, "TestAuth"));

            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            return controller;
        }

        [Fact]
        public async Task TransportAdmin_CanCreateAndRetrieveVehicle_WithinOrganization()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName, tenantId: 1);
            var controller = CreateTransportAdminController(context, orgId: 1);

            var vehicle = new Vehicle
            {
                VehicleId = 1,
                VehicleNumber = "AJK-TEST-01",
                RegistrationNumber = "REG-991",
                VehicleType = "Bus",
                Capacity = 30,
                Status = "Active"
            };

            var createResult = await controller.AddVehicle(vehicle);
            Assert.IsType<OkObjectResult>(createResult);

            var getResult = await controller.GetVehicles();
            var okGet = Assert.IsType<OkObjectResult>(getResult);
            var vehicles = Assert.IsAssignableFrom<IEnumerable<Vehicle>>(okGet.Value);
            
            var saved = vehicles.FirstOrDefault(v => v.VehicleNumber == "AJK-TEST-01");
            Assert.NotNull(saved);
            Assert.Equal(1, saved.OrganizationId);
        }

        [Fact]
        public async Task TransportAdmin_CannotSeeVehiclesOfOtherOrganizations()
        {
            var dbName = Guid.NewGuid().ToString();
            
            // Seed Org 2 vehicle
            using (var seedContext = CreateDbContext(dbName, tenantId: 2))
            {
                seedContext.Vehicles.Add(new Vehicle
                {
                    VehicleId = 2,
                    OrganizationId = 2,
                    VehicleNumber = "ORG2-BUS-01",
                    VehicleType = "Coaster",
                    Capacity = 25,
                    Status = "Active"
                });
                await seedContext.SaveChangesAsync();
            }

            // Query as Org 1 Transport Admin
            using (var org1Context = CreateDbContext(dbName, tenantId: 1))
            {
                var controller = CreateTransportAdminController(org1Context, orgId: 1);
                var result = await controller.GetVehicles();
                var okResult = Assert.IsType<OkObjectResult>(result);
                var list = Assert.IsAssignableFrom<IEnumerable<Vehicle>>(okResult.Value);
                Assert.Empty(list); // Org 2's vehicle is completely hidden
            }
        }

        [Fact]
        public async Task TransportAdmin_CanCreateRoute_AndAddStops()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName, tenantId: 1);
            var controller = CreateTransportAdminController(context, orgId: 1);

            var route = new Route
            {
                RouteId = 1,
                RouteName = "Kotli to Rawalpindi Express",
                StartPoint = "Kotli",
                EndPoint = "Rawalpindi",
                Distance = 140.0m,
                BaseFare = 700.0m
            };

            var addRouteRes = await controller.AddRoute(route);
            Assert.IsType<OkObjectResult>(addRouteRes);

            var stop = new RouteStop
            {
                RouteStopId = 1,
                StopName = "Sehnsa",
                StopOrder = 1,
                DistanceFromStart = 30,
                FareFromStart = 200.0m
            };

            var addStopRes = await controller.AddRouteStop(1, stop);
            Assert.IsType<OkObjectResult>(addStopRes);

            var stopsResult = await controller.GetRouteStops(1);
            var okStops = Assert.IsType<OkObjectResult>(stopsResult);
            var stops = Assert.IsAssignableFrom<IEnumerable<RouteStop>>(okStops.Value);
            Assert.Single(stops);
            Assert.Equal("Sehnsa", stops.First().StopName);
        }

        [Fact]
        public async Task ConcurrentBooking_SameSeatSimultaneousRequests_OnlyOneSucceeds()
        {
            var dbName = Guid.NewGuid().ToString();
            using var context = CreateDbContext(dbName);

            var vehicle = new Vehicle { VehicleId = 1, VehicleNumber = "AJK-CONCUR", VehicleType = "Van", Capacity = 10, Status = "Active" };
            var route = new Route { RouteId = 1, RouteName = "Muzaffarabad to Kohala", StartPoint = "Muzaffarabad", EndPoint = "Kohala" };
            var fare = new Fare { FareId = 1, RouteId = 1, Amount = 300.0m, PassengerType = "Standard" };
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

            // Simulate Passenger A and Passenger B simultaneously trying to book seat "01"
            var controllerA = CreatePassengerController(context, userId: 101);
            var controllerB = CreatePassengerController(context, userId: 102);

            var dtoA = new PassengerController.CreateBookingDto
            {
                TripId = 1,
                PassengerName = "Passenger A",
                PassengerPhone = "03001111111",
                PassengerCNIC = "82203-1111111-1",
                SelectedSeats = new List<string> { "01" }
            };

            var dtoB = new PassengerController.CreateBookingDto
            {
                TripId = 1,
                PassengerName = "Passenger B",
                PassengerPhone = "03002222222",
                PassengerCNIC = "82203-2222222-2",
                SelectedSeats = new List<string> { "01" }
            };

            // Execute concurrent requests
            var taskA = controllerA.BookTrip(dtoA);
            var taskB = controllerB.BookTrip(dtoB);

            var results = await Task.WhenAll(taskA, taskB);

            int successCount = results.Count(r => r is OkObjectResult);
            int failCount = results.Count(r => r is BadRequestObjectResult);

            Assert.Equal(1, successCount);
            Assert.Equal(1, failCount);

            // Verify final database state: exactly 1 booking exists, trip booked seats = 1
            var bookings = await context.Bookings.Where(b => b.TripId == 1 && b.Status == "Confirmed").ToListAsync();
            Assert.Single(bookings);
            Assert.Equal("01", bookings[0].SeatNumber);

            var finalTrip = await context.Trips.FindAsync(1);
            Assert.Equal(1, finalTrip.BookedSeats);
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
    }
}
