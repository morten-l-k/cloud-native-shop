import { Link } from "react-router-dom";

export function Header() {
  return (
    <header style={{ borderBottom: "1px solid #ddd", padding: "12px 20px" }}>
      <div style={{ maxWidth: 1200, margin: "0 auto", display: "flex", gap: 12, alignItems: "center" }}>
        
        <Link to="/" style={{ fontWeight: 700, textDecoration: "none" }}>MyShop</Link>
        <div style={{ flex: 1 }}>
          <input placeholder="Search products..." style={{ width: "100%", padding: 8, boxSizing: "border-box"}} />
        </div>
        <Link to="/cart" style={{ textDecoration: "none" }}>Cart</Link>
        <Link to="/profile" style={{ textDecoration: "none" }}>Profile</Link>

      </div>
    </header>
  );
}

