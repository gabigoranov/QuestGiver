import { Button } from "../ui/button";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { VotesService } from "@/services/votesService";
import type { VoteDTO } from "@/types/Receive/VoteDTO";
import { UsersService } from "@/services/usersService";

type VoteActionsProps = {
  vote: VoteDTO;
  chosenUserId: string;
};

/**
 * VoteDetails component
 *
 * Displays vote statistics and the current decision for a quest vote.
 * If the logged-in user is not the chosen user for the quest but belongs to the group,
 * they can submit their opinion (yes / no) if they haven’t voted yet.
 *
 * @param {VoteActionsProps} props - Component props
 * @param {VoteDTO} props.vote - The vote object to display
 *
 * @returns JSX.Element
 */
export default function VoteActions({ vote, chosenUserId }: VoteActionsProps) {
  const queryClient = useQueryClient();

  const {data:user } = useQuery({
    queryKey: ["me"],
    queryFn: UsersService.reloadSelf,
  });

  // Mutation to submit user vote
  const submitVote = useMutation({
    mutationFn: (decision: boolean) =>
      VotesService.submitIndividualVote(vote.id, decision),
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ["vote", vote.questId, "quest-vote"],
      });
    },
    onError: (err) => console.error("Failed to submit vote:", err),
  });

  // Handle vote click
  const handleVote = (decision: boolean) => {
    submitVote.mutate(decision);
  };

  return (
    <div className="flex flex-col gap-3">
      {/* Overall decision */}
      {vote.decision !== undefined && (
        <div className="text-lg font-semibold">
          Result:{" "}
          <span
            className={
              vote.decision != null
                ? vote.decision
                  ? "text-success"
                  : "text-error"
                : "text-muted-foreground"
            }
          >
            {vote.decision != null
              ? vote.decision
                ? "Approved"
                : "Rejected"
              : "Unknown"}
          </span>
        </div>
      )}

      {/* Voting buttons ( the user can revote if they wish ) */}
      {(user!.id !== chosenUserId && vote.decision == null) && (
        <div className="flex flex-col gap-2 mt-2">
          <Button
            onClick={() => handleVote(true)}
            disabled={submitVote.isPending}
            className="py-6 text-lg font-semibold"
          >
            {submitVote.isPending ? "Submitting..." : "Yes"}
          </Button>

          <Button
            variant="outline"
            onClick={() => handleVote(false)}
            disabled={submitVote.isPending}
            className="py-6 text-lg font-semibold"
          >
            {submitVote.isPending ? "Submitting..." : "No"}
          </Button>
        </div>
      )}
    </div>
  );
}