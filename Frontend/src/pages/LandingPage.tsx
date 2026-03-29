import CommonQuestions from "@/components/landing/CommonQuestions";
import CTA from "@/components/landing/CTA";
import Explanation from "@/components/landing/Explanation";
import Footer from "@/components/landing/Footer";
import Hero from "@/components/landing/Hero";
import Navbar from "@/components/landing/Navbar";


/**
 * LandingPage component that serves as the landing page for QuestGiver. It includes the Navbar, Hero section, Features section, and Footer.
 *
 * @export
 * @returns a container that holds the landing page layout 
 */
export function LandingPage() {
  return (
    <div className="min-h-screen flex flex-col">
      {/* Navbar */}
      <Navbar />

      {/* Hero Section */}
      <Hero />

      {/* Explanation Section */}
      <Explanation />

      {/* FAQ */}
      <CommonQuestions />

      {/* CTA */}
      <CTA />

      {/* Footer */}
      <Footer />
    </div>
  );
}
