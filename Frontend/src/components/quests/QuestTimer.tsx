import { useEffect, useState } from "react";
import { LucideClock } from "lucide-react";
import type { QuestDTO } from "@/types/Receive/QuestDTO";
import InfoTag from "../common/InfoTag";
import { useTranslation } from "react-i18next";


/**
 * Used in the main group view page to display the deadline for
 * today's quest
 *
 * @export
 * @param {{ quest: QuestDTO }} { quest }
 * @return {*}
 */
export function QuestTimer({ quest }: { quest: QuestDTO }) {
  const { t } = useTranslation();
  const [timeLeft, setTimeLeft] = useState<string>("");
  const [colorVariant, setColorVariant] = useState<"primary" | "secondary" | "tertiary" | "error">("primary");

  useEffect(() => {
    if (!quest?.scheduledDate) return;

    const updateTimer = () => {
      const now = new Date();
      const scheduled = new Date(quest.scheduledDate);

      // Calculate the next 24-hour "unit" from scheduled UTC date
      const nextDeadline = new Date(scheduled.getTime() + 24 * 60 * 60 * 1000);

      const diff = nextDeadline.getTime() - now.getTime();

      if (diff <= 0) {
        setTimeLeft(t("questTimer.deadlineReached"));
        return;
      }

      const hours = Math.floor(diff / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((diff % (1000 * 60)) / 1000);


      if(hours < 4) setColorVariant("error");
      else if(hours < 8) setColorVariant("tertiary");

      setTimeLeft(
        `${hours.toString().padStart(2, "0")}:${minutes
          .toString()
          .padStart(2, "0")}:${seconds.toString().padStart(2, "0")}`
      );
    };

    updateTimer(); // initial call
    const interval = setInterval(updateTimer, 1000);

    return () => clearInterval(interval);
  }, [quest]);

  if (!quest) return null;

  return (
    <InfoTag title={timeLeft} icon={<LucideClock />} colorVariant={colorVariant} />
  );
}