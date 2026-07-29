import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { AxiosError } from "axios";
import { Input } from "../common/Input";
import { Button } from "../common/Button";
import { useAuth } from "../../hooks/useAuth";
import { useToast } from "../../hooks/useToast";

export function LoginForm() {
  const { login } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();

    if (!email || !password) {
      showToast("ID AND ACCESS KEY REQUIRED", "warning");
      return;
    }

    setIsSubmitting(true);
    showToast("EXECUTING AUTH HANDSHAKE...", "info");

    try {
      await login({ email, password });
      showToast("SESSION ESTABLISHED. REDIRECTING...", "success");
      navigate("/dashboard");
    } catch (error) {
      const message =
        error instanceof AxiosError && error.response?.status === 401
          ? "INVALID CREDENTIALS"
          : "AUTH HANDSHAKE FAILED";
      showToast(message, "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="flex flex-col gap-lg" onSubmit={handleSubmit}>
      <div className="flex flex-col gap-md">
        <Input
          label="User Identification (Email)"
          icon="alternate_email"
          type="email"
          placeholder="operator@obsidian.io"
          monoPlaceholder
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          autoComplete="email"
        />
        <Input
          label="Access Key (Password)"
          icon="lock_open"
          type="password"
          placeholder="••••••••••••"
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="current-password"
        />
      </div>

      <Button type="submit" icon="bolt" isLoading={isSubmitting}>
        Execute Login
      </Button>
    </form>
  );
}
