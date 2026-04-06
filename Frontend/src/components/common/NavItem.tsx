import type { LucideIcon } from "lucide-react";

type NavItemProps = {
  icon: LucideIcon;
  label: string;
  path: string;
  active: boolean;
  onClick: (path: string) => void;
};

/**
 * Single navigation item
 *
 * Keeps consistent size across all items.
 * Active state only changes inner styling to avoid layout shift.
 */
export default function NavItem({
  icon: Icon,
  label,
  path,
  active,
  onClick,
}: NavItemProps) {
  return (
    <button
      onClick={() => onClick(path)}
      className={"flex-1 flex justify-center"}
    >
      {/* Inner container controls visual state */}
      <div
        className={`flex flex-col min-w-0 flex-1 items-center justify-center gap-1 text-xs px-2 py-2 rounded-full transition ${
          active ? "bg-primary-container text-primary" : "text-foreground/40"
        }`}
      >
        <Icon size={28} />
        <span className="uppercase mt-1 font-semibold tracking-wide">
          {label}
        </span>
      </div>
    </button>
  );
}