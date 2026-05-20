import { test, expect } from '@playwright/test';

test('homepage loads', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('heading', { name: /most popular products/i })).toBeVisible();
  await expect(page.getByRole('link', { name: 'Products', exact: true })).toBeVisible();
  await expect(page.getByRole('link', { name: /cart/i })).toBeVisible();
});

test('products page loads', async ({ page }) => {
  await page.goto('/customer/products');

  await expect(page.getByRole('heading', { name: /all products/i })).toBeVisible();
  await expect(page.getByPlaceholder(/search products/i).first()).toBeVisible();
});

test('customer login page shows validation', async ({ page }) => {
  await page.goto('/customer/dummy-login');

  await expect(page.getByRole('heading', { name: /customer login/i })).toBeVisible();

  await page.getByPlaceholder(/06b8999e/i).fill('');
  await page.getByPlaceholder(/password/i).fill('');

  await page.getByRole('main').getByRole('button', { name: 'Login' }).click();

  await expect(page.getByText(/please enter both customer id and password/i)).toBeVisible();
});

test('cart page loads', async ({ page }) => {
  await page.goto('/customer/cart');

  await expect(page.getByRole('heading', { name: /your cart/i })).toBeVisible();
});

test('navbar can navigate to products and cart', async ({ page }) => {
  await page.goto('/');

  await page.getByRole('link', { name: 'Products', exact: true }).click();
  await expect(page).toHaveURL(/\/customer\/products/);
  await expect(page.getByRole('heading', { name: /all products/i })).toBeVisible();

  await page.getByRole('link', { name: /cart/i }).click();
  await expect(page).toHaveURL(/\/customer\/cart/);
  await expect(page.getByRole('heading', { name: /your cart/i })).toBeVisible();
});

test('products page search field accepts text', async ({ page }) => {
  await page.goto('/customer/products');

  const searchBox = page.getByPlaceholder(/search products/i).first();

  await searchBox.fill('phone');
  await expect(searchBox).toHaveValue('phone');
});

test('cart is empty before adding products', async ({ page }) => {
  await page.goto('/customer/cart');

  await expect(page.getByText(/your cart is empty/i)).toBeVisible();
});

test('unknown route redirects to homepage', async ({ page }) => {
  await page.goto('/this-page-does-not-exist');

  await expect(page).toHaveURL(/\/customer/);
  await expect(page.getByRole('heading', { name: /most popular products/i })).toBeVisible();
});