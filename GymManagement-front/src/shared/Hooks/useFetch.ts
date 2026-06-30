import { useState, useEffect } from "react";

interface UseFetchOptions {
  skip?: boolean;
}

export const useFetch = <T>(
  fetchFn: () => Promise<T>,
  options: UseFetchOptions = {},
) => {
  const [data, setData] = useState<T | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (options.skip) {
      setLoading(false);
      return;
    }

    const fetchData = async () => {
      try {
        setLoading(true);
        const result = await fetchFn();
        setData(result);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Error desconocido");
      } finally {
        setLoading(false);
      }
    };

    fetchData();
  }, [fetchFn, options.skip]);

  return { data, loading, error };
};
