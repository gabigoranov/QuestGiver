import { Menu, User } from "lucide-react";
import AppLogo from "./AppLogo";
import { Link } from "react-router-dom";
import { useState } from "react";
import BottomSheet from "../common/BottomSheet";
import { ThemeToggle } from "./ThemeToggle";
import LanguageToggle from "./LanguageToggle";
import { useTranslation } from "react-i18next";

/**
 * Used in the main layout
 */
export default function TopBar() {
  const { t } = useTranslation();
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  return (
    <>
      <div className="px-4 py-3 flex bg-muted items-center justify-between shadow-glow-soft text-primary border border-b-border">
        {/* Hamburger menu */}
        <button
          onClick={() => setIsMenuOpen(true)}
          className="p-2 rounded-lg hover:bg-background/60 transition"
        >
          <Menu />
        </button>

        {/* Logo */}
        <Link to="/">
          <AppLogo />
        </Link>

        {/* Temporary default user avatar */}
        <div className="w-10 h-10 rounded-full bg-primary/20 flex items-center justify-center">
          <User size={24} />
        </div>
      </div>

      {/* Bottom Sheet Menu */}
      <BottomSheet isOpen={isMenuOpen} onClose={() => setIsMenuOpen(false)}>
        <div className="flex flex-col gap-6 pb-4">
          {/* Title */}
          <h2 className="text-lg font-semibold text-center">{t("topBar.settings")}</h2>

          {/* Theme */}
          <div className="flex items-center justify-between px-2">
            <span className="text-sm">{t("topBar.theme")}</span>
            <ThemeToggle />
          </div>

          {/* Language */}
          <div className="flex items-center justify-between px-2">
            <span className="text-sm">{t("topBar.language")}</span>
            <LanguageToggle />
          </div>
        </div>
      </BottomSheet>
    </>
  );
}
