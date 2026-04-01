import { Menu, User } from "lucide-react";
import AppLogo from "./AppLogo";
import { Link } from "react-router-dom";

/**
 * Used in the main layout
 *
 * @export
 * @return {*}
 */
export default function TopBar() {
  return (
    <div className="px-4 py-3 flex bg-muted items-center justify-between shadow-glow-soft text-primary border border-b-border">
      {/* Hamburger menu */}
      <Menu />

      <Link to="/">
        <AppLogo />
      </Link>

      {/* Temporary default user avatar */}
      <div className="w-10 h-10 rounded-full bg-primary/20 flex items-center justify-center">
        <User size={24} />
      </div>
    </div>
  );
}
