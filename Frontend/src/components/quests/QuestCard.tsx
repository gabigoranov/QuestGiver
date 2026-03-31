import type { QuestDTO } from "@/types/Receive/QuestDTO";
import { LucideCircleStar } from "lucide-react";
import { Button } from "../ui/button";
import InfoTag from "../common/InfoTag";

/**
 * QuestCard Component
 * Displays today's quest information, XP reward, chosen user, and action button.
 *
 * @param {Object} props
 * @param {QuestDTO} props.quest - The quest data to display
 * @returns JSX.Element
 *
 * @export
 */
export default function QuestCard({ quest }: { quest: QuestDTO }) {
  /**
   * TODO: Load the chosen user data via a query
   * Example:
   * const { data: chosenUser } = useQuery(["user", quest.userId], () => UsersService.getUserById(quest.userId));
   */
  const chosenUser = {
    username: "@Mandi_UQ",
    avatarUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQtkNjyu1q4JBpkQ4VtBkQ45ambxvph6Uxe6Q&s", // replace with actual data
  };

  return (
    <div className="bg-card rounded-2xl p-6 w-full max-w-sm shadow-glow-soft flex flex-col gap-4">
      {/* Header: Today's quest label + XP */}
      <div className="flex justify-between items-center mb-4">
        <span className="text-xs text-primary uppercase tracking-widest font-semibold">
          Today's Quest
        </span>
        <InfoTag title={`${quest.rewardPoints} XP`} icon={<LucideCircleStar size={16}/>} colorVariant="tertiary"/>
      </div>

      {/* Quest Title */}
      <h2 className="text-text font-heading text-3xl font-bold">{quest.title}</h2>

      {/* Quest Description */}
      <p className="text-muted-foreground text-lg">{quest.description}</p>

      {/* Chosen User Section */}
      <div className="flex items-center gap-3 bg-background/60 rounded-xl border border-border px-3 py-4 mt-4 mb-4">
        <img
          src={chosenUser.avatarUrl}
          alt={chosenUser.username}
          className="w-8 h-8 rounded-full"
        />
        <span className="text-primary text-md font-medium">
          {chosenUser.username}
        </span>
      </div>

      {/* Action Button */}
      <Button className="text-xl py-8 rounded-full font-bold shadow-glow-primary">
        Upload Evidence
      </Button>
    </div>
  );
}