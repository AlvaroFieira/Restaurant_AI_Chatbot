-- Insert sample data into RestaurantDetails
INSERT INTO ChatBot.RestaurantAttributes (attribute_field, attribute_value) VALUES
('name', 'The Magic Cauldron'),
('address', '9¾ Diagon Alley, London'),
('phone_number', '+44 207 123 4567'),
('email', 'contact@magiccauldron.com'),
('description', 'A magical restaurant with themed decor and a wizarding ambiance. Perfect for fans of magic and mystical experiences.'),
('vibe_description', 'Expect floating candles, mystical fog, and music straight out of a wizard’s playlist.');

-- Insert sample data into FAQs
INSERT INTO ChatBot.FAQs (question, answer, category) VALUES
('Do you serve Butterbeer?', 'Yes! Our Butterbeer is brewed with hints of butterscotch and served both warm and cold.', 'menu'),
('Where are you located?', 'We are right next to Platform 9¾, hidden behind a mystical brick wall in Diagon Alley.', 'location'),
('Is this restaurant family-friendly?', 'Absolutely! We welcome little wizards, witches, and even muggles.', 'vibe'),
('Do you have vegan options?', 'Yes, we have vegan options like "Darth Vader’s Blackened Veggie Burger" and the "Sorcerer’s Salad".', 'menu');

-- Insert sample data into Menu
INSERT INTO ChatBot.Menu (item_name, description, price, dietary_tags) VALUES
('Yoda’s Swamp Stew', 'A hearty vegetable stew with mysterious flavors from a galaxy far, far away.', 12.99, 'vegetarian, gluten-free'),
('Elsa’s Frozen Delight', 'A refreshing ice cream sundae with shimmering snowflakes and a touch of mint.', 8.50, 'dairy-free'),
('Sorting Hat Soup', 'A mysterious soup that reveals your Hogwarts house! Comes with house-colored croutons.', 9.75, 'vegetarian'),
('Chewbacca’s Roasted Beast', 'A large, smoky roast fit for a Wookiee. Comes with side veggies and a growl of approval.', 19.99, ''),
('Mickey’s Cheese Delight', 'Classic grilled cheese with an extra helping of gooey goodness, shaped like Mickey’s ears.', 7.99, 'nut-free'),
('Maleficent’s Spicy Dragon Wings', 'Hot and spicy chicken wings with a hint of fire-breathing dragon magic.', 11.50, 'gluten-free'),
('Dobby’s Delight', 'Freshly baked bread rolls, served with a magical dipping sauce. Free for all house-elves.', 4.50, 'vegan, nut-free');

-- Insert sample data into Availability
INSERT INTO ChatBot.Availability (date, time, max_party_size, current_booked, is_available) VALUES
('2024-11-04', '18:00:00', 20, 5),
('2024-11-04', '20:00:00', 20, 15),
('2024-11-05', '18:00:00', 20, 0),
('2024-11-05', '20:00:00', 20, 0),
('2024-11-06', '18:00:00', 20, 2),
('2024-11-06', '20:00:00', 20, 0);

-- Insert sample data into Feedback
INSERT INTO ChatBot.Feedback (customer_name, customer_email, message, responded) VALUES
('Luke Skywalker', 'luke@jediacademy.com', 'The Yoda’s Swamp Stew was amazing, but I think it could use more green.', 0),
('Hermione Granger', 'hermione@hogwarts.edu', 'Loved the Butterbeer! It tasted just like the Three Broomsticks!', 0),
('Buzz Lightyear', 'buzz@starcommand.com', 'I think Chewbacca’s Roasted Beast took me to infinity and beyond!', 1),
('Elsa of Arendelle', 'elsa@arendelle.com', 'Loved Elsa’s Frozen Delight! Very authentic, but maybe add a little extra mint?', 1);

-- Insert sample data into Bookings
INSERT INTO ChatBot.Bookings (customer_name, customer_email, customer_phone, booking_date, booking_time, party_size, status) VALUES
('Harry Potter', 'harry@hogwarts.edu', '+44 7700 900000', '2024-11-01', '18:00:00', 4, 'confirmed'),
('Leia Organa', 'leia@rebellion.org', '+44 7700 123456', '2024-11-01', '20:00:00', 2, 'confirmed'),
('Elsa', 'elsa@arendelle.com', '+47 5555 5555', '2024-11-02', '18:00:00', 3, 'confirmed'),
('Simba', 'simba@pridelands.co', '+254 700 123456', '2024-11-03', '20:00:00', 5, 'confirmed');

