import { useState, useEffect } from 'react';
import { Container, Row, Col, Card, Table, Form, Button, Tabs, Tab, Badge, ProgressBar } from 'react-bootstrap';
import api from '../api/axios';

const Reports = () => {
    const [abcData, setAbcData] = useState([]);
    const [salesData, setSalesData] = useState([]);
    const [dates, setDates] = useState({
        start: new Date(new Date().setDate(new Date().getDate() - 30)).toISOString().split('T')[0],
        end: new Date().toISOString().split('T')[0]
    });

    useEffect(() => {
        fetchAbc();
        fetchSales();
    }, []);

    const fetchAbc = async () => {
        try {
            const res = await api.get('/Report/abc-analysis');
            setAbcData(res.data);
        } catch (err) { console.error("ABC hiba:", err); }
    };

    const fetchSales = async () => {
        try {
            const res = await api.get(`/Report/Sales Report?startDate=${dates.start}&endDate=${dates.end}`);
            // Mivel a backend egy objektumot ad vissza, benne a listával:
            setSalesData(res.data.dailyBreakdown || res.data.DailyBreakdown || []);
        } catch (err) { 
            console.error("Sales hiba:", err); 
            setSalesData([]); // Hiba esetén legyen üres tömb
        }
    };

    const getCategoryBadge = (cat) => {
        if (cat === 'A') return <Badge bg="success">A - Kiemelt</Badge>;
        if (cat === 'B') return <Badge bg="warning text-dark">B - Átlagos</Badge>;
        return <Badge bg="secondary">C - Alacsony</Badge>;
    };

    return (
        <Container className="mt-4 pb-5">
            <h2 className="mb-4">Üzleti Riportok</h2>

            <Tabs defaultActiveKey="sales" className="mb-4 shadow-sm p-2 bg-white rounded">
                {/* ÉRTÉKESÍTÉSI JELENTÉS */}
                <Tab eventKey="sales" title="Értékesítési statisztika">
                    <Card className="border-0 shadow-sm">
                        <Card.Body>
                            <Row className="mb-4 align-items-end">
                                <Col md={4}>
                                    <Form.Label>Intervallum kezdete</Form.Label>
                                    <Form.Control type="date" value={dates.start} onChange={e => setDates({...dates, start: e.target.value})} />
                                </Col>
                                <Col md={4}>
                                    <Form.Label>Vége</Form.Label>
                                    <Form.Control type="date" value={dates.end} onChange={e => setDates({...dates, end: e.target.value})} />
                                </Col>
                                <Col md={4}>
                                    <Button variant="primary" className="w-100" onClick={fetchSales}>Frissítés</Button>
                                </Col>
                            </Row>

                            <Table responsive hover>
                                <thead className="table-light">
                                    <tr>
                                        <th>Dátum</th>
                                        <th className="text-center">Számlák száma</th>
                                        <th className="text-end">Napi forgalom</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {salesData.map((s, i) => (
                                        <tr key={i}>
                                            <td>{new Date(s.date || s.Date).toLocaleDateString()}</td>
                                            <td className="text-center">{s.invoiceCount || s.InvoiceCount}</td>
                                            <td className="text-end fw-bold">{(s.dailyRevenue || s.DailyRevenue).toLocaleString()} Ft</td>
                                        </tr>
                                    ))}
                                    {salesData.length === 0 && <tr><td colSpan="3" className="text-center py-4 text-muted">Nincs adat a megadott időszakra.</td></tr>}
                                </tbody>
                                <tfoot className="table-dark">
                                    <tr>
                                        <td>Összesen</td>
                                        <td className="text-center">{salesData.reduce((acc, curr) => acc + (curr.invoiceCount || curr.InvoiceCount), 0)}</td>
                                        <td className="text-end">
                                            {salesData.reduce((acc, curr) => acc + (curr.dailyRevenue || curr.DailyRevenue), 0).toLocaleString()} Ft
                                        </td>
                                    </tr>
                                </tfoot>
                            </Table>
                        </Card.Body>
                    </Card>
                </Tab>

                {/* ABC ANALÍZIS */}
                <Tab eventKey="abc" title="ABC Analízis (Készletoptimalizálás)">
                    <Card className="border-0 shadow-sm">
                        <Card.Body>
                            <p className="text-muted mb-4">
                                Az ABC analízis segít eldönteni, mely termékekre kell a legjobban figyelni. 
                                <strong> "A" kategória:</strong> A forgalom 80%-át adó legfontosabb termékek.
                            </p>
                            <Table responsive hover>
                                <thead className="table-light">
                                    <tr>
                                        <th>Termék</th>
                                        <th className="text-end">Összes Bevétel</th>
                                        <th className="text-center">Kumulált %</th>
                                        <th className="text-center">Kategória</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    {abcData.map((item, i) => (
                                        <tr key={i}>
                                            <td className="fw-bold">{item.productName || item.ProductName}</td>
                                            <td className="text-end">{(item.totalRevenue || item.TotalRevenue).toLocaleString()} Ft</td>
                                            <td className="text-center" style={{minWidth: '150px'}}>
                                                <small>{(item.cumulativePercentage || item.CumulativePercentage).toFixed(1)}%</small>
                                                <ProgressBar 
                                                    now={item.cumulativePercentage || item.CumulativePercentage} 
                                                    variant={item.category === 'A' || item.Category === 'A' ? "success" : "info"} 
                                                    style={{height: '5px'}}
                                                />
                                            </td>
                                            <td className="text-center">
                                                {getCategoryBadge(item.category || item.Category)}
                                            </td>
                                        </tr>
                                    ))}
                                </tbody>
                            </Table>
                        </Card.Body>
                    </Card>
                </Tab>
            </Tabs>
        </Container>
    );
};

export default Reports;