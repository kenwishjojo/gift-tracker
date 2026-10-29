// Matches the Gift class in the backend (table: gift).
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
}

// The allowed values of gift.status (same list as the check constraint in the database)
export const GIFT_STATUSES = ['idea', 'planned', 'bought', 'given'];
