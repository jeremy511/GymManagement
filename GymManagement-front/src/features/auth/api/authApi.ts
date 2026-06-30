import  api from "../../../api/axios";
import type { AuthResponse, LoginRequest } from "../types";

export async function login(request: LoginRequest): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>("/Auth/login", request);
  return response.data;
}