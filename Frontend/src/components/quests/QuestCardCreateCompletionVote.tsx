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
 * QuestCardCreateCompletionVote
 * Displays a button to create a completion vote for a quest.
 * Opens a confirmation dialog before submitting the request.
 */
export default function QuestCardCreateCompletionVote({ quest, groupId }: Props) {
  const queryClient = useQueryClient();
  const [isDialogOpen, setIsDialogOpen] = useState(false);

  // Mutation to create a completion vote
  const createCompletionVote = useMutation({
    mutationFn: (data: CreateVoteDTO) => VotesService.create(data),
    onSuccess: (vote: VoteDTO) => {
      // invalidate the group quest so it reloads and shows the vote
      queryClient.invalidateQueries({
        queryKey: ["group", groupId, "currentQuest"],
      });
      console.log("Completion vote created:", vote);
      setIsDialogOpen(false); // Close the dialog
    },
    onError: (error: unknown) => {
      console.error("Failed to create completion vote:", error);
    },
  });

  // Handler when the user confirms the completion vote
  const handleConfirm = () => {
    const data: CreateVoteDTO = {
      questId: quest.id,
      description: "Completion Vote",
      discriminator: VoteType.CompletionVote, // Assuming this is your VoteType
    };

    createCompletionVote.mutate(data);
  };

  return (
    <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
      {/* Trigger Button */}
      <DialogTrigger asChild>
        <Button
          className="text-xl py-8 rounded-full font-bold shadow-glow-primary flex items-center gap-3"
        >
          Upload Evidence
        </Button>
      </DialogTrigger>

      {/* Confirmation Dialog */}
      <DialogContent className="sm:max-w-sm">
        <DialogHeader>
          <DialogTitle>Are you sure?</DialogTitle>
        </DialogHeader>

        <DialogDescription className="text-sm text-muted-foreground mt-2">
          Creating a completion vote will allow other group members to vote on whether this quest is complete.
          Make sure you have uploaded all necessary evidence.
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
            disabled={createCompletionVote.isPending}
          >
            {createCompletionVote.isPending ? "Submitting..." : "Yes, Complete Quest"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}