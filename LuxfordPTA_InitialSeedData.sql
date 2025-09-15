-- =====================================================
-- Luxford PTA Event Management System - Initial Seed Data
-- Created: September 15, 2025
-- Purpose: Initial setup for production deployment
-- =====================================================

-- Note: Run this script manually after deploying the database schema
-- This is NOT part of Entity Framework migrations

USE [LuxfordPTAWeb] -- Replace with your actual database name
GO

-- =====================================================
-- 1. SCHOOL YEARS
-- =====================================================
SET IDENTITY_INSERT [SchoolYears] ON;

INSERT INTO [SchoolYears] ([Id], [Name], [StartDate], [EndDate], [PrintableEventCalendar])
VALUES 
    (1, '2023-2024', '2023-07-01', '2024-06-30', 'images/eventcal-2023-2024.pdf'),
    (2, '2024-2025', '2024-07-01', '2025-06-30', 'images/eventcal-2024-2025.pdf'),
    (3, '2025-2026', '2025-07-01', '2026-06-30', 'images/eventcal-2025-2026.jpg');

SET IDENTITY_INSERT [SchoolYears] OFF;

-- =====================================================
-- 2. EVENT TYPES
-- =====================================================
SET IDENTITY_INSERT [EventTypes] ON;

INSERT INTO [EventTypes] ([Id], [Name], [Slug], [Description], [DisplayOrder], [IsActive], [Size], [Icon], [ColorClass], [DisplayMode], [MaxEventsToShow], [ShowViewEventsButton], [ShowInlineOnMainPage])
VALUES 
    -- School Closed & Special Days
    (1, 'School Closed & Special Days', 'school-closed-days', 'All holidays, staff days, and special schedule days for the school year.', 0, 1, 1, 'bi-calendar-x', 'text-danger', 3, 0, 1, 1),
    
    -- PTA Events & Meetings
    (2, 'PTA Events & Meetings', 'pta-events', 'PTA meetings, board elections, volunteer opportunities, and general PTA business.', 1, 1, 0, 'bi-people-fill', 'text-primary', 0, 0, 1, 0),
    
    -- Fundraising Events
    (3, 'Fundraising Events', 'fundraising', 'Events that raise money for school programs, supplies, and activities.', 2, 1, 1, 'bi-cash-coin', 'text-success', 0, 0, 1, 0),
    
    -- Family Fun & Community Events
    (4, 'Family Fun & Community', 'family-events', 'Events designed to bring families together and build school community.', 3, 1, 1, 'bi-heart-fill', 'text-warning', 0, 0, 1, 0),
    
    -- Educational Programs
    (5, 'Educational Programs', 'educational', 'Special educational programs, guest speakers, and learning opportunities.', 4, 1, 0, 'bi-mortarboard-fill', 'text-info', 0, 0, 1, 0),
    
    -- Volunteer Opportunities
    (6, 'Volunteer Opportunities', 'volunteer', 'Ongoing volunteer needs and one-time volunteer events.', 5, 1, 0, 'bi-hand-thumbs-up', 'text-secondary', 0, 0, 1, 0),
    
    -- Spirit Nights & Restaurant Partnerships
    (7, 'Spirit Nights', 'spirit-nights', 'Restaurant partnerships where a percentage of sales supports our school.', 6, 1, 0, 'bi-shop', 'text-primary', 0, 0, 1, 0);

SET IDENTITY_INSERT [EventTypes] OFF;

-- =====================================================
-- 3. EVENT SUB-TYPES
-- =====================================================
SET IDENTITY_INSERT [EventSubTypes] ON;

INSERT INTO [EventSubTypes] ([Id], [Name], [Slug], [Description], [DisplayOrder], [IsActive], [Icon], [ColorClass], [EventTypeId])
VALUES 
    -- School Closed & Special Days Sub-types
    (1, 'Federal Holiday', 'federal-holiday', 'Federal holidays when school is closed', 0, 1, 'bi-flag', 'text-danger', 1),
    (2, 'Staff Development Day', 'staff-development', 'Professional development days - school closed for students', 1, 1, 'bi-mortarboard', 'text-warning', 1),
    (3, 'Early Dismissal', 'early-dismissal', 'Days with early dismissal schedule', 2, 1, 'bi-clock', 'text-info', 1),
    (4, 'Weather Closure', 'weather-closure', 'School closures due to weather conditions', 3, 1, 'bi-cloud-snow', 'text-primary', 1),
    
    -- Fundraising Events Sub-types
    (5, 'Product Sales', 'product-sales', 'Sales of products like books, wrapping paper, etc.', 0, 1, 'bi-bag', 'text-success', 3),
    (6, 'Entertainment Events', 'entertainment', 'Fun events that raise money - bingo, talent shows, etc.', 1, 1, 'bi-music-note', 'text-success', 3),
    (7, 'Community Events', 'community-fundraising', 'Large community events like fall festivals', 2, 1, 'bi-people', 'text-success', 3),
    
    -- Family Fun Sub-types
    (8, 'Seasonal Celebrations', 'seasonal', 'Holiday parties and seasonal events', 0, 1, 'bi-calendar-heart', 'text-warning', 4),
    (9, 'Game Nights', 'game-nights', 'Bingo, game nights, and interactive family events', 1, 1, 'bi-dice-5', 'text-warning', 4),
    (10, 'Outdoor Events', 'outdoor', 'Picnics, outdoor movie nights, field day', 2, 1, 'bi-tree', 'text-warning', 4),
    
    -- Educational Programs Sub-types
    (11, 'Parent Workshops', 'parent-workshops', 'Educational sessions for parents', 0, 1, 'bi-person-workspace', 'text-info', 5),
    (12, 'Student Programs', 'student-programs', 'Special programs and assemblies for students', 1, 1, 'bi-person-raised-hand', 'text-info', 5),
    (13, 'Guest Speakers', 'guest-speakers', 'Special speakers and presentations', 2, 1, 'bi-mic', 'text-info', 5);

SET IDENTITY_INSERT [EventSubTypes] OFF;

-- =====================================================
-- 4. SPONSORS (Sample Data)
-- =====================================================
SET IDENTITY_INSERT [Sponsors] ON;

INSERT INTO [Sponsors] ([Id], [Name], [LogoUrl], [WebsiteUrl])
VALUES 
    (1, 'Chuck E. Cheese''s', 'sponsors/chuck-e-cheeses-car-design-png-logo-8.png', 'https://www.chuckecheese.com/'),
    (2, 'Local Pizza Company', 'sponsors/sample-sponsor-logo.png', 'https://example.com/pizza'),
    (3, 'Community Bank', 'sponsors/community-bank-logo.png', 'https://example.com/bank'),
    (4, 'Local Bookstore', 'sponsors/bookstore-logo.png', 'https://example.com/books'),
    (5, 'Hardware Store', 'sponsors/hardware-logo.png', 'https://example.com/hardware');

SET IDENTITY_INSERT [Sponsors] OFF;

-- =====================================================
-- 5. SAMPLE EVENTS
-- =====================================================
SET IDENTITY_INSERT [Events] ON;

-- School Closed Days for 2025-2026
INSERT INTO [Events] ([Id], [Title], [Date], [Description], [Location], [ImageUrl], [Link], [EventCoordinatorId], [Status], [EventStartTime], [EventEndTime], [SetupStartTime], [CleanupEndTime], [MaxAttendees], [EstimatedAttendees], [RequiresVolunteers], [RequiresSetup], [RequiresCleanup], [Notes], [PublicInstructions], [WeatherBackupPlan], [ExcelImportId], [SchoolYearId], [EventTypeId], [EventSubTypeId])
VALUES 
    -- Federal Holidays 2025-2026
    (1, 'Labor Day Holiday', '2025-09-01', 'School closed for Labor Day', 'No School', '', '', NULL, 2, '2025-09-01 00:00:00', '2025-09-01 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School is closed today.', '', NULL, 3, 1, 1),
    (2, 'Thanksgiving Break', '2025-11-27', 'Thanksgiving holiday - school closed', 'No School', '', '', NULL, 2, '2025-11-27 00:00:00', '2025-11-28 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School is closed for Thanksgiving break.', '', NULL, 3, 1, 1),
    (3, 'Winter Break Begins', '2025-12-22', 'First day of Winter Break', 'No School', '', '', NULL, 2, '2025-12-22 00:00:00', '2026-01-02 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'Enjoy your winter break!', '', NULL, 3, 1, 1),
    (4, 'Martin Luther King Jr. Day', '2026-01-19', 'MLK Day - school closed', 'No School', '', '', NULL, 2, '2026-01-19 00:00:00', '2026-01-19 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School closed in observance of MLK Day.', '', NULL, 3, 1, 1),
    (5, 'Presidents Day', '2026-02-16', 'Presidents Day - school closed', 'No School', '', '', NULL, 2, '2026-02-16 00:00:00', '2026-02-16 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School closed for Presidents Day.', '', NULL, 3, 1, 1),
    (6, 'Memorial Day', '2026-05-25', 'Memorial Day - school closed', 'No School', '', '', NULL, 2, '2026-05-25 00:00:00', '2026-05-25 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School closed for Memorial Day.', '', NULL, 3, 1, 1),
    
    -- Staff Development Days
    (7, 'Staff Development Day', '2025-10-14', 'Professional development for teachers - no school for students', 'No School', '', '', NULL, 2, '2025-10-14 00:00:00', '2025-10-14 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'No school for students today.', '', NULL, 3, 1, 2),
    (8, 'Staff Development Day', '2026-02-13', 'Professional development for teachers - no school for students', 'No School', '', '', NULL, 2, '2026-02-13 00:00:00', '2026-02-13 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'No school for students today.', '', NULL, 3, 1, 2),
    
    -- PTA Events
    (9, 'PTA Board Election 2025-2026', '2025-08-20', 'Elect new PTA board members for the school year', 'School Cafeteria', '', '', NULL, 3, '2025-08-20 19:00:00', '2025-08-20 20:30:00', '2025-08-20 18:30:00', '2025-08-20 21:00:00', 100, 50, 1, 1, 1, 'Need volunteers for setup and registration table', 'Join us to vote for next year''s PTA board!', 'Will be held in the library if cafeteria unavailable', NULL, 3, 2, NULL),
    (10, 'Back to School Night', '2025-08-25', 'Meet teachers, learn about the school year, and connect with other families', 'Throughout School', '', '', NULL, 2, '2025-08-25 18:00:00', '2025-08-25 20:00:00', '2025-08-25 17:30:00', '2025-08-25 20:30:00', 500, 400, 1, 1, 1, '', 'Informal event to welcome families back', '', NULL, 3, 2, NULL),
    
    -- Fundraising Events
    (11, 'Fall Festival 2025', '2025-10-25', 'Annual fall festival with games, food, and fun for the whole family', 'School Campus', '', '', NULL, 0, '2025-10-25 10:00:00', '2025-10-25 14:00:00', '2025-10-25 08:00:00', '2025-10-25 16:00:00', 500, 300, 1, 1, 1, 'Major fundraising event - need many volunteers', 'Bring the whole family for a day of fun!', 'Will move to gym and cafeteria if weather is bad', NULL, 3, 3, 7),
    (12, 'Bingo Night - November', '2025-11-15', 'Family bingo night with great prizes and concessions', 'School Cafeteria', '', '', NULL, 0, '2025-11-15 19:00:00', '2025-11-15 21:00:00', '2025-11-15 18:00:00', '2025-11-15 21:30:00', 150, 100, 1, 1, 1, 'Popular recurring event', 'Doors open at 6:30 PM, games start at 7:00 PM', '', NULL, 3, 3, 6),
    (13, 'Book Fair - Spring', '2026-04-20', 'Scholastic Book Fair supporting literacy and school library', 'School Library', '', '', NULL, 0, '2026-04-20 08:00:00', '2026-04-24 15:00:00', '2026-04-19 15:00:00', '2026-04-24 16:00:00', NULL, 200, 1, 1, 1, 'Week-long event', 'Shop for books all week!', '', NULL, 3, 3, 5),
    
    -- Family Fun Events
    (14, 'Winter Holiday Party', '2025-12-15', 'Celebrate the holidays with crafts, treats, and festive fun', 'School Cafeteria', '', '', NULL, 0, '2025-12-15 15:30:00', '2025-12-15 17:00:00', '2025-12-15 15:00:00', '2025-12-15 17:30:00', 200, 150, 1, 1, 1, '', 'Bring your holiday spirit!', '', NULL, 3, 4, 8),
    (15, 'Outdoor Movie Night', '2026-05-15', 'Watch a family-friendly movie under the stars', 'School Field', '', '', NULL, 0, '2026-05-15 19:00:00', '2026-05-15 21:30:00', '2026-05-15 18:00:00', '2026-05-15 22:00:00', 300, 200, 1, 1, 1, 'Weather dependent', 'Bring blankets and snacks!', 'Will postpone to following Friday if rain', NULL, 3, 4, 10),
    
    -- Spirit Nights
    (16, 'Chuck E. Cheese Spirit Night', '2025-11-08', 'Dine at Chuck E. Cheese and 20% of sales benefit Luxford Elementary', 'Chuck E. Cheese - Town Center', '', 'https://www.chuckecheese.com/', NULL, 2, '2025-11-08 16:00:00', '2025-11-08 21:00:00', NULL, NULL, NULL, 50, 0, 0, 0, '', 'Mention Luxford Elementary when ordering', '', NULL, 3, 7, NULL),
    (17, 'Pizza Night Spirit Event', '2026-03-12', 'Support our school while enjoying dinner out', 'Local Pizza Company', '', '', NULL, 1, '2026-03-12 17:00:00', '2026-03-12 20:00:00', NULL, NULL, NULL, 75, 0, 0, 0, '', 'Show the flyer for discount', '', NULL, 3, 7, NULL);

SET IDENTITY_INSERT [Events] OFF;

-- =====================================================
-- 6. EVENT SPONSOR RELATIONSHIPS (Sample)
-- =====================================================
-- Main sponsors for major events
INSERT INTO [EventMainSponsors] ([EventId], [SponsorId])
VALUES 
    (11, 3), -- Fall Festival sponsored by Community Bank
    (13, 4), -- Book Fair sponsored by Local Bookstore
    (16, 1); -- Chuck E. Cheese sponsors their own spirit night

-- Other sponsors for events
INSERT INTO [EventOtherSponsors] ([EventId], [SponsorId])
VALUES 
    (11, 5), -- Hardware Store also supports Fall Festival
    (12, 2), -- Local Pizza Company supports Bingo Night
    (15, 3); -- Community Bank supports Movie Night

-- =====================================================
-- 7. VERIFICATION QUERIES
-- =====================================================
-- Use these to verify data was inserted correctly

-- Check School Years
SELECT 'School Years' as TableName, COUNT(*) as RecordCount FROM [SchoolYears];

-- Check Event Types and SubTypes
SELECT 'Event Types' as TableName, COUNT(*) as RecordCount FROM [EventTypes];
SELECT 'Event SubTypes' as TableName, COUNT(*) as RecordCount FROM [EventSubTypes];

-- Check Events by Type
SELECT 
    et.Name as EventType,
    COUNT(e.Id) as EventCount
FROM [EventTypes] et
LEFT JOIN [Events] e ON et.Id = e.EventTypeId
GROUP BY et.Name, et.DisplayOrder
ORDER BY et.DisplayOrder;

-- Check Events by Status
SELECT 
    CASE e.Status 
        WHEN 0 THEN 'Planning'
        WHEN 1 THEN 'SubmittedForApproval' 
        WHEN 2 THEN 'Active'
        WHEN 3 THEN 'InProgress'
        WHEN 4 THEN 'WrapUp'
        WHEN 5 THEN 'Completed'
        WHEN 6 THEN 'Cancelled'
        ELSE 'Unknown'
    END as Status,
    COUNT(*) as Count
FROM [Events] e
GROUP BY e.Status
ORDER BY e.Status;

-- Check Sponsors
SELECT 'Sponsors' as TableName, COUNT(*) as RecordCount FROM [Sponsors];
SELECT 'Main Sponsorships' as TableName, COUNT(*) as RecordCount FROM [EventMainSponsors];
SELECT 'Other Sponsorships' as TableName, COUNT(*) as RecordCount FROM [EventOtherSponsors];

-- =====================================================
-- 8. ADMIN USER SETUP (Optional)
-- =====================================================
-- Note: You'll need to add admin users through the application's 
-- registration process or through ASP.NET Identity tools
-- This is just a reminder of what you'll need to do:

/*
TODO: After running this script, remember to:
1. Register admin users through the application
2. Assign "Admin" or "BoardMember" roles to appropriate users
3. Update EventCoordinatorId values in Events table once you have users
4. Add actual sponsor logos to the wwwroot/sponsors/ folder
5. Update file paths for PrintableEventCalendar in SchoolYears
6. Test the application with this seed data
7. Customize event descriptions, dates, and details as needed
*/

-- =====================================================
-- END OF SEED DATA SCRIPT
-- =====================================================

PRINT 'Luxford PTA Event Management System seed data has been successfully inserted!';
PRINT 'Remember to:';
PRINT '- Set up admin users through the application';
PRINT '- Assign coordinator roles to events';
PRINT '- Add sponsor logo files to wwwroot/sponsors/';
PRINT '- Update calendar PDF/image paths as needed';-- =====================================================
-- Luxford PTA Event Management System - Initial Seed Data
-- Created: September 15, 2025
-- Purpose: Initial setup for production deployment
-- =====================================================

-- Note: Run this script manually after deploying the database schema
-- This is NOT part of Entity Framework migrations

USE [LuxfordPTAWeb] -- Replace with your actual database name
GO

-- =====================================================
-- 1. SCHOOL YEARS
-- =====================================================
SET IDENTITY_INSERT [SchoolYears] ON;

INSERT INTO [SchoolYears] ([Id], [Name], [StartDate], [EndDate], [PrintableEventCalendar])
VALUES 
    (1, '2023-2024', '2023-07-01', '2024-06-30', 'images/eventcal-2023-2024.pdf'),
    (2, '2024-2025', '2024-07-01', '2025-06-30', 'images/eventcal-2024-2025.pdf'),
    (3, '2025-2026', '2025-07-01', '2026-06-30', 'images/eventcal-2025-2026.jpg');

SET IDENTITY_INSERT [SchoolYears] OFF;

-- =====================================================
-- 2. EVENT TYPES
-- =====================================================
SET IDENTITY_INSERT [EventTypes] ON;

INSERT INTO [EventTypes] ([Id], [Name], [Slug], [Description], [DisplayOrder], [IsActive], [Size], [Icon], [ColorClass], [DisplayMode], [MaxEventsToShow], [ShowViewEventsButton], [ShowInlineOnMainPage])
VALUES 
    -- School Closed & Special Days
    (1, 'School Closed & Special Days', 'school-closed-days', 'All holidays, staff days, and special schedule days for the school year.', 0, 1, 1, 'bi-calendar-x', 'text-danger', 3, 0, 1, 1),
    
    -- PTA Events & Meetings
    (2, 'PTA Events & Meetings', 'pta-events', 'PTA meetings, board elections, volunteer opportunities, and general PTA business.', 1, 1, 0, 'bi-people-fill', 'text-primary', 0, 0, 1, 0),
    
    -- Fundraising Events
    (3, 'Fundraising Events', 'fundraising', 'Events that raise money for school programs, supplies, and activities.', 2, 1, 1, 'bi-cash-coin', 'text-success', 0, 0, 1, 0),
    
    -- Family Fun & Community Events
    (4, 'Family Fun & Community', 'family-events', 'Events designed to bring families together and build school community.', 3, 1, 1, 'bi-heart-fill', 'text-warning', 0, 0, 1, 0),
    
    -- Educational Programs
    (5, 'Educational Programs', 'educational', 'Special educational programs, guest speakers, and learning opportunities.', 4, 1, 0, 'bi-mortarboard-fill', 'text-info', 0, 0, 1, 0),
    
    -- Volunteer Opportunities
    (6, 'Volunteer Opportunities', 'volunteer', 'Ongoing volunteer needs and one-time volunteer events.', 5, 1, 0, 'bi-hand-thumbs-up', 'text-secondary', 0, 0, 1, 0),
    
    -- Spirit Nights & Restaurant Partnerships
    (7, 'Spirit Nights', 'spirit-nights', 'Restaurant partnerships where a percentage of sales supports our school.', 6, 1, 0, 'bi-shop', 'text-primary', 0, 0, 1, 0);

SET IDENTITY_INSERT [EventTypes] OFF;

-- =====================================================
-- 3. EVENT SUB-TYPES
-- =====================================================
SET IDENTITY_INSERT [EventSubTypes] ON;

INSERT INTO [EventSubTypes] ([Id], [Name], [Slug], [Description], [DisplayOrder], [IsActive], [Icon], [ColorClass], [EventTypeId])
VALUES 
    -- School Closed & Special Days Sub-types
    (1, 'Federal Holiday', 'federal-holiday', 'Federal holidays when school is closed', 0, 1, 'bi-flag', 'text-danger', 1),
    (2, 'Staff Development Day', 'staff-development', 'Professional development days - school closed for students', 1, 1, 'bi-mortarboard', 'text-warning', 1),
    (3, 'Early Dismissal', 'early-dismissal', 'Days with early dismissal schedule', 2, 1, 'bi-clock', 'text-info', 1),
    (4, 'Weather Closure', 'weather-closure', 'School closures due to weather conditions', 3, 1, 'bi-cloud-snow', 'text-primary', 1),
    
    -- Fundraising Events Sub-types
    (5, 'Product Sales', 'product-sales', 'Sales of products like books, wrapping paper, etc.', 0, 1, 'bi-bag', 'text-success', 3),
    (6, 'Entertainment Events', 'entertainment', 'Fun events that raise money - bingo, talent shows, etc.', 1, 1, 'bi-music-note', 'text-success', 3),
    (7, 'Community Events', 'community-fundraising', 'Large community events like fall festivals', 2, 1, 'bi-people', 'text-success', 3),
    
    -- Family Fun Sub-types
    (8, 'Seasonal Celebrations', 'seasonal', 'Holiday parties and seasonal events', 0, 1, 'bi-calendar-heart', 'text-warning', 4),
    (9, 'Game Nights', 'game-nights', 'Bingo, game nights, and interactive family events', 1, 1, 'bi-dice-5', 'text-warning', 4),
    (10, 'Outdoor Events', 'outdoor', 'Picnics, outdoor movie nights, field day', 2, 1, 'bi-tree', 'text-warning', 4),
    
    -- Educational Programs Sub-types
    (11, 'Parent Workshops', 'parent-workshops', 'Educational sessions for parents', 0, 1, 'bi-person-workspace', 'text-info', 5),
    (12, 'Student Programs', 'student-programs', 'Special programs and assemblies for students', 1, 1, 'bi-person-raised-hand', 'text-info', 5),
    (13, 'Guest Speakers', 'guest-speakers', 'Special speakers and presentations', 2, 1, 'bi-mic', 'text-info', 5);

SET IDENTITY_INSERT [EventSubTypes] OFF;

-- =====================================================
-- 4. SPONSORS (Sample Data)
-- =====================================================
SET IDENTITY_INSERT [Sponsors] ON;

INSERT INTO [Sponsors] ([Id], [Name], [LogoUrl], [WebsiteUrl])
VALUES 
    (1, 'Chuck E. Cheese''s', 'sponsors/chuck-e-cheeses-car-design-png-logo-8.png', 'https://www.chuckecheese.com/'),
    (2, 'Local Pizza Company', 'sponsors/sample-sponsor-logo.png', 'https://example.com/pizza'),
    (3, 'Community Bank', 'sponsors/community-bank-logo.png', 'https://example.com/bank'),
    (4, 'Local Bookstore', 'sponsors/bookstore-logo.png', 'https://example.com/books'),
    (5, 'Hardware Store', 'sponsors/hardware-logo.png', 'https://example.com/hardware');

SET IDENTITY_INSERT [Sponsors] OFF;

-- =====================================================
-- 5. SAMPLE EVENTS
-- =====================================================
SET IDENTITY_INSERT [Events] ON;

-- School Closed Days for 2025-2026
INSERT INTO [Events] ([Id], [Title], [Date], [Description], [Location], [ImageUrl], [Link], [EventCoordinatorId], [Status], [EventStartTime], [EventEndTime], [SetupStartTime], [CleanupEndTime], [MaxAttendees], [EstimatedAttendees], [RequiresVolunteers], [RequiresSetup], [RequiresCleanup], [Notes], [PublicInstructions], [WeatherBackupPlan], [ExcelImportId], [SchoolYearId], [EventTypeId], [EventSubTypeId])
VALUES 
    -- Federal Holidays 2025-2026
    (1, 'Labor Day Holiday', '2025-09-01', 'School closed for Labor Day', 'No School', '', '', NULL, 2, '2025-09-01 00:00:00', '2025-09-01 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School is closed today.', '', NULL, 3, 1, 1),
    (2, 'Thanksgiving Break', '2025-11-27', 'Thanksgiving holiday - school closed', 'No School', '', '', NULL, 2, '2025-11-27 00:00:00', '2025-11-28 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School is closed for Thanksgiving break.', '', NULL, 3, 1, 1),
    (3, 'Winter Break Begins', '2025-12-22', 'First day of Winter Break', 'No School', '', '', NULL, 2, '2025-12-22 00:00:00', '2026-01-02 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'Enjoy your winter break!', '', NULL, 3, 1, 1),
    (4, 'Martin Luther King Jr. Day', '2026-01-19', 'MLK Day - school closed', 'No School', '', '', NULL, 2, '2026-01-19 00:00:00', '2026-01-19 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School closed in observance of MLK Day.', '', NULL, 3, 1, 1),
    (5, 'Presidents Day', '2026-02-16', 'Presidents Day - school closed', 'No School', '', '', NULL, 2, '2026-02-16 00:00:00', '2026-02-16 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School closed for Presidents Day.', '', NULL, 3, 1, 1),
    (6, 'Memorial Day', '2026-05-25', 'Memorial Day - school closed', 'No School', '', '', NULL, 2, '2026-05-25 00:00:00', '2026-05-25 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'School closed for Memorial Day.', '', NULL, 3, 1, 1),
    
    -- Staff Development Days
    (7, 'Staff Development Day', '2025-10-14', 'Professional development for teachers - no school for students', 'No School', '', '', NULL, 2, '2025-10-14 00:00:00', '2025-10-14 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'No school for students today.', '', NULL, 3, 1, 2),
    (8, 'Staff Development Day', '2026-02-13', 'Professional development for teachers - no school for students', 'No School', '', '', NULL, 2, '2026-02-13 00:00:00', '2026-02-13 23:59:59', NULL, NULL, NULL, NULL, 0, 0, 0, '', 'No school for students today.', '', NULL, 3, 1, 2),
    
    -- PTA Events
    (9, 'PTA Board Election 2025-2026', '2025-08-20', 'Elect new PTA board members for the school year', 'School Cafeteria', '', '', NULL, 3, '2025-08-20 19:00:00', '2025-08-20 20:30:00', '2025-08-20 18:30:00', '2025-08-20 21:00:00', 100, 50, 1, 1, 1, 'Need volunteers for setup and registration table', 'Join us to vote for next year''s PTA board!', 'Will be held in the library if cafeteria unavailable', NULL, 3, 2, NULL),
    (10, 'Back to School Night', '2025-08-25', 'Meet teachers, learn about the school year, and connect with other families', 'Throughout School', '', '', NULL, 2, '2025-08-25 18:00:00', '2025-08-25 20:00:00', '2025-08-25 17:30:00', '2025-08-25 20:30:00', 500, 400, 1, 1, 1, '', 'Informal event to welcome families back', '', NULL, 3, 2, NULL),
    
    -- Fundraising Events
    (11, 'Fall Festival 2025', '2025-10-25', 'Annual fall festival with games, food, and fun for the whole family', 'School Campus', '', '', NULL, 0, '2025-10-25 10:00:00', '2025-10-25 14:00:00', '2025-10-25 08:00:00', '2025-10-25 16:00:00', 500, 300, 1, 1, 1, 'Major fundraising event - need many volunteers', 'Bring the whole family for a day of fun!', 'Will move to gym and cafeteria if weather is bad', NULL, 3, 3, 7),
    (12, 'Bingo Night - November', '2025-11-15', 'Family bingo night with great prizes and concessions', 'School Cafeteria', '', '', NULL, 0, '2025-11-15 19:00:00', '2025-11-15 21:00:00', '2025-11-15 18:00:00', '2025-11-15 21:30:00', 150, 100, 1, 1, 1, 'Popular recurring event', 'Doors open at 6:30 PM, games start at 7:00 PM', '', NULL, 3, 3, 6),
    (13, 'Book Fair - Spring', '2026-04-20', 'Scholastic Book Fair supporting literacy and school library', 'School Library', '', '', NULL, 0, '2026-04-20 08:00:00', '2026-04-24 15:00:00', '2026-04-19 15:00:00', '2026-04-24 16:00:00', NULL, 200, 1, 1, 1, 'Week-long event', 'Shop for books all week!', '', NULL, 3, 3, 5),
    
    -- Family Fun Events
    (14, 'Winter Holiday Party', '2025-12-15', 'Celebrate the holidays with crafts, treats, and festive fun', 'School Cafeteria', '', '', NULL, 0, '2025-12-15 15:30:00', '2025-12-15 17:00:00', '2025-12-15 15:00:00', '2025-12-15 17:30:00', 200, 150, 1, 1, 1, '', 'Bring your holiday spirit!', '', NULL, 3, 4, 8),
    (15, 'Outdoor Movie Night', '2026-05-15', 'Watch a family-friendly movie under the stars', 'School Field', '', '', NULL, 0, '2026-05-15 19:00:00', '2026-05-15 21:30:00', '2026-05-15 18:00:00', '2026-05-15 22:00:00', 300, 200, 1, 1, 1, 'Weather dependent', 'Bring blankets and snacks!', 'Will postpone to following Friday if rain', NULL, 3, 4, 10),
    
    -- Spirit Nights
    (16, 'Chuck E. Cheese Spirit Night', '2025-11-08', 'Dine at Chuck E. Cheese and 20% of sales benefit Luxford Elementary', 'Chuck E. Cheese - Town Center', '', 'https://www.chuckecheese.com/', NULL, 2, '2025-11-08 16:00:00', '2025-11-08 21:00:00', NULL, NULL, NULL, 50, 0, 0, 0, '', 'Mention Luxford Elementary when ordering', '', NULL, 3, 7, NULL),
    (17, 'Pizza Night Spirit Event', '2026-03-12', 'Support our school while enjoying dinner out', 'Local Pizza Company', '', '', NULL, 1, '2026-03-12 17:00:00', '2026-03-12 20:00:00', NULL, NULL, NULL, 75, 0, 0, 0, '', 'Show the flyer for discount', '', NULL, 3, 7, NULL);

SET IDENTITY_INSERT [Events] OFF;

-- =====================================================
-- 6. EVENT SPONSOR RELATIONSHIPS (Sample)
-- =====================================================
-- Main sponsors for major events
INSERT INTO [EventMainSponsors] ([EventId], [SponsorId])
VALUES 
    (11, 3), -- Fall Festival sponsored by Community Bank
    (13, 4), -- Book Fair sponsored by Local Bookstore
    (16, 1); -- Chuck E. Cheese sponsors their own spirit night

-- Other sponsors for events
INSERT INTO [EventOtherSponsors] ([EventId], [SponsorId])
VALUES 
    (11, 5), -- Hardware Store also supports Fall Festival
    (12, 2), -- Local Pizza Company supports Bingo Night
    (15, 3); -- Community Bank supports Movie Night

-- =====================================================
-- 7. VERIFICATION QUERIES
-- =====================================================
-- Use these to verify data was inserted correctly

-- Check School Years
SELECT 'School Years' as TableName, COUNT(*) as RecordCount FROM [SchoolYears];

-- Check Event Types and SubTypes
SELECT 'Event Types' as TableName, COUNT(*) as RecordCount FROM [EventTypes];
SELECT 'Event SubTypes' as TableName, COUNT(*) as RecordCount FROM [EventSubTypes];

-- Check Events by Type
SELECT 
    et.Name as EventType,
    COUNT(e.Id) as EventCount
FROM [EventTypes] et
LEFT JOIN [Events] e ON et.Id = e.EventTypeId
GROUP BY et.Name, et.DisplayOrder
ORDER BY et.DisplayOrder;

-- Check Events by Status
SELECT 
    CASE e.Status 
        WHEN 0 THEN 'Planning'
        WHEN 1 THEN 'SubmittedForApproval' 
        WHEN 2 THEN 'Active'
        WHEN 3 THEN 'InProgress'
        WHEN 4 THEN 'WrapUp'
        WHEN 5 THEN 'Completed'
        WHEN 6 THEN 'Cancelled'
        ELSE 'Unknown'
    END as Status,
    COUNT(*) as Count
FROM [Events] e
GROUP BY e.Status
ORDER BY e.Status;

-- Check Sponsors
SELECT 'Sponsors' as TableName, COUNT(*) as RecordCount FROM [Sponsors];
SELECT 'Main Sponsorships' as TableName, COUNT(*) as RecordCount FROM [EventMainSponsors];
SELECT 'Other Sponsorships' as TableName, COUNT(*) as RecordCount FROM [EventOtherSponsors];

-- =====================================================
-- 8. ADMIN USER SETUP (Optional)
-- =====================================================
-- Note: You'll need to add admin users through the application's 
-- registration process or through ASP.NET Identity tools
-- This is just a reminder of what you'll need to do:

/*
TODO: After running this script, remember to:
1. Register admin users through the application
2. Assign "Admin" or "BoardMember" roles to appropriate users
3. Update EventCoordinatorId values in Events table once you have users
4. Add actual sponsor logos to the wwwroot/sponsors/ folder
5. Update file paths for PrintableEventCalendar in SchoolYears
6. Test the application with this seed data
7. Customize event descriptions, dates, and details as needed
*/

-- =====================================================
-- END OF SEED DATA SCRIPT
-- =====================================================

PRINT 'Luxford PTA Event Management System seed data has been successfully inserted!';
PRINT 'Remember to:';
PRINT '- Set up admin users through the application';
PRINT '- Assign coordinator roles to events';
PRINT '- Add sponsor logo files to wwwroot/sponsors/';
PRINT '- Update calendar PDF/image paths as needed';