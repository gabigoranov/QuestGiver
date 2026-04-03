import { api } from "./api";
import type { QuestDTO } from "@/types/Receive/QuestDTO";

const BASE_URL = "/votes";

// Service for handling voting for quest completion / skipping
export const VotesService = {
  getCurrentQuestForGroup: async (groupid: string): Promise<QuestDTO> => {
    const res = await api.get(`${BASE_URL}/group/${groupid}`);
    return res.data;
  },
  
};
