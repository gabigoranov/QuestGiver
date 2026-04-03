import { Button } from "../ui/button";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { VotesService } from "@/services/votesService";
import useAuth from "@/hooks/useAuth";
import type { VoteDTO } from "@/types/Receive/VoteDTO";

type VoteDetailsProps = {
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
 * @param {VoteDetailsProps} props - Component props
 * @param {VoteDTO} props.vote - The vote object to display
 *
 * @returns JSX.Element
 */
export default function VoteDetails({ vote, chosenUserId }: VoteDetailsProps) {
  const { user } = useAuth(); // logged-in user
  const queryClient = useQueryClient();

  // Count votes
  const totalVotes = vote.userVotes.length;
  const yesVotes = vote.userVotes.filter(v => v.decision === true).length;
  const noVotes = vote.userVotes.filter(v => v.decision === false).length;

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
      {/* Vote statistics */}
      <div className="text-sm flex flex-col gap-1">
        <span>Total votes: {totalVotes}</span>
        <span className="text-success">Yes: {yesVotes}</span>
        <span className="text-error">No: {noVotes}</span>
      </div>

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
      {user!.id !== chosenUserId && (
        <div className="flex gap-2 mt-2">
          <Button
            variant="destructive"
            onClick={() => handleVote(false)}
            disabled={submitVote.isPending}
          >
            {submitVote.isPending ? "Submitting..." : "No"}
          </Button>

          <Button
            onClick={() => handleVote(true)}
            disabled={submitVote.isPending}
          >
            {submitVote.isPending ? "Submitting..." : "Yes"}
          </Button>
        </div>
      )}
    </div>
  );
}