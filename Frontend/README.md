# Frontend – Gift Tracker web app

Angular app (Angular 21, standalone components, Angular Material) that calls the API in `../Backend`.

```
Frontend/gift-tracker-app/src/app/
├── model/              ← one TypeScript interface per table we use (Profile, Occasion, Gift)
├── services/           ← one service per table – they call the API with HttpClient
├── occasion-list/      ← page: all occasions                 (/occasions)
├── occasion-detail/    ← page: one occasion and its gifts    (/occasion/1)
├── gift-form/          ← page: add or edit a gift            (/occasion/1/gift/new, /gift/1/edit)
├── profile/            ← page: one profile                   (/profile/1)
├── app.routes.ts       ← which page to show for which URL
└── app.config.ts       ← turns on the router and HttpClient
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

## Things worth knowing

- **API address**: each service has `baseUrl = 'http://localhost:5150/api/...'`.
- **Gifts for one occasion**: the backend has no filtered endpoint yet (step 4 of the plan), so
  `GiftService.getGiftsForOccasion()` gets all gifts and keeps the ones with the right `occasionId`.
- **No login yet**: "My profile" in the menu shows profile 1 (Anna Jensen).
- **Why Angular 21 and not 22**: in Angular 22 a page doesn't update by itself when you set a
  field inside `subscribe(...)` (OnPush is the default there). Angular 21 with `zone.js` works the
  way we learned it in the lectures.

## Status

Groundwork only: the four pages load their data from the API, but they have no design yet (that
waits for our brand book). The gift form page is still a placeholder – the form with
`[(ngModel)]` and validation (name required, price ≥ 0, status from the list) comes next.
