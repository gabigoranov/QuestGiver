import { Navbar } from '@/components/landing/Navbar';
import { Hero } from '@/components/landing/Hero';
import { Features } from '@/components/landing/Features';
import { Footer } from '@/components/landing/Footer';


/**
 * HomePage component that serves as the landing page for QuestGiver. It includes the Navbar, Hero section, Features section, and Footer.
 *
 * @export
 * @returns a container that holds the landing page layout 
 */
export function HomePage() {
  return (
    <div className="min-h-screen flex flex-col">
      <Navbar />
      <main className="flex-1">
        <Hero />
        <Features />
      </main>
      <Footer />
    </div>
  );
}
