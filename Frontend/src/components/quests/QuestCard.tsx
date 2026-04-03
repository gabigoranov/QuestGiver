import type { QuestDTO } from "@/types/Receive/QuestDTO";
import {
  LucideCircleStar,
  RotateCw,
  CheckCircle2,
  SkipForward,
} from "lucide-react";
import InfoTag from "../common/InfoTag";
import { UsersService } from "@/services/usersService";
import { useQuery } from "@tanstack/react-query";
import useAuth from "@/hooks/useAuth";
import { VotesService } from "@/services/votesService";
import QuestCardVote from "./QuestCardVote";
import { Button } from "../ui/button";
import { useState } from "react";
import CreateVoteDialog from "../votes/CreateVoteDialog";
import { VoteType } from "@/types/VoteType";
import { QuestStatusType } from "@/types/Receive/QuestStatusType";

type Props = {
  quest: QuestDTO;
  groupId: string;
};

/**
 * QuestCard Component
 *
 * Displays information about a daily quest, including:
 * - Title, description, and reward
 * - Assigned (chosen) user
 * - Active vote (if any)
 * - Action buttons for the chosen user
 *
 * Behavior:
 * - If the quest is Completed or Skipped:
 *   - Action buttons are hidden
 *   - A status InfoTag is displayed
 * - If there is an active vote:
 *   - Displays the vote dialog trigger instead of actions
 * - Only the chosen user can initiate votes
 *
 * @param {Props} props - Component props
 * @returns JSX.Element
 */
export default function QuestCard({ quest, groupId }: Props) {
  console.log("status:", quest.status, typeof quest.status);
  console.log("Skipped enum:", QuestStatusType.Skipped);

  const { user: appUser } = useAuth();

  const [isCompletionVoteOpen, setIsCompletionVoteOpen] = useState(false);
  const [isSkipVoteOpen, setIsSkipVoteOpen] = useState(false);

  /**
   * Determines whether the quest is already finished.
   * Used to disable actions and show status.
   */
  const isDone =
    quest.status === QuestStatusType.Completed ||
    quest.status === QuestStatusType.Skipped;

  /**
   * Fetch the chosen user for this quest
   */
  const { isPending: isUserPending, data: chosenUser } = useQuery({
    queryKey: ["user", quest!.userId, "chosenUser"],
    queryFn: () => UsersService.getById(quest!.userId!),
  });

  /**
   * Fetch the active vote for the quest (if one exists)
   */
  const { data: activeVote } = useQuery({
    queryKey: ["vote", quest.id, "quest-vote"],
    queryFn: () => VotesService.getQuestVote(quest.id),
  });

  return (
    <div className="bg-card rounded-2xl p-6 w-full max-w-sm shadow-glow-soft flex flex-col gap-4">
      {/* Header: Label + XP + Status */}
      <div className="flex justify-between items-center mb-4">
        <span className="text-xs text-primary uppercase tracking-widest font-semibold">
          Today's Quest
        </span>

        <div className="flex items-stretch gap-2">
          {/* XP Reward */}
          <InfoTag
            title={`${quest.rewardPoints} XP`}
            icon={<LucideCircleStar size={16} />}
            colorVariant="tertiary"
          />

          {/* Status Tag (shown only when quest is done) */}
          {isDone && (
            <InfoTag
              icon={
                quest.status === QuestStatusType.Completed ? (
                  <CheckCircle2 size={16} />
                ) : (
                  <SkipForward size={16} />
                )
              }
              colorVariant={
                quest.status === QuestStatusType.Completed
                  ? "success"
                  : "primary"
              }
            />
          )}
        </div>
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

      {/* Actions / Vote Section */}
      {activeVote ? (
        /**
         * If a vote is active, show the vote dialog trigger
         */
        <QuestCardVote vote={activeVote} chosenUserId={quest.userId} />
      ) : (
        /**
         * Otherwise, show action buttons only if:
         * - Quest is NOT done
         * - Current user is the chosen user
         */
        !isDone &&
        appUser?.id === quest.userId && (
          <>
            {/* Completion Vote Button */}
            <Button
              onClick={() => setIsCompletionVoteOpen(true)}
              className="text-xl py-8 rounded-full font-bold shadow-glow-primary flex items-center gap-3"
            >
              Upload Evidence
            </Button>

            {/* Completion Vote Dialog */}
            <CreateVoteDialog
              quest={quest}
              groupId={groupId}
              voteType={VoteType.CompletionVote}
              isOpen={isCompletionVoteOpen}
              setIsOpen={setIsCompletionVoteOpen}
            />

            {/* Skip Vote Button */}
            <Button
              onClick={() => setIsSkipVoteOpen(true)}
              variant="ghost"
              className="text-sm py-2 rounded-full uppercase font-semibold tracking-wider mt-2 flex items-center gap-3 text-muted-foreground"
            >
              <RotateCw size={16} />
              Skip Quest (Vote Required)
            </Button>

            {/* Skip Vote Dialog */}
            <CreateVoteDialog
              quest={quest}
              groupId={groupId}
              voteType={VoteType.SkipVote}
              isOpen={isSkipVoteOpen}
              setIsOpen={setIsSkipVoteOpen}
            />
          </>
        )
      )}
    </div>
  );
}
