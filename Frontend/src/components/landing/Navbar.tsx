    import AppLogo from "../common/AppLogo";
    import { Button } from "../ui/button";


    /**
     * Navigation bar for the landing page
     *
     * @export
     * @return a container that holds the logo and a sign in button 
     */
    export default function Navbar() {
    return (
        <div
        id="navbar"
        className="bg-surface flex items-center justify-between px-8 py-4 border-border border-b z-999"
        >
        <AppLogo />

        <Button value="primary" className="text-lg text-secondary font-semibold">
            Sign In
        </Button>
        </div>
    );
    }
