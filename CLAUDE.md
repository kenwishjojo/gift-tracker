# Gift Tracker – instructions for Claude Code

This is a student group project for the CBS course **Designing and Developing Digital Business
Applications (DDDBA)**. The app helps friends and family plan gifts for each other's occasions.

Read this whole file before changing anything.

## Working with us (important)

- **We will be examined individually in an oral exam** and must be able to explain every part of the
  code. Keep the code at course level, explain what you write, and prefer small steps we can follow.
- Follow the **course patterns** (the `CourseAdminSystem` example from the lectures) rather than
  "best practice" alternatives we haven't learned:
  - Backend: ASP.NET Core Web API + **Npgsql with hand-written SQL in repositories**. No Entity
    Framework, no Dapper, no MediatR, no extra architecture layers.
  - Frontend: Angular (standalone components, `@Input`, `@if`/`@for`, services with `HttpClient`,
    `[(ngModel)]` for template-driven forms, reactive forms + Angular Material as in Lecture 8).
- AI assistance must be declared in the report (CBS GenAI rules) – keep commit messages honest.
- Ask before big decisions (new libraries, changing the database design, deleting things).

## Project requirements (from the course's Project and Exam Guide)

| Part | Minimum requirement |
|---|---|
| Database (PostgreSQL) | Several related tables, primary keys + foreign keys, **at least 10 records** of test data |
| Backend (C# Web API) | CRUD for at least two entities, correct use of GET / POST / PUT / DELETE |
| Frontend (Angular) | At least **4 components**, **services** that call the API, a **form with two-way data binding**, **basic validation** on form fields |
| Submission | Max 10-page report (+5-page appendix) and a zip of all code incl. SQL – **no node_modules, bin, obj** |
| Exam | 20-min individual oral exam with a live demo (VS Code, Swagger, pgAdmin ready) |

## Current state of the repository

- `Database/GiftTrackerDb.sql` and `Backend/` implement the **old 13-table design ("Option C")**:
  profile, preference_type, preference_item, external_link, gift_group, group_member, occasion,
  wishlist, wishlist_share, wish_item, reservation, gift, gift_contribution. Full CRUD for all of
  them works and was tested against the database.
- `Frontend/` is still empty.

## Decision: switch to the new 6-table design

The group decided to build the app on the **"without preferences" design** instead. Diagram:
`docs/er-design-without-preferences.svg`. The old Option C SQL + backend are to be **replaced**
(they stay in git history).

### Tables (PostgreSQL types, lowercase names like the rest of the project)

**profile** (a user – `user` is a reserved word in PostgreSQL)
- `id` int PK (sequence) · `fullname` text · `email` text **UK** (case-insensitive: unique index on `lower(email)`)
- `passwordhash` text (BCrypt) · `dateofbirth` date · `profilepictureurl` text NULL · `createdat` timestamptz default now()

**connection** (friend requests between two profiles)
- `id` int PK · `requesterid` int FK → profile · `addresseeid` int FK → profile
- `status` text · `createdat` timestamptz default now()
- UK: the pair in **either direction** – unique index on `(least(requesterid, addresseeid), greatest(requesterid, addresseeid))`

**occasion**
- `id` int PK · `createdbyid` int FK → profile · `recipientid` int FK → profile
- `name` text · `eventdate` date · `type` text · `notes` text NULL
- UK `(recipientid, name, eventdate)`

**gift**
- `id` int PK · `occasionid` int FK → occasion (NOT NULL) · `buyerid` int FK → profile
- `name` text · `description` text NULL · `price` numeric NULL · `link` text NULL · `status` text
- The recipient is **not** stored on gift – it comes from `occasion.recipientid` (3NF).

**gift_group** (people splitting one gift – `group` is a reserved word)
- `id` int PK · `giftid` int FK → gift, **UK** (at most one group per gift) · `name` text
- `description` text NULL · `createdat` timestamptz default now()
- A gift is a "group gift" when a gift_group points to it (no `isgroupgift` column).

**group_member**
- `id` int PK · `groupid` int FK → gift_group · `profileid` int FK → profile
- `role` text · `joinedat` timestamptz default now() · UK `(groupid, profileid)`
- The group's owner is the member with role `owner` (no `ownerid` column on gift_group).

### Check constraints

- `connection.status IN ('pending', 'accepted')` and `requesterid <> addresseeid`
- `occasion.type IN ('birthday', 'christmas', 'anniversary', 'other')`
- `gift.status IN ('idea', 'planned', 'bought', 'given')` and `price >= 0`
- `group_member.role IN ('owner', 'viewer')`

### Rules checked in the API (not possible as simple database checks)

- The buyer of a gift can't be the gift's recipient (the occasion's recipient).
- A gift group has exactly one member with role `owner`.

### Foreign key delete behaviour (suggested – confirm with us)

- Deleting a profile cascades to their connections, occasions they created or receive, gifts they
  buy, and their group memberships.
- Deleting an occasion cascades to its gifts; deleting a gift cascades to its gift_group; deleting a
  gift_group cascades to its members.

## Plan – do these in order, one pull request each

1. **Database** – rewrite `Database/GiftTrackerDb.sql` for the 6 tables. Keep the existing script's
   structure and comments: drop → create tables + sequences → id defaults → test data → setval →
   primary keys → unique keys → foreign keys → checks. Test data: **at least 10 rows in the main
   tables** (profile, occasion, gift) and realistic Danish names/data; every check must pass.
   Verify by running the script in a local PostgreSQL (`psql -f`) and trying a few constraint
   violations.
2. **Backend** – replace the 13 old entities/repositories/controllers in `Backend/` with 6 new ones.
   Keep what already works: `BaseRepository` (helpers for NULL columns, `InsertAndGetId` with
   `returning id`), `Program.cs` (repository registration, CORS, Swagger, middleware that turns
   PostgreSQL constraint errors `23xxx` into **400 Bad Request**, `appsettings.Local.json` for the
   password), BCrypt hashing on `POST /api/Profile` and never returning `passwordhash`.
   Same endpoint shape per table: GET all, GET `{id}`, POST (returns the new row with its id),
   PUT (id in body), DELETE `{id}`. Add the two API rules above. Update `Backend/README.md`.
   Test every endpoint in Swagger with `dotnet run --project GiftTracker.API`.
3. **Frontend** – create the Angular app in `Frontend/` (Angular CLI, Angular Material). Suggested
   first slice that meets all requirements: occasion list → occasion detail with its gifts → add/edit
   gift form (two-way binding + validation: name required, price ≥ 0, status from the list) → profile
   page. One service per table that the pages use. Base the screens on our Figma wireframes
   (file "Wireframe Review – gift Tracker").
4. **Filtered endpoints** the frontend needs, e.g. `GET /api/Gift/occasion/{occasionId}`,
   `GET /api/Occasion/recipient/{profileId}`, `GET /api/GroupMember/group/{groupId}`.

## Running locally

- **Database**: pgAdmin 4 → create database `app-gifttrackerdb` (template0 if you get a locale
  error) → Query Tool → run `Database/GiftTrackerDb.sql`.
- **Backend**: put the pgAdmin password in `Backend/GiftTracker.API/appsettings.Local.json`
  (git-ignored – never commit it):
  ```json
  { "ConnectionStrings": { "GiftTrackerDb": "Host=localhost:5432;Username=postgres;Password=...;Database=app-gifttrackerdb" } }
  ```
  then from `Backend/`: `dotnet run --project GiftTracker.API` → http://localhost:5150/swagger
- **Frontend** (once created): from `Frontend/<app>`: `npm install` → `ng serve` → http://localhost:4200
  (the API base URL is `http://localhost:5150/api`).

## Git workflow

- `main` is protected by habit: work on a branch per feature (`database-v2`, `backend-v2`,
  `frontend-occasions`, ...), push, open a pull request so the group can review it.
- Never commit passwords, `bin/`, `obj/`, `node_modules/`, `.angular/` (already in `.gitignore`).
- Don't upload zip files into the repo – commit the real files.
