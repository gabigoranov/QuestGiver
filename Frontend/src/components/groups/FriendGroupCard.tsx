import type { GroupDTO } from "@/types/Receive/GroupDTO";
import InfoTag from "../common/InfoTag";
import { Button } from "../ui/button";

/**
 * Used in the groups page to display a signle group
 *
 * @export
 * @param {GroupDTO} data
 * @return {*}
 */
export default function FriendGroupCard(data: GroupDTO) {
  return (
    <div className="border border-border w-full h-auto bg-card rounded-3xl p-8">
      {/* top info bar */}
      <div className="flex flex-row items-start justify-between mb-4">
        <InfoTag title={"In Progress"} colorVariant="primary"/>

        <span className="text-muted-foreground font-semibold">
          Created{" "}
          {data.dateCreated.toLocaleDateString("en-US", {
            day: "numeric",
            month: "short",
          })}
        </span>
      </div>

      {/* Main info - title and description */}
      <div className="flex flex-col">
        <h3 className="text-xl font-bold">{data.title}</h3>
        <p className="text-muted-foreground mt-2 max-h-20">{data.description}</p>
      </div>

      {/* Divider */}
      <div className="border-t border-primary/10 mt-4 pt-4" />

      {/* Bottom info bar ( Members + CTA ) */}
      <div className="flex flex-row items-center justify-between mt-4">
        <span className="text-muted-foreground font-semibold">
          {data.membersCount} Members
        </span>
        <Button className="px-6 py-5 rounded-full text-sm font-semibold">
          Enter Group
        </Button>
      </div>

    </div>
  );
}
