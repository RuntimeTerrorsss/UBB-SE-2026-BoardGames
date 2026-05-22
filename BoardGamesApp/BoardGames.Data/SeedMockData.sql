DELETE FROM messages;
DELETE FROM conversation_participants;
DELETE FROM conversations;
DELETE FROM payments;
DELETE FROM rentals;
DELETE FROM games;
DELETE FROM users;

SET IDENTITY_INSERT users ON;
INSERT INTO users (id, username, display_name, email, password_hash, phone_number, avatar_url, is_suspended, created_at, updated_at, street, street_number, city, country, balance)
VALUES
(1, 'alice01', 'Alice', 'alice@example.com', 'hash1', '0711111111', 'https://media.istockphoto.com/id/1437816897/photo/business-woman-manager-or-human-resources-portrait-for-career-success-company-we-are-hiring.jpg?s=612x612&w=0&k=20&c=tyLvtzutRh22j9GqSGI33Z4HpIwv9vL_MZw_xOE19NQ=', 0, GETDATE(), GETDATE(), 'Main Street', '10', 'Cluj-Napoca', 'Romania', 150.00),
(2, 'bob02', 'Bob', 'bob@example.com', 'hash2', '0722222222', 'https://img.freepik.com/free-photo/portrait-white-man-isolated_53876-40306.jpg?semt=ais_hybrid&w=740&q=80', 0, GETDATE(), GETDATE(), 'Liberty Street', '21', 'Constanta', 'Romania', 75.50),
(3, 'carol03', 'Carol', 'carol@example.com', 'hash3', '0733333333', 'https://st4.depositphotos.com/4157265/40368/i/450/depositphotos_403682066-stock-photo-profile-picture-of-smiling-caucasian.jpg', 0, GETDATE(), GETDATE(), 'Oak Street', '5', 'Oradea', 'Romania', 200.00),
(4, 'david04', 'David', 'david@example.com', 'hash4', '0744444444', 'https://t3.ftcdn.net/jpg/06/99/46/60/360_F_699466075_DaPTBNlNQTOwwjkOiFEoOvzDV0ByXR9E.jpg', 0, GETDATE(), GETDATE(), 'River Street', '12', 'Suceava', 'Romania', 50.00),
(5, 'emma05', 'Emma', 'emma@example.com', 'hash5', '0755555555', 'https://media.istockphoto.com/id/1471845315/photo/happy-portrait-or-business-woman-taking-a-selfie-in-office-building-for-a-social-media.jpg?s=612x612&w=0&k=20&c=AOylBL01joI0zphCAFr6YVrsOgp_jd2XtVUychLXYho=', 0, GETDATE(), GETDATE(), 'Forest Street', '7', 'Bucharest', 'Romania', 320.00),
(6, 'frank06', 'Frank', 'frank@example.com', 'hash6', '0766666666', 'https://st2.depositphotos.com/4157265/43642/i/450/depositphotos_436429026-stock-photo-happy-young-man-posing-for.jpg', 0, GETDATE(), GETDATE(), 'Sunset Blvd', '45', 'Timisoara', 'Romania', 10.00),
(7, 'grace07', 'Grace', 'grace@example.com', 'hash7', '0777777777', 'https://i.pinimg.com/736x/97/30/df/9730dfdfd89550c365aaacc9760815b1.jpg', 0, GETDATE(), GETDATE(), 'Hill Road', '3', 'Iasi', 'Romania', 500.00),
(8, 'henry08', 'Henry', 'henry@example.com', 'hash8', '0788888888', 'https://files.idyllic.app/files/static/3929121?width=256&optimizer=image', 0, GETDATE(), GETDATE(), 'Lake Street', '19', 'Brasov', 'Romania', 0.00);
SET IDENTITY_INSERT users OFF;

SET IDENTITY_INSERT games ON;
INSERT INTO games (id, name, price, minimum_player_number, maximum_player_number, description, is_active, owner_id) VALUES
(1, 'Catan', 15.00, 3, 4, 'Trade and build on the island of Catan.', 1, 1),
(2, 'Monopoly', 10.00, 2, 6, 'Classic property trading game.', 1, 3), 
(3, 'Carcassonne', 12.50, 2, 5, 'Tile placement game.', 1, 1),
(4, 'Terraforming Mars', 20.00, 1, 5, 'Strategy game about developing Mars.', 0, 3),
(5, 'Ticket to Ride', 13.50, 2, 5, 'Build railway routes across the world.', 1, 1),
(6, 'Pandemic', 14.00, 2, 4, 'Work together to stop global outbreaks.', 1, 2),
(7, '7 Wonders', 16.00, 2, 7, 'Build a civilization and wonders.', 1, 3),
(8, 'Azul', 11.00, 2, 4, 'Decorate the royal palace walls.', 1, 1),
(9, 'Dixit', 10.50, 3, 6, 'Creative storytelling game.', 1, 2),
(10, 'Splendor', 12.00, 2, 4, 'Build your gem empire.', 1, 8),
(11, 'Codenames', 9.00, 2, 8, 'Team word guessing game.', 1, 6),
(12, 'Risk', 11.50, 2, 6, 'Classic world domination game.', 1, 5),
(13, 'Dominion', 13.00, 2, 4, 'Deck-building strategy game.', 1, 7),
(14, 'Love Letter', 7.50, 2, 4, 'Quick deduction card game.', 1, 8),
(15, 'Scythe', 22.00, 1, 5, 'Strategy game in alternate history.', 1, 2),
(16, 'Wingspan', 18.00, 1, 5, 'Build a bird sanctuary.', 1, 3),
(17, 'Gloomhaven', 25.00, 1, 4, 'Epic campaign dungeon crawler.', 1, 1),
(18, 'Brass Birmingham', 21.00, 2, 4, 'Industrial revolution strategy.', 1, 8),
(19, 'Root', 17.50, 2, 4, 'Asymmetric woodland warfare.', 1, 8),
(20, 'Terraforming Mars: Ares', 19.00, 1, 4, 'Faster Mars engine builder.', 1, 7),
(21, 'Ark Nova', 23.00, 1, 4, 'Build the best zoo.', 1, 6),
(22, 'Everdell', 16.50, 1, 4, 'Build a forest civilization.', 1, 6),
(23, 'The Crew', 9.50, 2, 5, 'Cooperative trick-taking game.', 1, 4),
(24, 'Hanabi', 8.00, 2, 5, 'Play cards without seeing them.', 1, 4),
(25, 'Agricola', 17.00, 1, 4, 'Farm-building strategy game.', 1, 4),
(26, 'Patchwork', 10.00, 2, 2, 'Two-player quilt game.', 1, 5),
(27, 'Carcassonne: Expansion', 13.50, 2, 6, 'Expand the classic Carcassonne.', 1, 6),
(28, 'Uno', 5.00, 2, 6, 'Classic card shedding game.', 1, 3),
(29, 'Exploding Kittens', 8.50, 2, 5, 'Explosive card game.', 1, 1),
(30, 'Bang!', 9.00, 4, 7, 'Wild west bluffing game.', 1, 2);
SET IDENTITY_INSERT games OFF;

SET IDENTITY_INSERT rentals ON;
INSERT INTO rentals (id, game_id, client_id, owner_id, start_date, end_date, total_price) VALUES
(1, 1, 2, 1, '2026-05-10T00:00:00', '2026-05-15T00:00:00', 75.00),
(2, 2, 1, 3, '2026-05-12T00:00:00', '2026-05-14T00:00:00', 20.00), 
(3, 1, 4, 1, '2026-04-01T00:00:00', '2026-04-05T00:00:00', 20.00),
(4, 5, 5, 1, '2026-05-01T00:00:00', '2026-05-10T00:00:00', 135.00), 
(5, 7, 6, 3, '2026-04-15T00:00:00', '2026-04-18T00:00:00', 48.00), 
(6, 12, 7, 5, '2026-05-01T00:00:00', '2026-05-07T00:00:00', 69.00), 
(7, 23, 2, 4, '2026-05-15T00:00:00', '2026-05-17T00:00:00', 19.00);
SET IDENTITY_INSERT rentals OFF;

SET IDENTITY_INSERT payments ON;
INSERT INTO payments (id, request_id, client_id, owner_id, paid_amount, payment_method, date_of_transaction, date_confirmed_buyer, date_confirmed_seller, payment_state, PaymentCategory) VALUES
(1, 1, 2, 1, 75.00, 'CARD', '2026-05-01 10:00:00', '2026-05-01 10:00:00', NULL, 1, 'Standard'),
(2, 2, 1, 3, 20.00, 'CASH', '2026-05-10 14:30:00', NULL, NULL, 1, 'Standard'), 
(3, 3, 4, 1, 20.00, 'CARD', '2026-03-25 09:00:00', '2026-03-25 09:00:00', NULL, 0, 'Standard'),
(4, 4, 5, 1, 135.00, 'CASH', '2026-04-25 08:00:00', NULL, NULL, 1, 'Standard'),
(5, 5, 6, 3, 48.00, 'CARD', '2026-04-10 11:00:00', '2026-04-10 11:00:00', NULL, 1, 'Standard'), 
(6, 6, 7, 5, 69.00, 'CASH', '2026-04-25 16:00:00', NULL, NULL, 0, 'Standard'),
(7, 7, 2, 4, 19.00, 'CARD', '2026-05-10 10:00:00', '2026-05-10 10:00:00', '2026-05-10 10:00:00', 1, 'Standard'); 
SET IDENTITY_INSERT payments OFF;


SET IDENTITY_INSERT conversations ON;
INSERT INTO conversations (id) VALUES (1), (2), (3), (4), (5), (6), (7);
SET IDENTITY_INSERT conversations OFF;

INSERT INTO conversation_participants (conversation_id, user_id, last_message_read_time, unread_messages_count) VALUES
(1, 1, '2026-04-01 12:00:00', 0), (1, 2, '2026-04-01 11:45:00', 0),
(2, 3, '2026-05-05 11:00:00', 0), (2, 1, '2026-05-05 11:00:00', 0), 
(3, 1, '2026-03-20 10:00:00', 0), (3, 4, '2026-03-20 09:55:00', 0),
(4, 1, '2026-04-20 09:00:00', 0), (4, 5, '2026-04-20 09:00:00', 0), 
(5, 3, '2026-04-01 12:00:00', 0), (5, 6, '2026-04-01 12:00:00', 0), 
(6, 5, '2026-04-15 17:00:00', 0), (6, 7, '2026-04-15 17:00:00', 0), 
(7, 4, '2026-05-08 10:00:00', 0), (7, 2, '2026-05-08 10:00:00', 0); 


SET IDENTITY_INSERT messages ON;
INSERT INTO messages (id, conversation_id, message_sender_id, message_receiver_id, message_sent_time, message_content_as_string, MessageCategory, text_message_content, rental_request_id, is_request_resolved, is_request_accepted, request_content, message_image_url) VALUES

(1, 1, 2, 1, '2026-04-01 09:00:00', 'Hey, is Catan available May 10-15?', 'RentalRequest', NULL, 1, 1, 1, 'Hey, is Catan available May 10-15?', NULL),
(2, 1, 1, 2, '2026-04-01 09:05:00', 'Yes, it''s free — it''s all yours!', 'Text', 'Yes, it''s free — it''s all yours!', NULL, NULL, NULL, NULL, NULL),
(3, 1, 2, 1, '2026-04-01 09:08:00', 'hamster.jpg', 'Image', NULL, NULL, NULL, NULL, NULL, 'hamster.jpg'),
(4, 1, 2, 1, '2026-04-01 09:10:00', 'Perfect, thanks a lot!', 'Text', 'Perfect, thanks a lot!', NULL, NULL, NULL, NULL, NULL),

(5, 2, 1, 3, '2026-05-05 10:00:00', 'Can I borrow Monopoly May 12-14?', 'RentalRequest', NULL, 2, 1, 1, 'Can I borrow Monopoly May 12-14?', NULL),
(6, 2, 3, 1, '2026-05-05 10:10:00', 'Sure, I can bring it over Monday.', 'Text', 'Sure, I can bring it over Monday.', NULL, NULL, NULL, NULL, NULL),
(7, 2, 1, 3, '2026-05-05 10:15:00', 'Great, see you then!', 'Text', 'Great, see you then!', NULL, NULL, NULL, NULL, NULL),

(8, 3, 4, 1, '2026-03-20 09:00:00', 'Hi, is Catan free from the 1st of April?', 'RentalRequest', NULL, 3, 1, 1, 'Hi, is Catan free from the 1st of April?', NULL),
(9, 3, 1, 4, '2026-03-20 09:05:00', 'Of course, come pick it up anytime.', 'Text', 'Of course, come pick it up anytime.', NULL, NULL, NULL, NULL, NULL),
(10, 3, 4, 1, '2026-03-20 09:08:00', 'hamster.jpg', 'Image', NULL, NULL, NULL, NULL, NULL, 'hamster.jpg'),
(11, 3, 1, 4, '2026-03-20 09:12:00', 'Will be there Tuesday morning!', 'Text', 'Will be there Tuesday morning!', NULL, NULL, NULL, NULL, NULL),

(12, 4, 5, 1, '2026-04-20 08:00:00', 'Would love to rent Ticket to Ride.', 'RentalRequest', NULL, 4, 1, 1, 'Would love to rent Ticket to Ride.', NULL), 
(13, 4, 1, 5, '2026-04-20 08:10:00', 'Sure, it''s available. Want to meet Saturday?', 'Text', 'Sure, it''s available. Want to meet Saturday?', NULL, NULL, NULL, NULL, NULL),
(14, 4, 5, 1, '2026-04-20 08:20:00', 'Saturday works perfectly for me.', 'Text', 'Saturday works perfectly for me.', NULL, NULL, NULL, NULL, NULL),

(15, 5, 6, 3, '2026-04-01 11:00:00', 'Is 7 Wonders available?', 'RentalRequest', NULL, 5, 1, 1, 'Is 7 Wonders available?', NULL),  
(16, 5, 3, 6, '2026-04-01 11:05:00', 'Yep, I''ll have it ready by Tuesday.', 'Text', 'Yep, I''ll have it ready by Tuesday.', NULL, NULL, NULL, NULL, NULL),
(17, 5, 6, 3, '2026-04-01 11:10:00', 'hamster.jpg', 'Image', NULL, NULL, NULL, NULL, NULL, 'hamster.jpg'),

(18, 6, 7, 5, '2026-04-15 16:00:00', 'Can I get Risk from May 1st to 7th?', 'RentalRequest', NULL, 6, 1, 1, 'Can I get Risk from May 1st to 7th?', NULL), 
(19, 6, 5, 7, '2026-04-15 16:10:00', 'Sounds good, just message me before you come.', 'Text', 'Sounds good, just message me before you come.', NULL, NULL, NULL, NULL, NULL),
(20, 6, 7, 5, '2026-04-15 16:20:00', 'Will do, cheers!', 'Text', 'Will do, cheers!', NULL, NULL, NULL, NULL, NULL),

(21, 7, 2, 4, '2026-05-08 09:00:00', 'Is The Crew free?', 'RentalRequest', NULL, 7, 1, 1, 'Is The Crew free?', NULL),  
(22, 7, 4, 2, '2026-05-08 09:10:00', 'Yes, grab it.', 'Text', 'Yes, grab it.', NULL, NULL, NULL, NULL, NULL);

SET IDENTITY_INSERT messages OFF;

SELECT * FROM users;
SELECT * FROM games;
SELECT * FROM rentals;
SELECT * FROM payments;
SELECT * FROM conversations;
SELECT * FROM messages;