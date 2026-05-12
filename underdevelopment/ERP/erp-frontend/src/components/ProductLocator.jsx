import { useState } from 'react';
import { Form, Button, ListGroup, Card, Alert, InputGroup, Spinner } from 'react-bootstrap';
import api from '../api/axios';

const ProductLocator = ({ productId, productName }) => {
    const [quantity, setQuantity] = useState(1);
    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(false);
    const [searched, setSearched] = useState(false);

    const handleSearch = async () => {
        setLoading(true);
        setSearched(false);
        try {
            // Fontos: a query paramétert pontosan adjuk át
            const res = await api.get(`/Stock/where-is/${productId}`, {
                params: { quantity: quantity }
            });
            
            // Kezeljük, ha a backend üres listát vagy null-t ad
            setResults(res.data || []);
        } catch (err) {
            console.error("Keresési hiba:", err);
            setResults([]);
        } finally {
            setLoading(false);
            setSearched(true);
        }
    };

    return (
        <Card className="shadow border-0 bg-light p-4">
            <Card.Body>
                <h4 className="text-center mb-4 text-primary">Készlet lokátor</h4>
                <p className="text-center text-muted mb-4">
                    Ellenőrizze, melyik raktárban van elérhető mennyiség: <strong>{productName}</strong>
                </p>

                <div className="d-flex justify-content-center">
                    <InputGroup className="mb-3" style={{ maxWidth: '500px' }}>
                        <Form.Control 
                            type="number" 
                            value={quantity} 
                            onChange={(e) => setQuantity(e.target.value)}
                            min="1"
                        />
                        <Button variant="primary" onClick={handleSearch} disabled={loading}>
                            {loading ? <Spinner size="bg" /> : 'Hol van?'}
                        </Button>
                    </InputGroup>
                </div>

                {searched && results.length > 0 && (
                    <div className="mt-4">
                        <h6 className="mb-3 text-secondary text-center">Találatok ({results.length} raktár):</h6>
                        <ListGroup>
                            {results.map((loc, idx) => (
                                <ListGroup.Item key={idx} className="d-flex justify-content-between align-items-center shadow-sm mb-2 rounded border-0">
                                    <div>
                                        <span className="fw-bold">{loc.warehouseName || loc.WarehouseName}</span>
                                        <div className="small text-muted">Raktár ID: #{loc.warehouseId || loc.WarehouseId}</div>
                                    </div>
                                    <div className="text-end">
                                        <span className="badge bg-success p-2 fs-6">
                                            {loc.currentStock || loc.CurrentStock} {loc.unit || loc.Unit || 'db'}
                                        </span>
                                    </div>
                                </ListGroup.Item>
                            ))}
                        </ListGroup>
                    </div>
                )}

                {searched && results.length === 0 && (
                    <Alert variant="warning" className="mt-4 text-center">
                        Sajnos egyik raktárban sem található meg a kért mennyiség ({quantity} db).
                    </Alert>
                )}
            </Card.Body>
        </Card>
    );
};

export default ProductLocator;