import {useEffect, useRef, useState} from "react";
import Customer from "./Customer";
import './Customers.css';
import './dialog.css';
import {useDebounce} from "usehooks-ts";

export default function Customers() {
    const [customers, setCustomers] = useState([]);
    const [q, setQ] = useState('');
    const [refreshCount, setRefreshCount] = useState(0);
    
    const newCustomerDialogRef = useRef();
    const [newCustomerFirstName, setNewCustomerFirstName] = useState('');
    const [newCustomerLastName, setNewCustomerLastName] = useState('');
    const [newCustomerEmail, setNewCustomerEmail] = useState('');
    
    const debouncedQ = useDebounce(q, 500);
    useEffect(() => {
        fetch(`/api/customers?${new URLSearchParams({ q: debouncedQ })}`)
            .then(res => res.json())
            .then(setCustomers);
    }, [debouncedQ, refreshCount]);
    
    const customerMarkup = customers.map(customer => (
        <Customer key={customer.id} customer={customer} onDelete={() => setRefreshCount(refreshCount + 1)} />
    ));
    
    const handleAddCustomer = () => {
        newCustomerDialogRef.current?.showModal();
    };
    
    const handleNewCustomerClose = () => {
        newCustomerDialogRef.current?.close();
        setNewCustomerFirstName('');
        setNewCustomerLastName('');
        setNewCustomerEmail('');
    };
    
    const handleNewCustomerSubmit = (e) => {
        e.preventDefault();
        fetch('/api/customers', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ firstName: newCustomerFirstName, lastName: newCustomerLastName, email: newCustomerEmail }),
        })
            .then(res => res.json())
            .then(data => {
                if (data.id == null) {
                    alert('Failed to create customer');
                } else {
                    setRefreshCount(refreshCount + 1);
                    handleNewCustomerClose();
                }
            });
    };
    
    return (
        <div className="customers-view">
            <div className="customers-view-header">
                <h1>Customers</h1>
                <input
                    className="customers-view-search"
                    value={q}
                    onChange={e => setQ(e.target.value)}
                    placeholder="Search customers..."
                />
                <button onClick={handleAddCustomer}>Add Customer</button>
            </div>
            <div className="customers">
                {customerMarkup}
            </div>
            <dialog ref={newCustomerDialogRef}>
                <form method="dialog" onSubmit={handleNewCustomerSubmit}>
                    <h3 className="dialog-title">New Customer</h3>
                    <div className="dialog-fields">
                        <div className="dialog-field">
                            <label className="dialog-field-key">First Name</label>
                            <input
                                type="text"
                                className="dialog-field-value"
                                value={newCustomerFirstName}
                                onChange={e => setNewCustomerFirstName(e.target.value)}
                            />
                        </div>
                        <div className="dialog-field">
                            <label className="dialog-field-key">Last Name</label>
                            <input
                                type="text"
                                className="dialog-field-value"
                                value={newCustomerLastName}
                                onChange={e => setNewCustomerLastName(e.target.value)}
                            />
                        </div>
                        <div className="dialog-field">
                            <label className="dialog-field-key">Email</label>
                            <input
                                type="text"
                                className="dialog-field-value"
                                value={newCustomerEmail}
                                onChange={e => setNewCustomerEmail(e.target.value)}
                            />
                        </div>
                    </div>
                    <div className="dialog-footer">
                        <button type="reset" onClick={handleNewCustomerClose}>Cancel</button>
                        <button type="submit">Confirm</button>
                    </div>
                </form>
            </dialog>
        </div>
    );
}
