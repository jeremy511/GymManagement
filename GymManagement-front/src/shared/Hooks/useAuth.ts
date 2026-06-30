import { useState, useCallback } from "react";
import { authAPI } from "../../api/auth";
import { User } from "../types";

export const useAuth = () => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const login = useCallback(async (email: string, password: string) => {
    setLoading(true);
    setError(null);
    try {
      const data = await authAPI.login(email, password);
      localStorage.setItem("token", data.token);
      setUser(data.user);
      return data;
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Error al iniciar sesión";
      setError(message);
      throw err;
    } finally {
      setLoading(false);
    }
  }, []);

  const logout = useCallback(async () => {
    setLoading(true);
    try {
      await authAPI.logout();
      localStorage.removeItem("token");
      setUser(null);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Error al cerrar sesión";
      setError(message);
    } finally {
      setLoading(false);
    }
  }, []);

  return { user, loading, error, login, logout, setUser };
};
