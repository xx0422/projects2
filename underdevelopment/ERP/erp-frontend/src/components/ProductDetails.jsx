import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Container, Row, Col, Card, Table, Button, Form, Badge, Alert } from 'react-bootstrap';
import api from '../api/axios';
import ProductLocator from './ProductLocator';

const ProductDetails = () => {
    const { id } = useParams();
    const navigate = useNavigate();
    const [product, setProduct] = useState(null);
    const [locations, setLocations] = useState([]);
    const [warehouses, setWarehouses] = useState([]);
    const [loading, setLoading] = useState(true);

    // Készletkezelő form state
    const [stockForm, setStockForm] = useState({ mode: 'receipt', warehouseId: '', toWarehouseId: '', qty: 1, price: 0, expirationDate: '' });

    const fetchData = async () => {
        try {
            const cleanId = id.toString().replace(/[^0-9]/g, '');
            
            // Előbb a terméket kérjük le
            const pRes = await api.get(`/Product/${cleanId}`);
            setProduct(pRes.data);

            // A raktárakat és a helyszíneket külön, hogy ne blokkolják egymást
            try {
                const lRes = await api.get(`/Stock/where-is/${cleanId}?quantity=0`);
                setLocations(lRes.data);
            } catch (e) {
                console.log("Még nincs készlet ehhez a termékhez");
                setLocations([]); // Üres lista, ha még nincs készlet
            }

            const wRes = await api.get('/Warehouse');
            setWarehouses(wRes.data);

            // CSAK AKKOR állítsuk be, ha még nincs értéke és van raktár a listában
            if (wRes.data.length > 0) {
                setStockForm(prev => ({ 
                    ...prev, 
                    warehouseId: wRes.data[0].id // Az első létező raktár ID-ja
                }));
            }
            setLoading(false);
        } catch (err) {
            console.error("Kritikus hiba a termék betöltésekor", err);
            setError("A termék alapadatainak betöltése sikertelen.");
            setLoading(false);
        }
    };

    useEffect(() => { fetchData(); }, [id]);

   // Kényszerítsük a szám típusokat a küldés előtt!
    const handleStockAction = async () => {
        try {
            const pId = parseInt(id); // Biztosan szám legyen
            const wId = parseInt(stockForm.warehouseId);
            const qty = parseFloat(stockForm.qty);
            const price = parseFloat(stockForm.price);

            if (stockForm.mode === 'receipt') {
                let url = `/Stock/receipt?productId=${pId}&warehouseId=${wId}&quantity=${qty}&unitPrice=${price}`;
                if (stockForm.expirationDate) {
                    url += `&expirationDate=${stockForm.expirationDate}`;
                }
                await api.post(url);
            } else if (stockForm.mode === 'issue') {
                await api.post(`/Stock/issue?productId=${pId}&warehouseId=${wId}&quantity=${qty}`);
            } else if (stockForm.mode === 'transfer') {
                const toWId = parseInt(stockForm.toWarehouseId);
                await api.post(`/Stock/transfer?productId=${pId}&fromWarehouseId=${wId}&toWarehouseId=${toWId}&quantity=${qty}`);
            }

            alert("Sikeres művelet!");
            await fetchData(); // Frissítés
        } catch (err) {
            console.error("Hiba részletei:", err.response?.data);
            // Így látni fogod a Backend pontos hibaüzenetét is!
            alert("Hiba: " + (err.response?.data?.message || JSON.stringify(err.response?.data) || "Ismeretlen hiba"));
        }
    };

    if (loading) return <Container className="mt-5 text-center">Betöltés...</Container>;

    return (
        <Container className="mt-4">
            <Button variant="link" onClick={() => navigate(-1)} className="mb-3 ps-0">← Vissza a listához</Button>
            
            <Row>
                {/* BAL OLDAL: Termék adatok és Raktári helyek */}
                <Col lg={8}>
                    <Card className="mb-4 shadow-sm">
                        <Card.Body>
                            <div className="d-flex justify-content-between align-items-center mb-3">
                                <h2>{product.name}</h2>
                                <Badge bg="primary" className="p-2">{product.sku}</Badge>
                            </div>
                            <Row>
                                <Col md={6}>
                                    <p><strong>Kategória:</strong> {product.category?.name}</p>
                                    <p><strong>Mértékegység:</strong> {product.unit}</p>
                                    <p><strong>Eladási ár:</strong> {product.purchasePrice?.toLocaleString()} Ft</p>
                                </Col>
                                <Col md={6}>
                                    <p><strong>Átlagos beszerzési ár:</strong> {product.currentAveragePrice?.toLocaleString()} Ft</p>
                                    <p><strong>Legolcsóbb: </strong>{product.minPurchasePrice ? product.minPurchasePrice.toLocaleString() : 0} Ft</p>
                                    <p><strong>Legdrágább: </strong>{product.maxPurchasePrice ? product.maxPurchasePrice.toLocaleString() : 0} Ft</p>
                                    <p><strong>Összes készlet:</strong> <span>{locations.reduce((sum, loc) => sum + loc.currentStock, 0)} </span></p>
                                </Col>
                                <hr className="my-5" /> {/* Egy elválasztó vonal jót tesz a szemnek */}
      
                                <Row className="justify-content-center">
                                    <Col md={10} lg={8}>
                                    <ProductLocator productId={product.id} productName={product.name} />
                                    </Col>
                                </Row>
                            </Row>
                        </Card.Body>
                    </Card>

                    <Card className="shadow-sm">
                        <Card.Header className="bg-white"><h5>Raktárankénti készlet</h5></Card.Header>
                        <Card.Body>
                            <Table striped borderless hover>
                                <thead>
                                    <tr>
                                        <th>Raktár (és lejáratok)</th>
                                        <th className="text-end">Összes Készlet</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {/* 1. Lépés: Csoportosítjuk az adatokat raktárak szerint */}
                                    {Object.values(locations.reduce((acc, loc) => {
                                        if (!acc[loc.warehouseId]) {
                                            acc[loc.warehouseId] = { ...loc, totalStock: 0, batches: [] };
                                        }
                                        acc[loc.warehouseId].totalStock += loc.currentStock;
                                        acc[loc.warehouseId].batches.push(loc);
                                        return acc;
                                    }, {})).map(group => (
                                        /* 2. Lépés: Megjelenítés */
                                        <tr key={group.warehouseId}>
                                            <td>
                                                <strong>{group.warehouseName}</strong>
                                                
                                                {/* Ha romlandó (van lejárata), mutatjuk a legördülőt */}
                                                {group.batches.some(b => b.expirationDate) && (
                                                    <details className="mt-1 text-muted" style={{fontSize: '0.9em', cursor: 'pointer'}}>
                                                        <summary>Lejáratok részletezése</summary>
                                                        <ul className="mb-0 mt-1 ps-3">
                                                            {group.batches.map((batch, idx) => (
                                                                <li key={idx}>
                                                                    {new Date(batch.expirationDate).toLocaleDateString('hu-HU')}: <strong>{batch.currentStock} {batch.unit}</strong>
                                                                </li>
                                                            ))}
                                                        </ul>
                                                    </details>
                                                )}
                                            </td>
                                            <td className="text-end fw-bold align-middle">{group.totalStock} {group.unit}</td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Card.Body>
                    </Card>
                </Col>

                {/* JOBB OLDAL: Gyors műveletek (Készletkezelés) */}
                <Col lg={4}>
                    <Card className="shadow-sm border-primary">
                        <Card.Header className="bg-primary text-white text-center"><h5>Készletmozgatás</h5></Card.Header>
                        <Card.Body>
                            <Form.Group className="mb-3">
                                <Form.Label>Művelet típusa</Form.Label>
                                <Form.Select value={stockForm.mode} onChange={e => setStockForm({...stockForm, mode: e.target.value})}>
                                    <option value="receipt">Bevételezés (+)</option>
                                    <option value="issue">Kiadás (-)</option>
                                    <option value="transfer">Átvezetés (→)</option>
                                </Form.Select>
                            </Form.Group>

                            <Form.Group className="mb-3">
                                <Form.Label>{stockForm.mode === 'transfer' ? 'Forrás raktár' : 'Raktár'}</Form.Label>
                                <Form.Select value={stockForm.warehouseId} onChange={e => setStockForm({...stockForm, warehouseId: e.target.value})}>
                                    {warehouses.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
                                </Form.Select>
                            </Form.Group>

                            {stockForm.mode === 'transfer' && (
                                <Form.Group className="mb-3">
                                    <Form.Label>Cél raktár</Form.Label>
                                    <Form.Select value={stockForm.toWarehouseId} onChange={e => setStockForm({...stockForm, toWarehouseId: e.target.value})}>
                                        <option value="">Válassz...</option>
                                        {warehouses.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
                                    </Form.Select>
                                </Form.Group>
                            )}

                            <Form.Group className="mb-3">
                                <Form.Label>Mennyiség</Form.Label>
                                <Form.Control type="number" value={stockForm.qty} onChange={e => setStockForm({...stockForm, qty: e.target.value})} />
                            </Form.Group>

                            {stockForm.mode === 'receipt' && (
                                <>
                                    <Form.Group className="mb-3">
                                        <Form.Label>Beszerzési ár (Ft)</Form.Label>
                                        <Form.Control 
                                            type="number" 
                                            value={stockForm.price} 
                                            onChange={e => setStockForm({...stockForm, price: e.target.value})} 
                                        />
                                    </Form.Group>
                                    {product.isPerishable && (
                                        <Form.Group className="mb-3 border border-warning p-2 rounded">
                                            <Form.Label className="text-warning fw-bold">Lejárati dátum (Kötelező)</Form.Label>
                                            <Form.Control 
                                                type="date" 
                                                value={stockForm.expirationDate || ''} 
                                                onChange={e => setStockForm({...stockForm, expirationDate: e.target.value})} 
                                            />
                                        </Form.Group>
                                    )}
                                </>
                            )}
                            <Button variant="primary" className="w-100 mt-2" onClick={handleStockAction}>
                                Művelet végrehajtása
                            </Button>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
};

export default ProductDetails;