import QuestCard from "@/components/quests/QuestCard";
import { QuestTimer } from "@/components/quests/QuestTimer";
import { GroupsService } from "@/services/groupsService";
import { QuestsService } from "@/services/questsService";
import { useQuery } from "@tanstack/react-query";
import { useParams } from "react-router-dom";

/**
 * The page to view a group the user belongs to
 * Will display the current quest + some stats
 *
 * @export
 */
export default function Group() {
  // Load groupId from params
  const { groupId } = useParams<{ groupId: string }>();
  const {
    isPending,
    isError,
    data: group,
    error,
  } = useQuery({
    queryKey: ["group", groupId],
    queryFn: () => GroupsService.getGroupById(groupId!),
    enabled: !!groupId, // Only run the query if groupId is available
  });

  // Load the current quest for the group
  const {
    isPending: isQuestPending,
    data: quest,
  } = useQuery({
    queryKey: ["group", groupId, "currentQuest"],
    queryFn: () => QuestsService.getCurrentQuestForGroup(groupId!),
    enabled: !!groupId, // Only run the query if groupId is available
  });

  console.log(group);
  console.log(quest);

  if (isPending || isQuestPending) {
    return <span>Loading...</span>;
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

      {/* Quest Card */}
      <div className="flex flex-row items-center justify-center">
        <QuestCard quest={quest!} />
      </div>
    </section>
  );
}
