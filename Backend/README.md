# Backend – Gift Tracker API

ASP.NET Core Web API (C#) that reads and writes the PostgreSQL database from `../Database`.
Built the same way as `CourseAdminSystem` in Lecture 4 and Exercise 6.

```
Backend/
├── GiftTracker.sln
├── GiftTracker.API/              ← Web API: controllers, Program.cs, appsettings.json
│   └── Controllers/              ← one controller per table (6)
└── GiftTracker.Model/            ← Class library
    ├── Entities/                 ← one C# class per table (6)
    └── Repositories/             ← SQL for each table + BaseRepository
```

The 6 tables are the "without preferences" design (`../docs/er-design-without-preferences.svg`):
`profile`, `connection`, `occasion`, `gift`, `gift_group`, `group_member`.

## Run it

1. Create and fill the database first: run `../Database/GiftTrackerDb.sql` in pgAdmin (see the main README).
2. In `GiftTracker.API/`, create a file called **`appsettings.Local.json`** with your own pgAdmin password.
   It's in `.gitignore`, so your password never ends up on GitHub:
   ```json
   {
     "ConnectionStrings": {
       "GiftTrackerDb": "Host=localhost:5432;Username=postgres;Password=YOUR_PASSWORD;Database=app-gifttrackerdb"
     }
   }
   ```
3. From the `Backend` folder:
   ```bash
   dotnet run --project GiftTracker.API
   ```
4. Open **http://localhost:5150/swagger** and try the endpoints.

The project targets .NET 8 but also runs on .NET 9 or 10 if that's what you have installed (`dotnet --version`).

## Endpoints

Every table has the same five endpoints, e.g. for gifts:

| Method | URL | Does |
|---|---|---|
| GET | `/api/Gift` | all gifts |
| GET | `/api/Gift/{id}` | one gift (404 if not found) |
| POST | `/api/Gift` | add one, returns it **with its new id** |
| PUT | `/api/Gift` | update (id in the body) |
| DELETE | `/api/Gift/{id}` | delete (204 No Content) |

Tables: `Profile`, `Connection`, `Occasion`, `Gift`, `GiftGroup`, `GroupMember`.

## Rules checked by the API

Two rules can't be written as simple database checks, so the controllers check them and answer
**400 Bad Request** with a message:

- **A gift's buyer can't be its recipient.** The recipient isn't stored on `gift` – it's the
  `recipientId` of the gift's occasion. `GiftController` looks up the occasion on POST and PUT and compares.
  `OccasionController` also checks it the other way round: you can't change an occasion's recipient to
  someone who is already buying one of its gifts.
- **A gift group has exactly one owner** (the `group_member` with role `owner`):
  - `POST /api/GiftGroup` needs an `ownerId`. It inserts the group **and** the owner member in one
    transaction, so a group never exists without an owner (if the owner insert fails, the group is undone too).
  - `GET /api/GiftGroup` returns `ownerId`, found by joining `group_member`. `PUT` ignores `ownerId`.
  - `/api/GroupMember` won't add a second owner, won't turn the owner into a viewer or move them to another
    group, and won't delete the owner (delete the gift group instead). Changing who the owner is isn't
    supported yet.

## Things worth knowing

- **Passwords**: `POST /api/Profile` takes a `password` (min. 8 characters), hashes it with BCrypt and stores
  only the hash in `passwordhash`. The API never selects or sends back the hash, and `PUT` doesn't change the password.
  All test profiles have the password `GaveListe2026`.
- **Dates**: `dateOfBirth` and `eventDate` are `DateOnly` – send them as `"2026-12-03"`.
  `createdAt` and `joinedAt` are set by the database (default `now()`), so you don't send them.
- **Optional (NULL) columns** are nullable in C# (`string?`, `decimal?`), e.g. `notes`, `description`, `price`, `link`.
- **Database rules → 400 Bad Request**: if the database rejects something (duplicate email in any upper/lower case,
  a friend request to yourself or a second request between the same two people, a status/type/role that isn't in
  the list, a negative price, a second group for the same gift, a wrong foreign key...), the API answers 400 with
  the rule's name instead of crashing. See the middleware in `Program.cs`.
- **Deleting cascades** (set in the database): deleting a profile also deletes their connections, occasions,
  gifts and group memberships; deleting an occasion deletes its gifts; deleting a gift deletes its gift group;
  deleting a gift group deletes its members.
- **CORS** is open so the Angular app on port 4200 can call the API.
