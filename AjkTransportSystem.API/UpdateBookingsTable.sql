-- Add Boarding and Dropping Stop columns to Bookings table
USE ajktransportdb;

ALTER TABLE Bookings 
ADD COLUMN BoardingStop VARCHAR(100) NULL,
ADD COLUMN DroppingStop VARCHAR(100) NULL;

-- Example: How fare is calculated based on stops
-- 
-- Route: Bagh → Rawalpindi (Via Murree)
-- Stops: Bagh (Rs.100) → Sudhnoti (Rs.400) → Rawalakot (Rs.760) → Murree (Rs.1240) → Rawalpindi (Rs.1840)
--
-- Scenario 1: Direct Passenger (Bagh to Rawalpindi)
--   BoardingStop: Bagh
--   DroppingStop: Rawalpindi
--   Fare: Rs. 1840 (Full route)
--
-- Scenario 2: Intermediate Passenger (Sudhnoti to Murree)
--   BoardingStop: Sudhnoti
--   DroppingStop: Murree
--   Fare: Rs. 1240 - Rs. 400 = Rs. 840
--
-- Scenario 3: Partial Route (Bagh to Rawalakot)
--   BoardingStop: Bagh
--   DroppingStop: Rawalakot
--   Fare: Rs. 760 - Rs. 100 = Rs. 660

-- Verify table structure
DESCRIBE Bookings;
