import Home from "@/pages/Home";
import { LandingPage } from "@/pages/LandingPage";
import SignIn from "@/pages/SignIn";
import SignUp from "@/pages/SignUp";
import { createBrowserRouter } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoute";

// Define the routes for the application
export const router = createBrowserRouter([
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
    path: "/home",
    element: (
      <ProtectedRoute>
        <Home />
      </ProtectedRoute>
    ),
  },
]);
