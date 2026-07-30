import { useState, type FormEvent } from "react";
import { useNavigate } from "react-router-dom";
import { AxiosError } from "axios";
import { Input } from "../common/Input";
import { Button } from "../common/Button";
import { useAuth } from "../../hooks/useAuth";
import { useToast } from "../../hooks/useToast";

export function RegisterForm() {
  const { register } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();

    if (!userName || !email || !password) {
      showToast("ALL FIELDS REQUIRED", "warning");
      return;
    }
    if (password.length < 6) {
      showToast("ACCESS KEY MUST BE 6+ CHARACTERS", "warning");
      return;
    }

    setIsSubmitting(true);
    showToast("REGISTERING NEW NODE...", "info");

    try {
      await register({ userName, email, password });
      showToast("NODE REGISTERED. REDIRECTING...", "success");
      navigate("/dashboard");
    } catch (error) {
      const message =
        error instanceof AxiosError && error.response?.status === 400
          ? "REGISTRATION REJECTED — CHECK DETAILS"
          : "REGISTRATION FAILED";
      showToast(message, "error");
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="flex flex-col gap-lg" onSubmit={handleSubmit}>
      <div className="flex flex-col gap-md">
        <Input
          label="Operator Callsign (Username)"
          icon="badge"
          type="text"
          placeholder="night_trader"
          monoPlaceholder
          value={userName}
          onChange={(e) => setUserName(e.target.value)}
          autoComplete="username"
        />
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
          placeholder="min. 6 characters"
          monoPlaceholder
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          autoComplete="new-password"
        />
      </div>

      <Button type="submit" variant="secondary" icon="person_add" isLoading={isSubmitting}>
        Create Account
      </Button>
    </form>
  );
}
