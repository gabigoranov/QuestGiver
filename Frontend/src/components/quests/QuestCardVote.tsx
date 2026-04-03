import type { VoteDTO } from "@/types/Receive/VoteDTO";
import { Button } from "../ui/button";
import VoteDetails from "../votes/VoteDetails";

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
 * Used in the Quest Card if a quest has an active vote
 * Displays a button to view the active vote via a dialog window
 */
export default function QuestCardVote({ vote, chosenUserId }: Props) {
  return (
    <Dialog>
      {/* Trigger Button */}
      <DialogTrigger asChild>
        <Button className="text-lg py-6 rounded-full font-semibold shadow-glow-primary">
          View Active Vote
        </Button>
      </DialogTrigger>

      {/* Dialog Content */}
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle>Active Vote</DialogTitle>
        </DialogHeader>

        <DialogDescription>{vote.description}</DialogDescription>

        <VoteDetails vote={vote} chosenUserId={chosenUserId} />
      </DialogContent>
    </Dialog>
  );
}
