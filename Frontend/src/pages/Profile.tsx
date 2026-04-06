import { useQuery } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import { QuestsService } from "@/services/questsService";
import StaticQuestCard from "@/components/quests/StaticQuestCard";
import { UsersService } from "@/services/usersService";
import { Avatar, AvatarImage } from "@/components/ui/avatar";

export default function Profile() {
  const { t } = useTranslation();
  const { data: user, isLoading } = useQuery({
    queryKey: ["me"],
    queryFn: UsersService.reloadSelf,
  });

  // Load user history quests via tanstack query
  const { data } = useQuery({
    queryKey: ["quest-history"],
    queryFn: QuestsService.getAllUserQuests,
  });

  if (isLoading || user == null) {
    return <div className="p-6 text-white">{t("profile.loading")}</div>;
  }
  const progress = (user.experiencePoints / user.nextLevelExperience) * 100;

  return (
    <div className="min-h-screen page p-6 flex flex-col items-center gap-10">
      {/* PROFILE HEADER */}
      <div className="flex flex-col items-center gap-4 text-center">
        <div className="p-0.5 rounded-full">
          <Avatar
            className="w-24 h-24 rounded-full object-cover "
          >
            <AvatarImage
              src={
                user.avatarUrl ??
                `https://api.dicebear.com/7.x/initials/svg?seed=${user.username}`
              }
            />
          </Avatar>
        </div>

        <div>
          <h1 className="text-xl font-semibold">@{user.username}</h1>
          <p className="text-sm mt-2 text-muted-foreground max-w-xs">
            {user.description || t("profile.noDescription")}
          </p>
        </div>
      </div>

      {/* PROGRESS CARD */}
      <div className="w-full max-w-md bg-card backdrop-blur rounded-2xl p-5 border border-border flex flex-col gap-4 shadow-glow-soft">
        <span className="text-xs text-tertiary uppercase tracking-wider font-semibold">
          {t("profile.currentStatus")}
        </span>

        <div className="flex justify-between items-center font-bold">
          <span className="text-lg">
            {t("profile.progressToLevel")}
            {user.level + 1}
          </span>
          <span className="text-sm font-semibold">{Math.floor(progress)}%</span>
        </div>

        <div className="w-full h-2 bg-muted-foreground/20 rounded-full overflow-hidden">
          <div
            className="h-full bg-linear-to-r from-primary to-primary/80"
            style={{ width: `${progress}%` }}
          />
        </div>

        <div className="flex justify-between text-xs text-zinc-500">
          <span>{user.experiencePoints} XP</span>
          <span>{user.nextLevelExperience} XP</span>
        </div>
      </div>

      {/* QUEST HISTORY */}
      <div className="w-full max-w-md flex flex-col gap-4">
        <h2 className="text-lg font-semibold font-heading">
          {t("profile.questHistory")}
        </h2>

        {/* QUEST ITEMS */}
        {!data ? (
          <div className="text-muted-foreground text-sm">
            {t("profile.loadingQuests")}
          </div>
        ) : data.length === 0 ? (
          <div className="text-muted-foreground text-sm">
            {t("profile.noQuests")}
          </div>
        ) : (
          data.map((quest) => <StaticQuestCard key={quest.id} quest={quest} />)
        )}
      </div>
    </div>
  );
}
