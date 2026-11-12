// Matches the Occasion class in the backend (table: occasion).
export interface Occasion {
  id: number;
  createdById: number; // the profile who made the occasion
  recipientId: number; // the profile who gets the gifts
  name: string;
  eventDate: string; // "YYYY-MM-DD"
  type: string; // "birthday", "christmas", "anniversary" or "other"
  notes: string | null;
}

// The allowed values of occasion.type (same list as the check constraint in the database)
export const OCCASION_TYPES = ['birthday', 'christmas', 'anniversary', 'other'];
