# Backend – Gift Tracker API

ASP.NET Core Web API (C#) that reads and writes the PostgreSQL database from `../Database`.
Built the same way as `CourseAdminSystem` in Lecture 4 and Exercise 6.

```
Backend/
├── GiftTracker.sln
├── GiftTracker.API/              ← Web API: controllers, Program.cs, appsettings.json
│   └── Controllers/              ← one controller per table (13)
└── GiftTracker.Model/            ← Class library
    ├── Entities/                 ← one C# class per table (13)
    └── Repositories/             ← SQL for each table + BaseRepository
```

## Run it

1. Create and fill the database first (see the main README).
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

Every table has the same five endpoints, e.g. for wish items:

| Method | URL | Does |
|---|---|---|
| GET | `/api/WishItem` | all wish items |
| GET | `/api/WishItem/{id}` | one wish item (404 if not found) |
| POST | `/api/WishItem` | add one, returns it **with its new id** |
| PUT | `/api/WishItem` | update (id in the body) |
| DELETE | `/api/WishItem/{id}` | delete (204 No Content) |

Tables: `PreferenceType`, `Profile`, `GiftGroup`, `GroupMember`, `PreferenceItem`, `ExternalLink`,
`Occasion`, `Wishlist`, `WishItem`, `Reservation`, `Gift`, plus the two junction tables:

- `WishlistShare` – found by two ids: `/api/WishlistShare/{wishlistId}/{groupId}` (no PUT; delete and re-add instead)
- `GiftContribution` – `/api/GiftContribution/{giftId}/{groupMemberId}` (PUT changes the amount)

## Things worth knowing

- **Passwords**: `POST /api/Profile` takes a `password` (min. 8 characters), hashes it with BCrypt and stores
  only the hash in `passwordhash`. The API never sends the hash back, and `PUT` doesn't change the password.
- **Dates**: `dateOfBirth` and `eventDate` are `DateOnly` – send them as `"2026-12-03"`.
  `createdAt` and `reservedAt` are set by the database, so you don't send them.
- **Optional (NULL) columns** are nullable in C# (`int?`, `string?`), e.g. `honoreeId`, `occasionId`, `guestName`.
- **Invite codes**: leave `inviteCode` empty when creating a group and the API makes one (e.g. `GRP-4F7A2C`).
- **Database rules → 400 Bad Request**: if the database rejects something (duplicate email, a member with both a
  profile and a guest name, a status that isn't idea/bought/given, reserving an item twice, a wrong foreign key...),
  the API answers 400 with the rule's name instead of crashing. See the middleware in `Program.cs`.
- **CORS** is open so the Angular app on port 4200 can call the API.
