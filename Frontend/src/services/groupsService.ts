import type { AuthResponse } from "@/types/Receive/AuthResponse";
import { api } from "./api";
import type { GroupDTO } from "@/types/Receive/GroupDTO";
import type { CreateGroupDTO } from "@/pages/CreateGroup";

const BASE_URL = "/groups";

// Service for handling authentication-related API calls
export const GroupsService = {
  getUserGroups: async (): Promise<GroupDTO[]> => {
    const res = await api.get(`${BASE_URL}`);
    return res.data;
  },
  getGroupById: async (groupId: string): Promise<GroupDTO> => {
    const res = await api.get(`${BASE_URL}/${groupId}`);
    return res.data;
  },
  create: async (data: CreateGroupDTO): Promise<GroupDTO> => {
    const res = await api.post(`${BASE_URL}`, data);
    return res.data;
  },
  // Used to refresh the auth ( JWT ) token when it expires
  join: async (groupId: string): Promise<AuthResponse> => {
    const res = await api.post(`${BASE_URL}/join`, {groupId});
    return res.data;
  },
  leave: async (groupId: string): Promise<void> => {
    await api.post(`${BASE_URL}/leave`, {groupId});
  },
};
