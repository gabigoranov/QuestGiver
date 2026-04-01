import { useNavigate } from "react-router-dom";

export default function PageNotFound() {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen flex items-center justify-center px-6 bg-background">
      <div className="text-center max-w-md rounded-2xl bg-card p-8 shadow-glow-soft">
        <div className="mb-6">
          <h1 className="text-6xl font-heading text-primary mb-4">404</h1>
          <p className="text-lg text-muted-foreground">
            Oops! The page you’re looking for doesn’t exist.
          </p>
        </div>

        <button
          onClick={() => navigate("/")}
          className="mt-6 w-full rounded-xl bg-primary text-primary-foreground py-3 font-medium shadow-glow-primary transition hover:brightness-110"
        >
          Go Home
        </button>

        <p className="mt-6 text-sm text-muted-foreground">
          Check the URL or return to the homepage.
        </p>
      </div>
    </div>
  );
}