import { useTranslation } from "react-i18next";
import { Button } from "../ui/button";

/**
 * The hero section of the landing page
 *
 * @export
 * @return a section holding a heading, description adn cta buttons
 */
export default function Hero() {
  const { t } = useTranslation();

  return (
    <section id="hero" className="relative flex flex-col px-8 py-32 overflow-hidden">
        {/* Glowing background elements */}
        <div className="absolute inset-0 pointer-events-none">
            <div className="absolute top-[10%] left-0 transform -translate-x-1/2 -translate-y-1/2 w-[110%] h-[50%] bg-primary/20 rounded-full blur-3xl -z-10"></div>
        </div>

        {/* Hero Heading */}
        <h1 className="text-5xl font-heading font-bold text-center" dangerouslySetInnerHTML={{ __html: t("hero.title") }}></h1>

        {/* Hero Description */}
        <p className="text-lg text-center mt-6 text-muted-foreground">{t("hero.description")}</p>

        {/* CTA Buttons */}
        <div className="flex flex-col gap-4 mt-12 justify-center">
            <Button className="text-lg font-bold py-7 rounded-4xl bg-linear-to-r from-primary to-secondary/20 shadow-glow-primary">{t("hero.startJourney")}</Button>
            <Button variant="secondary" className="text-lg font-semibold py-7 rounded-4xl">{t("hero.learnMore")}</Button>
        </div>

    </section>
  )
}
