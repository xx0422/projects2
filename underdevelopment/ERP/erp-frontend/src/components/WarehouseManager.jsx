import { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Button, Form, Modal } from 'react-bootstrap';
import api from '../api/axios';

const WarehouseManager = () => {
    const [warehouses, setWarehouses] = useState([]);
    const [selectedWarehouseId, setSelectedWarehouseId] = useState('');
    const [inventory, setInventory] = useState([]);
    const [showAddModal, setShowAddModal] = useState(false);
    const [newWarehouse, setNewWarehouse] = useState({ name: '', location: '' });

    useEffect(() => {
        fetchWarehouses();
    }, []);

    const fetchWarehouses = async () => {
        try {
            const res = await api.get('/Warehouse');
            setWarehouses(res.data);
        } catch (err) {
            console.error("Hiba a raktárak betöltésekor", err);
        }
    };

    const fetchInventory = async (id) => {
        if (!id) return;
        try {
            // A Backend StockController [HttpGet("by-warehouse/{warehouseId}")] végpontja
            const res = await api.get(`/Stock/warehouse/${id}`);
            setInventory(res.data);
            setSelectedWarehouseId(id);
        } catch (err) {
            console.error("Hiba a leltár lekérésekor", err);
        }
    };

    const handleCreate = async () => {
        try {
            await api.post('/Warehouse', newWarehouse);
            setShowAddModal(false);
            setNewWarehouse({ name: '', location: '' });
            fetchWarehouses();
        } catch (err) {
            alert("Hiba a raktár létrehozásakor");
        }
    };

    const handleDelete = async (id) => {
        if (window.confirm("Biztosan törölni szeretnéd ezt a raktárat? Csak üres raktár törölhető!")) {
            try {
                await api.delete(`/Warehouse/${id}`);
                fetchWarehouses();
                if (selectedWarehouseId === id) {
                    setInventory([]);
                    setSelectedWarehouseId('');
                }
            } catch (err) {
                alert("Hiba: " + (err.response?.data?.message || "Sikertelen törlés. Ellenőrizd, hogy a raktár üres-e!"));
            }
        }
    };

    return (
        <Container className="mt-4">
            <Row>
                {/* RAKTÁRAK LISTÁJA */}
                <Col md={4}>
                    <Card className="shadow-sm">
                        <Card.Header className="d-flex justify-content-between align-items-center bg-dark text-white">
                            <h5 className="mb-0">Raktárak</h5>
                            <Button variant="success" size="sm" onClick={() => setShowAddModal(true)}>+</Button>
                        </Card.Header>
                        <Card.Body className="p-0">
                            <Table hover className="mb-0">
                                <tbody>
                                    {warehouses.map(w => (
                                        <tr 
                                            key={w.id} 
                                            style={{ cursor: 'pointer' }}
                                            className={selectedWarehouseId === w.id ? 'table-primary' : ''}
                                            onClick={() => fetchInventory(w.id)}
                                        >
                                            <td>
                                                <div className="fw-bold">{w.name}</div>
                                                <small className="text-muted">{w.location}</small>
                                            </td>
                                            <td className="text-end align-middle">
                                                <Button variant="link" className="text-danger p-0" onClick={(e) => {
                                                    e.stopPropagation();
                                                    handleDelete(w.id);
                                                }}>Törlés</Button>
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Card.Body>
                    </Card>
                </Col>

                {/* KIVÁLASZTOTT RAKTÁR LELTÁRA */}
                <Col md={8}>
                    <Card className="shadow-sm">
                        <Card.Header className="bg-light">
                            <h5 className="mb-0">
                                {selectedWarehouseId 
                                    ? `Leltár: ${warehouses.find(w => w.id === selectedWarehouseId)?.name}` 
                                    : 'Válassz ki egy raktárat'}
                            </h5>
                        </Card.Header>
                        <Card.Body>
                            {selectedWarehouseId ? (
                                <Table striped hover responsive>
                                    <thead>
                                        <tr>
                                            <th>Termék</th>
                                            <th>Cikkszám</th>
                                            <th className="text-end">Mennyiség</th>
                                            <th className="text-end">Teljes érték (eladási ár)</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {/* Csoportosítjuk és szűrjük az elemeket */}
                                        {Object.values(inventory.reduce((acc, item) => {
                                            // 1. Kiszűrjük a 0-ás vagy negatív készleteket (amikből már nincs)
                                            if (item.quantity <= 0) return acc;

                                            // 2. Termék azonosító alapján csoportosítunk
                                            const key = item.productId || item.sku; 
                                            
                                            if (!acc[key]) {
                                                // Ha még nincs ilyen termék a kosárban, létrehozzuk
                                                acc[key] = { ...item, totalQuantity: 0, totalValue: 0 };
                                            }
                                            
                                            // 3. Összeadjuk a mennyiségeket és a részértékeket
                                            acc[key].totalQuantity += item.quantity; 
                                            acc[key].totalValue += item.subTotalValue; 
                                            
                                            return acc;
                                        }, {}))
                                        // Most már a csoportosított adatokon megyünk végig
                                        .map((item, index) => (
                                            <tr key={index}>
                                                <td>{item.productName}</td>
                                                <td>{item.sku}</td>
                                                <td className="fw-bold">{item.totalQuantity} {item.unit || 'db'}</td>
                                                <td className="text-end">{item.totalValue?.toLocaleString()} Ft</td>
                                            </tr>
                                        ))}
                                        
                                        {/* Üres állapot lekezelése */}
                                        {inventory.filter(i => i.quantity > 0).length === 0 && (
                                            <tr>
                                                <td colSpan="4" className="text-center text-muted py-3">
                                                    A raktár jelenleg teljesen üres.
                                                </td>
                                            </tr>
                                        )}
                                    </tbody>
                                </Table>
                            ) : (
                                <div className="text-center py-5 text-muted">
                                    <i className="bi bi-box-seam fs-1 d-block mb-3"></i>
                                    Kattints a bal oldali listában egy raktárra a készlet megtekintéséhez.
                                </div>
                            )}
                        </Card.Body>
                    </Card>
                </Col>
            </Row>

            {/* MODAL ÚJ RAKTÁRHOZ */}
            <Modal show={showAddModal} onHide={() => setShowAddModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>Új raktár létrehozása</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>Raktár neve</Form.Label>
                            <Form.Control 
                                type="text" 
                                placeholder="Pl. Központi Raktár" 
                                onChange={e => setNewWarehouse({...newWarehouse, name: e.target.value})} 
                            />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Helyszín / Cím</Form.Label>
                            <Form.Control 
                                type="text" 
                                placeholder="Pl. Budapest, 11. kerület" 
                                onChange={e => setNewWarehouse({...newWarehouse, location: e.target.value})} 
                            />
                        </Form.Group>
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowAddModal(false)}>Mégse</Button>
                    <Button variant="primary" onClick={handleCreate}>Létrehozás</Button>
                </Modal.Footer>
            </Modal>
        </Container>
    );
};

export default WarehouseManager;