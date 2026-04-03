type InfoTagProps = {
  title?: string;
  colorVariant?: "primary" | "secondary" | "tertiary" | "success" | "error";
  icon?: React.ReactNode;
};

const colorVariants = {
  primary: {
    bg: "bg-primary/15",
    border: "border-primary/30",
    text: "text-primary",
  },
  secondary: {
    bg: "bg-muted/15",
    border: "border-border",
    text: "text-muted-foreground",
  },
  tertiary: {
    bg: "bg-tertiary/15",
    border: "border-tertiary/30",
    text: "text-tertiary",
  },
  success: {
    bg: "bg-success/15",
    border: "border-success/30",
    text: "text-success",
  },
  error: {
    bg: "bg-error/15",
    border: "border-error/30",
    text: "text-error",
  },
};

export default function InfoTag({
  title,
  colorVariant = "primary",
  icon,
}: InfoTagProps) {
  const { bg, border, text } = colorVariants[colorVariant];

  return (
    <div
      className={`inline-flex items-center gap-2 px-4 py-1.5 rounded-full border ${bg} ${border} ${text} text-sm font-medium`}
    >
      {icon && <span className="flex items-center justify-center">{icon}</span>}
      {title && <span className="uppercase tracking-wide">{title}</span>}
    </div>
  );
}