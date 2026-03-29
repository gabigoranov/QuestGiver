import { Button } from "../ui/button";

/**
 * Used in the landing page as a final call to action before the footer.
 *
 * @export
 * @return {*}
 */
export default function CTA() {
  return (
    <section id="cta" className="flex flex-col items-center justify-center py-12">
      {/* Heading */}
      <h1 className="text-text font-heading text-4xl font-semibold text-center">
        Ready to Level Up?
      </h1>

      {/* Subheading */}
      <p className="text-center mt-6 text-muted-foreground max-w-70">
        Join thousands of friends already transforming their daily routines.
      </p>

      {/* CTA Button */}
      <Button className="uppercase px-12 py-8 font-bold text-2xl rounded-full shadow-glow-primary mt-6 mb-24">Start Your Journey</Button>
    </section>
  );
}
