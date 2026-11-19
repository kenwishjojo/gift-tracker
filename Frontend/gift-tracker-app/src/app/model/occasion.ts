import { Profile } from './profile';

// Matches the Occasion class in the backend (table: occasion).
export interface Occasion {
  id: number;
  createdById: number; // the profile who made the occasion
  // Who gets the gifts: a profile (recipientId) OR, for someone without an account, just a name
  // (recipientName). Exactly one of the two is set (the database checks it).
  recipientId: number | null;
  recipientName: string | null;
  name: string;
  eventDate: string; // "YYYY-MM-DD"
  type: string; // "birthday", "christmas", "anniversary" or "other"
  notes: string | null;
}

// Matches the OccasionInvite class in the backend (table: occasion_invite)
export interface OccasionInvite {
  id: number;
  occasionId: number;
  profileId: number; // the invited friend
  invitedAt: string;
}

// The allowed values of occasion.type (same list as the check constraint in the database)
export const OCCASION_TYPES = ['birthday', 'christmas', 'anniversary', 'other'];

// ---- Free functions used by several pages (like the ones in gift.ts) ----

// The name of the person the occasion is for: the profile's name, or the typed name if they
// have no account
export function recipientNameOf(occasion: Occasion, profiles: Profile[]): string {
  if (occasion.recipientId === null) {
    return occasion.recipientName ?? '';
  }
  return profiles.find((p) => p.id === occasion.recipientId)?.fullName ?? '';
}

// Can this person see the occasion? Yes if they created it or are invited – but never when it is
// for them (the gifts are a surprise). This is a rule in the frontend only: the API has no login
// tokens, so it can't know who is asking.
export function canSee(occasion: Occasion, profileId: number, invites: OccasionInvite[]): boolean {
  if (occasion.recipientId === profileId) {
    return false;
  }
  const invited = invites.some((i) => i.occasionId === occasion.id && i.profileId === profileId);
  return occasion.createdById === profileId || invited;
}

// True when the date has passed. Past occasions are locked: they can be viewed and deleted, but
// not changed (the API checks the same rule).
export function isPast(occasion: Occasion): boolean {
  const now = new Date();
  const today = `${now.getFullYear()}-${String(now.getMonth() + 1).padStart(2, '0')}-${String(now.getDate()).padStart(2, '0')}`;
  return occasion.eventDate < today; // "YYYY-MM-DD" texts sort the same way as the dates
}
