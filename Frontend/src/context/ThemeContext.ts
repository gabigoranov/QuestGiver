import { createContext } from 'react';

type Theme = 'light' | 'dark';


/**
 * Defines the props for the ThemeContext
 * which controls the app theme
 *
 * @interface ThemeContextType
 */
interface ThemeContextType {
  theme: Theme;
  toggleTheme: () => void;
}

export const ThemeContext = createContext<ThemeContextType | undefined>(undefined);



