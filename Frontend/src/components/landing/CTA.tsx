import { useTranslation } from "react-i18next";
import { Button } from "../ui/button";
import { Link } from "react-router-dom";

/**
 * Used in the landing page as a final call to action before the footer.
 *
 * @export
 * @return {*}
 */
export default function CTA() {
  const { t } = useTranslation();

  return (
    <section
      id="cta"
      className="flex flex-col items-center justify-center py-12"
    >
      {/* Heading */}
      <h1 className="text-text font-heading text-4xl font-semibold text-center">
        {t("cta.title")}
      </h1>

      {/* Subheading */}
      <p className="text-center mt-6 text-muted-foreground max-w-70">
        {t("cta.description")}
      </p>

      {/* CTA Button */}
      <Link to="/home" viewTransition>
        <Button className="uppercase px-12 py-8 font-bold text-2xl rounded-full shadow-glow-primary mt-6 mb-24">
          {t("cta.button")}
        </Button>
      </Link>
    </section>
  );
}
