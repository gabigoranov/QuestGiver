import { useNavigate } from "react-router-dom";
import { useTranslation } from "react-i18next";

export default function PageNotFound() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <div className="min-h-screen flex items-center justify-center px-6 bg-background">
      <div className="text-center max-w-md rounded-2xl bg-card p-8 shadow-glow-soft">
        <div className="mb-6">
          <h1 className="text-6xl font-heading text-primary mb-4">{t("pageNotFound.code")}</h1>
          <p className="text-lg text-muted-foreground">
            {t("pageNotFound.title")}
          </p>
        </div>

        <button
          onClick={() => navigate("/")}
          className="mt-6 w-full rounded-xl bg-primary text-primary-foreground py-3 font-medium shadow-glow-primary transition hover:brightness-110"
        >
          {t("pageNotFound.goHome")}
        </button>

        <p className="mt-6 text-sm text-muted-foreground">
          {t("pageNotFound.description")}
        </p>
      </div>
    </div>
  );
}