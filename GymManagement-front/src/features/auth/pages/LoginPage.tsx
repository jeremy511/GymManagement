import { useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { login } from "../api/authApi";

const loginSchema = z.object({
  email: z.string().email("Email inválido"),
  password: z.string().min(1, "La contraseña es requerida"),
});

type LoginFormValues = z.infer<typeof loginSchema>;

export function LoginPage() {
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: "admin@irontemple.com",
      password: "Admin123!",
    },
  });

  async function onSubmit(values: LoginFormValues) {
    try {
      setError(null);

      const response = await login(values);

      localStorage.setItem("access_token", response.token);
      localStorage.setItem("user_id", response.userId);
      localStorage.setItem("gym_id", response.gymId);
      localStorage.setItem("email", response.email);

      window.location.href = "/dashboard";
    } catch {
      setError("Credenciales incorrectas o error conectando con la API.");
    }
  }

  return (
    <main style={{ maxWidth: 400, margin: "80px auto", fontFamily: "Arial" }}>
      <h1>GymManagement</h1>
      <h2>Login</h2>

      <form onSubmit={handleSubmit(onSubmit)}>
        <div style={{ marginBottom: 12 }}>
          <label>Email</label>
          <input
            type="email"
            {...register("email")}
            style={{ width: "100%", padding: 8 }}
          />
          {errors.email && <p>{errors.email.message}</p>}
        </div>

        <div style={{ marginBottom: 12 }}>
          <label>Password</label>
          <input
            type="password"
            {...register("password")}
            style={{ width: "100%", padding: 8 }}
          />
          {errors.password && <p>{errors.password.message}</p>}
        </div>

        {error && <p style={{ color: "red" }}>{error}</p>}

        <button type="submit" disabled={isSubmitting}>
          {isSubmitting ? "Entrando..." : "Entrar"}
        </button>
      </form>
    </main>
  );
}