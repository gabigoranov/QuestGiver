import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import useAuth from "@/hooks/useAuth";
import { Eye, EyeOff, Mail } from "lucide-react";

// Shadcn UI components
import { Card    } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Link } from "react-router-dom";

/**
 * Zod schema for sign-in form validation
 */
const signInSchema = z.object({
  email: z.string().email({ message: "Invalid email address" }),
  password: z.string().min(6, { message: "Password must be at least 6 characters" }),
});

/**
 * SignIn Component
 *
 * A clean sign-in page using Shadcn UI components, Lucide icons, and React Hook Form + Zod validation
 */
export default function SignIn() {
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const { login } = useAuth();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<z.infer<typeof signInSchema>>({
    resolver: zodResolver(signInSchema),
  });

  const onSubmit = async (data: z.infer<typeof signInSchema>) => {
    setLoading(true);
    try {
      await login(data.email, data.password);
      console.log("Login successful");
    } catch (err) {
      console.error("Login failed:", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4">
      {/* Sign-in Card */}
      <Card className="w-full max-w-sm p-6 space-y-6 bg-card border-border rounded-3xl">
        {/* Heading */}
        <div className="text-center space-y-2">
          <h1 className="text-2xl font-bold tracking-tight">Welcome back</h1>
          <p className="text-sm text-on-surface-variant">
            Sign in to continue your journey
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Email Field */}
          <div className="space-y-1">
            <Label>Email</Label>
            <div className="relative flex items-center">
              <Mail className="absolute left-3 w-4 h-4 text-on-surface-variant" />
              <Input
                type="email"
                placeholder="you@example.com"
                {...register("email")}
                className="pl-10"
              />
            </div>
            {errors.email && <p className="text-xs text-red-500">{errors.email.message}</p>}
          </div>

          {/* Password Field */}
          <div className="space-y-1">
            <div className="flex items-center justify-between">
              <Label>Password</Label>
              <Button variant="link" size="sm" type="button">
                Forgot password?
              </Button>
            </div>
            <div className="relative flex items-center">
              <Input
                type={showPassword ? "text" : "password"}
                placeholder="••••••••"
                {...register("password")}
              />
              <Button
                variant="ghost"
                size="icon"
                type="button"
                className="absolute right-2 top-1/2 -translate-y-1/2 p-0"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? <EyeOff className="w-4 h-4" /> : <Eye className="w-4 h-4" />}
              </Button>
            </div>
            {errors.password && <p className="text-xs text-red-500">{errors.password.message}</p>}
          </div>

          {/* Submit Button */}
          <Button type="submit" className="w-full text-lg font-semibold" disabled={loading}>
            {loading ? "Signing in..." : "Sign in"}
          </Button>
        </form>

        {/* Footer */}
        <p className="mt-4 text-center text-xs text-on-surface-variant">
          Don't have an account?{" "}
          <Link to="/signup" className="font-semibold text-primary hover:underline">
            Sign up
          </Link>
        </p>
      </Card>
    </div>
  );
}