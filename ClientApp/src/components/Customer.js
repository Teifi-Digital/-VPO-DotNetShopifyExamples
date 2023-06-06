import './Customer.css';
import {useState} from "react";

export default function Customer({ customer, onDelete }) {
    const [note, setNote] = useState(customer.note ?? '');
    
    const handleSave = () => {
        const id = customer.id;
        fetch(`/api/customers/${id.substring(id.lastIndexOf('/') + 1)}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ ...customer, note }),
        })
            .then(res => res.json())
            .then(data => {
                if (data.id == null) {
                    alert('Failed to save customer');
                } else {
                    setNote(data.note);
                    alert('Customer saved');
                }
            });
    };
    
    const handleDelete = () => {
        const id = customer.id;
        fetch(`/api/customers/${id.substring(id.lastIndexOf('/') + 1)}`, { method: 'DELETE' })
            .then(res => res.json())
            .then(data => {
                if (data.deletedCustomerId == null) {
                    alert('Failed to delete customer');
                } else {
                    alert('Customer deleted');
                    onDelete();
                }
            });
    };
    
    return (
        <div className="customer">
            <h2 className="customer-title">{customer.displayName}</h2>
            <div className="customer-properties">
                <div className="customer-property">
                    <p className="customer-property-key">ID</p>
                    <p className="customer-property-value">{customer.id}</p>
                </div>
                <div className="customer-property">
                    <p className="customer-property-key">Email</p>
                    <p className="customer-property-value">{customer.email}</p>
                </div>
                <div className="customer-property">
                    <p className="customer-property-key">Note</p>
                    <textarea
                        className="customer-property-value customer-property-textarea"
                        value={note}
                        onChange={e => setNote(e.target.value)}
                    >
                        {note}
                    </textarea>
                </div>
            </div>
            <div className="customer-footer">
                <button onClick={handleDelete}>Delete</button>
                <button onClick={handleSave}>Save</button>
            </div>
        </div>
    );
}
