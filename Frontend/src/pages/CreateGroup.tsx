import { useState } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Button } from "@/components/ui/button";
import { useNavigate } from "react-router-dom";
import { GroupsService } from "@/services/groupsService";

/**
 * Zod schema for CreateGroup form validation
 */
const createGroupSchema = z.object({
  title: z.string().min(3, { message: "Title must be at least 3 characters" }),
  description: z.string().min(10, { message: "Description must be at least 10 characters" }),
});

export type CreateGroupDTO = z.infer<typeof createGroupSchema>;

/**
 * CreateGroup Component
 */
export default function CreateGroup() {
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<CreateGroupDTO>({
    resolver: zodResolver(createGroupSchema),
  });

  const onSubmit = async (data: CreateGroupDTO) => {
    setLoading(true);
    try {
      await GroupsService.create(data);
      console.log("Group created successfully");
      navigate("/home"); // Redirect to home after success
    } catch (err) {
      console.error("Failed to create group:", err);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-background px-4">
      <Card className="w-full max-w-md p-6 space-y-6 bg-card border-border rounded-3xl">
        {/* Heading */}
        <div className="text-center space-y-2">
          <h1 className="text-2xl font-bold tracking-tight">Create a Friend Group</h1>
          <p className="text-sm text-on-surface-variant">
            Fill out the details to start a new group
          </p>
        </div>

        {/* Form */}
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Title Field */}
          <div className="space-y-1">
            <Label>Group Title</Label>
            <Input
              type="text"
              placeholder="My Awesome Friends"
              {...register("title")}
            />
            {errors.title && <p className="text-xs text-error">{errors.title.message}</p>}
          </div>

          {/* Description Field */}
          <div className="space-y-1">
            <Label>Description</Label>
            <Textarea
              placeholder="A short description of your friend group..."
              {...register("description")}
              rows={4}
            />
            {errors.description && (
              <p className="text-xs text-error">{errors.description.message}</p>
            )}
          </div>

          {/* Submit Button */}
          <Button type="submit" className="w-full text-lg font-semibold" disabled={loading}>
            {loading ? "Creating..." : "Create Group"}
          </Button>
        </form>
      </Card>
    </div>
  );
}