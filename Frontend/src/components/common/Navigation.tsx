import { useTranslation } from "react-i18next";
import { useLocation, useNavigate } from "react-router-dom";
import { Compass, Users, Plus, BarChart3, User } from "lucide-react";
import NavItem from "./NavItem";

/**
 * Bottom navigation bar used across authenticated pages.
 */
export default function Navigation() {
  const { t } = useTranslation();
  const location = useLocation();
  const navigate = useNavigate();

  const isActive = (path: string) => location.pathname === path;

  return (
    <nav className="fixed bottom-0 left-1/2 -translate-x-1/2 w-full rounded-t-3xl border-t border-border bg-muted backdrop-blur-md px-2 pb-4 pt-3 flex justify-between">
      <NavItem
        icon={Compass}
        label={t('navigation.quests')}
        path="/home"
        active={isActive("/home")}
        onClick={navigate}
      />
      <NavItem
        icon={Users}
        label={t('navigation.groups')}
        path="/groups"
        active={isActive("/groups")}
        onClick={navigate}
      />
      <NavItem
        icon={Plus}
        label={t('navigation.create')}
        path="/create"
        active={isActive("/create")}
        onClick={navigate}
      />
      <NavItem
        icon={BarChart3}
        label={t('navigation.rank')}
        path="/rank"
        active={isActive("/rank")}
        onClick={navigate}
      />
      <NavItem
        icon={User}
        label={t('navigation.profile')}
        path="/profile"
        active={isActive("/profile")}
        onClick={navigate}
      />
    </nav>
  );
}
