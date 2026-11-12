// Matches the Gift class in the backend (table: gift).
// An interface only describes the shape of the JSON the API sends – it has no code of its own
// (like the Student interface in the course's CourseAdminSystem example).
// The recipient isn't stored on the gift – it is the recipientId of the gift's occasion.
export interface Gift {
  id: number;
  occasionId: number;
  buyerId: number; // can't be the occasion's recipient (checked by the API)
  name: string;
  description: string | null;
  price: number | null; // null if the price isn't known yet
  link: string | null;
  status: string; // "idea", "planned", "bought" or "given"
  splitCount: number; // how many people split the price, 1 = not split
}

// The allowed values of gift.status (same list as the check constraint in the database)
export const GIFT_STATUSES = ['idea', 'planned', 'bought', 'given'];

// The three functions below are plain "free functions", not methods in a class – normal in
// TypeScript, unlike C# where everything lives in a class (Lecture 6, TypeScript for C# programmers).
// They are shared by the gift form, the occasion page and the occasion card.

// What each person pays for a gift: the price divided by the number of people splitting it.
// Not stored in the database because it can always be calculated from price and splitCount (3NF).
// Returns null when the price isn't known yet.
export function shareOf(gift: Gift): number | null {
  // No price yet, or the number of people is empty/0 while the user is typing: nothing to show
  if (gift.price === null || !gift.splitCount) {
    return null;
  }
  return gift.price / gift.splitCount;
}

// The total price of a list of gifts (gifts without a price count as 0)
export function totalPrice(gifts: Gift[]): number {
  let total = 0;
  for (const gift of gifts) {
    total += gift.price ?? 0; // ?? uses 0 when the price is null
  }
  return total;
}

// What one person pays in total: their share of every gift they are the buyer of
export function totalShareFor(gifts: Gift[], profileId: number): number {
  let total = 0;
  for (const gift of gifts) {
    if (gift.buyerId === profileId) {
      total += shareOf(gift) ?? 0;
    }
  }
  return total;
}
