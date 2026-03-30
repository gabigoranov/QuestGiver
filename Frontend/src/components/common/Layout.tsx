import { Outlet } from "react-router-dom";
import Navigation from "./Navigation.tsx";

/**
 * Common layout used in the authenticated pages
 *
 * @export
 * @return A container that holds the main content and a bottom navigation bars
 */
export default function Layout() {
  return (
    <div className="min-h-screen flex flex-col">
      <main className="flex-1">
        <Outlet />
      </main>

      {/* Bottom navbar */}
      <Navigation />
    </div>
  );
}
