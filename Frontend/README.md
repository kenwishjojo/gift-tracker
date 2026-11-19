# Frontend – Gift Tracker web app

Angular app (Angular 21, standalone components, Angular Material) that calls the API in `../Backend`.
The look follows our brand book: `../docs/GiftTracker-brandbog.pdf`.

```
Frontend/gift-tracker-app/src/
├── styles.css            ← the design system: brand colours, fonts, .os-btn / .os-field / .os-occ / .os-badge
├── index.html            ← loads the fonts (Outfit, Inter) and the icon font
└── app/
    ├── model/            ← one TypeScript interface per table (Profile, Occasion, OccasionInvite, Gift, Connection, GiftGroup)
    │                       + small shared functions, e.g. canSee() and isPast() in occasion.ts
    ├── services/         ← one service per table (+ AuthService) – they call the API with HttpClient
    ├── login/            ← page: log in                         (/login – the first page)
    ├── signup/           ← page: create an account              (/signup)
    ├── terms/            ← page: terms and conditions           (/terms)
    ├── home/             ← page: landing page + feed of gifts you split (/home)
    ├── occasion-list/    ← page: all occasions                  (/occasions)
    ├── occasion-detail/  ← page: one occasion and its gifts     (/occasion/1)
    ├── occasion-form/    ← page: add or edit an occasion        (/occasion/new, /occasion/1/edit)
    ├── gift-form/        ← page: add or edit a gift             (/occasion/1/gift/new, /gift/1/edit)
    ├── profile/          ← page: one profile and their occasions (/profile/1)
    ├── friends/          ← page: friend requests and friends    (/friends)
    ├── occasion-card/    ← small component: one occasion as a card (used on the list and profile pages)
    ├── avatar/           ← small component: a person's initials in a circle
    ├── confirm-dialog/   ← small component: the warning box ("Are you sure?") used everywhere
    ├── app.routes.ts     ← which page to show for which URL
    └── app.config.ts     ← turns on the router and HttpClient
```

## Run it

1. Start the database and the backend first (see `../Backend/README.md`). The API must run on
   http://localhost:5150.
2. From `Frontend/gift-tracker-app`:
   ```bash
   npm install
   ```
   (only the first time, or after someone adds a package – it creates `node_modules/`, which is
   not in git)
3. Then:
   ```bash
   npx ng serve
   ```
4. Open **http://localhost:4200**. If port 4200 is busy, use `npx ng serve --port 4201`.
5. Log in with a test profile, e.g. `mads.hansen@example.com`. All test profiles have the password
   written in the comment above the test data in `../Database/GiftTrackerDb.sql`.

## How it meets the course requirements

| Requirement | Where |
|---|---|
| At least 4 components | login, signup, terms, home, occasion-list, occasion-detail, occasion-form, gift-form, profile, friends, occasion-card, avatar, confirm-dialog |
| Services that call the API | `services/` – `OccasionService`, `GiftService`, `ProfileService`, `ConnectionService`, `GiftGroupService`, `InviteService`, `AuthService` |
| Form with two-way binding | `signup`, `gift-form`, `occasion-form`, `friends` (add a friend) and `login`: every field uses `[(ngModel)]` |
| Basic validation | gift name `required`, price `min="0"`, status `required` from the list, split at least 2 people; occasion person, name and date required, type from the list; email/password on login; a valid email to add a friend; on sign-up: name, email, birthday (not in the future), password (at least 8) and repeat password (must match – cross-field), and the terms box must be ticked. Errors show under the field after you touch it, and Save is disabled while the form is invalid |

## Things worth knowing

- **Design (brand book)**: colours, corners and fonts are CSS variables in `src/styles.css`, and
  components only use the variables (never hex codes). One blue (primary) button per screen, black
  text (also on blue), black underlined links, status always written as text, 16px text, buttons and
  fields at least 44px high, spacing in steps of 8px. Angular Material is used for the top bar
  (`mat-toolbar`) and icons (`mat-icon`); the rest is plain HTML with the brand book's classes.
- **Route parameters as `@Input`**: `withComponentInputBinding()` in `app.config.ts` puts `:id` from
  the URL into `@Input() id` (Lecture 7). Route parameters are text, so we use `Number(this.id)`.
- **Gifts for one occasion / occasions for one person**: the backend has no filtered endpoints yet
  (step 4 of the plan), so the services get everything and filter it.
- **Who buys a new gift**: the logged-in user. The API doesn't allow the occasion's recipient to be
  the buyer, so on your own occasion the form shows a message instead.
- **Who sees an occasion** (`canSee()` in `model/occasion.ts`): its creator and the friends they invited –
  never the person it is for, so gifts stay a surprise. Your own occasions aren't listed anywhere, and
  opening one by its URL says "You aren't invited". This is a rule in the frontend only: the API has no
  tokens, so it doesn't know who is asking and still returns every row (e.g. in Swagger). Hiding it in
  the API would need real authentication (future work).
- **Friends only**: "Who is it for?" lists your friends (or "Someone without an account" + a name), and
  you can only invite friends. You can only split a gift with friends who are invited to its occasion.
- **Past occasions** (`isPast()`): no Edit/Add/Delete buttons for gifts and no editing the occasion; only
  the creator can still delete it. The API refuses the same changes.
- **The warning box** (`confirm-dialog/`): shown before every delete (gift, occasion, friend, account) and
  before saving changes to an existing gift or occasion. Data goes in with `@Input`; the answer comes back
  with `@Output` events, `(confirmed)` and `(cancelled)` – the child → parent half of `@Input`.
- **Splitting a gift with friends**: tick "Split the price with others", then tick the friends (at least
  one). Only your accepted friends are offered, never the person the gift is for. Saving stores the gift,
  then replaces its `gift_group`: the old group is deleted (its members go with it) and a new one is created
  with the buyer as owner and the friends as viewers. `splitCount` = friends + you. Your share is
  price ÷ people, calculated in `model/gift.ts` and never stored.
- **"Your share"** adds up your share of the gifts you buy **and** the gifts a friend splits with you.
- **Home feed**: the gifts you split (as buyer or friend) for upcoming occasions, built in the browser from
  gifts, occasions, groups and members (no filtered endpoints yet – step 4).
- **API errors**: if the backend says no (400), the form shows its message.
- **Why Angular 21 and not 22**: in Angular 22 a page doesn't update by itself when you set a
  field inside `subscribe(...)` (OnPush is the default there). Angular 21 with `zone.js` works the
  way we learned it in the lectures.
