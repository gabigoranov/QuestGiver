import FriendGroupCard from "@/components/groups/FriendGroupCard";
import { GroupsService } from "@/services/groupsService";
import { useQuery } from "@tanstack/react-query";

/**
 * Page for the authenticated user
 * Will display the user's friend groups
 *
 * @export
 * @return {*}
 */
export default function Groups() {
  const { isPending, isError, data, error } = useQuery({
    queryKey: ["groups"],
    queryFn: GroupsService.getUserGroups,
  });

  if (isPending) {
    return <span>Loading...</span>;
  }

  if (isError) {
    return <span>Error: {error.message}</span>;
  }

  return (
    <section id="groups-page" className="page">
      {/* Headings */}
      <div className="flex flex-col">
        <h4 className="uppercase text-sans text-primary font-semibold tracking-[0.3rem] text-xs">
          Community hub
        </h4>
        <h2 className="text-4xl font-heading font-bold">Your Circles</h2>

        {/* Filtes */}
        <div className="flex flex-row items-center mt-4 flex-wrap max-w-5/7 gap-x-2 gap-y-4">
          <button className="px-6 py-3 bg-primary text-primary-foreground rounded-full text-sm font-medium">
            All Groups
          </button>
          <button className="px-6 py-3 bg-card text-muted-foreground rounded-full text-sm font-medium">
            New Quest
          </button>
          <button className="px-6 py-3 bg-card text-muted-foreground rounded-full text-sm font-medium">
            In Progress
          </button>
        <button className="px-6 py-3 bg-card text-muted-foreground rounded-full text-sm font-medium">
            Finished
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
