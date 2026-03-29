type ExplanationStepComponentProps = {
  colorVariant?: "primary" | "secondary" | "tertiary";
  icon: React.ReactNode;
  headingText: string;
  descriptionText: string;
};

const colorVariants = {
  primary: {
    bg: "bg-primary",
    border: "border-primary/30",
    text: "text-primary-foreground",
  },
  secondary: {
    bg: "bg-secondary",
    border: "border-secondary/30",
    text: "text-secondary-foreground",
  },
  tertiary: {
    bg: "bg-tertiary",
    border: "border-tertiary/30",
    text: "text-tertiary-foreground",
  },
};

/**
 * Used in the How-It-Works section
 * Provides a customizeable component for the steps section
 *
 * @export
 * @param {ExplanationStepComponentProps} { primaryColor, secondaryColor }
 * @return a container representing the component
 */
export default function ExplanationStepComponent({
  colorVariant = "primary",
  headingText = "Step Heading",
  descriptionText = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod.",
  icon,
}: ExplanationStepComponentProps) {
  const { bg, border, text } = colorVariants[colorVariant];

  return (
    <div
      className={`w-full h-64 p-8 bg-card rounded-4xl ${border} border flex items-start justify-between shadow-glow-large`}
    >
      {/* Icon container */}
      <div
        className={`w-20 h-20 rounded-2xl ${bg} flex items-center justify-center ${text} text-lg font-bold shadow-glow-soft`}
      >
        {icon}
      </div>

      {/* Content */}
      <div className="flex flex-col items-start justify-start text-start w-4/6">
        <h3 className={`text-3xl font-bold text-text`}>
          {headingText}
        </h3>
        <p className={`text-muted-foreground mt-2 text-text`}>
          {descriptionText}
        </p>
      </div>
    </div>
  );
}
