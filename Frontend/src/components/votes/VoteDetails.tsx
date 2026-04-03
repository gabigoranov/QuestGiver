import type { VoteDTO } from "@/types/Receive/VoteDTO";

type VoteDetailsProps = {
  vote: VoteDTO;
};

export default function VoteDetails({ vote }: VoteDetailsProps) {
  const totalVotes = vote.userVotes.length;
  const yesVotes = vote.userVotes.filter(v => v.decision === true).length;
  const noVotes = vote.userVotes.filter(v => v.decision === false).length;

  return (
    <div className="flex flex-col gap-3">
      <div className="text-sm flex flex-col gap-1">
        <span>Total votes: {totalVotes}</span>
        <span className="text-green-500">Yes: {yesVotes}</span>
        <span className="text-red-500">No: {noVotes}</span>
      </div>

      {vote.decision !== undefined && (
        <div className="text-lg font-semibold">
          Result:{" "}
          <span
            className={
              vote.decision != null ? vote.decision ? "text-green-500" : "text-red-500" : "text-muted-foreground"
            }
          >
            {vote.decision != null ? vote.decision ? "Approved" : "Rejected" : "Unknown"}
          </span>
        </div>
      )}
    </div>
  );
}