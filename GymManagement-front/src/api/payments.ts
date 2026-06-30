import { apiClient } from "./axios";
import type { Payment } from "../shared/types";

export const paymentsAPI = {
  getAll: async () => {
    const response = await apiClient.get("/payments");
    return response.data;
  },

  getById: async (id: string) => {
    const response = await apiClient.get(`/payments/${id}`);
    return response.data;
  },

  getByMemberId: async (memberId: string) => {
    const response = await apiClient.get(`/payments/member/${memberId}`);
    return response.data;
  },

  create: async (paymentData: Omit<Payment, "id">) => {
    const response = await apiClient.post("/payments", paymentData);
    return response.data;
  },

  update: async (id: string, paymentData: Partial<Payment>) => {
    const response = await apiClient.put(`/payments/${id}`, paymentData);
    return response.data;
  },

  delete: async (id: string) => {
    return await apiClient.delete(`/payments/${id}`);
  },

  getPaymentStats: async () => {
    const response = await apiClient.get("/payments/stats");
    return response.data;
  },
};
