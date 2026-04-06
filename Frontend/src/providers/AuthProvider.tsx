import { AuthContext } from "@/context/AuthContext";
import { AuthService } from "@/services/authService";
import type { TokenDTO } from "@/types/Receive/TokenDTO";
import type { UserDTO } from "@/types/Receive/UserDTO";
import type { CreateUserDTO } from "@/types/Send/CreateUserDTO";
import { useEffect, useState } from "react";

export default function AuthProvider({ children }: { children: React.ReactNode }) {
  const [token, setToken] = useState<TokenDTO | null>(null);
  const [loading, setLoading] = useState(true); // tracks initial auth restore

  const saveData = (user: UserDTO, token: TokenDTO) => {
    setToken(token);
    localStorage.setItem("refreshToken", token.refreshToken);
    localStorage.setItem("accessToken", token.accessToken);
    
    console.log(user);
    console.log(token);
  };

  const login = async (email: string, password: string) => {
    const res = await AuthService.login(email, password);
    saveData(res.user, res.token);
  };

  const logout = () => {
    setToken(null);
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("accessToken");
  };

  const signUp = async (data: CreateUserDTO) => {
    const res = await AuthService.signup(data);
    saveData(res.user, res.token);
  };

  const loginWithGoogle = async (credential: string) => {
    const res = await AuthService.loginWithGoogle(credential);
    saveData(res.user, res.token);
  };

  const refreshOnce = async () => {
    const refreshToken = localStorage.getItem("refreshToken");
    if (!refreshToken) {
      logout();
      return;
    }

    try {
      const res = await AuthService.refreshOnce(refreshToken);
      saveData(res.user, res.token);
    } catch {
      logout();
    }
  };

  // Restore user from localStorage on mount
  useEffect(() => {
    const initAuth = async () => {
      const refreshToken = localStorage.getItem("refreshToken");
      if (!refreshToken) {
        setLoading(false);
        return;
      }

      try {
        const res = await AuthService.refreshOnce(refreshToken);
        saveData(res.user, res.token);
      } catch {
        //logout();
      } finally {
        setLoading(false); // Done restoring
      }
    };

    initAuth();
  }, []);

  return (
    <AuthContext.Provider value={{ token, login, loginWithGoogle, signUp, refreshOnce, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
}