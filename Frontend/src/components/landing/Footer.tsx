import { useTranslation } from "react-i18next";
import AppLogo from "../common/AppLogo";
import LanguageToggle from "../common/LanguageToggle";

/**
 * Simple footer for the landing page
 *
 * @export
 * @return {*}
 */
export default function Footer() {
  const { t } = useTranslation();

  return (
    <footer className="w-full border-t border-border bg-muted backdrop-blur-md">
      <div className="max-w-5xl mx-auto px-6 py-10 flex flex-col items-center gap-6 text-center">
        {/* Footer Links */}
        <div className="flex gap-8 text-sm text-muted-foreground">
          <a href="#" className="hover:text-(--on-surface) transition-colors">
            {t("footer.terms")}
          </a>
          <a href="#" className="hover:text-(--on-surface) transition-colors">
            {t("footer.privacy")}
          </a>
          <a href="#" className="hover:text-(--on-surface) transition-colors">
            {t("footer.support")}
          </a>
        </div>

        {/* Language Selector */}
        <LanguageToggle />

        <AppLogo />

        <p className="text-xs text-muted-foreground">
          {t("footer.copyright")}
        </p>
      </div>
    </footer>
  );
}
