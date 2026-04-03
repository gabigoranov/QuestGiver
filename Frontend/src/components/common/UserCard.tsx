import type { UserDTO } from "@/types/Receive/UserDTO";
import { User } from "lucide-react"; // Default user icon
import { Avatar, AvatarFallback, AvatarImage } from "../ui/avatar";

type UserCardProps = {
  user: UserDTO;
  highlight?: boolean; // Optional: highlight the logged-in user
};

/**
 * UserCard component
 *
 * Displays a user's avatar (or default icon), username, description snippet,
 * and level. Can optionally highlight the logged-in user.
 *
 * @param {UserCardProps} props
 * @param {UserDTO} props.user - The user to display
 * @param {boolean} [props.highlight=false] - Whether to visually highlight the user
 *
 * @returns JSX.Element
 */
export default function UserCard({ user, highlight = false }: UserCardProps) {
  return (
    <div
      className={`flex items-center gap-4 p-3 rounded-lg border ${
        highlight ? "border-primary bg-primary/10" : "border-border bg-background"
      }`}
    >
      {/* Avatar */}
      <Avatar className="w-12 h-12">
        {user.avatarUrl ? (
          <AvatarImage src={user.avatarUrl} alt={user.username} />
        ) : (
          <AvatarFallback>
            <User className="w-6 h-6 text-muted-foreground" />
          </AvatarFallback>
        )}
      </Avatar>

      {/* User info */}
      <div className="flex-1 flex flex-col">
        {/* Username */}
        <span className="font-semibold text-base text-foreground">{user.username}</span>

        {/* Description snippet */}
        <span className="text-sm text-muted-foreground truncate w-45">
          {user.description || "No description provided."}
        </span>
      </div>

      {/* Level display */}
      <div className="flex flex-col items-end">
        <span className="text-sm font-medium text-foreground">Lvl {user.level}</span>
        <span className="text-xs text-muted-foreground">
          {user.experiencePoints}/{user.nextLevelExperience} XP
        </span>
      </div>
    </div>
  );
}