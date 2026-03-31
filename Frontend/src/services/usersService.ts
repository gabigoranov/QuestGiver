import type { UserDTO } from "@/types/Receive/UserDTO";
import { api } from "./api";

const BASE_URL = "/users";

// Service for handling authentication-related API calls
export const UsersService = {
  getById: async (userId: string): Promise<UserDTO> => {
    const res = await api.get(`${BASE_URL}/${userId}`);
    return res.data;
  },
  
};
