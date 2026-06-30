export type LoginRequest = {
  email: string;
  password: string;
};

export type AuthResponse = {
  email: string;
  gymId: string;
  token: string;
  userId: string;
};
