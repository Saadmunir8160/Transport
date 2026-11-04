INSERT INTO RouteStops (RouteId, StopName, StopOrder, DistanceFromStart, FareFromStart, Latitude, Longitude, MinutesFromDeparture) VALUES 
(1, 'Central Station', 1, 0, 0, 34.0, 74.8, 0), 
(1, 'Downtown Square', 2, 5.2, 50, 34.05, 74.85, 15), 
(1, 'University Campus', 3, 12.5, 100, 34.1, 74.9, 30), 
(1, 'North Terminal', 4, 20.0, 150, 34.2, 75.0, 45); 

-- Assigning organizationid = 1 for the trip if organization is needed, or just let it be NULL if it allows NULL, let's omit it if it fails.
-- Let's check if OrganizationId allows null. But first I'll set it to NULL.
INSERT INTO Trips (RouteId, DriverId, VehicleId, TravelDate, DepartureTime, Status, BookedSeats, OrganizationId) VALUES 
(1, 5, 1, GETDATE(), '08:00:00', 'InProgress', 3, NULL);

DECLARE @TripId INT = (SELECT TOP 1 TripId FROM Trips WHERE Status = 'InProgress'); 

INSERT INTO Bookings (UserId, RouteId, TripId, PassengerName, PassengerPhone, Seats, TotalFare, TravelDate, BookingDate, Status, BoardingStop, DroppingStop, SeatNumber, IsNoShow, PaymentStatus, TicketCode) VALUES 
(5, 1, @TripId, 'Ali Khan', '03001234567', 1, 50, GETDATE(), GETDATE(), 'Confirmed', 'Central Station', 'Downtown Square', 'A1', 0, 'Paid', 'TKT-001'), 
(5, 1, @TripId, 'Sara Ahmed', '03001234568', 2, 200, GETDATE(), GETDATE(), 'Confirmed', 'Downtown Square', 'North Terminal', 'A2,A3', 0, 'Pending', 'TKT-002'); 

INSERT INTO Notifications (UserId, Type, Message, IsRead, CreatedAt) VALUES 
(5, 'TripStarted', 'You have a new trip assigned for today at 08:00 AM.', 0, GETDATE()), 
(5, 'RoadAdvisory', 'Heavy traffic expected near Downtown Square.', 0, GETDATE());
