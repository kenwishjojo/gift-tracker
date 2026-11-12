# Gift Tracker

Help friends and family plan gifts for each other's occasions: birthdays, Christmas and anniversaries,
with gift ideas and gifts that several people split.

Student group project for the CBS course *Designing and Developing Digital Business Applications* (DDDBA).

## Status

| Part | What it is | Tech | Status |
|---|---|---|---|
| `Database/` | Script that creates the 6 tables with test data | PostgreSQL / pgAdmin 4 | ✅ Done |
| `Backend/` | The REST API and the code that talks to the database | C# / ASP.NET Core Web API, Npgsql | ✅ CRUD for all 6 tables + login |
| `Frontend/` | The web app | Angular 21, Angular Material | ✅ First version done |
| `docs/` | ER diagram and brand book (`GiftTracker-brandbog.pdf`) | | |

The frontend calls the API, and only the API talks to the database.

### What the app can do

- **Log in** with email and password (the first page), then land on **Home**.
- **Occasions**: see upcoming and past occasions as cards, and add, edit or delete an occasion.
- **Gifts**: add, edit and delete gift ideas for an occasion, with name, description, price, link and
  status (idea, planned, bought or given).
- **Split a gift**: choose how many people share the price. The app shows the total price and
  **your share**, on the gift form, the occasion page and the occasion cards.
- **Gifts are a surprise**: you don't see the gifts planned for your own occasions.
- **Profile page**: a person's details and the occasions where they get gifts.
- Design from our brand book: colours, fonts (Outfit, Inter) and components.

### What's next

- **Filtered endpoints** (step 4 of the plan), e.g. `GET /api/Gift/occasion/{id}`. Today the app gets
  all rows and filters them itself.
- **Sharing occasions** with other people (on hold: the group still has to choose a design).
- **Known limitation**: the login only identifies you in the app. The API has no tokens, so it doesn't
  know who is asking (e.g. Swagger still shows every gift). Real authentication is future work.

### The 6 tables

| Table | What it stores |
|---|---|
| `profile` | A user (`user` is a reserved word in PostgreSQL). The password is stored as a BCrypt hash |
| `connection` | A friend request between two profiles (`pending` or `accepted`) |
| `occasion` | An event for one recipient, e.g. a birthday |
| `gift` | A gift for an occasion, who buys it, and how many people split the price (`splitcount`). The recipient comes from the occasion |
| `gift_group` | People splitting one gift (at most one group per gift) |
| `group_member` | A member of a gift group, either `owner` (exactly one) or `viewer` |

Two rules are checked in the API because the database can't check them on its own:
a gift's buyer can't be the occasion's recipient, and a gift group has exactly one owner.

## Getting started

### Database
1. In pgAdmin 4, create a database, e.g. `app-gifttrackerdb` (use template0 if you get a locale error).
2. Open the Query Tool **on that database**, open `Database/GiftTrackerDb.sql` and press F5.

Running the script again deletes and recreates all tables with the test data. **Re-run it after
pulling** if the script has changed (e.g. the `splitcount` column was added in PR #7).

### Backend
1. Put your pgAdmin password and database name in `Backend/GiftTracker.API/appsettings.Local.json`
   (see `Backend/README.md`). The database name must match pgAdmin exactly, including capital letters.
2. From `Backend/`: `dotnet run --project GiftTracker.API`
3. Open http://localhost:5150/swagger

### Frontend
1. From `Frontend/gift-tracker-app`: `npm install` (only the first time)
2. Then `npx ng serve` and open http://localhost:4200 (or `npx ng serve --port 4201` if 4200 is busy)
3. Log in with a test profile, e.g. `anna.jensen@example.com`. The password is in the comment above
   the profile test data in `Database/GiftTrackerDb.sql`.

More detail in `Backend/README.md` and `Frontend/README.md`.

## Working together
- Pull before you start: `git pull`
- Work on a branch per feature, e.g. `git checkout -b occasion-sharing`
- Push your branch and open a pull request on GitHub so the group can review it
- Never commit passwords, `bin/`, `obj/`, `node_modules/` or `.angular/`
- Parts of the code were written with help from Claude Code – this is declared in the report (CBS GenAI rules)
