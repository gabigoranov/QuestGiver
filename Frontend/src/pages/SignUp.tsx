import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import useAuth from "@/hooks/useAuth";
import { Eye, EyeOff, Mail, User, Calendar } from "lucide-react";

// Shadcn UI components
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Link, useNavigate } from "react-router-dom";

/**
 * Zod schema for sign-up form validation
 */
const signUpSchema = z.object({
  username: z.string().min(3, { message: "Username must be at least 3 characters" }),
  birthDate: z.string().refine(val => !isNaN(Date.parse(val)), { message: "Invalid date" }),
  description: z.string().max(200, { message: "Description must be 200 characters or less" }).optional(),
  email: z.string().email({ message: "Invalid email address" }),
  password: z.string().min(8, { message: "Password must be at least 8 characters" }),
});

/**
 * SignUp Component
 *
 * Renders a clean sign-up page using Shadcn UI components, Lucide icons,
 * and React Hook Form + Zod validation
 */
export default function SignUp() {
  const [showPassword, setShowPassword] = useState(false);
  const [loading, setLoading] = useState(false);
  const { signUp: authRegister } = useAuth();
  const navigate = useNavigate();

  // React Hook Form with Zod validation
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<z.infer<typeof signUpSchema>>({
    resolver: zodResolver(signUpSchema),
  });

  /**
   * Handle form submission
   */
  const onSubmit = async (data: z.infer<typeof signUpSchema>) => {
    setLoading(true);
    try {
      await authRegister({
        username: data.username,
        birthDate: new Date(data.birthDate),
        description: data.description || "",
        email: data.email,
        password: data.password,
      });
      console.log("Sign up successful");
      navigate("/home"); // Redirect to home after success
    } catch (err) {
      console.error("Sign up failed:", err);
      // TODO: show notification
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4">
      {/* Sign-up Card */}
      <Card className="w-full max-w-md p-6 space-y-6 bg-card border-border rounded-3xl">
        {/* Heading */}
        <div className="text-center space-y-2">
          <h1 className="text-2xl font-bold tracking-tight">Create your account</h1>
          <p className="text-sm text-on-surface-variant">
            Fill in your details to get started
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Username Field */}
          <div className="space-y-1">
            <Label>Username</Label>
            <div className="relative flex items-center">
              <User className="absolute left-3 w-4 h-4 text-on-surface-variant" />
              <Input
                type="text"
                placeholder="Your username"
                {...register("username")}
                className="pl-10"
              />
            </div>
            {errors.username && <p className="text-xs text-red-500">{errors.username.message}</p>}
          </div>

          {/* Birth Date Field */}
          <div className="space-y-1">
            <Label>Birth Date</Label>
            <div className="relative flex items-center">
              <Calendar className="absolute left-3 w-4 h-4 text-on-surface-variant" />
              <Input
                type="date"
                {...register("birthDate")}
                className="pl-10"
              />
            </div>
            {errors.birthDate && <p className="text-xs text-red-500">{errors.birthDate.message}</p>}
          </div>

          {/* Description Field */}
          <div className="space-y-1">
            <Label>Description</Label>
            <Textarea
              placeholder="Tell us a bit about yourself"
              {...register("description")}
              rows={3}
            />
            {errors.description && <p className="text-xs text-red-500">{errors.description.message}</p>}
          </div>

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
            <Label>Password</Label>
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
            {loading ? "Creating account..." : "Sign up"}
          </Button>
        </form>

        {/* Footer */}
        <p className="mt-4 text-center text-xs text-on-surface-variant">
          Already have an account?{" "}
          <Link to="/signin" className="font-semibold text-primary hover:underline">
            Sign in
          </Link>
        </p>
      </Card>
    </div>
  );
}