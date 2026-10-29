-- Gift Tracker – database script (ER design "without preferences", 6 tables)
-- Diagram: docs/er-design-without-preferences.svg
-- Run this in pgAdmin's Query Tool on the app-gifttrackerdb database.
-- Built in the same order as the class script: drop -> create tables + sequences ->
-- id defaults -> test data -> setval -> primary keys -> unique keys -> foreign keys -> checks

DROP TABLE IF EXISTS public.group_member CASCADE;
DROP TABLE IF EXISTS public.gift_group CASCADE;
DROP TABLE IF EXISTS public.gift CASCADE;
DROP TABLE IF EXISTS public.occasion CASCADE;
DROP TABLE IF EXISTS public.connection CASCADE;
DROP TABLE IF EXISTS public.profile CASCADE;

DROP SEQUENCE IF EXISTS public.group_member_id_seq;
DROP SEQUENCE IF EXISTS public.gift_group_id_seq;
DROP SEQUENCE IF EXISTS public.gift_id_seq;
DROP SEQUENCE IF EXISTS public.occasion_id_seq;
DROP SEQUENCE IF EXISTS public.connection_id_seq;
DROP SEQUENCE IF EXISTS public.profile_id_seq;

-- Tables from the old 13-table design (Option C) that no longer exist.
-- Dropped so a database that ran the old script ends up with only the 6 new tables.
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


CREATE TABLE public.occasion (
    id integer NOT NULL,
    createdbyid integer NOT NULL,
    recipientid integer NOT NULL,
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
    status text NOT NULL
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
-- End table creation
COMMIT;

BEGIN;
-- Alter tables
ALTER TABLE ONLY public.profile ALTER COLUMN id SET DEFAULT nextval('public.profile_id_seq'::regclass);

ALTER TABLE ONLY public.connection ALTER COLUMN id SET DEFAULT nextval('public.connection_id_seq'::regclass);

ALTER TABLE ONLY public.occasion ALTER COLUMN id SET DEFAULT nextval('public.occasion_id_seq'::regclass);

ALTER TABLE ONLY public.gift ALTER COLUMN id SET DEFAULT nextval('public.gift_id_seq'::regclass);

ALTER TABLE ONLY public.gift_group ALTER COLUMN id SET DEFAULT nextval('public.gift_group_id_seq'::regclass);

ALTER TABLE ONLY public.group_member ALTER COLUMN id SET DEFAULT nextval('public.group_member_id_seq'::regclass);

ALTER TABLE ONLY public.profile ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.connection ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.gift_group ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.group_member ALTER COLUMN joinedat SET DEFAULT now();
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
INSERT INTO public.connection (id, requesterid, addresseeid, status, createdat) VALUES (13, 9, 10, 'pending', '2026-10-20 20:50:00+02');

-- The same name and date is fine for different recipients (Christmas), and the same name for one
-- recipient is fine on a different date (Anna's birthday in 2025 and 2026)
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (1, 2, 1, 'Anna''s birthday', '2026-12-03', 'birthday', 'She turns 31');
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (2, 1, 2, 'Mads'' 40th birthday', '2027-02-10', 'birthday', 'Big party at his place in Aarhus');
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (3, 1, 3, 'Christmas 2026', '2026-12-24', 'christmas', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (4, 1, 6, 'Christmas 2026', '2026-12-24', 'christmas', 'Dad says he wants nothing – ignore him');
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (5, 1, 7, 'Christmas 2026', '2026-12-24', 'christmas', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (6, 6, 7, 'Wedding anniversary', '2027-06-12', 'anniversary', '30 years married');
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (7, 5, 4, 'Frederik''s birthday', '2027-04-15', 'birthday', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (8, 4, 5, 'Ida''s birthday', '2026-11-18', 'birthday', 'Dinner at Ida''s on Friday');
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (9, 3, 8, 'Emil''s graduation', '2027-06-25', 'other', 'Finishes his bachelor at CBS');
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (10, 9, 10, 'Kasper''s housewarming', '2026-11-14', 'other', 'New flat in Valby');
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (11, 5, 9, 'Freja''s birthday', '2026-12-19', 'birthday', NULL);
INSERT INTO public.occasion (id, createdbyid, recipientid, name, eventdate, type, notes) VALUES (12, 3, 1, 'Anna''s birthday', '2025-12-03', 'birthday', NULL);

-- The buyer is never the occasion's recipient (that rule is checked by the API)
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (1, 1, 2, 'Wool scarf', 'Dark green – her favourite colour', 349, 'https://example.com/wool-scarf', 'bought');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (2, 1, 3, 'Concert tickets', 'Two tickets for DR Koncerthuset', 600, NULL, 'idea');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (3, 1, 6, 'Ceramic bowl set', 'From the whole family', 1200, 'https://example.com/bowl-set', 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (4, 2, 1, 'Board game', 'Wingspan', 399, 'https://example.com/wingspan', 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (5, 2, 3, 'Weekend in Skagen', 'Two nights at a small hotel', 2400, NULL, 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (6, 3, 1, 'Chalk bag', 'For climbing', 199, 'https://example.com/chalk-bag', 'bought');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (7, 3, 2, 'Headphones', NULL, 1299, 'https://example.com/headphones', 'idea');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (8, 4, 1, 'Coffee grinder', 'Hand grinder for his morning coffee', 450, NULL, 'idea');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (9, 5, 1, 'Gardening book', NULL, 249, NULL, 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (10, 6, 1, 'Spa weekend', 'From the kids', 3000, 'https://example.com/spa-weekend', 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (11, 7, 5, 'Running watch', 'Still comparing models', NULL, NULL, 'idea');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (12, 8, 4, 'Pottery class', 'Two-hour wheel throwing class', 550, 'https://example.com/pottery', 'bought');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (13, 9, 3, 'Leather briefcase', NULL, 900, NULL, 'idea');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (14, 10, 9, 'Olive tree', 'For the balcony', 350, NULL, 'bought');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (15, 11, 5, 'Knitting kit', 'Merino wool, beginner level', 300, 'https://example.com/knitting-kit', 'planned');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (16, 12, 3, 'Vegetarian cookbook', NULL, 249, NULL, 'given');
INSERT INTO public.gift (id, occasionid, buyerid, name, description, price, link, status) VALUES (17, 12, 2, 'Photo album', 'Homemade, with photos from our trips', 0, NULL, 'given');

-- Gifts 3, 5 and 10 are group gifts
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (1, 3, 'Family gift for Anna', 'Lars buys it, the rest of us pay him back', '2026-10-02 19:00:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (2, 5, 'Skagen trip for Mads', NULL, '2026-10-10 11:20:00+02');
INSERT INTO public.gift_group (id, giftid, name, description, createdat) VALUES (3, 10, 'Spa weekend for Mum', 'Split equally', '2026-10-18 15:45:00+02');

-- Each group has exactly one owner (checked by the API), who is also the gift's buyer
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (1, 1, 6, 'owner', '2026-10-02 19:00:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (2, 1, 7, 'viewer', '2026-10-02 19:30:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (3, 1, 3, 'viewer', '2026-10-03 08:15:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (4, 2, 3, 'owner', '2026-10-10 11:20:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (5, 2, 1, 'viewer', '2026-10-10 12:05:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (6, 2, 8, 'viewer', '2026-10-11 17:40:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (7, 3, 1, 'owner', '2026-10-18 15:45:00+02');
INSERT INTO public.group_member (id, groupid, profileid, role, joinedat) VALUES (8, 3, 6, 'viewer', '2026-10-18 16:00:00+02');
COMMIT;

BEGIN;
SELECT pg_catalog.setval('public.profile_id_seq', 10, true);

SELECT pg_catalog.setval('public.connection_id_seq', 13, true);

SELECT pg_catalog.setval('public.occasion_id_seq', 12, true);

SELECT pg_catalog.setval('public.gift_id_seq', 17, true);

SELECT pg_catalog.setval('public.gift_group_id_seq', 3, true);

SELECT pg_catalog.setval('public.group_member_id_seq', 8, true);
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


-- Check constraints ("Rules enforced by checks" in the ER diagram)
ALTER TABLE ONLY public.connection
    ADD CONSTRAINT chk_connection_status CHECK (status IN ('pending', 'accepted'));

-- You can't send a friend request to yourself
ALTER TABLE ONLY public.connection
    ADD CONSTRAINT chk_connection_not_self CHECK (requesterid <> addresseeid);

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT chk_occasion_type CHECK (type IN ('birthday', 'christmas', 'anniversary', 'other'));

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT chk_gift_status CHECK (status IN ('idea', 'planned', 'bought', 'given'));

-- price may be NULL (not known yet); a NULL value passes a check
ALTER TABLE ONLY public.gift
    ADD CONSTRAINT chk_gift_price CHECK (price >= 0);

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT chk_group_member_role CHECK (role IN ('owner', 'viewer'));
