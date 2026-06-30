// Funciones utilitarias generales

export const formatDate = (date: Date): string => {
  return new Date(date).toLocaleDateString("es-ES");
};

export const formatCurrency = (amount: number): string => {
  return new Intl.NumberFormat("es-ES", {
    style: "currency",
    currency: "USD",
  }).format(amount);
};

export const validateEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

export const truncateText = (text: string, length: number): string => {
  return text.length > length ? text.substring(0, length) + "..." : text;
};
