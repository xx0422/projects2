import { useState, useEffect } from 'react';
import { Navbar, Nav, Container } from 'react-bootstrap'
import { BrowserRouter as Router, Routes, Route, Navigate, Link } from 'react-router-dom';
import Login from './components/Login';
import ProductList from './components/ProductList';

const Dashboard = () => (
    <div className="container mt-5">
        <div className="p-5 mb-4 bg-light rounded-3 shadow border">
            <h1>Üdv az ERP vezérlőpulton!</h1>
            <p className="lead">Sikeresen azonosítottad magad a .NET Backenden keresztül.</p>
            <button className="btn btn-danger" onClick={() => {
                localStorage.removeItem('token');
                window.location.reload();
            }}>Kijelentkezés</button>
        </div>
    </div>
);


function App() {
  const [isAuth, setIsAuth] = useState(!!localStorage.getItem('token'));

  return (
    <Router>
      {/* Csak akkor mutassuk a menüt, ha be van lépve */}
      {isAuth && (
        <Navbar bg="dark" variant="dark" expand="lg">
          <Container>
            <Navbar.Brand as={Link} to="/">ERP Rendszer</Navbar.Brand>
            <Nav className="me-auto">
              <Nav.Link as={Link} to="/">Dashboard</Nav.Link>
              <Nav.Link as={Link} to="/products">Termékek</Nav.Link>
            </Nav>
            <Nav>
              <Nav.Link onClick={() => { localStorage.removeItem('token'); window.location.reload(); }}>
                Kijelentkezés
              </Nav.Link>
            </Nav>
          </Container>
        </Navbar>
      )}

      <Routes>
        <Route path="/login" element={!isAuth ? <Login onLoginSuccess={() => setIsAuth(true)} /> : <Navigate to="/" />} />
        <Route path="/" element={isAuth ? <Dashboard /> : <Navigate to="/login" />} />
        <Route path="/products" element={isAuth ? <ProductList /> : <Navigate to="/login" />} />
      </Routes>
    </Router>
  );
}

export default App;