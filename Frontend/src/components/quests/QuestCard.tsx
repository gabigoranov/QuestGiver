import type { QuestDTO } from "@/types/Receive/QuestDTO";
import { LucideCircleStar } from "lucide-react";
import InfoTag from "../common/InfoTag";
import { UsersService } from "@/services/usersService";
import { useQuery } from "@tanstack/react-query";
import useAuth from "@/hooks/useAuth";
import { VotesService } from "@/services/votesService";
import QuestCardVote from "./QuestCardVote";
import QuestCardCreateSkipVote from "./QuestCardCreateSkipVote";
import QuestCardCreateCompletionVote from "./QuestCardCreateCompletionVote";

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
export default function QuestCard({ quest, groupId }: { quest: QuestDTO, groupId: string }) {
  const { user: appUser } = useAuth();

  // Load the current user for the group
  const { isPending: isUserPending, data: chosenUser } = useQuery({
    queryKey: ["user", quest!.userId, "chosenUser"],
    queryFn: () => UsersService.getById(quest!.userId!),
  });

  console.log(quest);

  // Load the current vote for the quest ( there could not be one )
  const { data: activeVote } = useQuery({
    queryKey: ["vote", quest.id, "quest-vote"],
    queryFn: () => VotesService.getQuestVote(quest.id),
    enabled: !!quest.hasActiveVote,
  });



  return (
    <div className="bg-card rounded-2xl p-6 w-full max-w-sm shadow-glow-soft flex flex-col gap-4">
      {/* Header: Today's quest label + XP */}
      <div className="flex justify-between items-center mb-4">
        <span className="text-xs text-primary uppercase tracking-widest font-semibold">
          Today's Quest
        </span>
        <InfoTag
          title={`${quest.rewardPoints} XP`}
          icon={<LucideCircleStar size={16} />}
          colorVariant="tertiary"
        />
      </div>

      {/* Quest Title */}
      <h2 className="text-text font-heading text-3xl font-bold">
        {quest.title}
      </h2>

      {/* Quest Description */}
      <p className="text-muted-foreground text-lg">{quest.description}</p>

      {/* Chosen User Section */}
      <div className="flex items-center gap-3 bg-background/60 rounded-xl border border-border px-3 py-4 mt-4 mb-4">
        <img
          src={
            isUserPending
              ? "https://static.vecteezy.com/system/resources/previews/013/360/247/non_2x/default-avatar-photo-icon-social-media-profile-sign-symbol-vector.jpg"
              : (chosenUser?.avatarUrl ??
                "https://static.vecteezy.com/system/resources/previews/013/360/247/non_2x/default-avatar-photo-icon-social-media-profile-sign-symbol-vector.jpg")
          }
          alt="user avatar"
          className="w-8 h-8 rounded-full"
        />

        <span
          className={`text-md font-medium ${
            isUserPending
              ? "text-muted-foreground animate-pulse"
              : "text-primary"
          }`}
        >
          {isUserPending ? "Loading user..." : chosenUser!.username}
        </span>
      </div>

      {/* Action Button - if the current user is chosen */}
      {activeVote ? (
        <QuestCardVote vote={activeVote} chosenUserId={quest.userId} />
      ) : (
        appUser?.id === quest.userId && (
          <>
            <QuestCardCreateCompletionVote quest={quest} groupId={groupId} />

            <QuestCardCreateSkipVote quest={quest} groupId={groupId} />
          </>
        )
      )}
    </div>
  );
}
