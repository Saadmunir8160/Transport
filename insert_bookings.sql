DECLARE @TripId INT = (SELECT TOP 1 TripId FROM Trips WHERE Status = 'InProgress'); 

INSERT INTO Bookings (UserId, RouteId, TripId, PassengerName, PassengerCNIC, PassengerPhone, Seats, TotalFare, TravelDate, BookingDate, Status, BoardingStop, DroppingStop, SeatNumber, IsNoShow, PaymentStatus, TicketCode) VALUES 
(5, 1, @TripId, 'Ali Khan', '12345-1234567-1', '03001234567', 1, 50, GETDATE(), GETDATE(), 'Confirmed', 'Central Station', 'Downtown Square', 'A1', 0, 'Paid', 'TKT-001'), 
(5, 1, @TripId, 'Sara Ahmed', '12345-1234567-2', '03001234568', 2, 200, GETDATE(), GETDATE(), 'Confirmed', 'Downtown Square', 'North Terminal', 'A2,A3', 0, 'Pending', 'TKT-002'); 
