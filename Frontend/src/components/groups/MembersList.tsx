import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import type { UserDTO } from "@/types/Receive/UserDTO";
import { GroupsService } from "@/services/groupsService";
import { Button } from "../ui/button";
import useAuth from "@/hooks/useAuth";
import UserCard from "../common/UserCard";
import { useNavigate } from "react-router-dom";

type MembersListProps = {
  groupId: string;
};

/**
 * MembersList component
 *
 * Displays the list of members in a group and allows the logged-in user
 * to leave the group. Each member is displayed using a reusable `UserCard`.
 *
 * @param {MembersListProps} props - Component props
 * @param {string} props.groupId - The ID of the group
 *
 * @returns JSX.Element
 */
export default function MembersList({ groupId }: MembersListProps) {
  const queryClient = useQueryClient();
  const { user } = useAuth(); // Logged-in user
  const navigate = useNavigate();

  // Fetch group members
  const { data: members, isLoading, isError } = useQuery<UserDTO[]>({
    queryKey: ["group", groupId, "members"],
    queryFn: () => GroupsService.getGroupMembers(groupId),
  });

  // Mutation to leave the group
  const leaveGroup = useMutation({
    mutationFn: () => GroupsService.leave(groupId),
    onSuccess: () => {
      // Optionally, invalidate the group list or redirect the user
      queryClient.invalidateQueries({ queryKey: ["groups"] });
      navigate("/groups");
      console.log("Left the group successfully");
    },
    onError: (err) => console.error("Failed to leave the group:", err),
  });

  if (isLoading) return <div>Loading members...</div>;
  if (isError) return <div>Failed to load members.</div>;

  return (
    <div className="flex flex-col gap-4">
      {/* List of group members */}
      {members && members.length > 0 ? (
        members.map((member) => (
          <UserCard
            key={member.id}
            user={member}
            highlight={member.id === user!.id} // Highlight logged-in user
          />
        ))
      ) : (
        <div>No members in this group.</div>
      )}

      {/* Leave group button */}
      <div className="mt-4 mb-2 flex justify-center">
        <Button
          variant="destructive"
          onClick={() => leaveGroup.mutate()}
          disabled={leaveGroup.isPending}
        >
          {leaveGroup.isPending ? "Leaving..." : "Leave Group"}
        </Button>
      </div>
    </div>
  );
}