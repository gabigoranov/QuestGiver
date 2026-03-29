import { useTranslation } from "react-i18next";
import { ChevronDown } from "lucide-react";
import { useState } from "react";

/**
 * Available languages in the application
 */
const languages = [
  { code: "en", label: "English" },
  { code: "bg", label: "Български" },
] as const;

/**
 * LanguageToggle component with dropdown selector
 * 
 * Displays the currently selected language and allows users to switch
 * between available languages (English and Bulgarian).
 * 
 * Features:
 * - Dropdown menu with language options
 * - Current language highlighted in the dropdown
 * - Smooth chevron rotation animation
 * - Uses app's design tokens for consistent styling
 * 
 * @example
 * ```tsx
 * <LanguageToggle />
 * ```
 * 
 * @returns A button with dropdown for language selection
 */
export default function LanguageToggle() {
  const { t, i18n } = useTranslation();
  const [isOpen, setIsOpen] = useState(false);

  /**
   * Currently selected language based on i18n state
   */
  const currentLanguage = languages.find((lang) => lang.code === i18n.language) || languages[0];

  /**
   * Handles language selection from dropdown
   * Changes the application language and closes the dropdown
   * 
   * @param code - The language code to switch to (e.g., "en" or "bg")
   */
  const handleLanguageSelect = (code: string) => {
    i18n.changeLanguage(code);
    setIsOpen(false);
  };

  /**
   * Toggles dropdown open/close state
   */
  const toggleDropdown = () => {
    setIsOpen(!isOpen);
  };

  return (
    <div className="relative">
      {/* Language selector button */}
      <button
        onClick={toggleDropdown}
        className="flex items-center gap-2 px-4 py-2 rounded-xl bg-surface hover:bg-surface-container-low border border-border text-sm text-foreground transition-colors"
        aria-label={t("footer.languageToggle.ariaLabel", "Select language")}
        aria-expanded={isOpen}
      >
        <span>{currentLanguage.label}</span>
        <ChevronDown
          size={16}
          className={`transition-transform duration-200 ${isOpen ? "rotate-180" : ""}`}
        />
      </button>

      {/* Dropdown menu */}
      {isOpen && (
        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-40 rounded-xl bg-background border border-border overflow-hidden z-50">
          {languages.map((lang) => (
            <button
              key={lang.code}
              onClick={() => handleLanguageSelect(lang.code)}
              className={`w-full px-4 py-2.5 text-left text-sm transition-colors ${
                lang.code === i18n.language
                  ? "bg-primary/10 text-primary"
                  : "text-foreground hover:bg-surface-container-low"
              }`}
            >
              {lang.label}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
