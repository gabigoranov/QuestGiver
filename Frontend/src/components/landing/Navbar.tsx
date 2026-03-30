import { useTranslation } from "react-i18next";
import AppLogo from "../common/AppLogo";
import { Button } from "../ui/button";
import { Link } from "react-router-dom";

/**
 * Navigation bar for the landing page
 *
 * @export
 * @return a container that holds the logo and a sign in button
 */
export default function Navbar() {
  const { t } = useTranslation();

  return (
    <div
      id="navbar"
      className="bg-surface flex items-center justify-between px-8 py-4 border-border border-b z-999"
    >
      <AppLogo />

      <Link to="/home">
        <Button
          value="primary"
          className="text-lg font-semibold"
        >
          {t("navigation.signIn")}
        </Button>
      </Link>
    </div>
  );
}
