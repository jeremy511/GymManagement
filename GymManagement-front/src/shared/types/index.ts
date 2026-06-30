// Tipos generales de la aplicación
export interface User {
  id: string;
  email: string;
  name: string;
  role: "admin" | "member" | "instructor";
}

export interface Member {
  id: string;
  name: string;
  email: string;
  phone: string;
  joinDate: Date;
  status: "active" | "inactive" | "paused";
}

export interface Class {
  id: string;
  name: string;
  description: string;
  instructor: string;
  schedule: string;
  capacity: number;
  enrolled: number;
}

export interface Payment {
  id: string;
  memberId: string;
  amount: number;
  date: Date;
  status: "pending" | "completed" | "failed";
  method: "card" | "cash" | "transfer";
}
