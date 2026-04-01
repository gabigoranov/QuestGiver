import BottomSheet from "@/components/common/BottomSheet";
import ErrorBoundary from "@/components/common/ErrorBoundary";
import LoadingScreen from "@/components/common/LoadingScreen";
import QuestCard from "@/components/quests/QuestCard";
import QuestCardFallback from "@/components/quests/QuestCardFallback";
import { QuestTimer } from "@/components/quests/QuestTimer";
import { Button } from "@/components/ui/button";
import { GroupsService } from "@/services/groupsService";
import { QuestsService } from "@/services/questsService";
import { useQuery } from "@tanstack/react-query";
import { ArrowRight, RotateCw } from "lucide-react";
import { useState } from "react";
import { useParams } from "react-router-dom";

/**
 * The page to view a group the user belongs to
 * Will display the current quest + some stats
 *
 * @export
 */
export default function Group() {
  const [isInviteOpen, setIsInviteOpen] = useState(false);
  // Load groupId from params
  const { groupId } = useParams<{ groupId: string }>();
  const { isPending, isError, error } = useQuery({
    queryKey: ["group", groupId],
    queryFn: () => GroupsService.getGroupById(groupId!),
    enabled: !!groupId, // Only run the query if groupId is available
  });

  // Load the current quest for the group
  const { isPending: isQuestPending, data: quest } = useQuery({
    queryKey: ["group", groupId, "currentQuest"],
    queryFn: () => QuestsService.getCurrentQuestForGroup(groupId!),
    enabled: !!groupId, // Only run the query if groupId is available
  });

  if (isPending || isQuestPending) {
    return <LoadingScreen />
  }

  if (isError) {
    return <span>Error: {error.message}</span>;
  }

  return (
    <section id="group-page" className="page flex flex-col gap-8">
      {/* Timer for deadline of today's quest */}
      <div className="flex flex-row items-center justify-center">
        <QuestTimer quest={quest!} />
      </div>

      <ErrorBoundary fallback={<QuestCardFallback />}>
        {/* Quest Card */}
        <div className="flex flex-row items-center justify-center">
          <QuestCard quest={quest!} />
        </div>

        {/* Skip button */}
        <Button
          variant="ghost"
          className="text-sm py-2 rounded-full uppercase font-semibold tracking-wider -mt-2 flex items-center gap-3 text-muted-foreground"
        >
          <RotateCw size={16} />
          Skip Quest (Vote Required)
        </Button>
      </ErrorBoundary>

      {/* Controls - Invite Users, Manage Members */}
      <div className="flex items-center gap-3 mt-4">
        {/* Invite (Primary) */}
        <Button
          onClick={() => setIsInviteOpen(true)}
          className="flex-1 rounded-2xl h-12 flex items-center justify-center gap-2 shadow-sm uppercase text-md tracking-wider font-semibold"
        >
          Invite
        </Button>

        {/* View Members (Secondary) */}
        <Button
          variant="outline"
          className="flex-1 rounded-2xl h-12 flex items-center justify-center gap-2 uppercase text-md tracking-wider font-semibold"
        >
          Members
          <ArrowRight size={16} />
        </Button>
      </div>

      {/* Bottom sheets for inviting a user */}
      <BottomSheet isOpen={isInviteOpen} onClose={() => setIsInviteOpen(false)}>
        <div className="flex flex-col gap-4">
          <h2 className="text-lg font-semibold">Invite Link</h2>

          <div className="flex items-center justify-between gap-3 bg-muted/50 border border-border rounded-xl px-4 py-3">
            <span className="text-sm text-muted-foreground truncate">
              {window.location.toString()}/group/join/{groupId}
            </span>

            <Button
              size="sm"
              variant="ghost"
              className="rounded-full px-3"
              onClick={() => {
                navigator.clipboard.writeText(
                  `https://questbound/group/join/${groupId}`,
                );

                setIsInviteOpen(false);
              }}
            >
              Copy
            </Button>
          </div>
        </div>
      </BottomSheet>
    </section>
  );
}
