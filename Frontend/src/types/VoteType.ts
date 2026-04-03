export const VoteType = {
  CompletionVote: 0,
  SkipVote: 1
} as const;

export type VoteType =
  (typeof VoteType)[keyof typeof VoteType];