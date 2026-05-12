import React, { useState, useEffect } from 'react';
import { Container, Table, Badge, Card, Form } from 'react-bootstrap';
import api from '../api/axios';

const AuditLogPage = () => {
    const [logs, setLogs] = useState([]);

    useEffect(() => {
        api.get('/Audit').then(res => setLogs(res.data));
    }, []);

    const getActionBadge = (action) => {
        if (action.includes("LOGIN")) return <Badge bg="primary">{action}</Badge>;
        if (action.includes("STOCK")) return <Badge bg="warning" text="dark">{action}</Badge>;
        if (action.includes("INVOICE")) return <Badge bg="success">{action}</Badge>;
        return <Badge bg="secondary">{action}</Badge>;
    };

    return (
        <Container className="mt-4">
            <Card className="shadow border-0">
                <Card.Header className="bg-dark text-white p-3">
                    <h4 className="mb-0">📜 Rendszer Tevékenységnapló</h4>
                </Card.Header>
                <Card.Body>
                    <Table hover responsive>
                        <thead className="table-light">
                            <tr>
                                <th>Időpont</th>
                                <th>Felhasználó</th>
                                <th>Művelet</th>
                                <th>Részletek</th>
                            </tr>
                        </thead>
                        <tbody>
                            {logs.map(log => (
                                <tr key={log.id}>
                                    <td>{new Date(log.timestamp).toLocaleString()}</td>
                                    <td className="fw-bold">{log.userEmail}</td>
                                    <td>{getActionBadge(log.action)}</td>
                                    <td>{log.details}</td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                </Card.Body>
            </Card>
        </Container>
    );
};

export default AuditLogPage;