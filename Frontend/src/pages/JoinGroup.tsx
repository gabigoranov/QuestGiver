import LoadingScreen from "@/components/common/LoadingScreen";
import { GroupsService } from "@/services/groupsService";
import { useQuery } from "@tanstack/react-query";
import { Navigate, useParams } from "react-router-dom";
import { useTranslation } from "react-i18next";

export default function JoinGroup() {
  const { t } = useTranslation();
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
    return <h1>{t("joinGroup.error")}</h1>
  }

  return <Navigate to={`/group/${groupId}`} replace/>;
}
