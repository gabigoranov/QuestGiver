export const QuestStatusType = {
  InProgress: 0,
  Completed: 1,
  New: 3,
} as const;

export type QuestStatusType =
  (typeof QuestStatusType)[keyof typeof QuestStatusType];