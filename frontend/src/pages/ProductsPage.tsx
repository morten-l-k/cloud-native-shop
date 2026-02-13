import { useMemo, useState } from "react";
import { ProductCard } from "../shared/ProductCard";
import { mockProducts } from "../entities/product/mock";

export function ProductsPage() {
  const [selectedCategory, setSelectedCategory] = useState<string>("ALL");

  // Collect unique categories once
  const categories = useMemo(() => {
    const set = new Set<string>();
    for (const p of mockProducts) set.add(p.category);
    return ["ALL", ...Array.from(set).sort()];
  }, []);

  // Filter products based on selection
  const filteredProducts = useMemo(() => {
    if (selectedCategory === "ALL") return mockProducts;
    return mockProducts.filter((p) => p.category === selectedCategory);
  }, [selectedCategory]);

  return (
    <div>
      {/* Filter bar */}
      <div style={styles.toolbar}>
        <label>
          Category{" "}
          <select
            value={selectedCategory}
            onChange={(e) => setSelectedCategory(e.target.value)}
          >
            {categories.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
        </label>
      </div>

      {/* Products grid */}
      <div style={styles.grid}>
        {filteredProducts.map((p) => (
          <ProductCard key={p.id} product={p} />
        ))}
      </div>
    </div>
  );
}

const styles: Record<string, React.CSSProperties> = {
  toolbar: {
    display: "flex",
    gap: 12,
    alignItems: "center",
    marginBottom: 16,
  },
  grid: {
    display: "grid",
    gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))",
    gap: 16,
  },
};