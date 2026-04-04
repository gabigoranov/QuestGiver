import type { VoteDTO } from "@/types/Receive/VoteDTO";
import { Button } from "../ui/button";
import VoteActions from "../votes/VoteDetails";
import { useTranslation } from "react-i18next";

import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";

type Props = {
  vote: VoteDTO;
  chosenUserId: string;
};

/**
 * Used in the quest card if an active vote is available
 *
 * @export
 * @param {Props} { vote, chosenUserId }
 * @return {*}
 */
export default function QuestCardVote({ vote, chosenUserId }: Props) {
  const { t } = useTranslation();
  const total = vote.userVotes.length;
  const yes = vote.userVotes.filter((v) => v.decision === true).length;
  const no = vote.userVotes.filter((v) => v.decision === false).length;
  const yesPercentage = Math.round((yes / total) * 100);
  const noPercentage = Math.round((no / total) * 100);

  return (
    <Dialog>
      <DialogTrigger asChild>
        <Button className="text-lg py-6 rounded-full font-semibold shadow-glow-primary">
          {t("questCardVote.viewActive")}
        </Button>
      </DialogTrigger>

      <DialogContent
        className="sm:max-w-md border border-border bg-card shadow-xl rounded-2xl"
        showCloseButton={false}
      >
        <DialogHeader>
          <DialogTitle className="text-3xl text-center font-semibold">
            {t("questCardVote.activeVote")}
          </DialogTitle>
        </DialogHeader>

        <DialogDescription className="text-md text-center text-muted-foreground mt-1">
          {vote.description}
        </DialogDescription>

        {/* VOTE VISUAL */}
        <div className="mt-6 space-y-4">
          {/* Percent labels */}
          <div className="flex justify-between text-sm font-medium">
            <span className="text-primary">{t("questCardVote.yes")} {yesPercentage}%</span>
            <span className="text-muted-foreground">{t("questCardVote.no")} {noPercentage}%</span>
          </div>

          {/* Split bar */}
          <div className="w-full h-3 rounded-full bg-muted overflow-hidden flex">
            <div
              className="bg-primary transition-all duration-500"
              style={{ width: `${yesPercentage}%` }}
            />
            <div
              className="bg-muted-foreground/30 transition-all duration-500"
              style={{ width: `${noPercentage}%` }}
            />
          </div>

          {/* Counts */}
          <div className="flex justify-between text-xs text-muted-foreground">
            <span>{yes} {t("questCardVote.votes")}</span>
            <span>{no} {t("questCardVote.votes")}</span>
          </div>
        </div>

        {/* Details */}
        <div className="mt-6">
          <VoteActions vote={vote} chosenUserId={chosenUserId} />
        </div>
      </DialogContent>
    </Dialog>
  );
}
