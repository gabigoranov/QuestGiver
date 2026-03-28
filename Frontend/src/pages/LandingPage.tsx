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
    </div>
  );
}
