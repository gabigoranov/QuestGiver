import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { RotateCw } from "lucide-react";

import { VotesService } from "@/services/votesService";
import type { QuestDTO } from "@/types/Receive/QuestDTO";
import type { VoteDTO } from "@/types/Receive/VoteDTO";
import type { CreateVoteDTO } from "@/types/Send/CreateVoteDTO";

import { Button } from "../ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogFooter,
  DialogTitle,
  DialogTrigger,
  DialogClose,
  DialogDescription,
} from "../ui/dialog";
import { VoteType } from "@/types/VoteType";

type Props = {
  quest: QuestDTO;
  groupId: string;
};

/**
 * QuestCardCreateSkipVote
 * Displays a button to create a skip vote for a quest.
 * Opens a confirmation dialog before submitting the request.
 */
export default function QuestCardCreateSkipVote({ quest, groupId }: Props) {
  const queryClient = useQueryClient();
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  // Mutation to create a skip vote
  const createSkipVote = useMutation({
    mutationFn: (data: CreateVoteDTO) => VotesService.create(data),
    onSuccess: (vote: VoteDTO) => {
      // invalidate the group quest so it reloads and shows the vote
      queryClient.invalidateQueries({
        queryKey: ["group", groupId, "currentQuest"],
      });
      console.log("Vote created:", vote);
      setIsDialogOpen(false); // Close the dialog
    },
    onError: (error: unknown) => {
      console.error("Failed to create vote:", error);
    },
  });

  // Handler when the user confirms the skip vote
  const handleConfirm = () => {
    const data: CreateVoteDTO = {
      questId: quest.id,
      description: "Skip Quest Vote",
      discriminator: VoteType.SkipVote, // Assuming this is your VoteType
    };

    createSkipVote.mutate(data);
  };

  return (
    <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
      {/* Trigger Button */}
      <DialogTrigger asChild>
        <Button
          variant="ghost"
          className="text-sm py-2 rounded-full uppercase font-semibold tracking-wider mt-2 flex items-center gap-3 text-muted-foreground"
        >
          <RotateCw size={16} />
          Skip Quest (Vote Required)
        </Button>
      </DialogTrigger>

      {/* Confirmation Dialog */}
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Are you sure?</DialogTitle>
        </DialogHeader>

        <DialogDescription className="text-sm text-muted-foreground mt-2">
          Creating a skip vote will allow other group members to vote on skipping this quest.
          Are you sure you want to continue?
        </DialogDescription>

        <DialogFooter className="mt-4">
          <DialogClose asChild>
            <Button variant="outline" className="mr-2">
              Cancel
            </Button>
          </DialogClose>

          <Button
            variant="destructive"
            onClick={handleConfirm}
            disabled={createSkipVote.isPending}
          >
            {createSkipVote.isPending ? "Submitting..." : "Yes, Skip Quest"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}