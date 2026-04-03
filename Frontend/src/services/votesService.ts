import type { CreateVoteDTO } from "@/types/Send/CreateVoteDTO";
import { api } from "./api";
import type { VoteDTO } from "@/types/Receive/VoteDTO";

const BASE_URL = "/votes";

// Service for handling voting for quest completion / skipping
export const VotesService = {
  create: async (data: CreateVoteDTO): Promise<VoteDTO> => {
    const res = await api.post(`${BASE_URL}`, data);
    return res.data;
  },
  submitIndividualVote: async (voteId: string, decision: boolean) => {
    await api.post(`${BASE_URL}/${voteId}/vote`, {decision});
  },
  getQuestVote: async (questId: string): Promise<VoteDTO> => {
    const res = await api.get(`${BASE_URL}/quest/${questId}`);
    return res.data;
  },
};
