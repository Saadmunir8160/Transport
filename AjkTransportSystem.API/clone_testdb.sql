-- Clone tables into isolated test database
USE AjkTransport_TestDb;

IF OBJECT_ID('dbo.__EFMigrationsHistory') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[__EFMigrationsHistory] FROM [AjkTransport].[dbo].[__EFMigrationsHistory] WHERE 1=0;
IF OBJECT_ID('dbo.Organizations') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Organizations] FROM [AjkTransport].[dbo].[Organizations] WHERE 1=0;
IF OBJECT_ID('dbo.Users') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Users] FROM [AjkTransport].[dbo].[Users] WHERE 1=0;
IF OBJECT_ID('dbo.Vehicles') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Vehicles] FROM [AjkTransport].[dbo].[Vehicles] WHERE 1=0;
IF OBJECT_ID('dbo.Routes') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Routes] FROM [AjkTransport].[dbo].[Routes] WHERE 1=0;
IF OBJECT_ID('dbo.RouteStops') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[RouteStops] FROM [AjkTransport].[dbo].[RouteStops] WHERE 1=0;
IF OBJECT_ID('dbo.Timings') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Timings] FROM [AjkTransport].[dbo].[Timings] WHERE 1=0;
IF OBJECT_ID('dbo.Trips') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Trips] FROM [AjkTransport].[dbo].[Trips] WHERE 1=0;
IF OBJECT_ID('dbo.Fares') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Fares] FROM [AjkTransport].[dbo].[Fares] WHERE 1=0;
IF OBJECT_ID('dbo.Feedbacks') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Feedbacks] FROM [AjkTransport].[dbo].[Feedbacks] WHERE 1=0;
IF OBJECT_ID('dbo.Bookings') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Bookings] FROM [AjkTransport].[dbo].[Bookings] WHERE 1=0;
IF OBJECT_ID('dbo.Notifications') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Notifications] FROM [AjkTransport].[dbo].[Notifications] WHERE 1=0;
IF OBJECT_ID('dbo.Complaints') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Complaints] FROM [AjkTransport].[dbo].[Complaints] WHERE 1=0;
IF OBJECT_ID('dbo.Advisories') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Advisories] FROM [AjkTransport].[dbo].[Advisories] WHERE 1=0;
IF OBJECT_ID('dbo.AuditLogs') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[AuditLogs] FROM [AjkTransport].[dbo].[AuditLogs] WHERE 1=0;
IF OBJECT_ID('dbo.UserSessions') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[UserSessions] FROM [AjkTransport].[dbo].[UserSessions] WHERE 1=0;
IF OBJECT_ID('dbo.Documents') IS NULL SELECT * INTO [AjkTransport_TestDb].[dbo].[Documents] FROM [AjkTransport].[dbo].[Documents] WHERE 1=0;
