import type { QuestDTO } from "@/types/Receive/QuestDTO";
import { LucideCircleStar, CheckCircle2, SkipForward } from "lucide-react";
import { QuestStatusType } from "@/types/Receive/QuestStatusType";
import InfoTag from "../common/InfoTag";

type Props = {
  quest: QuestDTO;
};

export default function StaticQuestCard({ quest }: Props) {
  const isCompleted = quest.status === QuestStatusType.Completed;
  const isSkipped = quest.status === QuestStatusType.Skipped;

  return (
    <div className="flex justify-between items-center bg-card border border-border rounded-xl p-4 gap-4">
      {/* LEFT */}
      <div className="flex flex-col flex-1 min-w-0 gap-2">
        <div className="flex items-stretch gap-4 justify-between">
          {/* TITLE */}
          <h3 className="font-semibold text-xl truncate">{quest.title}</h3>

          {/* STATUS */}
          {(isCompleted || isSkipped) && (
            <div>
              <InfoTag
                title={isCompleted ? "Completed" : "Skipped"}
                icon={
                  isCompleted ? (
                    <CheckCircle2 size={14} />
                  ) : (
                    <SkipForward size={14} />
                  )
                }
                colorVariant={isCompleted ? "success" : "secondary"}
              />
            </div>
          )}

          {/* RIGHT */}
          <div className="shrink-0">
            <InfoTag
              title={`${quest.rewardPoints} XP`}
              icon={<LucideCircleStar size={16} />}
              colorVariant="tertiary"
            />
          </div>
        </div>

        {/* DESCRIPTION */}
        <p className="text-sm text-muted-foreground line-clamp-2">
          {quest.description}
        </p>
      </div>
    </div>
  );
}
