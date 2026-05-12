import React, { useState, useEffect } from 'react';
import { Table, Button, Modal, Form, Badge, Card } from 'react-bootstrap';
import api from '../api/axios';

const LogisticsManager = () => {
    const [orders, setOrders] = useState([]);
    const [showModal, setShowModal] = useState(false);
    const [selectedOrder, setSelectedOrder] = useState(null);
    const [shipmentData, setShipmentData] = useState({ carrier: '' }); // Tracking törölve, mert a backend generálja
    const [loading, setLoading] = useState(false); // Ezt definiálni kell, ha használod a finally-ban

    const fetchOrders = async () => {
        try {
            const res = await api.get('/Logistics/pending-orders');
            setOrders(res.data);
        } catch (err) {
            console.error("Hiba a rendelések betöltésekor:", err);
        }
    };

    useEffect(() => {
        fetchOrders();
    }, []);

    const handleDispatch = async () => {
        try {
            setLoading(true); // Hiányzott a hívás elejéről
            await api.post(`/Logistics/dispatch`, null, {
                params: {
                    orderId: selectedOrder.orderId, 
                    carrier: shipmentData.carrier
                }
            });
            
            setShowModal(false);
            setShipmentData({ carrier: '' });
            fetchOrders();
            alert("Rendelés útnak indítva!");
        } catch (err) {
            alert("Hiba: " + (err.response?.data || "Sikertelen mentés"));
        } finally {
            setLoading(false);
        } 
    };

    const viewPdf = async (id) => {
        try {
            // Lekérjük a PDF-et blob formátumban
            const res = await api.get(`/Invoice/${id}/pdf`, { responseType: 'blob' });
            
            // Létrehozunk egy URL-t a blobhoz
            const file = new Blob([res.data], { type: 'application/pdf' });
            const fileURL = URL.createObjectURL(file);
            
            // Megnyitjuk egy új fülön
            window.open(fileURL, '_blank');
            
        } catch (err) {
            console.error("Hiba a PDF megnyitásakor:", err);
            alert("Nem sikerült megnyitni a számlát.");
        }
    };

    return (
        <div className="container mt-4">
            <Card className="shadow-sm">
                <Card.Header className="bg-primary text-white d-flex justify-content-between align-items-center">
                    <h4 className="mb-0">🚚 Szállításra Váró Rendelések</h4>
                    <Badge bg="light" text="dark">{orders.length} új feladat</Badge>
                </Card.Header>
                <Card.Body>
                    <Table hover responsive>
                        <thead className="table-light">
                            <tr>
                                <th>Rendelés #</th>
                                <th>Dátum</th>
                                <th>Számlaszám</th>
                                <th>Státusz</th>
                                <th>Művelet</th>
                            </tr>
                        </thead>
                        <tbody>
                            {orders.map(order => (
                                <tr key={order.orderId}>
                                    <td>{order.orderId}</td>
                                    <td>{new Date(order.orderDate).toLocaleDateString()}</td>
                                    <td>
                                        {/* Most már kattintható a számlaszám */}
                                        <Button 
                                            variant="link" 
                                            className="p-0 text-decoration-none fw-bold"
                                            onClick={() => viewPdf(order.invoiceId)}
                                        >
                                            {order.invoiceNumber}
                                        </Button>
                                    </td>
                                    <td>{order.status === "Processing" ? "Feldolgozás alatt" : order.status}</td>                           
                                    <td>
                                        <Button variant="primary" size="sm" onClick={() => {
                                            setSelectedOrder(order);
                                            setShowModal(true);
                                        }}>
                                            Szállítás indítása
                                        </Button>
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </Card.Body>
            </Card>

            <Modal show={showModal} onHide={() => setShowModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Szállítás indítása - Számla: {selectedOrder?.invoiceNumber}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>Futárszolgálat</Form.Label>
                            <Form.Control 
                                type="text" 
                                placeholder="Pl. GLS, MPL, DHL"
                                value={shipmentData.carrier}
                                onChange={(e) => setShipmentData({carrier: e.target.value})}
                            />
                        </Form.Group>
                        <p className="text-muted small">A csomagazonosító automatikusan generálódik.</p>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowModal(false)}>Mégse</Button>
                    <Button 
                        variant="primary" 
                        onClick={handleDispatch}
                        disabled={!shipmentData.carrier || loading} // Itt kivettem a tracking feltételt
                    >
                        {loading ? "Feldolgozás..." : "Futárnak átadva"}
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    );
};

export default LogisticsManager;