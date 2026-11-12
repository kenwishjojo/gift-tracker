# Frontend – Gift Tracker web app

Angular app (Angular 21, standalone components, Angular Material) that calls the API in `../Backend`.
The look follows our brand book: `../docs/GiftTracker-brandbog.pdf`.

```
Frontend/gift-tracker-app/src/
├── styles.css            ← the design system: brand colours, fonts, .os-btn / .os-field / .os-occ / .os-badge
├── index.html            ← loads the fonts (Outfit, Inter) and the icon font
└── app/
    ├── model/            ← one TypeScript interface per table we use (Profile, Occasion, Gift)
    ├── services/         ← one service per table (+ AuthService) – they call the API with HttpClient
    ├── login/            ← page: log in                         (/login – the first page)
    ├── home/             ← page: landing page after login       (/home)
    ├── occasion-list/    ← page: all occasions                  (/occasions)
    ├── occasion-detail/  ← page: one occasion and its gifts     (/occasion/1)
    ├── gift-form/        ← page: add or edit a gift             (/occasion/1/gift/new, /gift/1/edit)
    ├── profile/          ← page: one profile and their occasions (/profile/1)
    ├── occasion-card/    ← small component: one occasion as a card (used on the list and profile pages)
    ├── avatar/           ← small component: a person's initials in a circle
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
| At least 4 components | login, home, occasion-list, occasion-detail, gift-form, profile, occasion-card, avatar |
| Services that call the API | `services/` – `OccasionService`, `GiftService`, `ProfileService`, `AuthService` |
| Form with two-way binding | `gift-form` (and `login`): every field uses `[(ngModel)]` |
| Basic validation | gift name `required`, price `min="0"`, status `required` from the list; email/password on login. Errors show under the field after you touch it, and Save is disabled while the form is invalid |

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
- **Gifts are a surprise**: on your own occasions you don't see the gifts, their prices or whether
  one is bought – not on the occasion page, the occasion cards, your profile or `/gift/{id}/edit`.
  This is a rule in the frontend only: the API has no tokens, so it doesn't know who is asking and
  `GET /api/Gift` still returns every gift (e.g. in Swagger). Hiding it in the API would need real
  authentication (future work).
- **Splitting a gift**: tick "Split the price with others" and enter how many people (at least 2).
  Your share is price ÷ people, calculated in `model/gift.ts` and never stored. "Your share" on an
  occasion adds up your share of the gifts you are the buyer of.
- **API errors**: if the backend says no (400), the form shows its message.
- **Why Angular 21 and not 22**: in Angular 22 a page doesn't update by itself when you set a
  field inside `subscribe(...)` (OnPush is the default there). Angular 21 with `zone.js` works the
  way we learned it in the lectures.
