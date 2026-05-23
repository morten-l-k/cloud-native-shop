import { test, expect } from '@playwright/test';

test('products page shows product cards', async ({ page }) => {
  await page.goto('/customer/products');

  await expect(page.getByRole('button', { name: 'View' }).first()).toBeVisible();
  await expect(page.getByRole('button', { name: /add to cart/i }).first()).toBeVisible();
});

test('products page has pagination controls', async ({ page }) => {
  await page.goto('/customer/products');

  await expect(page.getByRole('button', { name: /prev/i })).toBeVisible();
  await expect(page.getByRole('button', { name: /next/i })).toBeVisible();
  await expect(page.getByText(/page 1 of/i)).toBeVisible();
});

test('previous page button is disabled on first page', async ({ page }) => {
  await page.goto('/customer/products');

  await expect(page.getByRole('button', { name: /prev/i })).toBeDisabled();
});

test('next page button can be clicked', async ({ page }) => {
  await page.goto('/customer/products');

  await page.getByRole('button', { name: /next/i }).click();

  await expect(page.getByText(/page 2 of/i)).toBeVisible();
});

test('category dropdown contains all categories option', async ({ page }) => {
  await page.goto('/customer/products');

  const categoryDropdown = page.getByRole('combobox').first();

  await expect(categoryDropdown).toBeVisible();
  await expect(categoryDropdown).toContainText(/all categories/i);
});

test('sort dropdown contains price sorting options', async ({ page }) => {
  await page.goto('/customer/products');

  const sortDropdown = page.getByRole('combobox').nth(1);

  await expect(sortDropdown).toBeVisible();
  await expect(sortDropdown).toContainText(/price: low to high/i);
  await expect(sortDropdown).toContainText(/price: high to low/i);
});

test('price filter fields accept numbers', async ({ page }) => {
  await page.goto('/customer/products');

  const minPrice = page.getByRole('spinbutton').first();
  const maxPrice = page.getByRole('spinbutton').nth(1);

  await minPrice.fill('10');
  await maxPrice.fill('100');

  await expect(minPrice).toHaveValue('10');
  await expect(maxPrice).toHaveValue('100');
});

test('filter buttons are visible', async ({ page }) => {
  await page.goto('/customer/products');

  await expect(page.getByRole('button', { name: 'Apply' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Clear' })).toBeVisible();
});


test('product view button opens a product detail page', async ({ page }) => {
  await page.goto('/customer/products');

  await page.getByRole('button', { name: 'View' }).first().click();

  await expect(page).toHaveURL(/\/customer\/products\//);
});