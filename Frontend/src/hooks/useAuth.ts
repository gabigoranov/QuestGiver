import { AuthContext } from "@/context/AuthContext";
import { useContext } from "react";

/**
 * Hook for using authentication
 *
 * @return {*} 
 */
export default function useAuth() {
  const context = useContext(AuthContext);
  if (!context) throw new Error("useAuth must be inside AuthProvider");
  return context;
}
