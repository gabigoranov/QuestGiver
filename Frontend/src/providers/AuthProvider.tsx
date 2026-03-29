import { AuthContext } from "@/context/AuthContext";
import { AuthService } from "@/services/authService";
import type { TokenDTO } from "@/types/Receive/TokenDTO";
import type { UserDTO } from "@/types/Receive/UserDTO";
import { useEffect, useState } from "react";


/**
 * Provides functions for authenticating the user
 *
 * @export
 * @param {{
 *   children: React.ReactNode;
 * }} {
 *   children,
 * }
 * @return {*} 
 */
export default function AuthProvider({
  children,
}: {
  children: React.ReactNode;
}) {
  const [user, setUser] = useState<UserDTO | null>(null);
  const [token, setToken] = useState<TokenDTO | null>(null);

  const login = async (email: string, password: string) => {
    const res = await AuthService.login(email, password);

    setUser(res.user);
    setToken(res.token);

    // Save to local storage in case of a page refresh
    localStorage.setItem("refreshToken", token!.refreshToken);
    localStorage.setItem("accessToken", token!.accessToken);

    console.log(res);

    // TODO: Navigate to home page
  };

  const logout = () => {
    // Clear everything

    setUser(null);
    setToken(null);
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("accessToken");
  };

  // On app load, try to restore user from local storage (if refresh token exists)
  useEffect(() => {
    // runs once when AuthProvider mounts
    const token = localStorage.getItem("refreshToken");
    if (token) {
      // call API to restore user
      AuthService.refresh(token)
        .then((res) => {
            setUser(res.user);
            setToken(res.token);
        })
        .catch(() => {
          // token invalid, logout
          logout();
        });
    }
  }, []); // <- empty array = run once

  return (
    <AuthContext.Provider value={{ user, token, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
