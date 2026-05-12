import React, { useState, useEffect } from 'react';
import api from '../api/axios';
import { Table, Button, Container, Badge } from 'react-bootstrap';

const DeliveryTracker = () => {
    const [orders, setOrders] = useState([]);

    useEffect(() => {
        fetchInTransit();
    }, []);

    const fetchInTransit = async () => {
        const res = await api.get('/Logistics/in-transit-orders');
        setOrders(res.data);
    };

    const handleDelivered = async (orderId) => {
        if (!window.confirm("Biztosan rögzíti a kiszállítást?")) return;
        try {
            await api.post(`/Logistics/mark-as-delivered?orderId=${orderId}`);
            alert("Rendelés lezárva!");
            fetchInTransit(); // Lista frissítése
        } catch (err) {
            alert("Hiba történt a státusz frissítésekor.");
        }
    };

    return (
        <Container className="mt-4">
            <h2>🚚 Kiszállítás alatt lévő rendelések</h2>
            <Table striped bordered hover className="mt-3">
                <thead>
                    <tr>
                        <th>Rendelés #</th>
                        <th>Számlaszám</th>
                        <th>Vevő</th>
                        <th>Futár</th>
                        <th>Indítás dátuma</th>
                        <th>Művelet</th>
                    </tr>
                </thead>
                <tbody>
                    {orders.map(o => (
                        <tr key={o.orderId}>
                            <td>#{o.orderId}</td>
                            <td>{o.invoiceNumber}</td>
                            <td>{o.customerName}</td>
                            <td><Badge bg="dark">{o.carrier}</Badge></td>
                            <td>{new Date(o.dispatchDate).toLocaleString()}</td>
                            <td>
                                <Button variant="success" size="sm" onClick={() => handleDelivered(o.orderId)}>
                                    Kiszállítva
                                </Button>
                            </td>
                        </tr>
                    ))}
                    {orders.length === 0 && <tr><td colSpan="6" className="text-center">Nincs folyamatban lévő kiszállítás.</td></tr>}
                </tbody>
            </Table>
        </Container>
    );
};

export default DeliveryTracker;