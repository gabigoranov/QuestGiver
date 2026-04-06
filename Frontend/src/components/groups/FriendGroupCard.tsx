import { useTranslation } from "react-i18next";
import type { GroupDTO } from "@/types/Receive/GroupDTO";
import InfoTag from "../common/InfoTag";
import { Button } from "../ui/button";
import { QuestStatusType } from "@/types/Receive/QuestStatusType";
import { Link } from "react-router-dom";

/**
 * Used in the groups page to display a signle group
 *
 * @export
 * @param {GroupDTO} data
 * @return {*}
 */
export default function FriendGroupCard(data: GroupDTO) {
  const { t } = useTranslation();
  // create a dictionaty to map quest status to title and color variant
  const questStatusMap = {
    [QuestStatusType.InProgress]: {
      title: t("groups.card.inProgress"),
      colorVariant: "primary",
    },
    [QuestStatusType.Completed]: {
      title: t("groups.card.completed"),
      colorVariant: "success",
    },
    [QuestStatusType.Skipped]: {
      title: t("groups.card.skipped"),
      colorVariant: "secondary",
    },
    [QuestStatusType.New]: {
      title: t("groups.card.new"),
      colorVariant: "tertiary",
    },
  } as const;

  return (
    <div className="border border-border w-full h-auto bg-card rounded-3xl p-8">
      {/* top info bar */}
      <div className="flex flex-row items-stretch justify-between mb-4">
        <InfoTag
          title={questStatusMap[data.currentQuestStatus]?.title}
          colorVariant={questStatusMap[data.currentQuestStatus]?.colorVariant}
        />

        <span className="text-muted-foreground font-semibold text-center flex items-center">
          {t("groups.card.created")}{" "}
          {data.dateCreated.toLocaleDateString("en-US", {
            day: "numeric",
            month: "short",
          })}
        </span>
      </div>

      {/* Main info - title and description */}
      <div className="flex flex-col">
        <h3 className="text-xl font-bold">{data.title}</h3>
        <p className="text-muted-foreground mt-2 max-h-20">
          {data.description}
        </p>
      </div>

      {/* Divider */}
      <div className="border-t border-primary/10 mt-4 pt-4" />

      {/* Bottom info bar ( Members + CTA ) */}
      <div className="flex flex-row items-center justify-between mt-4">
        <span className="text-muted-foreground font-semibold">
          {data.membersCount} {t("groups.card.members")}
        </span>

        <Link to={`/group/${data.id}`} viewTransition>
          <Button className="px-6 py-5 rounded-full text-sm font-semibold">
            {t("groups.card.enterGroup")}
          </Button>
        </Link>
      </div>
    </div>
  );
}
