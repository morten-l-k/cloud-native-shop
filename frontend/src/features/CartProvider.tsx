import React, { createContext, useContext, useMemo, useReducer } from "react";
import type { CartItem } from "../entities/cart/types";

type CartState = {
  items: CartItem[];
};

type CartActions = {
  addItem: (productId: string, qty?: number) => void;
  removeItem: (productId: string) => void;
  setQty: (productId: string, qty: number) => void;
  clear: () => void;
};

type CartContextValue = CartState & CartActions;

const CartContext = createContext<CartContextValue | null>(null);

type Action =
  | { type: "ADD"; productId: string; qty: number }
  | { type: "REMOVE"; productId: string }
  | { type: "SET_QTY"; productId: string; qty: number }
  | { type: "CLEAR" };

function cartReducer(state: CartState, action: Action): CartState {
  switch (action.type) {
    case "ADD": {
      const existing = state.items.find((i) => i.productId === action.productId);
      if (existing) {
        return {
          items: state.items.map((i) =>
            i.productId === action.productId ? { ...i, qty: i.qty + action.qty } : i
          ),
        };
      }
      return { items: [...state.items, { productId: action.productId, qty: action.qty }] };
    }
    case "REMOVE":
      return { items: state.items.filter((i) => i.productId !== action.productId) };
    case "SET_QTY": {
      const qty = Math.max(1, Math.floor(action.qty)); // keep >= 1
      return {
        items: state.items.map((i) => (i.productId === action.productId ? { ...i, qty } : i)),
      };
    }
    case "CLEAR":
      return { items: [] };
    default:
      return state;
  }
}

export function CartProvider({ children }: { children: React.ReactNode }) {
  const [state, dispatch] = useReducer(cartReducer, { items: [] });

  const value: CartContextValue = useMemo(
    () => ({
      items: state.items,
      addItem: (productId, qty = 1) => dispatch({ type: "ADD", productId, qty }),
      removeItem: (productId) => dispatch({ type: "REMOVE", productId }),
      setQty: (productId, qty) => dispatch({ type: "SET_QTY", productId, qty }),
      clear: () => dispatch({ type: "CLEAR" }),
    }),
    [state.items]
  );

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart() {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error("useCart must be used within CartProvider");
  return ctx;
}
