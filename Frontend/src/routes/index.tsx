import { LandingPage } from "@/pages/LandingPage";
import SignIn from "@/pages/SignIn";
import SignUp from "@/pages/SignUp";
import { createBrowserRouter } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoute";
import Layout from "@/components/common/Layout";
import Profile from "@/pages/Profile";
import CreateGroup from "@/pages/CreateGroup";
import Groups from "@/pages/Groups";
import Home from "@/pages/Home";
import Group from "@/pages/Group";
import JoinGroup from "@/pages/JoinGroup";
import Guide from "@/pages/Guide";
import PageNotFound from "@/components/common/PageNotFound";

// Define the routes for the application
export const router = createBrowserRouter([
  {
    path: "*",
    element: <PageNotFound />, // For unexisting pages
  },
  {
    path: "/",
    element: <LandingPage />,
  },
  {
    path: "/signin",
    element: <SignIn />,
  },
  {
    path: "/signup",
    element: <SignUp />,
  },
  {
    element: (
      <ProtectedRoute>
        <Layout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: "/home",
        element: <Home />,
      },
      {
        path: "/groups",
        element: <Groups />,
      },
      {
        path: "/profile",
        element: <Profile />,
      },
      {
        path: "/create",
        element: <CreateGroup />,
      },
      {
        path: "/group/:groupId",
        element: <Group />,
      },
      {
        path: "/group/join/:groupId",
        element: <JoinGroup />,
      },
      {
        path: "/guide",
        element: <Guide />,
      },
      // future pages:
    ],
  },
]);
