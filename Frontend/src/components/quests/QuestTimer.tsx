import { useEffect, useState } from "react";
import { LucideClock } from "lucide-react";
import type { QuestDTO } from "@/types/Receive/QuestDTO";
import InfoTag from "../common/InfoTag";


/**
 * Used in the main group view page to display the deadline for
 * today's quest
 *
 * @export
 * @param {{ quest: QuestDTO }} { quest }
 * @return {*} 
 */
export function QuestTimer({ quest }: { quest: QuestDTO }) {
  const [timeLeft, setTimeLeft] = useState<string>("");

  useEffect(() => {
    if (!quest?.scheduledDate) return;

    const updateTimer = () => {
      const now = new Date();
      const scheduled = new Date(quest.scheduledDate);

      // Calculate the next 24-hour "unit" from scheduled UTC date
      const nextDeadline = new Date(scheduled.getTime() + 24 * 60 * 60 * 1000);

      const diff = nextDeadline.getTime() - now.getTime();

      if (diff <= 0) {
        setTimeLeft("Deadline reached");
        return;
      }

      const hours = Math.floor(diff / (1000 * 60 * 60));
      const minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
      const seconds = Math.floor((diff % (1000 * 60)) / 1000);

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
    <InfoTag title={timeLeft} icon={<LucideClock />} colorVariant="tertiary" />
  );
}