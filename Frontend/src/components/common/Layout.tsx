import { Outlet } from "react-router-dom";
import Navigation from "./Navigation.tsx";
import TopBar from "./TopBar.tsx";

/**
 * Common layout used in the authenticated pages
 *
 * @export
 * @return A container that holds the main content and a bottom navigation bars
 */
export default function Layout() {
  return (
    <div className="h-screen flex flex-col max-w-106.25">
      <TopBar />

      <main className="flex-1 min-h-0 overflow-y-auto">
        <Outlet />
      </main>

      {/* Bottom navbar */}
      <Navigation />
    </div>
  );
}
