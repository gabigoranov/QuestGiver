import { api } from "./api";
import type { QuestDTO } from "@/types/Receive/QuestDTO";

const BASE_URL = "/quests";

// Service for handling authentication-related API calls
export const QuestsService = {
  getCurrentQuestForGroup: async (groupid: string): Promise<QuestDTO> => {
    const res = await api.get(`${BASE_URL}/group/${groupid}`);
    return res.data;
  },
  getAllUserQuests: async (): Promise<QuestDTO[]> => {
    const res = await api.get(`${BASE_URL}/history`);
    return res.data;
  },
};
