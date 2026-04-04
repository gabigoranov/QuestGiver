import type { TokenDTO } from "@/types/Receive/TokenDTO";
import type { CreateUserDTO } from "@/types/Send/CreateUserDTO";
import { createContext } from "react";

// Define the shape of our authentication context
type AuthContextType = {
  token: TokenDTO | null;
  login: (email: string, password: string) => Promise<void>;
  signUp: (data: CreateUserDTO) => Promise<void>;
  refreshOnce: () => Promise<void>;
  logout: () => void;
  loading: boolean;
};

export const AuthContext = createContext<AuthContextType | null>(null);
