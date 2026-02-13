import { Outlet } from "react-router-dom";
import { Header } from "../shared/Header";
import { Footer } from "../shared/Footer";

export function AppLayout() {
  return (
    <div style={{ minHeight: "100dvh", display: "flex", flexDirection: "column" }}>
      <Header />

      <main style={{ flex: 1, padding: "16px 20px", maxWidth: 1200, width: "100%", margin: "0 auto" }}>
        <Outlet />
      </main>

      <Footer />
    </div>
  );
}