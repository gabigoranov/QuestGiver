import { createBrowserRouter } from 'react-router-dom';
import { HomePage } from '@/pages/LandingPage';


// Define the routes for the application
export const router = createBrowserRouter([
  {
    path: '/',
    element: <HomePage />,
  }
]);
