import { useState, useEffect } from 'react';
import { Container, Row, Col, Card, ProgressBar, Table, Spinner } from 'react-bootstrap';
import api from '../api/axios';

const Dashboard = () => {
    const [stats, setStats] = useState({
        inventory: null,
        critical: [],
        profitability: []
    });
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchDashboardData = async () => {
            try {
                const [invRes, critRes, profRes] = await Promise.all([
                    api.get('/Report/Inventory Value'),
                    api.get('/Report/critical-stock'),
                    api.get('/Report/warehouse-profitability')
                ]);

                setStats({
                    inventory: invRes.data,
                    critical: critRes.data,
                    profitability: profRes.data
                });
            } catch (err) {
                console.error("Dashboard hiba:", err);
            } finally {
                setLoading(false);
            }
        };
        fetchDashboardData();
    }, []);

    if (loading) return <div className="text-center mt-5"><Spinner animation="border" /></div>;

    return (
        <Container className="mt-4 pb-5">
            <h2 className="mb-4">Vezérlőpult</h2>
            
            <Row className="mb-4">
                {/* Összesített készletérték kártya */}
                <Col md={4}>
                    <Card className="shadow-sm border-0 bg-primary text-white text-center p-3">
                        <Card.Body>
                            <h6 className="text-uppercase opacity-75">Teljes Készletérték</h6>
                            <h2 className="display-6 fw-bold">
                                {stats.inventory?.grandTotalValue?.toLocaleString()} Ft
                            </h2>
                            <small>Frissítve: {new Date(stats.inventory?.generatedAt).toLocaleTimeString()}</small>
                        </Card.Body>
                    </Card>
                </Col>

                {/* Profitabilitás kártya (Átlag árrés) */}
                <Col md={4}>
                    <Card className="shadow-sm border-0 bg-success text-white text-center p-3">
                        <Card.Body>
                            <h6 className="text-uppercase opacity-75">Átlagos Árrés</h6>
                            <h2 className="display-6 fw-bold">
                                {(stats.profitability.reduce((acc, curr) => acc + curr.marginPercentage, 0) / (stats.profitability.length || 1)).toFixed(2)} %
                            </h2>
                            <small>{stats.profitability.length} raktár alapján</small>
                        </Card.Body>
                    </Card>
                </Col>

                {/* Kritikus készlet figyelmeztetés */}
                <Col md={4}>
                    <Card className={`shadow-sm border-0 text-white text-center p-3 ${stats.critical.length > 0 ? 'bg-danger' : 'bg-secondary'}`}>
                        <Card.Body>
                            <h6 className="text-uppercase opacity-75">Kritikus Készlethiány</h6>
                            <h2 className="display-6 fw-bold">{stats.critical.length} db</h2>
                            <small>Azonnali rendelés szükséges!</small>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>

            <Row>
                {/* Raktárankénti statisztikák */}
                <Col lg={7}>
                    <Card className="shadow-sm border-0 mb-4">
                        <Card.Header className="bg-white fw-bold">Raktárak Jövedelmezősége</Card.Header>
                        <Card.Body>
                            {stats.profitability.map((w, idx) => (
                                <div key={idx} className="mb-3">
                                    <div className="d-flex justify-content-between mb-1">
                                        <span>{w.warehouseName}</span>
                                        <span className="fw-bold">{w.marginPercentage}% árrés</span>
                                    </div>
                                    <ProgressBar now={w.marginPercentage} variant="success" style={{height: '10px'}} />
                                    <div className="d-flex justify-content-between mt-1">
                                        <small className="text-muted">Profit: {w.profit.toLocaleString()} Ft</small>
                                        <small className="text-muted">Forgalom: {w.totalRevenue.toLocaleString()} Ft</small>
                                    </div>
                                </div>
                            ))}
                        </Card.Body>
                    </Card>
                </Col>

                {/* Kritikus lista táblázat */}
                <Col lg={5}>
                    <Card className="shadow-sm border-0 h-100">
                        <Card.Header className="bg-white fw-bold text-danger">Készlethiány riasztások</Card.Header>
                        <Card.Body className="p-0">
                            <Table responsive hover className="mb-0">
                                <thead className="table-light">
                                    <tr>
                                        <th>Termék</th>
                                        <th>Raktár</th>
                                        <th className="text-end">Készlet</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {/* 1. Csoportosítjuk a kritikus készleteket */}
                                    {Object.values(stats.critical.reduce((acc, item) => {
                                        const key = `${item.productName}-${item.warehouseName}`;
                                        
                                        if (!acc[key]) {
                                            // Létrehozunk egy új kulcsot: totalStock (ebbe fogjuk összeadni)
                                            acc[key] = { ...item, totalStock: 0 };
                                        }
                                        // Hozzáadjuk a jelenlegi adag mennyiségét a teljeshez
                                        acc[key].totalStock += item.currentStock; 
                                        
                                        return acc;
                                    }, {})).map((item, idx) => (
                                        <tr key={idx}>
                                            <td><small className="fw-bold">{item.productName}</small></td>
                                            <td><small>{item.warehouseName}</small></td>
                                            {/* Itt már a totalStock-ot írjuk ki! */}
                                            <td className="text-end text-danger fw-bold">{item.totalStock} db</td>
                                        </tr>
                                    ))}

                                    {/* Az üres állapot ellenőrzése maradhat pontosan úgy, ahogy volt */}
                                    {stats.critical.length === 0 && (
                                        <tr><td colSpan="3" className="text-center py-4">Minden készlet rendben.</td></tr>
                                    )}
                                </tbody>
                            </Table>
                        </Card.Body>
                    </Card>
                </Col>
            </Row>
        </Container>
    );
};

export default Dashboard;