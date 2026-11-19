-- Gift Tracker – database script (ER design "without preferences", 7 tables: the 6 from the
-- diagram + occasion_invite, added so occasions are only seen by their creator and invited friends)
-- Diagram: docs/er-design-without-preferences.svg
-- Run this in pgAdmin's Query Tool on the app-gifttrackerdb database.
-- Built in the same order as the class script: drop -> create tables + sequences ->
-- id defaults -> test data -> setval -> primary keys -> unique keys -> foreign keys -> checks

DROP TABLE IF EXISTS public.occasion_invite CASCADE;
DROP TABLE IF EXISTS public.group_member CASCADE;
DROP TABLE IF EXISTS public.gift_group CASCADE;
DROP TABLE IF EXISTS public.gift CASCADE;
DROP TABLE IF EXISTS public.occasion CASCADE;
DROP TABLE IF EXISTS public.connection CASCADE;
DROP TABLE IF EXISTS public.profile CASCADE;

DROP SEQUENCE IF EXISTS public.occasion_invite_id_seq;
DROP SEQUENCE IF EXISTS public.group_member_id_seq;
DROP SEQUENCE IF EXISTS public.gift_group_id_seq;
DROP SEQUENCE IF EXISTS public.gift_id_seq;
DROP SEQUENCE IF EXISTS public.occasion_id_seq;
DROP SEQUENCE IF EXISTS public.connection_id_seq;
DROP SEQUENCE IF EXISTS public.profile_id_seq;

-- Tables from the old 13-table design (Option C) that no longer exist.
-- Dropped so a database that ran the old script ends up with only the new tables.
DROP TABLE IF EXISTS public.gift_contribution CASCADE;
DROP TABLE IF EXISTS public.reservation CASCADE;
DROP TABLE IF EXISTS public.wish_item CASCADE;
DROP TABLE IF EXISTS public.wishlist_share CASCADE;
DROP TABLE IF EXISTS public.wishlist CASCADE;
DROP TABLE IF EXISTS public.external_link CASCADE;
DROP TABLE IF EXISTS public.preference_item CASCADE;
DROP TABLE IF EXISTS public.preference_type CASCADE;

DROP SEQUENCE IF EXISTS public.reservation_id_seq;
DROP SEQUENCE IF EXISTS public.wish_item_id_seq;
DROP SEQUENCE IF EXISTS public.wishlist_id_seq;
DROP SEQUENCE IF EXISTS public.external_link_id_seq;
DROP SEQUENCE IF EXISTS public.preference_item_id_seq;
DROP SEQUENCE IF EXISTS public.preference_type_id_seq;

BEGIN;
-- Create the tables

-- A user of the app ("user" is a reserved word in PostgreSQL)
CREATE TABLE public.profile (
    id integer NOT NULL,
    fullname text NOT NULL,
    email text NOT NULL,
    passwordhash text NOT NULL,
    dateofbirth date NOT NULL,
    profilepictureurl text,
    createdat timestamptz NOT NULL
);

CREATE SEQUENCE public.profile_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.profile_id_seq OWNED BY public.profile.id;


-- A friend request from one profile (requester) to another (addressee)
CREATE TABLE public.connection (
    id integer NOT NULL,
    requesterid integer NOT NULL,
    addresseeid integer NOT NULL,
    status text NOT NULL,
    createdat timestamptz NOT NULL
);

CREATE SEQUENCE public.connection_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.connection_id_seq OWNED BY public.connection.id;


-- Who the occasion is for: a profile (recipientid) OR, for someone without an account, just a name
-- (recipientname). Exactly one of the two is filled – checked by chk_occasion_recipient at the bottom.
CREATE TABLE public.occasion (
    id integer NOT NULL,
    createdbyid integer NOT NULL,
    recipientid integer,
    recipientname text,
    name text NOT NULL,
    eventdate date NOT NULL,
    type text NOT NULL,
    notes text
);

CREATE SEQUENCE public.occasion_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.occasion_id_seq OWNED BY public.occasion.id;


-- The recipient is not stored here: it is the recipient of the gift's occasion (3NF)
CREATE TABLE public.gift (
    id integer NOT NULL,
    occasionid integer NOT NULL,
    buyerid integer NOT NULL,
    name text NOT NULL,
    description text,
    price numeric,
    link text,
    status text NOT NULL,
    -- How many people split the price (1 = not split). Each person's share is price / splitcount,
    -- calculated by the app and not stored, because it can be derived from these two columns (3NF).
    splitcount integer NOT NULL
);

CREATE SEQUENCE public.gift_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.gift_id_seq OWNED BY public.gift.id;


-- People splitting one gift ("group" is a reserved word in PostgreSQL).
-- A gift is a group gift when a gift_group points to it (no isgroupgift column).
CREATE TABLE public.gift_group (
    id integer NOT NULL,
    giftid integer NOT NULL,
    name text NOT NULL,
    description text,
    createdat timestamptz NOT NULL
);

CREATE SEQUENCE public.gift_group_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.gift_group_id_seq OWNED BY public.gift_group.id;


-- The group's owner is the member with role 'owner' (no ownerid column on gift_group)
CREATE TABLE public.group_member (
    id integer NOT NULL,
    groupid integer NOT NULL,
    profileid integer NOT NULL,
    role text NOT NULL,
    joinedat timestamptz NOT NULL
);

CREATE SEQUENCE public.group_member_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.group_member_id_seq OWNED BY public.group_member.id;


-- Who is invited to an occasion. You see an occasion when you created it or are invited to it –
-- and never when it is for you (the gifts are a surprise). Only the creator's friends are invited
-- (checked by the API, because it needs the connection table).
CREATE TABLE public.occasion_invite (
    id integer NOT NULL,
    occasionid integer NOT NULL,
    profileid integer NOT NULL,
    invitedat timestamptz NOT NULL
);

CREATE SEQUENCE public.occasion_invite_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.occasion_invite_id_seq OWNED BY public.occasion_invite.id;
-- End table creation
COMMIT;

BEGIN;
-- Alter tables
ALTER TABLE ONLY public.profile ALTER COLUMN id SET DEFAULT nextval('public.profile_id_seq'::regclass);

ALTER TABLE ONLY public.connection ALTER COLUMN id SET DEFAULT nextval('public.connection_id_seq'::regclass);

ALTER TABLE ONLY public.occasion ALTER COLUMN id SET DEFAULT nextval('public.occasion_id_seq'::regclass);

ALTER TABLE ONLY public.gift ALTER COLUMN id SET DEFAULT nextval('public.gift_id_seq'::regclass);

ALTER TABLE ONLY public.gift ALTER COLUMN splitcount SET DEFAULT 1;

ALTER TABLE ONLY public.gift_group ALTER COLUMN id SET DEFAULT nextval('public.gift_group_id_seq'::regclass);

ALTER TABLE ONLY public.group_member ALTER COLUMN id SET DEFAULT nextval('public.group_member_id_seq'::regclass);

ALTER TABLE ONLY public.occasion_invite ALTER COLUMN id SET DEFAULT nextval('public.occasion_invite_id_seq'::regclass);

ALTER TABLE ONLY public.profile ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.connection ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.gift_group ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.group_member ALTER COLUMN joinedat SET DEFAULT now();

ALTER TABLE ONLY public.occasion_invite ALTER COLUMN invitedat SET DEFAULT now();
COMMIT;

BEGIN;
-- Test data
-- Every test profile has the password GaveListe2026 (a real BCrypt hash, so log-in can be demoed)
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (1, 'Anna Jensen', 'anna.jensen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1995-12-03', 'https://example.com/pictures/anna.jpg');
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (2, 'Mads Hansen', 'mads.hansen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1987-02-10', NULL);
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (3, 'Sofie Larsen', 'sofie.larsen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '2000-06-21', 'https://example.com/pictures/sofie.jpg');
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (4, 'Frederik Nielsen', 'frederik.nielsen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1992-04-15', NULL);
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (5, 'Ida Pedersen', 'ida.pedersen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1998-11-18', 'https://example.com/pictures/ida.jpg');
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (6, 'Lars Jensen', 'lars.jensen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1964-01-22', NULL);
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (7, 'Mette Jensen', 'mette.jensen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1966-08-30', NULL);
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (8, 'Emil Andersen', 'emil.andersen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '2003-03-07', NULL);
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (9, 'Freja Sørensen', 'freja.sorensen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1997-12-19', 'https://example.com/pictures/freja.jpg');
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, profilepictureurl) VALUES (10, 'Kasper Rasmussen', 'kasper.rasmussen@example.com', '$2y$10$n5ob56t86N4TtS8fgBTj..z5Y.5O.WuokOcZWmkgZdqfoRAkeF0oy', '1990-05-25', NULL);

-- Each pair of people appears only once, whichever of them sent the request
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (1, 1, 2, 'accepted', '2026-09-01 18:05:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (2, 1, 3, 'accepted', '2026-09-01 18:07:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (3, 3, 2, 'accepted', '2026-09-02 10:30:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (4, 1, 4, 'accepted', '2026-09-03 21:15:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (5, 4, 5, 'accepted', '2026-09-04 08:45:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (6, 6, 1, 'accepted', '2026-09-05 19:00:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (7, 7, 1, 'accepted', '2026-09-05 19:20:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (8, 6, 7, 'accepted', '2026-09-05 19:25:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (9, 5, 1, 'accepted', '2026-09-10 12:00:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (10, 5, 9, 'accepted', '2026-09-12 16:40:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (11, 3, 8, 'accepted', '2026-09-20 14:10:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (12, 2, 8, 'pending', '2026-10-15 09:30:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (13, 9, 10, 'accepted', '2026-10-20 20:50:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (14, 4, 10, 'accepted', '2026-10-22 18:10:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (15, 10, 2, 'accepted', '2026-10-23 12:00:00+02');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (16, 6, 3, 'accepted', '2026-10-24 09:45:00+02');
-- More friendships, so every person who buys or splits a gift is a friend of the occasion's creator
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (17, 2, 6, 'accepted', '2026-10-26 10:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (18, 7, 2, 'accepted', '2026-10-26 11:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (19, 8, 1, 'accepted', '2026-10-26 12:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (20, 2, 5, 'accepted', '2026-10-26 13:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (21, 5, 10, 'accepted', '2026-10-26 14:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (22, 3, 5, 'accepted', '2026-10-26 15:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (23, 9, 3, 'accepted', '2026-10-26 16:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (24, 3, 10, 'accepted', '2026-10-26 17:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (25, 4, 9, 'accepted', '2026-10-26 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (26, 8, 9, 'accepted', '2026-10-26 19:00:00+01');
-- A request still waiting for an answer (log in as Freja to accept or decline it)
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (27, 6, 9, 'pending', '2026-11-05 19:15:00+01');
-- More requests waiting for an answer: every test profile has sent one and received one,
-- so each of them can try Accept, Decline and Cancel on the Friends page
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (28, 1, 10, 'pending', '2026-11-10 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (29, 10, 8, 'pending', '2026-11-11 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (30, 8, 7, 'pending', '2026-11-12 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (31, 7, 4, 'pending', '2026-11-13 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (32, 4, 3, 'pending', '2026-11-14 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (33, 3, 7, 'pending', '2026-11-15 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (34, 5, 6, 'pending', '2026-11-16 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (35, 9, 1, 'pending', '2026-11-17 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (36, 4, 2, 'pending', '2026-11-18 18:00:00+01');
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (37, 7, 5, 'pending', '2026-11-19 18:00:00+01');

-- The same name and date is fine for different recipients (Christmas), and the same name for one
-- recipient is fine on a different date (Anna's birthday in 2025 and 2026)
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (1, 2, 1, NULL, 'Anna''s birthday', '2026-12-03', 'birthday', 'She turns 31');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (2, 1, 2, NULL, 'Mads'' 40th birthday', '2027-02-10', 'birthday', 'Big party at his place in Aarhus');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (3, 1, 3, NULL, 'Christmas 2026', '2026-12-24', 'christmas', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (4, 1, 6, NULL, 'Christmas 2026', '2026-12-24', 'christmas', 'Dad says he wants nothing – ignore him');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (5, 1, 7, NULL, 'Christmas 2026', '2026-12-24', 'christmas', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (6, 6, 7, NULL, 'Wedding anniversary', '2027-06-12', 'anniversary', '30 years married');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (7, 5, 4, NULL, 'Frederik''s birthday', '2027-04-15', 'birthday', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (8, 4, 5, NULL, 'Ida''s birthday', '2026-11-18', 'birthday', 'Dinner at Ida''s on Friday');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (9, 3, 8, NULL, 'Emil''s graduation', '2027-06-25', 'other', 'Finishes his bachelor at CBS');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (10, 9, 10, NULL, 'Kasper''s housewarming', '2026-11-14', 'other', 'New flat in Valby');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (11, 5, 9, NULL, 'Freja''s birthday', '2026-12-19', 'birthday', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (12, 3, 1, NULL, 'Anna''s birthday', '2025-12-03', 'birthday', NULL);
-- For someone without an account: only a name, no recipientid
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (13, 1, NULL, 'Mormor', 'Mormor''s 80th birthday', '2027-03-05', 'birthday', 'Family lunch in Odense');
-- Made by Mette, Emil and Kasper, so every test profile has made at least one occasion
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (14, 7, 6, NULL, 'Lars'' birthday', '2027-01-22', 'birthday', 'Small dinner at home');
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (15, 8, 3, NULL, 'Sofie''s birthday', '2027-06-21', 'birthday', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, recipientname, name, eventdate, type, notes) VALUES (16, 10, 4, NULL, 'Christmas 2026', '2026-12-24', 'christmas', 'Secret Santa among friends');

-- The buyer is never the occasion's recipient (that rule is checked by the API)
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (1, 1, 2, 'Wool scarf', 'Dark green – her favourite colour', 349, 'https://example.com/wool-scarf', 'bought');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (2, 1, 3, 'Concert tickets', 'Two tickets for DR Koncerthuset', 600, NULL, 'idea', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (3, 1, 6, 'Ceramic bowl set', 'From the whole family', 1200, 'https://example.com/bowl-set', 'planned', 3);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (4, 2, 1, 'Board game', 'Wingspan', 399, 'https://example.com/wingspan', 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (5, 2, 3, 'Weekend in Skagen', 'Two nights at a small hotel', 2400, NULL, 'planned', 3);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (6, 3, 1, 'Chalk bag', 'For climbing', 199, 'https://example.com/chalk-bag', 'bought');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (7, 3, 2, 'Headphones', NULL, 1299, 'https://example.com/headphones', 'idea', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (8, 4, 1, 'Coffee grinder', 'Hand grinder for his morning coffee', 450, NULL, 'idea', 3);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (9, 5, 1, 'Gardening book', NULL, 249, NULL, 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (10, 6, 1, 'Spa weekend', 'From the kids', 3000, 'https://example.com/spa-weekend', 'planned', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (11, 7, 5, 'Running watch', 'Still comparing models', NULL, NULL, 'idea');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (12, 8, 4, 'Pottery class', 'Two-hour wheel throwing class', 550, 'https://example.com/pottery', 'bought', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (13, 9, 3, 'Leather briefcase', NULL, 900, NULL, 'idea');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (14, 10, 9, 'Olive tree', 'For the balcony', 350, NULL, 'bought', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (15, 11, 5, 'Knitting kit', 'Merino wool, beginner level', 300, 'https://example.com/knitting-kit', 'planned', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (16, 12, 3, 'Vegetarian cookbook', NULL, 249, NULL, 'given');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (17, 12, 2, 'Photo album', 'Homemade, with photos from our trips', 0, NULL, 'given');
-- Split gifts: every test profile buys at least two gifts that are split with others
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (18, 9, 2, 'Graduation dinner', 'Dinner at a nice restaurant in town', 1600, NULL, 'planned', 4);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (19, 11, 4, 'Concert tickets', 'Two tickets for her favourite band', 800, NULL, 'idea', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (20, 10, 5, 'Coffee machine', 'For the new kitchen', 1800, 'https://example.com/coffee-machine', 'planned', 3);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (21, 5, 6, 'Weekend cottage stay', 'A weekend at the coast', 2000, NULL, 'idea', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (22, 1, 7, 'Cooking class for two', 'Italian cooking evening', 900, 'https://example.com/cooking-class', 'planned', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (23, 4, 7, 'Fishing rod', NULL, 1100, NULL, 'idea', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (24, 2, 8, 'Record player', 'He wants to start collecting vinyl', 1500, 'https://example.com/record-player', 'idea', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (25, 10, 8, 'Plant set', 'Three easy plants for the flat', 400, NULL, 'bought', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (26, 9, 9, 'Laptop backpack', 'For his first job', 700, NULL, 'planned', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (27, 11, 10, 'Yoga course', 'Ten classes at her local studio', 1200, NULL, 'idea', 3);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (28, 7, 10, 'Climbing day', 'Indoor climbing with instructor', 900, 'https://example.com/climbing', 'planned', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (29, 13, 1, 'Photo book', 'Photos of the whole family', 450, NULL, 'planned', 3);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (30, 14, 7, 'Golf lessons', 'Five lessons with a pro', 1200, NULL, 'planned', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (31, 15, 8, 'Plant workshop', 'Evening workshop at a garden centre', 500, 'https://example.com/plant-workshop', 'idea', 2);
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status, splitcount) VALUES (32, 16, 10, 'Board game café night', 'Table for four and snacks', 400, NULL, 'idea', 2);

-- Every split gift has a group: the buyer is the owner and the friends they split with are viewers.
-- splitcount on the gift = the number of members in its group.
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (1, 3, 'Family gift for Anna', 'Lars buys it, the rest of us pay him back', '2026-10-02 19:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (2, 5, 'Skagen trip for Mads', NULL, '2026-10-10 11:20:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (3, 10, 'Spa weekend for Mum', 'Split equally', '2026-10-18 15:45:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (4, 2, 'Concert tickets', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (5, 7, 'Headphones', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (6, 8, 'Coffee grinder', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (7, 12, 'Pottery class', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (8, 14, 'Olive tree', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (9, 15, 'Knitting kit', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (10, 18, 'Graduation dinner', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (11, 19, 'Concert tickets', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (12, 20, 'Coffee machine', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (13, 21, 'Weekend cottage stay', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (14, 22, 'Cooking class for two', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (15, 23, 'Fishing rod', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (16, 24, 'Record player', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (17, 25, 'Plant set', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (18, 26, 'Laptop backpack', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (19, 27, 'Yoga course', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (20, 28, 'Climbing day', NULL, '2026-10-25 10:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (21, 29, 'Photo book', NULL, '2026-11-01 10:00:00+01');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (22, 30, 'Golf lessons', NULL, '2026-11-10 10:00:00+01');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (23, 31, 'Plant workshop', NULL, '2026-11-10 10:00:00+01');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (24, 32, 'Board game café night', NULL, '2026-11-10 10:00:00+01');

-- Each group has exactly one owner (checked by the API), who is also the gift's buyer
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (1, 1, 6, 'owner', '2026-10-02 19:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (2, 1, 7, 'viewer', '2026-10-02 19:30:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (3, 1, 3, 'viewer', '2026-10-03 08:15:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (4, 2, 3, 'owner', '2026-10-10 11:20:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (5, 2, 1, 'viewer', '2026-10-10 12:05:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (6, 2, 8, 'viewer', '2026-10-11 17:40:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (7, 3, 1, 'owner', '2026-10-18 15:45:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (8, 3, 6, 'viewer', '2026-10-18 16:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (9, 4, 3, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (10, 4, 2, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (11, 5, 2, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (12, 5, 1, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (13, 6, 1, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (14, 6, 7, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (15, 6, 3, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (16, 7, 4, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (17, 7, 1, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (18, 8, 9, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (19, 8, 5, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (20, 9, 5, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (21, 9, 4, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (22, 10, 2, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (23, 10, 1, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (24, 10, 3, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (25, 10, 10, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (26, 11, 4, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (27, 11, 5, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (28, 12, 5, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (29, 12, 4, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (30, 12, 9, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (31, 13, 6, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (32, 13, 1, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (33, 14, 7, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (34, 14, 6, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (35, 15, 7, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (36, 15, 1, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (37, 16, 8, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (38, 16, 3, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (39, 17, 8, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (40, 17, 3, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (41, 18, 9, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (42, 18, 5, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (43, 19, 10, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (44, 19, 4, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (45, 19, 2, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (46, 20, 10, 'owner', '2026-10-25 10:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (47, 20, 2, 'viewer', '2026-10-25 12:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (48, 21, 1, 'owner', '2026-11-01 10:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (49, 21, 6, 'viewer', '2026-11-01 12:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (50, 21, 7, 'viewer', '2026-11-01 12:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (51, 22, 7, 'owner', '2026-11-10 12:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (52, 22, 1, 'viewer', '2026-11-10 12:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (53, 23, 8, 'owner', '2026-11-10 12:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (54, 23, 9, 'viewer', '2026-11-10 12:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (55, 24, 10, 'owner', '2026-11-10 12:00:00+01');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (56, 24, 2, 'viewer', '2026-11-10 12:00:00+01');

-- Invites: everyone who buys or splits a gift on an occasion is invited to it, and all of them are
-- friends of the occasion's creator
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (1, 1, 3, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (2, 1, 6, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (3, 1, 7, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (4, 2, 3, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (5, 2, 8, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (6, 3, 2, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (7, 4, 3, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (8, 4, 7, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (9, 5, 6, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (10, 6, 1, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (11, 7, 2, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (12, 7, 10, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (13, 8, 1, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (14, 9, 1, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (15, 9, 2, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (16, 9, 5, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (17, 9, 9, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (18, 9, 10, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (19, 10, 3, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (20, 10, 4, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (21, 10, 5, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (22, 10, 8, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (23, 11, 2, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (24, 11, 4, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (25, 11, 10, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (26, 12, 2, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (27, 13, 6, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (28, 13, 7, '2026-10-27 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (29, 14, 1, '2026-11-10 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (30, 14, 2, '2026-11-10 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (31, 15, 1, '2026-11-10 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (32, 15, 9, '2026-11-10 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (33, 16, 2, '2026-11-10 09:00:00+01');
INSERT INTO public.occasion_invite (id, occasionid, profileid, invitedat) VALUES (34, 16, 5, '2026-11-10 09:00:00+01');
COMMIT;

BEGIN;
SELECT pg_catalog.setval('public.profile_id_seq', 10, true);

SELECT pg_catalog.setval('public.connection_id_seq', 37, true);

SELECT pg_catalog.setval('public.occasion_id_seq', 16, true);

SELECT pg_catalog.setval('public.gift_id_seq', 32, true);

SELECT pg_catalog.setval('public.gift_group_id_seq', 24, true);

SELECT pg_catalog.setval('public.group_member_id_seq', 56, true);

SELECT pg_catalog.setval('public.occasion_invite_id_seq', 34, true);
COMMIT;

-- Primary keys
ALTER TABLE ONLY public.profile
    ADD CONSTRAINT profile_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.connection
    ADD CONSTRAINT connection_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT occasion_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT gift_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.gift_group
    ADD CONSTRAINT gift_group_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT group_member_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.occasion_invite
    ADD CONSTRAINT occasion_invite_pkey PRIMARY KEY (id);


-- Unique keys (stop duplicate rows)
-- One account per email address, regardless of upper/lower case
CREATE UNIQUE INDEX uk_profile_email ON public.profile (lower(email));

-- Two people are connected only once: a request A -> B also blocks B -> A,
-- because the pair is stored as (smallest id, largest id) in the index
CREATE UNIQUE INDEX uk_connection_pair ON public.connection (least(requesterid, addresseeid), greatest(requesterid, addresseeid));

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT uk_occasion_recipientid_name_eventdate UNIQUE (recipientid, name, eventdate);

-- At most one gift group per gift
ALTER TABLE ONLY public.gift_group
    ADD CONSTRAINT uk_gift_group_giftid UNIQUE (giftid);

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT uk_group_member_groupid_profileid UNIQUE (groupid, profileid);

-- A person is invited to an occasion only once
ALTER TABLE ONLY public.occasion_invite
    ADD CONSTRAINT uk_occasion_invite_occasionid_profileid UNIQUE (occasionid, profileid);


-- Foreign keys (the arrows in the ER diagram)
ALTER TABLE ONLY public.connection
    ADD CONSTRAINT fk_connection_requesterid FOREIGN KEY (requesterid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.connection
    ADD CONSTRAINT fk_connection_addresseeid FOREIGN KEY (addresseeid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT fk_occasion_createdbyid FOREIGN KEY (createdbyid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT fk_occasion_recipientid FOREIGN KEY (recipientid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT fk_gift_occasionid FOREIGN KEY (occasionid) REFERENCES public.occasion(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT fk_gift_buyerid FOREIGN KEY (buyerid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.gift_group
    ADD CONSTRAINT fk_gift_group_giftid FOREIGN KEY (giftid) REFERENCES public.gift(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT fk_group_member_groupid FOREIGN KEY (groupid) REFERENCES public.gift_group(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT fk_group_member_profileid FOREIGN KEY (profileid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

-- Deleting an occasion or a profile also deletes their invites
ALTER TABLE ONLY public.occasion_invite
    ADD CONSTRAINT fk_occasion_invite_occasionid FOREIGN KEY (occasionid) REFERENCES public.occasion(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.occasion_invite
    ADD CONSTRAINT fk_occasion_invite_profileid FOREIGN KEY (profileid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


-- Check constraints ("Rules enforced by checks" in the ER diagram)
ALTER TABLE ONLY public.connection
    ADD CONSTRAINT chk_connection_status CHECK (status IN ('pending', 'accepted'));

-- You can't send a friend request to yourself
ALTER TABLE ONLY public.connection
    ADD CONSTRAINT chk_connection_not_self CHECK (requesterid <> addresseeid);

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT chk_occasion_type CHECK (type IN ('birthday', 'christmas', 'anniversary', 'other'));

-- An occasion is for a profile OR for a name, never both and never neither
-- ("IS NULL <> IS NULL" is true only when exactly one of the two is filled)
ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT chk_occasion_recipient CHECK ((recipientid IS NULL) <> (recipientname IS NULL));

-- You can't make an occasion for yourself (it would be hidden from you – the gifts are a surprise)
ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT chk_occasion_not_self CHECK (recipientid <> createdbyid);

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT chk_gift_status CHECK (status IN ('idea', 'planned', 'bought', 'given'));

-- price may be NULL (not known yet); a NULL value passes a check
ALTER TABLE ONLY public.gift
    ADD CONSTRAINT chk_gift_price CHECK (price >= 0);

-- A gift is paid by at least one person. The database is the strongest validation layer and the
-- last line of defence: it also stops wrong data sent straight to the API (Lecture 7)
ALTER TABLE ONLY public.gift
    ADD CONSTRAINT chk_gift_splitcount CHECK (splitcount >= 1);

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT chk_group_member_role CHECK (role IN ('owner', 'viewer'));
