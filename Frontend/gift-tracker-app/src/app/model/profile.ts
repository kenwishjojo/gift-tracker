// Matches the Profile class in the backend (table: profile).
// The API never sends the password hash, so it isn't here.
export interface Profile {
  id: number;
  fullName: string;
  email: string;
  dateOfBirth: string; // "YYYY-MM-DD"
  profilePictureUrl: string | null;
  createdAt: string; // set by the database
}
