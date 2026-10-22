-- Gift Tracker – database script (ER Design, Option C full, 13 tables)
-- Run this in pgAdmin's Query Tool on the app-gifttrackerdb database.
-- Sign-up only asks for name, email, password and date of birth, so the other
-- profile columns are optional and filled in later from the profile page.
-- Built in the same order as the class script: drop -> create tables + sequences ->
-- id defaults -> test data -> setval -> primary keys -> unique keys -> foreign keys -> checks

DROP TABLE IF EXISTS public.gift_contribution CASCADE;
DROP TABLE IF EXISTS public.gift CASCADE;
DROP TABLE IF EXISTS public.reservation CASCADE;
DROP TABLE IF EXISTS public.wish_item CASCADE;
DROP TABLE IF EXISTS public.wishlist_share CASCADE;
DROP TABLE IF EXISTS public.wishlist CASCADE;
DROP TABLE IF EXISTS public.occasion CASCADE;
DROP TABLE IF EXISTS public.external_link CASCADE;
DROP TABLE IF EXISTS public.preference_item CASCADE;
DROP TABLE IF EXISTS public.group_member CASCADE;
DROP TABLE IF EXISTS public.gift_group CASCADE;
DROP TABLE IF EXISTS public.profile CASCADE;
DROP TABLE IF EXISTS public.preference_type CASCADE;

DROP SEQUENCE IF EXISTS public.gift_id_seq;
DROP SEQUENCE IF EXISTS public.reservation_id_seq;
DROP SEQUENCE IF EXISTS public.wish_item_id_seq;
DROP SEQUENCE IF EXISTS public.wishlist_id_seq;
DROP SEQUENCE IF EXISTS public.occasion_id_seq;
DROP SEQUENCE IF EXISTS public.external_link_id_seq;
DROP SEQUENCE IF EXISTS public.preference_item_id_seq;
DROP SEQUENCE IF EXISTS public.group_member_id_seq;
DROP SEQUENCE IF EXISTS public.gift_group_id_seq;
DROP SEQUENCE IF EXISTS public.profile_id_seq;
DROP SEQUENCE IF EXISTS public.preference_type_id_seq;

BEGIN;
-- Create the tables

CREATE TABLE public.preference_type (
    id integer NOT NULL,
    name text NOT NULL
);

CREATE SEQUENCE public.preference_type_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.preference_type_id_seq OWNED BY public.preference_type.id;


CREATE TABLE public.profile (
    id integer NOT NULL,
    fullname text NOT NULL,
    email text NOT NULL,
    passwordhash text NOT NULL,
    dateofbirth date,
    phone text,
    street text,
    postalcode text,
    city text,
    giftnotes text,
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


CREATE TABLE public.gift_group (
    id integer NOT NULL,
    name text NOT NULL,
    invitecode text NOT NULL,
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


CREATE TABLE public.group_member (
    id integer NOT NULL,
    groupid integer NOT NULL,
    profileid integer,
    guestname text,
    role text NOT NULL
);

CREATE SEQUENCE public.group_member_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.group_member_id_seq OWNED BY public.group_member.id;


CREATE TABLE public.preference_item (
    id integer NOT NULL,
    profileid integer NOT NULL,
    preferencetypeid integer NOT NULL,
    subject text,
    value text NOT NULL
);

CREATE SEQUENCE public.preference_item_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.preference_item_id_seq OWNED BY public.preference_item.id;


CREATE TABLE public.external_link (
    id integer NOT NULL,
    profileid integer NOT NULL,
    service text NOT NULL,
    url text NOT NULL
);

CREATE SEQUENCE public.external_link_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.external_link_id_seq OWNED BY public.external_link.id;


CREATE TABLE public.occasion (
    id integer NOT NULL,
    groupid integer NOT NULL,
    honoreeid integer,
    name text NOT NULL,
    eventdate date NOT NULL,
    budget numeric NOT NULL
);

CREATE SEQUENCE public.occasion_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.occasion_id_seq OWNED BY public.occasion.id;


CREATE TABLE public.wishlist (
    id integer NOT NULL,
    ownerid integer NOT NULL,
    occasionid integer,
    title text NOT NULL
);

CREATE SEQUENCE public.wishlist_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.wishlist_id_seq OWNED BY public.wishlist.id;


-- Junction table: no own id, the primary key is (wishlistid, groupid)
CREATE TABLE public.wishlist_share (
    wishlistid integer NOT NULL,
    groupid integer NOT NULL
);


CREATE TABLE public.wish_item (
    id integer NOT NULL,
    wishlistid integer NOT NULL,
    name text NOT NULL,
    link text NOT NULL,
    price numeric NOT NULL,
    priority integer NOT NULL
);

CREATE SEQUENCE public.wish_item_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.wish_item_id_seq OWNED BY public.wish_item.id;


CREATE TABLE public.reservation (
    id integer NOT NULL,
    wishitemid integer NOT NULL,
    groupmemberid integer NOT NULL,
    reservedat timestamptz NOT NULL
);

CREATE SEQUENCE public.reservation_id_seq
    AS integer
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1;

ALTER SEQUENCE public.reservation_id_seq OWNED BY public.reservation.id;


CREATE TABLE public.gift (
    id integer NOT NULL,
    occasionid integer NOT NULL,
    recipientid integer NOT NULL,
    wishitemid integer,
    description text NOT NULL,
    purchasetype text NOT NULL,
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


-- Junction table: no own id, the primary key is (giftid, groupmemberid)
CREATE TABLE public.gift_contribution (
    giftid integer NOT NULL,
    groupmemberid integer NOT NULL,
    amount numeric NOT NULL
);
-- End table creation
COMMIT;

BEGIN;
-- Alter tables
ALTER TABLE ONLY public.preference_type ALTER COLUMN id SET DEFAULT nextval('public.preference_type_id_seq'::regclass);

ALTER TABLE ONLY public.profile ALTER COLUMN id SET DEFAULT nextval('public.profile_id_seq'::regclass);

ALTER TABLE ONLY public.gift_group ALTER COLUMN id SET DEFAULT nextval('public.gift_group_id_seq'::regclass);

ALTER TABLE ONLY public.group_member ALTER COLUMN id SET DEFAULT nextval('public.group_member_id_seq'::regclass);

ALTER TABLE ONLY public.preference_item ALTER COLUMN id SET DEFAULT nextval('public.preference_item_id_seq'::regclass);

ALTER TABLE ONLY public.external_link ALTER COLUMN id SET DEFAULT nextval('public.external_link_id_seq'::regclass);

ALTER TABLE ONLY public.occasion ALTER COLUMN id SET DEFAULT nextval('public.occasion_id_seq'::regclass);

ALTER TABLE ONLY public.wishlist ALTER COLUMN id SET DEFAULT nextval('public.wishlist_id_seq'::regclass);

ALTER TABLE ONLY public.wish_item ALTER COLUMN id SET DEFAULT nextval('public.wish_item_id_seq'::regclass);

ALTER TABLE ONLY public.reservation ALTER COLUMN id SET DEFAULT nextval('public.reservation_id_seq'::regclass);

ALTER TABLE ONLY public.gift ALTER COLUMN id SET DEFAULT nextval('public.gift_id_seq'::regclass);

ALTER TABLE ONLY public.profile ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.gift_group ALTER COLUMN createdat SET DEFAULT now();

ALTER TABLE ONLY public.reservation ALTER COLUMN reservedat SET DEFAULT now();
COMMIT;

BEGIN;
-- Test data
INSERT INTO public.preference_type (id, name) VALUES (1, 'Clothing size');
INSERT INTO public.preference_type (id, name) VALUES (2, 'Favourite colour');
INSERT INTO public.preference_type (id, name) VALUES (3, 'Allergy');
INSERT INTO public.preference_type (id, name) VALUES (4, 'Hobby');

INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, phone, street, postalcode, city, giftnotes) VALUES (1, 'Anna Jensen', 'anna@example.com', '$2b$10$examplehashanna', '1995-12-03', '11223344', 'Norrebrogade 10', '2200', 'Copenhagen N', 'I love things for the kitchen');
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, phone, street, postalcode, city, giftnotes) VALUES (2, 'Mads Hansen', 'mads@example.com', '$2b$10$examplehashmads', '1987-02-10', '22334455', 'Vesterbrogade 5', '1620', 'Copenhagen V', 'No more mugs please');
INSERT INTO public.profile (id, fullname, email, passwordhash, dateofbirth, phone, street, postalcode, city, giftnotes) VALUES (3, 'Sofie Larsen', 'sofie@example.com', '$2b$10$examplehashsofie', '2000-06-21', '33445566', 'Amagerbrogade 77', '2300', 'Copenhagen S', 'Anything outdoors');

INSERT INTO public.gift_group (id, name, invitecode, createdat) VALUES (1, 'Jensen family', 'FAM-1234', '2026-09-01 18:00:00+02');
INSERT INTO public.gift_group (id, name, invitecode, createdat) VALUES (2, 'Office friends', 'OFF-5678', '2026-09-15 12:30:00+02');

-- A member has either a profileid or a guestname, never both
INSERT INTO public.group_member (id, groupid, profileid, guestname, role) VALUES (1, 1, 1, NULL, 'owner');
INSERT INTO public.group_member (id, groupid, profileid, guestname, role) VALUES (2, 1, 2, NULL, 'member');
INSERT INTO public.group_member (id, groupid, profileid, guestname, role) VALUES (3, 1, 3, NULL, 'member');
INSERT INTO public.group_member (id, groupid, profileid, guestname, role) VALUES (4, 1, NULL, 'Grandma Lis', 'guest');
INSERT INTO public.group_member (id, groupid, profileid, guestname, role) VALUES (5, 2, 2, NULL, 'owner');
INSERT INTO public.group_member (id, groupid, profileid, guestname, role) VALUES (6, 2, 3, NULL, 'member');

INSERT INTO public.preference_item (id, profileid, preferencetypeid, subject, value) VALUES (1, 1, 1, 'Shoes', '38');
INSERT INTO public.preference_item (id, profileid, preferencetypeid, subject, value) VALUES (2, 1, 1, 'T-shirt', 'M');
INSERT INTO public.preference_item (id, profileid, preferencetypeid, subject, value) VALUES (3, 1, 2, NULL, 'Green');
INSERT INTO public.preference_item (id, profileid, preferencetypeid, subject, value) VALUES (4, 2, 3, NULL, 'Nuts');
INSERT INTO public.preference_item (id, profileid, preferencetypeid, subject, value) VALUES (5, 3, 4, NULL, 'Climbing');

INSERT INTO public.external_link (id, profileid, service, url) VALUES (1, 1, 'Pinterest', 'https://pinterest.com/annajensen/gifts');
INSERT INTO public.external_link (id, profileid, service, url) VALUES (2, 2, 'Amazon', 'https://amazon.de/hz/wishlist/madshansen');

INSERT INTO public.occasion (id, groupid, honoreeid, name, eventdate, budget) VALUES (1, 1, 1, 'Anna''s birthday', '2026-12-03', 1500);
INSERT INTO public.occasion (id, groupid, honoreeid, name, eventdate, budget) VALUES (2, 1, NULL, 'Christmas', '2026-12-24', 3000);
INSERT INTO public.occasion (id, groupid, honoreeid, name, eventdate, budget) VALUES (3, 2, 2, 'Mads'' 40th', '2027-02-10', 800);

INSERT INTO public.wishlist (id, ownerid, occasionid, title) VALUES (1, 1, 1, 'Birthday 2026');
INSERT INTO public.wishlist (id, ownerid, occasionid, title) VALUES (2, 2, NULL, 'Always welcome');
INSERT INTO public.wishlist (id, ownerid, occasionid, title) VALUES (3, 3, 2, 'Christmas wishes');

INSERT INTO public.wishlist_share (wishlistid, groupid) VALUES (1, 1);
INSERT INTO public.wishlist_share (wishlistid, groupid) VALUES (2, 1);
INSERT INTO public.wishlist_share (wishlistid, groupid) VALUES (2, 2);
INSERT INTO public.wishlist_share (wishlistid, groupid) VALUES (3, 1);

INSERT INTO public.wish_item (id, wishlistid, name, link, price, priority) VALUES (1, 1, 'Wool scarf', 'https://example.com/wool-scarf', 349, 1);
INSERT INTO public.wish_item (id, wishlistid, name, link, price, priority) VALUES (2, 1, 'Cookbook', 'https://example.com/cookbook', 249, 2);
INSERT INTO public.wish_item (id, wishlistid, name, link, price, priority) VALUES (3, 2, 'Board game', 'https://example.com/board-game', 399, 1);
INSERT INTO public.wish_item (id, wishlistid, name, link, price, priority) VALUES (4, 3, 'Chalk bag', 'https://example.com/chalk-bag', 199, 1);
INSERT INTO public.wish_item (id, wishlistid, name, link, price, priority) VALUES (5, 3, 'Headphones', 'https://example.com/headphones', 1299, 2);

INSERT INTO public.reservation (id, wishitemid, groupmemberid, reservedat) VALUES (1, 2, 2, '2026-10-01 20:15:00+02');
INSERT INTO public.reservation (id, wishitemid, groupmemberid, reservedat) VALUES (2, 4, 1, '2026-10-05 09:40:00+02');

INSERT INTO public.gift (id, occasionid, recipientid, wishitemid, description, purchasetype, status) VALUES (1, 1, 1, 1, 'Wool scarf from Anna''s wishlist', 'shared', 'bought');
INSERT INTO public.gift (id, occasionid, recipientid, wishitemid, description, purchasetype, status) VALUES (2, 1, 1, NULL, 'Concert tickets', 'individual', 'idea');
INSERT INTO public.gift (id, occasionid, recipientid, wishitemid, description, purchasetype, status) VALUES (3, 2, 3, 5, 'Headphones for Sofie', 'shared', 'idea');

-- A shared gift has several contributions, an individual gift exactly one
INSERT INTO public.gift_contribution (giftid, groupmemberid, amount) VALUES (1, 2, 125);
INSERT INTO public.gift_contribution (giftid, groupmemberid, amount) VALUES (1, 3, 124);
INSERT INTO public.gift_contribution (giftid, groupmemberid, amount) VALUES (1, 4, 100);
INSERT INTO public.gift_contribution (giftid, groupmemberid, amount) VALUES (2, 3, 600);
INSERT INTO public.gift_contribution (giftid, groupmemberid, amount) VALUES (3, 1, 650);
INSERT INTO public.gift_contribution (giftid, groupmemberid, amount) VALUES (3, 2, 649);
COMMIT;

BEGIN;
SELECT pg_catalog.setval('public.preference_type_id_seq', 4, true);

SELECT pg_catalog.setval('public.profile_id_seq', 3, true);

SELECT pg_catalog.setval('public.gift_group_id_seq', 2, true);

SELECT pg_catalog.setval('public.group_member_id_seq', 6, true);

SELECT pg_catalog.setval('public.preference_item_id_seq', 5, true);

SELECT pg_catalog.setval('public.external_link_id_seq', 2, true);

SELECT pg_catalog.setval('public.occasion_id_seq', 3, true);

SELECT pg_catalog.setval('public.wishlist_id_seq', 3, true);

SELECT pg_catalog.setval('public.wish_item_id_seq', 5, true);

SELECT pg_catalog.setval('public.reservation_id_seq', 2, true);

SELECT pg_catalog.setval('public.gift_id_seq', 3, true);
COMMIT;

-- Primary keys
ALTER TABLE ONLY public.preference_type
    ADD CONSTRAINT preference_type_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.profile
    ADD CONSTRAINT profile_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.gift_group
    ADD CONSTRAINT gift_group_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT group_member_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.preference_item
    ADD CONSTRAINT preference_item_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.external_link
    ADD CONSTRAINT external_link_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT occasion_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.wishlist
    ADD CONSTRAINT wishlist_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.wishlist_share
    ADD CONSTRAINT wishlist_share_pkey PRIMARY KEY (wishlistid, groupid);

ALTER TABLE ONLY public.wish_item
    ADD CONSTRAINT wish_item_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.reservation
    ADD CONSTRAINT reservation_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT gift_pkey PRIMARY KEY (id);

ALTER TABLE ONLY public.gift_contribution
    ADD CONSTRAINT gift_contribution_pkey PRIMARY KEY (giftid, groupmemberid);


-- Unique keys (stop duplicate rows)
ALTER TABLE ONLY public.preference_type
    ADD CONSTRAINT uk_preference_type_name UNIQUE (name);

-- One account per email address, regardless of upper/lower case
CREATE UNIQUE INDEX uk_profile_email ON public.profile (lower(email));

ALTER TABLE ONLY public.gift_group
    ADD CONSTRAINT uk_gift_group_invitecode UNIQUE (invitecode);

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT uk_group_member_groupid_profileid UNIQUE (groupid, profileid);

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT uk_group_member_groupid_guestname UNIQUE (groupid, guestname);

ALTER TABLE ONLY public.preference_item
    ADD CONSTRAINT uk_preference_item_profile_type_subject_value UNIQUE (profileid, preferencetypeid, subject, value);

ALTER TABLE ONLY public.external_link
    ADD CONSTRAINT uk_external_link_profileid_url UNIQUE (profileid, url);

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT uk_occasion_groupid_name_eventdate UNIQUE (groupid, name, eventdate);

ALTER TABLE ONLY public.wishlist
    ADD CONSTRAINT uk_wishlist_ownerid_title UNIQUE (ownerid, title);

ALTER TABLE ONLY public.reservation
    ADD CONSTRAINT uk_reservation_wishitemid UNIQUE (wishitemid);

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT uk_gift_wishitemid UNIQUE (wishitemid);


-- Foreign keys (the arrows in the ER diagram)
ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT fk_group_member_groupid FOREIGN KEY (groupid) REFERENCES public.gift_group(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT fk_group_member_profileid FOREIGN KEY (profileid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.preference_item
    ADD CONSTRAINT fk_preference_item_profileid FOREIGN KEY (profileid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.preference_item
    ADD CONSTRAINT fk_preference_item_preferencetypeid FOREIGN KEY (preferencetypeid) REFERENCES public.preference_type(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.external_link
    ADD CONSTRAINT fk_external_link_profileid FOREIGN KEY (profileid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT fk_occasion_groupid FOREIGN KEY (groupid) REFERENCES public.gift_group(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

-- honoreeid is nullable, so the occasion stays if the honoree's profile is deleted
ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT fk_occasion_honoreeid FOREIGN KEY (honoreeid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL;

ALTER TABLE ONLY public.wishlist
    ADD CONSTRAINT fk_wishlist_ownerid FOREIGN KEY (ownerid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.wishlist
    ADD CONSTRAINT fk_wishlist_occasionid FOREIGN KEY (occasionid) REFERENCES public.occasion(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL;

ALTER TABLE ONLY public.wishlist_share
    ADD CONSTRAINT fk_wishlist_share_wishlistid FOREIGN KEY (wishlistid) REFERENCES public.wishlist(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.wishlist_share
    ADD CONSTRAINT fk_wishlist_share_groupid FOREIGN KEY (groupid) REFERENCES public.gift_group(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.wish_item
    ADD CONSTRAINT fk_wish_item_wishlistid FOREIGN KEY (wishlistid) REFERENCES public.wishlist(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.reservation
    ADD CONSTRAINT fk_reservation_wishitemid FOREIGN KEY (wishitemid) REFERENCES public.wish_item(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.reservation
    ADD CONSTRAINT fk_reservation_groupmemberid FOREIGN KEY (groupmemberid) REFERENCES public.group_member(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT fk_gift_occasionid FOREIGN KEY (occasionid) REFERENCES public.occasion(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT fk_gift_recipientid FOREIGN KEY (recipientid) REFERENCES public.profile(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

-- wishitemid is nullable, so a bought gift stays if the wish item is removed
ALTER TABLE ONLY public.gift
    ADD CONSTRAINT fk_gift_wishitemid FOREIGN KEY (wishitemid) REFERENCES public.wish_item(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE SET NULL;

ALTER TABLE ONLY public.gift_contribution
    ADD CONSTRAINT fk_gift_contribution_giftid FOREIGN KEY (giftid) REFERENCES public.gift(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;

ALTER TABLE ONLY public.gift_contribution
    ADD CONSTRAINT fk_gift_contribution_groupmemberid FOREIGN KEY (groupmemberid) REFERENCES public.group_member(id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE CASCADE;


-- Check constraints ("Rules enforced by checks" in the ER diagram)
ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT chk_group_member_profile_or_guest CHECK ((profileid IS NULL) <> (guestname IS NULL));

ALTER TABLE ONLY public.group_member
    ADD CONSTRAINT chk_group_member_role CHECK (role IN ('owner', 'member', 'guest'));

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT chk_gift_status CHECK (status IN ('idea', 'bought', 'given'));

ALTER TABLE ONLY public.gift
    ADD CONSTRAINT chk_gift_purchasetype CHECK (purchasetype IN ('individual', 'shared'));

ALTER TABLE ONLY public.occasion
    ADD CONSTRAINT chk_occasion_budget CHECK (budget >= 0);

ALTER TABLE ONLY public.wish_item
    ADD CONSTRAINT chk_wish_item_price CHECK (price >= 0);

ALTER TABLE ONLY public.gift_contribution
    ADD CONSTRAINT chk_gift_contribution_amount CHECK (amount > 0);
