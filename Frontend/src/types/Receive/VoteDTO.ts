import type { VoteType } from "../VoteType";
import type { UserVoteDTO } from "./UserVoteDTO";

export type VoteDTO = {
    id: string,
    description: string,
    discriminator: VoteType,
    dateCreated: Date,
    questId: string,
    decision?: boolean,
    userVotes: UserVoteDTO[]
}