import ExplanationStepComponent from "@/components/landing/ExplanationStepComponent";
import { Users, Sparkles, Camera, Gavel, TrendingUp } from "lucide-react";

const steps = [
  {
    colorVariant: "primary" as const,
    icon: <Users size={28} />,
    headingText: "Form Your Circle",
    descriptionText:
      "True legends never walk alone. Gather up to 6 of your closest allies to form a private guild.",
  },
  {
    colorVariant: "tertiary" as const,
    icon: <Sparkles size={28} />,
    headingText: "The Daily Choice",
    descriptionText:
      "Every sunrise, our AI Overseer selects one player from your circle to receive a unique, personalized quest.",
  },
  {
    colorVariant: "primary" as const,
    icon: <Camera size={28} />,
    headingText: "Proof of Valor",
    descriptionText:
      "Action is the only currency. Upload a photo or video documenting your quest completion before the midnight bell tolls.",
  },
  {
    colorVariant: "tertiary" as const,
    icon: <Gavel size={28} />,
    headingText: "The Council's Verdict",
    descriptionText:
      "Your circle acts as the judges. They can authenticate your glory or vote to skip quests that don't fit the vibe.",
  },
  {
    colorVariant: "primary" as const,
    icon: <TrendingUp size={28} />,
    headingText: "Ascension",
    descriptionText:
      "Amass XP, level up your profile, and unlock Legendary-tier quests with higher stakes and greater rewards.",
  },
];

export default function Guide() {
  return (
    <div className="page min-h-screen">
      {/* Header */}
      <div className="mb-8">
        <h1 className="font-heading text-4xl font-bold text-foreground leading-tight">
          Begin Your{" "}
          <span className="text-primary italic">Odyssey</span>
        </h1>
        <p className="text-muted-foreground mt-3 text-md leading-relaxed">
          QuestBound is a gamified daily adventure for you and your inner
          circle. Level up your real life, one quest at a time.
        </p>
      </div>

      {/* Steps with vertical connector line */}
      <div className="relative flex flex-col gap-6">
        {steps.map((step, index) => (
          <div key={index} className="relative">
            <ExplanationStepComponent
              colorVariant={step.colorVariant}
              icon={step.icon}
              headingText={step.headingText}
              descriptionText={step.descriptionText}
            />
          </div>
        ))}
      </div>
    </div>
  );
}