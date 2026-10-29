# Gift Tracker

Help friends and family find the perfect gift: wishlists, groups, occasions and shared gifts.

## Project structure

| Folder | What it is | Tech | Status |
|---|---|---|---|
| `Database/` | Script that creates all 13 tables with test data (ER Design, Option C) | PostgreSQL / pgAdmin 4 | ✅ Done |
| `Backend/` | The REST API and the code that talks to the database | C# / ASP.NET Core Web API, Npgsql | ✅ CRUD for all 13 tables |
| `Frontend/` | The web app | Angular | 🚧 Not started |

The frontend will call the API, and only the API talks to the database.

> **Design change:** the group switched to the 6-table design in `docs/er-design-without-preferences.svg`.
> `Database/` and `Backend/` still implement the old 13-table design and will be rebuilt –
> see `CLAUDE.md` for the new tables and the plan.

## Getting started

### Database
1. In pgAdmin 4, create a database called `app-gifttrackerdb`.
2. Open the Query Tool on it, open `Database/GiftTrackerDb.sql` and press F5.

Running the script again deletes and recreates all tables.

### Backend
1. Put your pgAdmin password in `Backend/GiftTracker.API/appsettings.Local.json` (see `Backend/README.md`).
2. From `Backend/`: `dotnet run --project GiftTracker.API`
3. Open http://localhost:5150/swagger

## Working together
- Pull before you start: `git pull`
- Work on a branch per feature, e.g. `git checkout -b wishlist-page`
- Push your branch and open a pull request on GitHub so the group can review it
- Never commit passwords
