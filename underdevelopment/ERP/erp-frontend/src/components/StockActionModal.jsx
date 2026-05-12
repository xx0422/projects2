import { useState, useEffect } from 'react';
import { Modal, Button, Form, Row, Col } from 'react-bootstrap';
import api from '../api/axios';

const StockActionModal = ({ show, onHide, productId, productName, isPerishable, onEntrySuccess }) => {
    const [warehouses, setWarehouses] = useState([]);
    const [formData, setFormData] = useState({
        warehouseId: '',
        quantity: 1,
        unitPrice: 0,
        expirationDate: ''
    });

    useEffect(() => {
        if (show) {
            // Raktárak lekérése a legördülő listához
            api.get('/Warehouse').then(res => {
                setWarehouses(res.data);
                if (res.data.length > 0) setFormData(prev => ({ ...prev, warehouseId: res.data[0].id }));
            });
        }
    }, [show]);

    const handleSubmit = async () => {
        try {
            let url = `/Stock/receipt?productId=${productId}&warehouseId=${formData.warehouseId}&quantity=${formData.quantity}&unitPrice=${formData.unitPrice}`;
            if (formData.expirationDate) {
                url += `&expirationDate=${formData.expirationDate}`;
            }
            // A StockController "receipt" végpontjának hívása
            await api.post(url);
            alert("Sikeres bevételezés!");
            onEntrySuccess(); // Lista frissítése
            onHide();
        } catch (err) {
            alert("Hiba: " + (err.response?.data?.message || "Sikertelen művelet"));
        }
    };

    return (
        <Modal show={show} onHide={onHide}>
            <Modal.Header closeButton className="bg-success text-white">
                <Modal.Title>Bevételezés: {productName}</Modal.Title>
            </Modal.Header>
            <Modal.Body>
                <Form>
                    <Form.Group className="mb-3">
                        <Form.Label>Célraktár</Form.Label>
                        <Form.Select 
                            value={formData.warehouseId} 
                            onChange={e => setFormData({...formData, warehouseId: e.target.value})}
                        >
                            {warehouses.map(w => <option key={w.id} value={w.id}>{w.name} ({w.location})</option>)}
                        </Form.Select>
                    </Form.Group>
                    <Row>
                        <Col>
                            <Form.Group className="mb-3">
                                <Form.Label>Mennyiség</Form.Label>
                                <Form.Control 
                                    type="number" 
                                    value={formData.quantity} 
                                    onChange={e => setFormData({...formData, quantity: e.target.value})} 
                                />
                            </Form.Group>
                        </Col>
                        <Col>
                            <Form.Group className="mb-3">
                                <Form.Label>Beszerzési egységár (Ft)</Form.Label>
                                <Form.Control 
                                    type="number" 
                                    value={formData.unitPrice} 
                                    onChange={e => setFormData({...formData, unitPrice: e.target.value})} 
                                />
                            </Form.Group>
                        </Col>
                    </Row>
                    {isPerishable && (
                    <Form.Group className="mb-3 border p-2 border-warning rounded">
                        <Form.Label className="text-warning fw-bold">Lejárati dátum</Form.Label>
                        <Form.Control 
                            type="date" 
                            onChange={e => setFormData({...formData, expirationDate: e.target.value})} 
                        />
                    </Form.Group>
                )}
                </Form>
            </Modal.Body>
            <Modal.Footer>
                <Button variant="secondary" onClick={onHide}>Mégse</Button>
                <Button variant="success" onClick={handleSubmit}>Bevételezés rögzítése</Button>
            </Modal.Footer>
        </Modal>
    );
};

export default StockActionModal;