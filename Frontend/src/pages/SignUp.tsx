import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTranslation } from "react-i18next";
import useAuth from "@/hooks/useAuth";
import { Eye, EyeOff, Mail, User, Calendar } from "lucide-react";

// Shadcn UI components
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Link, useNavigate } from "react-router-dom";
import {
  GoogleLogin,
  type GoogleCredentialResponse,
} from "@react-oauth/google";

/**
 * Zod schema for sign-up form validation
 */
const signUpSchema = z.object({
  username: z
    .string()
    .min(3, { message: "Username must be at least 3 characters" }),
  birthDate: z
    .string()
    .refine((val) => !isNaN(Date.parse(val)), { message: "Invalid date" }),
  description: z
    .string()
    .max(200, { message: "Description must be 200 characters or less" })
    .optional(),
  email: z.string().email({ message: "Invalid email address" }),
  password: z
    .string()
    .min(8, { message: "Password must be at least 8 characters" }),
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
  const { t } = useTranslation();
  const { signUp: authRegister, loginWithGoogle } = useAuth();
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

  const handleGoogleLogin = async (
    credentialResponse: GoogleCredentialResponse,
  ) => {
    setLoading(true);

    try {
      if (!credentialResponse?.credential) return;

      await loginWithGoogle(credentialResponse.credential);

      // TODO: Handle setting interests if they are null

      navigate("/home"); // redirect after login
    } catch (err) {
      console.error("Google login error:", err);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4">
      {/* Sign-up Card */}
      <Card className="w-full max-w-md p-6 space-y-6 bg-card border-border rounded-3xl">
        {/* Heading */}
        <div className="text-center space-y-2">
          <h1 className="text-2xl font-bold tracking-tight">
            {t("auth.signUp.title")}
          </h1>
          <p className="text-sm text-on-surface-variant">
            {t("auth.signUp.subtitle")}
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Username Field */}
          <div className="space-y-1">
            <Label>{t("auth.signUp.username")}</Label>
            <div className="relative flex items-center">
              <User className="absolute left-3 w-4 h-4 text-on-surface-variant" />
              <Input
                type="text"
                placeholder={t("auth.signUp.usernamePlaceholder")}
                {...register("username")}
                className="pl-10"
              />
            </div>
            {errors.username && (
              <p className="text-xs text-error">{errors.username.message}</p>
            )}
          </div>

          {/* Birth Date Field */}
          <div className="space-y-1">
            <Label>{t("auth.signUp.birthDate")}</Label>
            <div className="relative flex items-center">
              <Calendar className="absolute left-3 w-4 h-4 text-on-surface-variant" />
              <Input type="date" {...register("birthDate")} className="pl-10" />
            </div>
            {errors.birthDate && (
              <p className="text-xs text-error">{errors.birthDate.message}</p>
            )}
          </div>

          {/* Description Field */}
          <div className="space-y-1">
            <Label>{t("auth.signUp.description")}</Label>
            <Textarea
              placeholder={t("auth.signUp.descriptionPlaceholder")}
              {...register("description")}
              rows={3}
            />
            {errors.description && (
              <p className="text-xs text-error">{errors.description.message}</p>
            )}
          </div>

          {/* Email Field */}
          <div className="space-y-1">
            <Label>{t("auth.signUp.email")}</Label>
            <div className="relative flex items-center">
              <Mail className="absolute left-3 w-4 h-4 text-on-surface-variant" />
              <Input
                type="email"
                placeholder={t("auth.signUp.emailPlaceholder")}
                {...register("email")}
                className="pl-10"
              />
            </div>
            {errors.email && (
              <p className="text-xs text-error">{errors.email.message}</p>
            )}
          </div>

          {/* Password Field */}
          <div className="space-y-1">
            <Label>{t("auth.signUp.password")}</Label>
            <div className="relative flex items-center">
              <Input
                type={showPassword ? "text" : "password"}
                placeholder={t("auth.signUp.passwordPlaceholder")}
                {...register("password")}
              />
              <Button
                variant="ghost"
                size="icon"
                type="button"
                className="absolute right-2 top-1/2 -translate-y-1/2 p-0"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? (
                  <EyeOff className="w-4 h-4" />
                ) : (
                  <Eye className="w-4 h-4" />
                )}
              </Button>
            </div>
            {errors.password && (
              <p className="text-xs text-error">{errors.password.message}</p>
            )}
          </div>

          {/* Submit Button */}
          <Button
            type="submit"
            className="w-full text-lg font-semibold"
            disabled={loading}
          >
            {loading ? t("auth.signUp.submitting") : t("auth.signUp.submit")}
          </Button>
        </form>

        <GoogleLogin
          onSuccess={handleGoogleLogin}
          onError={() => console.error("Login Failed")}
          shape="pill"
          theme="outline"
          size="large"
        />

        {/* Footer */}
        <p className="mt-4 text-center text-xs text-on-surface-variant">
          {t("auth.signUp.hasAccount")}{" "}
          <Link
            to="/signin"
            className="font-semibold text-primary hover:underline"
            viewTransition
          >
            {t("auth.signUp.signIn")}
          </Link>
        </p>
      </Card>
    </div>
  );
}

