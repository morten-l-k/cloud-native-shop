import { test, expect } from '@playwright/test';

test('homepage has popular products section', async ({ page }) => {
  await page.goto('/');

  await expect(page.getByRole('heading', { name: /most popular products/i })).toBeVisible();
});

test('view all products button opens products page', async ({ page }) => {
  await page.goto('/');

  await page.getByRole('link', { name: /view all products/i }).click();

  await expect(page).toHaveURL(/\/customer\/products/);
  await expect(page.getByRole('heading', { name: /all products/i })).toBeVisible();
});

test('products page has search and category controls', async ({ page }) => {
  await page.goto('/customer/products');

  await expect(page.getByPlaceholder(/search products/i).first()).toBeVisible();
  await expect(page.locator('select').first()).toBeVisible();
});

test('search field can be typed into and cleared', async ({ page }) => {
  await page.goto('/customer/products');

  const searchBox = page.getByPlaceholder(/search products/i).first();

  await searchBox.fill('test');
  await expect(searchBox).toHaveValue('test');

  await searchBox.clear();
  await expect(searchBox).toHaveValue('');
});

test('cart page shows cart heading', async ({ page }) => {
  await page.goto('/customer/cart');

  await expect(page.getByRole('heading', { name: /your cart/i })).toBeVisible();
});

test('customer login page contains login form', async ({ page }) => {
  await page.goto('/customer/dummy-login');

  await expect(page.getByRole('main').getByRole('button', { name: 'Login' })).toBeVisible();
  await expect(page.getByRole('textbox', { name: 'Customer ID' })).toBeVisible();
  await expect(page.getByRole('textbox', { name: 'Password' })).toBeVisible();
});

test('customer login accepts typed credentials', async ({ page }) => {
  await page.goto('/customer/dummy-login');

  const customerIdInput = page.getByRole('textbox', { name: 'Customer ID' });
  const passwordInput = page.getByRole('textbox', { name: 'Password' });

  await customerIdInput.fill('test-customer');
  await passwordInput.fill('password');

  await expect(customerIdInput).toHaveValue('test-customer');
  await expect(passwordInput).toHaveValue('password');
});

test('navbar home link returns to homepage', async ({ page }) => {
  await page.goto('/customer/products');

  await page.getByRole('link', { name: 'Home', exact: true }).click();

  await expect(page).toHaveURL(/\/customer/);
  await expect(page.getByRole('heading', { name: /most popular products/i })).toBeVisible();
});