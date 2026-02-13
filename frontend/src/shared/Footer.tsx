export function Footer() {
  return (
    <footer style={{ borderTop: "1px solid #ddd", padding: "16px 20px" }}>
      <div style={{ maxWidth: 1200, margin: "0 auto", fontSize: 14 }}>
        © {new Date().getFullYear()} MyShop
      </div>
    </footer>
  );
}