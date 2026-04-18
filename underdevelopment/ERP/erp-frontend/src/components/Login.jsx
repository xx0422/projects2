import { useState } from 'react';
import api from '../api/axios';
import { Form, Button, Card, Container, Alert } from 'react-bootstrap';

const Login = ({ onLoginSuccess }) => {
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [error, setError] = useState('');

    const handleSubmit = async (e) => {
        e.preventDefault();
        try {
            const response = await api.post('/auth/login', { email, password });
            localStorage.setItem('token', response.data.token);
            onLoginSuccess();
        } catch (err) {
            setError('Hibás email vagy jelszó!');
        }
    };

    return (
        <Container className="d-flex align-items-center justify-content-center" style={{ minHeight: "100vh" }}>
            <Card style={{ width: '400px' }} className="shadow-lg p-3">
                <Card.Body>
                    <h3 className="text-center mb-4">ERP Rendszer</h3>
                    {error && <Alert variant="danger">{error}</Alert>}
                    <Form onSubmit={handleSubmit}>
                        <Form.Group className="mb-3">
                            <Form.Label>Email cím</Form.Label>
                            <Form.Control type="email" placeholder="admin@erp.hu" onChange={e => setEmail(e.target.value)} required />
                        </Form.Group>
                        <Form.Group className="mb-4">
                            <Form.Label>Jelszó</Form.Label>
                            <Form.Control type="password" placeholder="********" onChange={e => setPassword(e.target.value)} required />
                        </Form.Group>
                        <Button variant="primary" type="submit" className="w-100">Belépés</Button>
                    </Form>
                </Card.Body>
            </Card>
        </Container>
    );
};

export default Login;