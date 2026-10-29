# Gift Tracker

Help friends and family plan gifts for each other's occasions: birthdays, Christmas and anniversaries,
with gift ideas and gifts that several people split.

## Project structure

| Folder | What it is | Tech | Status |
|---|---|---|---|
| `Database/` | Script that creates the 6 tables with test data | PostgreSQL / pgAdmin 4 | ✅ Done |
| `Backend/` | The REST API and the code that talks to the database | C# / ASP.NET Core Web API, Npgsql | ✅ CRUD for all 6 tables |
| `Frontend/` | The web app | Angular, Angular Material | 🚧 In progress |
| `docs/` | ER diagram (`er-design-without-preferences.svg`) | | |

The frontend will call the API, and only the API talks to the database.

### The 6 tables

| Table | What it stores |
|---|---|
| `profile` | A user (`user` is a reserved word in PostgreSQL) |
| `connection` | A friend request between two profiles (`pending` or `accepted`) |
| `occasion` | An event for one recipient, e.g. a birthday |
| `gift` | A gift for an occasion and who buys it. The recipient comes from the occasion |
| `gift_group` | People splitting one gift (at most one group per gift) |
| `group_member` | A member of a gift group, either `owner` (exactly one) or `viewer` |

Two rules are checked in the API because the database can't check them on its own:
a gift's buyer can't be the occasion's recipient, and a gift group has exactly one owner.

## Getting started

### Database
1. In pgAdmin 4, create a database, e.g. `app-gifttrackerdb` (use template0 if you get a locale error).
2. Open the Query Tool **on that database**, open `Database/GiftTrackerDb.sql` and press F5.

Running the script again deletes and recreates all tables.

### Backend
1. Put your pgAdmin password and database name in `Backend/GiftTracker.API/appsettings.Local.json`
   (see `Backend/README.md`). The database name must match pgAdmin exactly, including capital letters.
2. From `Backend/`: `dotnet run --project GiftTracker.API`
3. Open http://localhost:5150/swagger

### Frontend
Coming soon: see `Frontend/` once the first pages are merged.

## Working together
- Pull before you start: `git pull`
- Work on a branch per feature, e.g. `git checkout -b frontend-occasions`
- Push your branch and open a pull request on GitHub so the group can review it
- Never commit passwords, `bin/`, `obj/`, `node_modules/` or `.angular/`
