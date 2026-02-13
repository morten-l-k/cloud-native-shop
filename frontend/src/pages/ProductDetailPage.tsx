import { useParams } from "react-router-dom";
import { mockProducts } from "../entities/product/mock";
import { useCart } from "../features/CartProvider";

export function ProductDetailPage() {
  const { id } = useParams();
  const { addItem } = useCart();

  const product = mockProducts.find((p) => p.id === id);

  if (!product) return <div>Product not found.</div>;

  return (
    <div>
      <h1>{product.name}</h1>
      <p>
        Description: {product.description}
      </p>
      <p>
        Price: {product.price.toFixed(2)}
      </p>
      <button onClick={() => addItem(product.id, 1)} style={{ padding: "10px 14px" }}>
        Add to cart
      </button>
    </div>
  );
}
