// Matches the Connection class in the backend (table: connection) – a friend request between two profiles.
export interface Connection {
  id: number;
  requesterId: number; // the profile who sent the request
  addresseeId: number; // the profile who received it (only they can accept)
  status: string; // "pending" (waiting for an answer) or "accepted" (they are friends)
  createdAt: string; // set by the database
}
