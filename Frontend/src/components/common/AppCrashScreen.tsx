import { LucideTriangleAlert } from "lucide-react";
import { useEffect } from "react";

type Props = {
  error?: Error;
  reset?: () => void;
};

export default function AppCrashScreen({ error, reset }: Props) {
  useEffect(() => {
    console.error(error);
  }, [error]);

  return (
    <div className="min-h-screen flex items-center justify-center px-6">
      <div className="w-full max-w-md rounded-2xl bg-card p-8 text-center shadow-glow-soft">
        <div className="mb-6">
          <div className="mx-auto h-14 w-14 rounded-full bg-primary/80 flex items-center justify-center">
            <span className="text-2xl"><LucideTriangleAlert/></span>
          </div>
        </div>

        <h1 className="text-xl font-heading mb-2">
          Something went wrong
        </h1>

        <p className="text-sm text-muted-foreground mb-6">
          The app ran into an unexpected error. Try refreshing or come back later.
        </p>

        <div className="flex flex-col gap-3">
          <button
            onClick={() => window.location.reload()}
            className="w-full rounded-xl bg-primary text-primary-foreground py-3 font-medium shadow-glow-primary"
          >
            Reload app
          </button>

          {reset && (
            <button
              onClick={reset}
              className="w-full rounded-xl bg-muted text-muted-foreground py-3 font-medium"
            >
              Try again
            </button>
          )}
        </div>

        {error && (
          <p className="mt-6 text-xs text-muted-foreground line-clamp-2">
            {error.message}
          </p>
        )}
      </div>
    </div>
  );
}