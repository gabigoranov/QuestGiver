import type { TokenDTO } from "@/types/Receive/TokenDTO";
import type { UserDTO } from "@/types/Receive/UserDTO";
import type { CreateUserDTO } from "@/types/Send/CreateUserDTO";
import { createContext } from "react";

// Define the shape of our authentication context
type AuthContextType = {
  user: UserDTO | null;
  token: TokenDTO | null;
  login: (email: string, password: string) => Promise<void>;
  signUp: (data: CreateUserDTO) => Promise<void>;
  refresh: () => Promise<void>;
  logout: () => void;
  loading: boolean;
};

export const AuthContext = createContext<AuthContextType | null>(null);
