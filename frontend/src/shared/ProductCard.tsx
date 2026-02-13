import { Link } from "react-router-dom";
import type { Product } from "../entities/product/types";

type Props = {
  product: Product;
};

export function ProductCard({ product }: Props) {
  return (
     <Link to={`/products/${product.id}`} style={styles.link}>
        <div style={styles.card}>
        <div style={styles.imagePlaceholder}>
                <span>No Image</span>
        </div>

        <div style={styles.info}>
            <div style={styles.name}>{product.name}</div>
            <div style={styles.price}>
            {product.price.toFixed(2)}
            </div>
        </div>
        </div>
    </Link>
  );
}

const styles = {
  link: {
    textDecoration: "none",
    color: "inherit",
  },
  card: {
    border: "1px solid #ddd",
    borderRadius: 12,
    overflow: "hidden",
    background: "white",
    transition: "0.2s",
    cursor: "pointer",
  },
  imagePlaceholder: {
    height: 180,
    background: "#f5f5f5",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
  },
  info: {
    padding: 12,
  },
  name: {
    fontWeight: 600,
    marginBottom: 6,
  },
  price: {
    color: "#444",
  },
};
