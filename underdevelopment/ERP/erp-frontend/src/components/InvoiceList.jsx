import { useState, useEffect } from 'react';
import { Container, Table, Button, Badge, Form, Card, Row, Col } from 'react-bootstrap';
import api from '../api/axios';

const InvoiceList = () => {
    const [invoices, setInvoices] = useState([]);
    const [statusFilter, setStatusFilter] = useState('');

    useEffect(() => {
        fetchInvoices();
    }, [statusFilter]);

    const fetchInvoices = async () => {
        try {
            const url = statusFilter ? `/Invoice?status=${statusFilter}` : '/Invoice';
            const res = await api.get(url);
            setInvoices(res.data);
        } catch (err) {
            console.error("Hiba a számlák lekérésekor:", err);
        }
    };

    const handlePayment = async (id, newStatus) => {
        try {
            await api.patch(`/Invoice/${id}/status`, newStatus, {headers: { 'Content-Type': 'application/json' }}); // 1 = Paid (PaymentStatus)
            alert("Pénzügyi státusz frissítve: Fizetve");
            fetchInvoices();
        } catch (err) {
            console.error("Hiba a fizetés rögzítésekor:", err);
            alert("Nem sikerült a státuszváltás.");
        }
    };

    const handleStatusChange = async (id, newStatus) => {
        try {
            await api.patch(`/Invoice/${id}/status`, newStatus, {headers: { 'Content-Type': 'application/json' }});
            alert(newStatus === 3 ? "Számla sztornózva, készlet visszatöltve." : "Státusz frissítve.");
            fetchInvoices();
        } catch (err) {
            console.error("Hiba a státuszváltáskor:", err);
            alert("Hiba történt: " + (err.response?.data?.error || "Ismeretlen hiba"));
        }
    };

    const downloadPdf = async (id, number) => {
    try {
        // 1. Lekérjük a PDF-et az axios-szal (ez már küldi a tokent a háttérben)
        const response = await api.get(`/Invoice/${id}/pdf`, {
            responseType: 'blob', // Megmondjuk, hogy bináris fájlt várunk
        });

        // 2. Létrehozunk egy belső URL-t a letöltött adathoz
        const file = new Blob([response.data], { type: 'application/pdf' });
        const fileURL = URL.createObjectURL(file);

        // 3. Megnyitjuk ezt a belső URL-t egy új ablakban
        window.open(fileURL, '_blank');

    } catch (err) {
        console.error("Hiba a PDF letöltésekor:", err);
        alert("Nem sikerült letölteni a PDF-et. Próbálj meg újra bejelentkezni!");
    }
};

    const getStatusBadge = (status) => {
        switch (status) {
            case 0: return <Badge bg="warning text-dark">Függőben (Pending)</Badge>;
            case 1: return <Badge bg="success">Fizetve (Paid)</Badge>;
            case 2: return <Badge bg="danger">Lejárt (Overdue)</Badge>;
            case 3: return <Badge bg="secondary">Sztornózott (Cancelled)</Badge>;
            default: return <Badge bg="light text-dark">{status}</Badge>;
        }
    };

    const renderDeliveryStatus = (status) => {
        switch(status) {
            case "Processing": return <Badge bg="warning">Raktározás alatt</Badge>;
            case "InTransit": return <Badge bg="primary">Kiszállítás alatt</Badge>;
            case "Delivered": return <Badge bg="success">Kiszállítva</Badge>;
            default: return <Badge bg="secondary">Nincs rendelés</Badge>;
        }
    };

    return (
        <Container className="mt-4">
            <Card className="shadow-sm">
                <Card.Header className="bg-dark text-white d-flex justify-content-between align-items-center">
                    <h4 className="mb-0">Számlatár</h4>
                    <div className="d-flex gap-2 align-items-center">
                        <small>Szűrés:</small>
                        <Form.Select 
                            size="sm" 
                            style={{ width: '150px' }}
                            onChange={(e) => setStatusFilter(e.target.value)}
                        >
                            <option value="">Mindegyik</option>
                            <option value="0">Pending</option>
                            <option value="1">Paid</option>
                            <option value="3">Cancelled</option>
                        </Form.Select>
                    </div>
                </Card.Header>
                <Card.Body className="p-0">
                    <Table responsive hover className="mb-0">
                        <thead className="table-light">
                            <tr>
                                <th>Számlaszám</th>
                                <th>Kelt</th>
                                <th>Vevő</th>
                                <th className="text-end">Bruttó összeg</th>
                                <th className="text-center">Fizetési státusz</th>
                                <th>Szállítási státusz</th>
                                <th className="text-end">Műveletek</th>
                            </tr>
                        </thead>
                        <tbody>
                            {invoices.length > 0 ? invoices.map(inv => (
                                <tr key={inv.id}>
                                    <td className="fw-bold">{inv.invoiceNumber}</td>
                                    <td>{new Date(inv.issueDate).toLocaleDateString()}</td>
                                    <td>{inv.customerName}</td>
                                    <td className="text-end">{inv.totalGross?.toLocaleString()} Ft</td>
                                    <td className="text-center">{getStatusBadge(inv.status)}</td>
                                    <td>{inv.order ? renderDeliveryStatus(inv.order.status) : <Badge bg="light" text="dark">N/A</Badge>}</td>
                                    <td className="text-end">
                                        <div className="d-flex justify-content-end gap-2">
                                            <Button variant="outline-info" size="sm" onClick={() => downloadPdf(inv.id, inv.invoiceNumber)}>
                                                PDF
                                            </Button>
                                            
                                            {(inv.status === "Pending" || inv.status === "Overdue") &&(
                                                <Button variant="outline-success" size="sm" onClick={() => handlePayment(inv.id, 1)}>
                                                    Fizetve
                                                </Button>
                                            )}
                                            
                                            {inv.status !== "Cancelled" && (
                                                <Button variant="outline-danger" size="sm" onClick={() => {
                                                    if(window.confirm("Biztosan sztornózod? A készlet visszakerül a raktárba!")) 
                                                        handleStatusChange(inv.id, 3)
                                                }}>
                                                    Sztornó
                                                </Button>
                                            )}
                                        </div>
                                    </td>
                                </tr>
                            )) : (
                                <tr><td colSpan="6" className="text-center py-4 text-muted">Nincs találat a számlák között.</td></tr>
                            )}
                        </tbody>
                    </Table>
                </Card.Body>
            </Card>
        </Container>
    );
};

export default InvoiceList;