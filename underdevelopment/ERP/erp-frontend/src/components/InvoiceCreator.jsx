import { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Form, Button, Table, Badge } from 'react-bootstrap';
import api from '../api/axios';

const InvoiceCreator = () => {
    // Vevő adatai
    const [customer, setCustomer] = useState({ name: '', address: '', email: '' });
    
    // Alapadatok a választáshoz
    const [products, setProducts] = useState([]);
    const [warehouses, setWarehouses] = useState([]);
    
    // Kosár és aktuális tétel
    const [cart, setCart] = useState([]);
    const [currentItem, setCurrentItem] = useState({ 
        productId: '', 
        warehouseId: '', 
        qty: 1, 
        taxRate: 27 
    });

    const [loading, setLoading] = useState(false);

    useEffect(() => {
        const loadInitialData = async () => {
            try {
                const [pRes, wRes] = await Promise.all([
                    api.get('/Product'),
                    api.get('/Warehouse')
                ]);
                setProducts(pRes.data);
                setWarehouses(wRes.data);
                
                // Alapértelmezett raktár beállítása, ha van
                if (wRes.data.length > 0) {
                    setCurrentItem(prev => ({ ...prev, warehouseId: wRes.data[0].id }));
                }
            } catch (err) {
                console.error("Hiba az adatok betöltésekor:", err);
            }
        };
        loadInitialData();
    }, []);

    const addToCart = () => {
        const prod = products.find(p => p.id === parseInt(currentItem.productId));
        const wh = warehouses.find(w => w.id === parseInt(currentItem.warehouseId));
        
        if (!prod) return alert("Kérlek, válassz ki egy terméket!");
        if (!wh) return alert("Kérlek, válassz ki egy raktárat!");
        if (currentItem.qty <= 0) return alert("A mennyiségnek nagyobbnak kell lennie 0-nál!");

        const newItem = {
            productId: prod.id,
            productName: prod.name,
            warehouseId: wh.id,
            warehouseName: wh.name,
            qty: parseFloat(currentItem.qty),
            price: prod.purchasePrice || 0, // Nálad a purchasePrice a Vevői eladási ár
            taxRate: parseFloat(currentItem.taxRate)
        };

        setCart([...cart, newItem]);
        // Termék választó visszaállítása, de a raktár maradhat
        setCurrentItem({ ...currentItem, productId: '', qty: 1 });
    };

    const removeFromCart = (index) => {
        setCart(cart.filter((_, i) => i !== index));
    };

    const calculateTotal = () => {
        return cart.reduce((sum, item) => {
            const net = item.qty * item.price;
            const tax = net * (item.taxRate / 100);
            return sum + net + tax;
        }, 0);
    };

    const submitInvoice = async () => {
        if (cart.length === 0) return alert("A kosár üres!");
        if (!customer.name.trim()) return alert("A vevő neve kötelező!");

        setLoading(true);
        try {
            // A Backend specifikus hívása:
            // warehouseId és customerName QueryString-ben, a tételek a Body-ban
            const requestData = {
            warehouseId: parseInt(cart[0].warehouseId), // Az első tétel raktárát használjuk
            customerName: customer.name,
            items: cart.map(item => ({
                productId: item.productId,
                quantity: item.qty,
                unitPrice: item.price,
                taxRate: item.taxRate
            }))
        };

            const response = await api.post('/Invoice', requestData);  
            try {
                // 1. PDF lekérése azonosított kéréssel
                const pdfResponse = await api.get(`/Invoice/${response.data.id}/pdf`, {
                    responseType: 'blob', // Jelezzük, hogy bináris adatot várunk
                });

                // 2. Blob objektum létrehozása
                const file = new Blob([pdfResponse.data], { type: 'application/pdf' });
                
                // 3. Ideiglenes URL generálása a böngésző memóriájában
                const fileURL = URL.createObjectURL(file);
      
            } catch (pdfError) {
                console.error("PDF hiba:", pdfError);
                alert("A számla létrejött, de a PDF-et nem sikerült letölteni.");
            }    

            alert("Számla sikeresen kiállítva és készlet levonva!");
                    
            // Form ürítése
            setCart([]);
            setCustomer({ name: '', address: '', email: '' });
            
        } catch (err) {
            console.error(err);
            alert("Számlázási hiba: " + (err.response?.data?.error || "Ismeretlen hiba történt. Ellenőrizd a készletet!"));
        } finally {
            setLoading(false);
        }
    };

    return (
        <Container className="mt-4 pb-5">
            <h2 className="mb-4">Új Számla Kiállítása</h2>
            
            <Row>
                {/* VEVŐ ÉS FEJLÉC ADATOK */}
                <Col lg={4}>
                    <Card className="shadow-sm mb-4">
                        <Card.Header className="bg-primary text-white">Vevő adatai</Card.Header>
                        <Card.Body>
                            <Form.Group className="mb-3">
                                <Form.Label>Vevő neve</Form.Label>
                                <Form.Control 
                                    type="text" 
                                    value={customer.name} 
                                    onChange={e => setCustomer({...customer, name: e.target.value})} 
                                    placeholder="Példa Kft."
                                />
                            </Form.Group>
                            <Form.Group className="mb-3">
                                <Form.Label>Cím</Form.Label>
                                <Form.Control 
                                    type="text" 
                                    value={customer.address} 
                                    onChange={e => setCustomer({...customer, address: e.target.value})} 
                                    placeholder="1234 Város, Utca 1."
                                />
                            </Form.Group>
                            <Form.Group className="mb-3">
                                <Form.Label>Email</Form.Label>
                                <Form.Control 
                                    type="email" 
                                    value={customer.email} 
                                    onChange={e => setCustomer({...customer, email: e.target.value})} 
                                    placeholder="vevo@email.com"
                                />
                            </Form.Group>
                        </Card.Body>
                    </Card>

                    <Card className="shadow-sm">
                        <Card.Header className="bg-dark text-white">Tétel hozzáadása</Card.Header>
                        <Card.Body>
                            <Form.Group className="mb-2">
                                <Form.Label>Termék</Form.Label>
                                <Form.Select 
                                    value={currentItem.productId} 
                                    onChange={e => setCurrentItem({...currentItem, productId: e.target.value})}
                                >
                                    <option value="">Válassz terméket...</option>
                                    {products.map(p => (
                                        <option key={p.id} value={p.id}>
                                            {p.name} (Nettó: {p.purchasePrice} Ft)
                                        </option>
                                    ))}
                                </Form.Select>
                            </Form.Group>

                            <Form.Group className="mb-2">
                                <Form.Label>Forrás raktár</Form.Label>
                                <Form.Select 
                                    value={currentItem.warehouseId} 
                                    onChange={e => setCurrentItem({...currentItem, warehouseId: e.target.value})}
                                >
                                    {warehouses.map(w => (
                                        <option key={w.id} value={w.id}>{w.name}</option>
                                    ))}
                                </Form.Select>
                            </Form.Group>

                            <Row>
                                <Col>
                                    <Form.Group className="mb-3">
                                        <Form.Label>Mennyiség</Form.Label>
                                        <Form.Control 
                                            type="number" 
                                            value={currentItem.qty} 
                                            onChange={e => setCurrentItem({...currentItem, qty: e.target.value})}
                                        />
                                    </Form.Group>
                                </Col>
                                <Col>
                                    <Form.Group className="mb-3">
                                        <Form.Label>ÁFA %</Form.Label>
                                        <Form.Select 
                                            value={currentItem.taxRate} 
                                            onChange={e => setCurrentItem({...currentItem, taxRate: e.target.value})}
                                        >
                                            <option value="27">27%</option>
                                            <option value="5">5%</option>
                                            <option value="0">0%</option>
                                        </Form.Select>
                                    </Form.Group>
                                </Col>
                            </Row>

                            <Button variant="outline-primary" className="w-100" onClick={addToCart}>
                                Tétel hozzáadása
                            </Button>
                        </Card.Body>
                    </Card>
                </Col>

                {/* KOSÁR ÉS ÖSSZESÍTŐ */}
                <Col lg={8}>
                    <Card className="shadow-sm h-100">
                        <Card.Header className="bg-white d-flex justify-content-between align-items-center">
                            <h5 className="mb-0">Számla tételei</h5>
                            <Badge bg="info">{cart.length} tétel</Badge>
                        </Card.Header>
                        <Card.Body>
                            <Table responsive hover>
                                <thead className="table-light">
                                    <tr>
                                        <th>Termék</th>
                                        <th>Raktár</th>
                                        <th className="text-end">Menny.</th>
                                        <th className="text-end">Nettó egységár</th>
                                        <th className="text-end">Összesen (Bruttó)</th>
                                        <th></th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {cart.length > 0 ? cart.map((item, idx) => {
                                        const lineTotal = (item.qty * item.price) * (1 + item.taxRate / 100);
                                        return (
                                            <tr key={idx}>
                                                <td>{item.productName}</td>
                                                <td><small>{item.warehouseName}</small></td>
                                                <td className="text-end">{item.qty}</td>
                                                <td className="text-end">{item.price.toLocaleString()} Ft</td>
                                                <td className="text-end fw-bold">{lineTotal.toLocaleString()} Ft</td>
                                                <td className="text-center">
                                                    <Button variant="link" className="text-danger p-0" onClick={() => removeFromCart(idx)}>
                                                        ✕
                                                    </Button>
                                                </td>
                                            </tr>
                                        );
                                    }) : (
                                        <tr>
                                            <td colSpan="6" className="text-center py-5 text-muted">
                                                Nincsenek tételek a számlán.
                                            </td>
                                        </tr>
                                    )}
                                </tbody>
                            </Table>
                        </Card.Body>
                        <Card.Footer className="bg-light p-4">
                            <Row className="align-items-center">
                                <Col>
                                    <h4 className="mb-0 text-end">Fizetendő Bruttó:</h4>
                                </Col>
                                <Col xs="auto">
                                    <h2 className="mb-0 text-primary">{calculateTotal().toLocaleString()} Ft</h2>
                                </Col>
                            </Row>
                            <Button 
                                variant="success" 
                                size="lg" 
                                className="w-100 mt-4 shadow" 
                                onClick={submitInvoice}
                                disabled={loading || cart.length === 0}
                            >
                                {loading ? 'Feldolgozás...' : 'SZÁMLA VÉGLEGESÍTÉSE ÉS PDF'}
                            </Button>
                        </Card.Footer>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
};

export default InvoiceCreator;