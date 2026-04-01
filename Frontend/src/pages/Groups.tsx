import { useTranslation } from "react-i18next";
import FriendGroupCard from "@/components/groups/FriendGroupCard";
import { GroupsService } from "@/services/groupsService";
import { useQuery } from "@tanstack/react-query";
import LoadingScreen from "@/components/common/LoadingScreen";

/**
 * Page for the authenticated user
 * Will display the user's friend groups
 *
 * @export
 * @return {*}
 */
export default function Groups() {
  const { t } = useTranslation();
  const { isPending, isError, data, error } = useQuery({
    queryKey: ["groups"],
    queryFn: GroupsService.getUserGroups,
  });

  if (isPending) {
    return <LoadingScreen />;
  }

  if (isError) {
    return <span>{t('common.error')}: {error.message}</span>;
  }

  return (
    <section id="groups-page" className="page">
      {/* Headings */}
      <div className="flex flex-col">
        <h4 className="uppercase text-sans text-primary font-semibold tracking-[0.3rem] text-xs">
          {t('groups.page.subtitle')}
        </h4>
        <h2 className="text-4xl font-heading font-bold">{t('groups.page.title')}</h2>

        {/* Filtes */}
        <div className="flex flex-row items-center mt-4 flex-wrap max-w-5/7 gap-x-2 gap-y-4">
          <button className="px-6 py-3 bg-primary text-primary-foreground rounded-full text-sm font-medium">
            {t('groups.filters.all')}
          </button>
          <button className="px-6 py-3 bg-card text-muted-foreground rounded-full text-sm font-medium">
            {t('groups.filters.newQuest')}
          </button>
          <button className="px-6 py-3 bg-card text-muted-foreground rounded-full text-sm font-medium">
            {t('groups.filters.inProgress')}
          </button>
        <button className="px-6 py-3 bg-card text-muted-foreground rounded-full text-sm font-medium">
            {t('groups.filters.finished')}
          </button>
        </div>
      </div>

      <div className="mt-10 flex flex-col gap-6">
        {data?.map((group) => (
          <FriendGroupCard key={group.id} {...group} />
        ))}
      </div>
    </section>
  );
}
