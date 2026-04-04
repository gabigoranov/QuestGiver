import useAuth from "@/hooks/useAuth";
import { Navigate } from "react-router-dom";

export default function ProtectedRoute({ children }: { children: React.ReactNode }) {
  const { token, loading } = useAuth();

  if (loading) return null; // small skeleton or nothing

  return token ? <>{children}</> : <Navigate to="/signin" />;
}
