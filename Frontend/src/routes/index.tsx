import { LandingPage } from '@/pages/LandingPage';
import { createBrowserRouter } from 'react-router-dom';


// Define the routes for the application
export const router = createBrowserRouter([
  {
    path: '/',
    element: <LandingPage />,
  }
]);
