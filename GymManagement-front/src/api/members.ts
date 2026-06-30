import { apiClient } from "./axios";
import type { Member } from "../shared/types";

export const membersAPI = {
  getAll: async () => {
    const response = await apiClient.get("/members");
    return response.data;
  },

  getById: async (id: string) => {
    const response = await apiClient.get(`/members/${id}`);
    return response.data;
  },

  create: async (memberData: Omit<Member, "id">) => {
    const response = await apiClient.post("/members", memberData);
    return response.data;
  },

  update: async (id: string, memberData: Partial<Member>) => {
    const response = await apiClient.put(`/members/${id}`, memberData);
    return response.data;
  },

  delete: async (id: string) => {
    return await apiClient.delete(`/members/${id}`);
  },

  updateStatus: async (id: string, status: Member["status"]) => {
    const response = await apiClient.patch(`/members/${id}/status`, { status });
    return response.data;
  },
};
