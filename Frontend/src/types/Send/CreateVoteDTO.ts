import type { VoteType } from "../VoteType"

export type CreateVoteDTO = {
    description: string,
    discriminator: VoteType,
    questId: string
}