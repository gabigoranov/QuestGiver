import AppLogo from "../common/AppLogo";


/**
 * Simple footer for the landing page
 *
 * @export
 * @return {*} 
 */
export default function Footer() {
  return (
    <footer className="w-full border-t border-border bg-muted backdrop-blur-md">
      <div className="max-w-5xl mx-auto px-6 py-10 flex flex-col items-center gap-6 text-center">
        {/* Footer Links */}
        <div className="flex gap-8 text-sm text-muted-foreground">
          <a href="#" className="hover:text-(--on-surface) transition-colors">
            Terms
          </a>
          <a href="#" className="hover:text-(--on-surface) transition-colors">
            Privacy
          </a>
          <a href="#" className="hover:text-(--on-surface) transition-colors">
            Support
          </a>
        </div>


        <AppLogo />

        <p className="text-xs text-muted-foreground">
          © 2026 QuestBound. Forge your destiny.
        </p>
      </div>
    </footer>
  );
}