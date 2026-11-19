// Matches the GiftGroup class in the backend (table: gift_group) – the people splitting one gift.
export interface GiftGroup {
  id: number;
  giftId: number; // the gift being split (at most one group per gift)
  name: string;
  description: string | null;
  ownerId: number | null; // the member with role "owner" = the gift's buyer
  memberIds: number[]; // only used when creating a group (POST): the friends to add as viewers
  createdAt: string;
}

// Matches the GroupMember class in the backend (table: group_member)
export interface GroupMember {
  id: number;
  groupId: number;
  profileId: number;
  role: string; // "owner" or "viewer"
  joinedAt: string;
}

// The ids of the gifts a person is splitting: the gifts whose group they are a member of.
// Used for "Your share" and for the Home feed. (A free function like the ones in gift.ts.)
export function giftIdsSplitBy(profileId: number, groups: GiftGroup[], members: GroupMember[]): number[] {
  const ids: number[] = [];
  for (const member of members) {
    if (member.profileId === profileId) {
      const group = groups.find((g) => g.id === member.groupId);
      if (group) {
        ids.push(group.giftId);
      }
    }
  }
  return ids;
}

// Names as one readable phrase: ["Mads", "Anna", "Kasper"] -> "Mads, Anna and Kasper"
export function joinNames(names: string[]): string {
  if (names.length <= 1) {
    return names.join('');
  }
  return names.slice(0, -1).join(', ') + ' and ' + names[names.length - 1];
}
