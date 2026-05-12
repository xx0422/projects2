import { useEffect, useState, useCallback } from 'react';
import api from '../api/axios';
import { Table, Container, Spinner, Alert, Button, Modal, Form, Row, Col, Dropdown, ButtonGroup } from 'react-bootstrap';
import StockActionModal from './StockActionModal';
import { useNavigate } from 'react-router-dom';

const ProductList = () => {
    const [products, setProducts] = useState([]);
    const [allProducts, setAllProducts] = useState([]); // Eredeti lista tárolása a visszaállításhoz
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);
    const [searchId, setSearchId] = useState('');
    const [showModal, setShowModal] = useState(false);
    const [isEdit, setIsEdit] = useState(false);
    const [currentProduct, setCurrentProduct] = useState({ name: '', sku: '', categoryId: 1, unit: 'db', purchasePrice: 0, isPerishable: false});
    const [showDetails, setShowDetails] = useState(false);
    const [detailsProduct, setDetailsProduct] = useState(null);
    const [showStockModal, setShowStockModal] = useState(false);
    const [selectedProduct, setSelectedProduct] = useState(null);
    const navigate = useNavigate();

    const handleShowDetails = async (id) => {
        const res = await api.get(`/Product/${id}`);
        setDetailsProduct(res.data);
        setShowDetails(true);
    };

    // A fetchProducts-ot kiemeljük, hogy a mentés után újra meg tudjuk hívni
    const fetchProducts = useCallback(async () => {
        try {
            const response = await api.get('/Product');
            setProducts(response.data);
            setAllProducts(response.data); // Elmentjük az eredeti listát is
            setLoading(false);
        } catch (err) {
            console.error("API hiba:", err);
            setError('Nem sikerült betölteni a termékeket.');
            setLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchProducts();
    }, [fetchProducts]);

    // Törlés (már megvolt)
    const handleDelete = async (id) => {
        if (window.confirm("Biztosan törölni szeretnéd ezt a terméket?")) {
            try {
                await api.delete(`/Product/${id}`);
                setProducts(products.filter(p => p.id !== id));
            } catch (err) {
                alert("Hiba történt a törlés során: " + (err.response?.data?.message || err.message));
            }
        }
    };

    const handleEditOpen = async (id) => {
        try {
            setLoading(true);
            const response = await api.get(`/Product/${id}`); // ID alapú lekérés a szerverről
            setCurrentProduct(response.data);
            setIsEdit(true);
            setShowModal(true);
            setLoading(false);
        } catch (err) {
            alert("Hiba a termék betöltésekor: " + (err.response?.data?.message || err.message));
            setLoading(false);
        }
    };

    const handleSearchById = async () => {
        if (!searchId) {
            setProducts(allProducts); // Ha üres, visszaállítjuk a teljes listát
            return;
        }
        
        try {
            setLoading(true);
            // A ProductController [HttpGet("{id}")] végpontját hívjuk
            const response = await api.get(`/Product/${searchId}`);
            
            if (response.data) {
                // Ha megtalálta, azonnal megnyitjuk szerkesztésre
               setProducts([response.data]);
            }
        } catch (err) {
            console.error("Keresési hiba:", err);
            alert("A megadott ID-vel nem található termék.");
            setProducts(allProducts);        
        } finally{
            setLoading(false);
        }
    };

    // Ha a felhasználó törli a keresőmező tartalmát, azonnal jöjjön vissza a teljes lista
    const handleInputChange = (e) => {
        const value = e.target.value;
        setSearchId(value);
        if (value === '') {
            setProducts(allProducts);
        }
    };

    // Új termék Modal megnyitása
    const handleAddOpen = () => {
        setIsEdit(false);
        setCurrentProduct({ name: '', sku: '', categoryId: 1, unit: 'db' });
        setShowModal(true);
    };

    // Mentés (Update / Create)
    const handleSave = async () => {
        try {
            // Tisztítsuk meg az adatot: a backendnek a kategória ID kell, nem a teljes objektum
            const productToSave = {
                id: currentProduct.id,
                name: currentProduct.name,
                sku: currentProduct.sku,
                categoryId: currentProduct.categoryId,
                unit: currentProduct.unit,
                description: currentProduct.description || "",
                purchasePrice: currentProduct.purchasePrice || 0,
                currentAveragePrice: currentProduct.currentAveragePrice || 0,

                safetyDocumentURL: currentProduct.safetyDocumentURL || "",
                foodSafetyCertificateId: currentProduct.foodSafetyCertificateId || ""
            };

            if (isEdit) {
                // PUT kérésnél az URL-ben és a testben is ott kell lennie az ID-nak
                await api.put(`/Product/${productToSave.id}`, productToSave);
            } else {
                await api.post('/Product', productToSave);
            }
            
            setShowModal(false);
            fetchProducts(); 
        } catch (err) {
            // Részletesebb hibaüzenet, hogy lássuk mi a baj
            console.error("Szerver válasz hiba:", err.response?.data);
            alert("Hiba a mentés során: " + (err.response?.data?.message || err.response?.data || "Ismeretlen hiba"));
        }
    };

    if (loading) return <Container className="mt-5 text-center"><Spinner animation="border" /></Container>;
    if (error) return <Container className="mt-5"><Alert variant="danger">{error}</Alert></Container>;

    return (
        <Container className="mt-4">
            <div className="d-flex justify-content-between align-items-center mb-3">
                <h2>Terméklista és Készlet</h2>

                <div className="d-flex gap-2 border p-2 rounded bg-light shadow-sm">
                        <Form.Control
                            type="number"
                            placeholder="Termék ID keresése..."
                            value={searchId}
                            onChange={(e) => setSearchId(e.target.value)}
                            style={{ width: '200px' }}
                            onKeyPress={(e) => e.key === 'Enter' && handleSearchById()}
                        />
                        <Button variant="primary" onClick={handleSearchById}>
                            Keresés
                        </Button>
                </div>

                <Button variant="success" onClick={handleAddOpen}>
                    + Új termék rögzítése
                </Button>
            </div>

            <Table striped bordered hover responsive className="shadow-sm mt-3">
                <thead className="table-dark">
                    <tr>
                        <th>ID</th>
                        <th>Név</th>
                        <th>Cikkszám</th>
                        <th>Kategória</th>
                        <th>Átlag beszerzési ár</th>
                        <th>Eladási ár</th>
                        <th>Összes készlet</th>
                        <th>Műveletek</th>
                    </tr>
                </thead>
                <tbody>
                    {products.map(product => (
                        <tr key={product.id}>
                            <td><strong>{product.id}</strong></td>
                            <td>{product.name}</td>
                            <td>{product.sku}</td>
                            <td>{product.category?.name || 'Nincs'}</td>
                            <td>{product.currentAveragePrice?.toLocaleString() ?? "0"} Ft</td>
                            <td>{product.purchasePrice?.toLocaleString() ?? "0"} Ft</td>
                            <td>{product.totalStock} {product.unit}</td>
                            <td>
                                <Dropdown as={ButtonGroup} size="sm">
                                    {/* Elsődleges gomb: A legfontosabb funkció */}
                                    <Button 
                                        variant="outline-secondary" 
                                        onClick={() => navigate(`/product/details/${product.id}`)}
                                    >
                                        Készletkezelés
                                    </Button>

                                    {/* Lenyíló rész minden máshoz */}
                                    <Dropdown.Toggle split variant="outline-secondary" id={`dropdown-split-${product.id}`} container="body" />

                                    <Dropdown.Menu style={{ zIndex: 1050 }}>
                                        <Dropdown.Header>Információ</Dropdown.Header>
                                        <Dropdown.Item onClick={() => handleShowDetails(product.id)}>
                                            <i className="bi bi-info-circle me-2"></i> Technikai részletek (Modal)
                                        </Dropdown.Item>

                                        <Dropdown.Divider />
                                        
                                        <Dropdown.Header>Módosítás</Dropdown.Header>
                                        <Dropdown.Item onClick={() => handleEditOpen(product.id)}>
                                            <i className="bi bi-pencil me-2"></i> Termék szerkesztése
                                        </Dropdown.Item>
                                        
                                        <Dropdown.Divider />
                                        
                                        <Dropdown.Item 
                                            className="text-danger" 
                                            onClick={() => handleDelete(product.id)}
                                        >
                                            <i className="bi bi-trash me-2"></i> Termék törlése
                                        </Dropdown.Item>
                                    </Dropdown.Menu>
                                </Dropdown>
                            </td>
                        </tr>
                    ))}
                </tbody>
            </Table>

            <Modal show={showModal} onHide={() => setShowModal(false)}>
                <Modal.Header closeButton>
                    <Modal.Title>{isEdit ? 'Termék szerkesztése' : 'Új termék rögzítése'}</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form>
                        <Form.Group className="mb-3">
                            <Form.Label>Termék név</Form.Label>
                            <Form.Control 
                                type="text" 
                                value={currentProduct.name}
                                onChange={(e) => setCurrentProduct({...currentProduct, name: e.target.value})}
                            />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Cikkszám (SKU)</Form.Label>
                            <Form.Control 
                                type="text" 
                                value={currentProduct.sku}
                                onChange={(e) => setCurrentProduct({...currentProduct, sku: e.target.value})}
                                disabled={isEdit} // Szerkesztéskor általában nem változtatunk SKU-t
                            />
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Vevői eladási ár (Ft)</Form.Label>
                            <Form.Control 
                                type="number" 
                                placeholder="0"
                                value={currentProduct.purchasePrice} 
                                onChange={e => setCurrentProduct({...currentProduct, purchasePrice: e.target.value})} 
                                required 
                            />
                            <Form.Text className="text-muted">Ezen az áron fogják megvenni az ügyfelek.</Form.Text>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Mértékegység</Form.Label>
                            <Form.Select 
                                value={currentProduct.unit}
                                onChange={(e) => setCurrentProduct({...currentProduct, unit: e.target.value})}
                            >
                                <option value="db">db</option>
                                <option value="kg">kg</option>
                                <option value="l">l</option>
                                <option value="karton">karton</option>
                            </Form.Select>
                        </Form.Group>
                        <Form.Group className="mb-3">
                            <Form.Label>Kategória ID (1: Standard, 2: Romlandó, 3: Veszélyes, 4: Élőlény)</Form.Label>
                            <Form.Control 
                                type="number" 
                                value={currentProduct.categoryId} 
                                onChange={(e) => setCurrentProduct({...currentProduct, categoryId: parseInt(e.target.value)})} 
                            />
                        </Form.Group>

                        {currentProduct.categoryId === 3 && (
                            <Form.Group className="mb-3 p-2 border border-danger">
                                <Form.Label>Biztonsági adatlap URL (Veszélyes áru)</Form.Label>
                                <Form.Control 
                                    type="text" 
                                    placeholder="http://..." 
                                    value={currentProduct.safetyDocumentURL || ''} 
                                    onChange={(e) => setCurrentProduct({...currentProduct, safetyDocumentURL: e.target.value})} 
                                />
                            </Form.Group>
                        )}

                        {currentProduct.categoryId === 4 && (
                            <Form.Group className="mb-3 p-2 border border-success">
                                <Form.Label>Élelmiszerbiztonsági igazolás ID (Élőlény)</Form.Label>
                                <Form.Control 
                                    type="text" 
                                    placeholder="Igazolás száma..."
                                    value={currentProduct.foodSafetyCertificateId || ''} 
                                    onChange={(e) => setCurrentProduct({...currentProduct, foodSafetyCertificateId: e.target.value})} 
                                />
                            </Form.Group>
                        )}
                    </Form>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={() => setShowModal(false)}>Mégse</Button>
                    <Button variant="primary" onClick={handleSave}>Mentés</Button>
                </Modal.Footer>
            </Modal>
            <Modal show={showDetails} onHide={() => setShowDetails(false)} size="lg">
                <Modal.Header closeButton className="bg-info text-white">
                    <Modal.Title>Termék technikai adatlap (ID: {detailsProduct?.id})</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    {detailsProduct && (
                        <Row>
                            <Col md={6}>
                                <p><strong>Név:</strong> {detailsProduct.name}</p>
                                <p><strong>Cikkszám (SKU):</strong> {detailsProduct.sku}</p>
                                <p><strong>Mértékegység:</strong> {detailsProduct.unit}</p>
                                <p><strong>Eladási ár:</strong> {detailsProduct.purchasePrice?.toLocaleString()} Ft</p>
                                <p><strong>Átlagár:</strong> {detailsProduct.currentAveragePrice?.toLocaleString()} Ft</p>
                            </Col>
                            <Col md={6}>
                                <p><strong>Kategória:</strong> {detailsProduct.category?.name}</p>
                                <p><strong>Típus:</strong> {detailsProduct.category?.type}</p>
                                <p><strong>Létrehozva:</strong> {new Date(detailsProduct.dateCreated).toLocaleString()}</p>
                                
                                {/* Speciális adatok megjelenítése ha léteznek */}
                                {detailsProduct.expirationDate && <p className="text-danger"><strong>Lejárat:</strong> {detailsProduct.expirationDate}</p>}
                                {detailsProduct.safetyDocumentURL && <p><strong>Biztonsági adatlap:</strong> <a href={detailsProduct.safetyDocumentURL} target="_blank">Megnyitás</a></p>}
                                {detailsProduct.foodSafetyCertificateId && <p><strong>Élelmiszerbizt. ID:</strong> {detailsProduct.foodSafetyCertificateId}</p>}
                            </Col>
                            <Col md={12} className="mt-3">
                                <h6>Raktárkészlet eloszlás:</h6>
                                <ul>
                                    {detailsProduct.stockItems
                                        ?.filter(item => item.quantity > 0) // 1. CSAK a nullánál nagyobb készleteket engedjük át
                                        .map(item => (
                                            <li key={item.id}>
                                                Raktár ID {item.warehouseId}: <strong>{item.quantity} {detailsProduct.unit}</strong>
                                                
                                                {/* 2. Ha van lejárati dátum, akkor azt is kiírjuk zárójelben */}
                                                {item.expirationDate && (
                                                    <span className="text-muted ms-2">
                                                        (Lejárat: {new Date(item.expirationDate).toLocaleDateString('hu-HU')})
                                                    </span>
                                                )}
                                            </li>
                                        ))}
                                </ul>
                            </Col>
                        </Row>
                    )}
                </Modal.Body>
            </Modal>
            <StockActionModal 
                show={showStockModal} 
                onHide={() => setShowStockModal(false)}
                productId={selectedProduct?.id}
                productName={selectedProduct?.name}
                onEntrySuccess={fetchProducts} // Frissíti a TotalStock-ot a listában
            />
        </Container>
    );
};

export default ProductList;