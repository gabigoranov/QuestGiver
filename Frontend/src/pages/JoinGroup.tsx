import LoadingScreen from "@/components/common/LoadingScreen";
import { GroupsService } from "@/services/groupsService";
import { useQuery } from "@tanstack/react-query";
import { Navigate, useParams } from "react-router-dom";

export default function JoinGroup() {
  const { groupId } = useParams<{ groupId: string }>();

  const {
    isPending,
    isError
  } = useQuery({
    queryKey: ["join-group", groupId],
    queryFn: () => GroupsService.join(groupId!),
    enabled: !!groupId, // Only run the query if groupId is available
  });

  if(isPending) {
    return <LoadingScreen />;
  }

  if(isError) {
    return <h1>Something went wrong, could not join group...</h1>
  }

  return <Navigate to={`/group/${groupId}`} replace/>;
}
