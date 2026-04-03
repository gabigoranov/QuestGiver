import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";

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
  DialogClose,
  DialogDescription,
} from "../ui/dialog";
import { VoteType } from "@/types/VoteType";
import { Textarea } from "../ui/textarea";
import { LucideBadgeCheck, LucideSkipForward } from "lucide-react";

type Props = {
  quest: QuestDTO;
  groupId: string;
  voteType: VoteType;
  isOpen: boolean;
  setIsOpen: (status: boolean) => void;
};

/**
 * QuestCardCreateCompletionVote
 * Displays a button to create a completion vote for a quest.
 * Opens a confirmation dialog before submitting the request.
 */
export default function QuestCardCreateCompletionVote({
  quest,
  groupId,
  voteType,
  setIsOpen,
  isOpen
}: Props) {
  const queryClient = useQueryClient();
  const [selectedVoteType, setSelectedVoteType] = useState<VoteType>(voteType);
  const [description, setDescription] = useState("");

  // Mutation to create a vote
  const createVote = useMutation({
    mutationFn: (data: CreateVoteDTO) => VotesService.create(data),
    onSuccess: (vote: VoteDTO) => {
      // invalidate the group quest so it reloads and shows the vote
      queryClient.invalidateQueries({
        queryKey: ["group", groupId, "currentQuest"],
      });
      console.log("Completion vote created:", vote);
      setIsOpen(false); // Close the dialog
    },
    onError: (error: unknown) => {
      console.error("Failed to create completion vote:", error);
    },
  });

  // Handler when the user confirms the completion vote
  const handleConfirm = () => {
    const data: CreateVoteDTO = {
      questId: quest.id,
      description: description,
      discriminator: selectedVoteType,
    };

    createVote.mutate(data);
  };

  return (
    <Dialog open={isOpen} onOpenChange={setIsOpen}>
      <DialogContent className="sm:max-w-md" showCloseButton={false}>
        <DialogHeader>
          <DialogTitle className="text-xl font-semibold">
            Start a Vote
          </DialogTitle>
          <DialogDescription className="text-sm text-muted-foreground mt-1">
            Initiate a squad consensus for the current quest.
          </DialogDescription>
        </DialogHeader>

        {/* FORM */}
        <div className="mt-4 space-y-4">
          {/* Vote Type */}
          <div>
            <p className="text-xs tracking-widest text-muted-foreground mb-2">
              VOTE TYPE
            </p>

            <div className="grid grid-cols-2 gap-2">
              <Button
                type="button"
                onClick={() => setSelectedVoteType(VoteType.SkipVote)}
                className={`rounded-xl border transition flex flex-col gap-2 items-center justify-center px-4 py-12 text-lg
              ${
                selectedVoteType === VoteType.SkipVote
                  ? "border-primary bg-primary font-bold text-xl"
                  : "border-border bg-card text-card-foreground"
              }`}
              >
                <LucideSkipForward className="w-7! h-7!" />
                Skip Quest
              </Button>

              <Button
                type="button"
                onClick={() => setSelectedVoteType(VoteType.CompletionVote)}
                className={`rounded-xl border transition flex flex-col gap-2 items-center justify-center px-4 py-12 text-lg
              ${
                selectedVoteType === VoteType.CompletionVote
                  ? "border-primary bg-primary font-bold text-xl"
                  : "border-border bg-card text-card-foreground"
              }`}
              >
                <LucideBadgeCheck className="w-7! h-7!" />
                Verify
              </Button>
            </div>
          </div>

          {/* Description */}
          <div>
            <p className="text-xs tracking-widest text-muted-foreground mb-2">
              DESCRIPTION / REASON
            </p>

            <Textarea
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              placeholder="Why is this vote necessary? Provide details for your squad..."
              className="w-full rounded-xl border bg-muted/40 p-3 text-sm outline-none focus:ring-2 focus:ring-primary min-h-25"
            />
          </div>
        </div>

        {/* FOOTER */}
        <DialogFooter className="mt-6 flex flex-col gap-2 sm:flex-row sm:justify-end">
          <Button
            variant={"default"}
            onClick={handleConfirm}
            disabled={createVote.isPending}
            className="text-lg font-bold shadow-glow-primary py-6"
          >
            {createVote.isPending ? "Submitting..." : "Propose Vote"}
          </Button>

          <DialogClose asChild>
            <Button variant="outline" className="py-6">
              Cancel
            </Button>
          </DialogClose>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
