CREATE SCHEMA ChatBot
GO

DROP TABLE IF EXISTS ChatBot.Bookings
CREATE TABLE ChatBot.Bookings (
    booking_id INT PRIMARY KEY IDENTITY(1,1),
    customer_name VARCHAR(100),
    customer_email VARCHAR(100),
    customer_phone VARCHAR(20),
    booking_date DATE,
    booking_time TIME,
    party_size INT,
    status VARCHAR(20) DEFAULT 'confirmed',
    created_at DATETIME DEFAULT GETDATE()
);

DROP TABLE IF EXISTS ChatBot.Availability
CREATE TABLE ChatBot.Availability (
    availability_id INT PRIMARY KEY IDENTITY(1,1),
    date DATE,
    time TIME,
    max_party_size INT,
    current_booked INT DEFAULT 0
);

DROP TABLE IF EXISTS ChatBot.FAQs
CREATE TABLE ChatBot.FAQs (
    faq_id INT PRIMARY KEY IDENTITY(1,1),
    question TEXT,
    answer TEXT,
    category VARCHAR(20)
);

DROP TABLE IF EXISTS ChatBot.Menu
CREATE TABLE ChatBot.Menu (
    item_id INT PRIMARY KEY IDENTITY(1,1),
    item_name VARCHAR(100),
    description TEXT,
    price DECIMAL(6, 2),
    dietary_tags VARCHAR(100) 
);

DROP TABLE IF EXISTS ChatBot.Feedback
CREATE TABLE ChatBot.Feedback (
    feedback_id INT PRIMARY KEY IDENTITY(1,1),
    customer_name VARCHAR(100),
    customer_email VARCHAR(100),
    message TEXT,
    response TEXT,
    responded BIT DEFAULT 0,
    created_at DATETIME DEFAULT GETDATE()
);

DROP TABLE IF EXISTS ChatBot.RestaurantAttributes
CREATE TABLE ChatBot.RestaurantAttributes (
    attribute_id INT PRIMARY KEY IDENTITY(1,1),
    attribute_field VARCHAR(50),
    attribute_value TEXT
);

SELECT * FROM ChatBot.Bookings
SELECT * FROM ChatBot.Availability
SELECT * FROM ChatBot.FAQs
SELECT * FROM ChatBot.Menu
SELECT * FROM ChatBot.Feedback
SELECT * FROM ChatBot.RestaurantAttributes