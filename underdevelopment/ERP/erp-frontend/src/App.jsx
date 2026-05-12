import { useState, useEffect } from 'react';
import { Navbar, Nav, Container } from 'react-bootstrap'
import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';
import Login from './components/Login';
import ProductList from './components/ProductList';
import ProductDetails from './components/ProductDetails';
import WarehouseManager from './components/WarehouseManager';
import InvoiceCreator from './components/InvoiceCreator';
import InvoiceList from './components/InvoiceList';
import Dashboard from './components/Dashboard'; 
import Reports from './components/Reports';
import LogisticsManager from './components/LogisticsManager';
import DeliveryTracker from './components/DeliveryTracker';
import AuditLog from './components/AuditLog';




function App() {
  const [isAuth, setIsAuth] = useState(!!sessionStorage.getItem('token'));
  const [userRole, setUserRole] = useState(null);

  useEffect(() => {
    const token = sessionStorage.getItem('token');
    if (token) {
      try {
        // JWT Payload dekódolása (a középső rész a pontok között)
        const payload = JSON.parse(atob(token.split('.')[1]));
        
        // A .NET Identity általában ebbe a hosszú nevű mezőbe rakja a Role-t, 
        // de biztos ami biztos, a sima 'role'-t is megnézzük.
        const role = payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload.role;
        setUserRole(role);
      } catch (e) {
        console.error("Érvénytelen token");
      }
    } else {
      setUserRole(null);
    }
  }, [isAuth]);

  return (
    <Router>
      {/* Csak akkor mutassuk a menüt, ha be van lépve */}
      {isAuth && (
        <Navbar bg="dark" variant="dark" expand="lg">
          <Container>
            <Nav className="me-auto">
              <Nav.Link as={Link} to="/dashboard">Dashboard</Nav.Link>
              {["Admin", "StockKeeper"].includes(userRole) && (
                <Nav.Link as={Link} to="/products">Termékek</Nav.Link>
              )}  
              {["Admin", "StockKeeper"].includes(userRole) && (
                <Nav.Link as={Link} to="/warehouses">Raktárak</Nav.Link>
              )} 
              {["Admin", "Salesman"].includes(userRole) && (
                <Nav.Link as={Link} to="/invoice/new">Új számla</Nav.Link>
              )} 
              {["Admin", "Salesman"].includes(userRole) && (
                <Nav.Link as={Link} to="/invoices">Számlák listája</Nav.Link>
              )}
              {userRole === 'Admin' && (
                <Nav.Link as={Link} to="/reports">Riportok</Nav.Link>
              )}
              {["Admin", "Logistic"].includes(userRole) && (
                <Nav.Link as={Link} to="/logistics">Rendelések</Nav.Link>
              )}
              {["Admin", "Logistic"].includes(userRole) && (
                <Nav.Link as={Link} to="/delivery-tracker">Kiszállítás</Nav.Link>
              )}
              {userRole === 'Admin' && (
                <Nav.Link as={Link} to="/auditlog">Tevékenységek</Nav.Link>
              )}
            </Nav>
            <Nav>
              <Nav.Link onClick={() => { sessionStorage.removeItem('token'); window.location.reload(); }}>
                Kijelentkezés
              </Nav.Link>
            </Nav>
          </Container>
        </Navbar>
      )}

      <Routes>
        <Route path="/" element={<Navigate to={isAuth ? "/dashboard" : "/login"} />} />

        <Route path="/login" element={!isAuth ? <Login onLoginSuccess={() => setIsAuth(true)} /> : <Navigate to="/" />} />

        <Route path="/dashboard" element={isAuth ? <Dashboard /> : <Navigate to="/login" />} />
        <Route path="/products" element={isAuth && ["Admin", "StockKeeper"].includes(userRole) ? <ProductList /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
        <Route path="/product/details/:id" element={isAuth ? <ProductDetails /> : <Navigate to="/login" />} />
        <Route path="/warehouses" element={isAuth && ["Admin", "StockKeeper"].includes(userRole) ? <WarehouseManager /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
        <Route path="/invoice/new" element={ isAuth && ["Admin", "Salesman"].includes(userRole) ? <InvoiceCreator /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
        <Route path="/invoices" element={isAuth && ["Admin", "Salesman"].includes(userRole) ? <InvoiceList /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
        <Route path="/reports" element={isAuth && userRole === 'Admin' ? <Reports /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
        <Route path="/logistics" element={isAuth && ["Admin", "Logistic"].includes(userRole) ? <LogisticsManager /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
        <Route path="/delivery-tracker" element={isAuth && ["Admin", "Logistic"].includes(userRole) ? <DeliveryTracker /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
        <Route path="/auditlog" element={isAuth && userRole === 'Admin' ? <AuditLog /> : <Navigate to={isAuth ? "/dashboard" : "/login"} />} />
      </Routes>
    </Router>
  );
}

export default App;