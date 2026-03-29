import { LucideCamera, LucideSparkles, LucideUsersRound } from "lucide-react";
import ExplanationStepComponent from "./ExplanationStepComponent";

/**
 * Part of the Landing page
 *
 * @export
 * @return A container holding the different steps of the quest cycle and a heading
 */
export default function Explanation() {
  return (
    <section id="how-it-works" className="relative w-full h-auto">
      {/* Grid background */}
      <div className="absolute inset-0 h-full w-full bg-muted bg-[linear-gradient(to_right,var(--border),transparent_1px),linear-gradient(to_bottom,var(--border),transparent_1px)] bg-size-[calc(100%/12)_calc(100%/12)]"></div>

      {/* Content */}
      <div className="relative z-10 px-6 py-20 flex flex-col items-center text-center gap-12">
        {/* Headings */}
        <div className="flex flex-col gap-4">
          <h4 className="uppercase text-sans text-primary font-semibold tracking-[0.3rem] text-sm">
            The Gameplay Experience
          </h4>
          <h2 className="text-4xl font-heading font-bold">The Quest Cycle</h2>
        </div>

        {/* Explanation Steps */}
        <div className="relative flex flex-col gap-10 w-full h-auto">
          {/* Connecting line */}
          <div className="absolute left-1/2 top-0 translate-x-[-50%] h-full w-1 bg-muted-foreground/30 -z-1"></div>

          <ExplanationStepComponent
            colorVariant="primary"
            headingText="AI Quest Generation"
            descriptionText="AI crafts a unique quest based on your profile, city, history, and real-time location."
            icon={<LucideSparkles size={40} />}
          />

          <ExplanationStepComponent
            colorVariant="tertiary"
            headingText="Upload Evidence"
            descriptionText="Complete the objective and capture a moment of victory. Every proof adds to your legacy."
            icon={<LucideCamera size={40} />}
          />

          <ExplanationStepComponent
            colorVariant="primary"
            headingText="Community Vouch"
            descriptionText="Your friends verify your feats, awarding XP and rare badges."
            icon={<LucideUsersRound size={40} />}
          />
        </div>
      </div>
    </section>
  );
}
