import ExplanationStepComponent from "@/components/landing/ExplanationStepComponent";
import { useTranslation } from "react-i18next";
import { Users, Sparkles, Camera, Gavel, TrendingUp } from "lucide-react";

/**
 * A simple page to give an idea of how to user the app
 * to the user
 *
 * @export
 * @return {*}
 */
export default function Guide() {
  const { t } = useTranslation();
  
  const steps = [
    {
      colorVariant: "primary" as const,
      icon: <Users size={28} />,
      headingText: t("guide.formCircle.title"),
      descriptionText: t("guide.formCircle.description"),
    },
    {
      colorVariant: "tertiary" as const,
      icon: <Sparkles size={28} />,
      headingText: t("guide.dailyChoice.title"),
      descriptionText: t("guide.dailyChoice.description"),
    },
    {
      colorVariant: "primary" as const,
      icon: <Camera size={28} />,
      headingText: t("guide.proofOfValor.title"),
      descriptionText: t("guide.proofOfValor.description"),
    },
    {
      colorVariant: "tertiary" as const,
      icon: <Gavel size={28} />,
      headingText: t("guide.councilVerdict.title"),
      descriptionText: t("guide.councilVerdict.description"),
    },
    {
      colorVariant: "primary" as const,
      icon: <TrendingUp size={28} />,
      headingText: t("guide.ascension.title"),
      descriptionText: t("guide.ascension.description"),
    },
  ];

  return (
    <div className="page min-h-screen">
      {/* Header */}
      <div className="mb-8">
        <h1 className="font-heading text-4xl font-bold text-foreground leading-tight">
          {t("guide.beginYour")}
          <span className="text-primary italic">{t("guide.odyssey")}</span>
        </h1>
        <p className="text-muted-foreground mt-3 text-md leading-relaxed">
          {t("guide.description")}
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