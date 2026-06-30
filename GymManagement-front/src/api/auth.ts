import { apiClient } from "./axios";
import type { User } from "../shared/types";

export const authAPI = {
  login: async (email: string, password: string) => {
    const response = await apiClient.post("/auth/login", { email, password });
    return response.data;
  },

  logout: async () => {
    return await apiClient.post("/auth/logout");
  },

  register: async (
    userData: Omit<User, "id" | "role"> & { password: string },
  ) => {
    const response = await apiClient.post("/auth/register", userData);
    return response.data;
  },

  getCurrentUser: async () => {
    const response = await apiClient.get("/auth/me");
    return response.data;
  },

  refreshToken: async () => {
    const response = await apiClient.post("/auth/refresh");
    return response.data;
  },
};
