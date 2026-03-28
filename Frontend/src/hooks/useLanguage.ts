import { LanguageContext } from "@/context/LanguageContext";
import { useContext } from "react";

// Hook for accessing language context
export const useLanguage = () => {
  const context = useContext(LanguageContext);
  if (!context) {
    throw new Error('useLanguage must be used within a LanguageProvider');
  }
  return context;
};
