-- ========================================
-- AJK TRANSPORT SYSTEM - PRODUCTION DATABASE SETUP
-- Run this ONCE before deployment
-- ========================================

USE ajktransportdb;

-- 1. Create RouteStops Table (if not exists)
CREATE TABLE IF NOT EXISTS RouteStops (
    RouteStopId INT AUTO_INCREMENT PRIMARY KEY,
    RouteId INT NOT NULL,
    StopName VARCHAR(100) NOT NULL,
    StopOrder INT NOT NULL,
    DistanceFromStart INT NOT NULL,
    FareFromStart DECIMAL(10,2) NOT NULL,
    FOREIGN KEY (RouteId) REFERENCES Routes(RouteId) ON DELETE CASCADE,
    INDEX idx_route (RouteId),
    INDEX idx_stop_order (RouteId, StopOrder)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2. Update Bookings Table (add stop columns)
ALTER TABLE Bookings 
ADD COLUMN IF NOT EXISTS BoardingStop VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS DroppingStop VARCHAR(100) NULL;

-- 3. Verify all tables exist
SELECT 
    TABLE_NAME,
    TABLE_ROWS,
    CREATE_TIME
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = 'ajktransportdb'
ORDER BY TABLE_NAME;

-- 4. Check indexes
SHOW INDEX FROM Routes;
SHOW INDEX FROM Vehicles;
SHOW INDEX FROM Timings;
SHOW INDEX FROM Bookings;

-- SUCCESS MESSAGE
SELECT 'Database setup complete! Ready for deployment.' as Status;
