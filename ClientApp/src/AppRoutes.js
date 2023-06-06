import { Home } from "./components/Home";
import Customers from "./components/Customers";

const AppRoutes = [
  {
    index: true,
    element: <Home />
  },
  {
    path: '/customers',
    element: <Customers />
  },
];

export default AppRoutes;
