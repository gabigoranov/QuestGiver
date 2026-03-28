import { createContext } from 'react';
import type { i18n } from 'i18next';


/**
 * Props for the LanguageContext
 * which handles switching between the available localizations
 *
 * @interface LanguageContextType
 */
interface LanguageContextType {
  language: string;
  changeLanguage: (lng: string) => Promise<void>;
  i18n: i18n;
}

export const LanguageContext = createContext<LanguageContextType | undefined>(undefined);

