import { Link } from "react-router-dom";
import { useCart } from "../features/CartProvider";
import { mockProducts } from "../entities/product/mock";

export function CartPage() {
  const { items, setQty, removeItem, clear } = useCart();

  const rows = items
    .map((item) => {
      const product = mockProducts.find((p) => p.id === item.productId);
      if (!product) return null;
      const lineTotal = product.price * item.qty;
      return { item, product, lineTotal };
    })
    .filter(Boolean) as Array<{
    item: { productId: string; qty: number };
    product: { id: string; name: string; price: number; description: string };
    lineTotal: number;
  }>;

  const subtotal = rows.reduce((sum, r) => sum + r.lineTotal, 0);

  return (
    <div>
      <h1 style={{ marginBottom: 16 }}>Cart</h1>

      {rows.length === 0 ? (
        <div>
          <p>Your cart is empty.</p>
          <Link to="/">Go to products</Link>
        </div>
      ) : (
        <>
          <div style={styles.table}>
            <div style={{ ...styles.row, ...styles.headerRow }}>
              <div>Product</div>
              <div style={{ textAlign: "right" }}>Price</div>
              <div style={{ textAlign: "center" }}>Qty</div>
              <div style={{ textAlign: "right" }}>Total</div>
              <div />
            </div>

            {rows.map(({ product, item, lineTotal }) => (
              <div key={product.id} style={styles.row}>
                <div>
                  <div style={{ fontWeight: 600 }}>{product.name}</div>
                  <div style={{ color: "#666", fontSize: 12 }}>
                    {product.description.slice(0, 80)}{product.description.length > 80 ? "…" : ""}
                  </div>
                </div>

                <div style={{ textAlign: "right" }}>{product.price.toFixed(2)}</div>

                <div style={{ display: "flex", justifyContent: "center" }}>
                  <input
                    type="number"
                    min={1}
                    value={item.qty}
                    onChange={(e) => setQty(product.id, Number(e.target.value))}
                    style={styles.qtyInput}
                  />
                </div>

                <div style={{ textAlign: "right", fontWeight: 600 }}>
                  {lineTotal.toFixed(2)}
                </div>

                <div style={{ textAlign: "right" }}>
                  <button onClick={() => removeItem(product.id)} style={styles.buttonDanger}>
                    Remove
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div style={styles.summary}>
            <div style={{ fontSize: 18, fontWeight: 700 }}>
              Subtotal: {subtotal.toFixed(2)}
            </div>

            <div style={{ display: "flex", gap: 10, marginTop: 10 }}>
              <button onClick={clear} style={styles.button}>
                Clear cart
              </button>
              <Link to="/checkout" style={styles.buttonLink}>
                Checkout →
              </Link>
            </div>
          </div>
        </>
      )}
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  table: {
    border: "1px solid #ddd",
    borderRadius: 12,
    overflow: "hidden",
    background: "white",
  },
  row: {
    display: "grid",
    gridTemplateColumns: "2fr 1fr 1fr 1fr auto",
    gap: 12,
    padding: 12,
    borderBottom: "1px solid #eee",
    alignItems: "center",
  },
  headerRow: {
    background: "#fafafa",
    fontWeight: 700,
  },
  qtyInput: {
    width: 70,
    padding: 6,
  },
  summary: {
    marginTop: 16,
    padding: 12,
    border: "1px solid #ddd",
    borderRadius: 12,
    background: "white",
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
    flexWrap: "wrap",
    gap: 12,
  },
  button: {
    padding: "8px 12px",
    borderRadius: 10,
    border: "1px solid #ccc",
    background: "white",
    cursor: "pointer",
  },
  buttonDanger: {
    padding: "8px 12px",
    borderRadius: 10,
    border: "1px solid #f0b4b4",
    background: "white",
    cursor: "pointer",
  },
  buttonLink: {
    padding: "8px 12px",
    borderRadius: 10,
    border: "1px solid #ccc",
    textDecoration: "none",
    color: "inherit",
    display: "inline-flex",
    alignItems: "center",
  },
};
