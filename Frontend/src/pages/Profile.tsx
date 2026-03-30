import LanguageToggle from "@/components/common/LanguageToggle";
import { ThemeToggle } from "@/components/common/ThemeToggle";

export default function Profile() {
  return (
    <div className="flex flex-col gap-12">
      <ThemeToggle />

      <LanguageToggle />  
    </div>
  );
}
