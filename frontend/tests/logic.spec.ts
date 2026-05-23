import { test, expect } from '@playwright/test';

test('min and max price filters can be applied', async ({ page }) => {
  await page.goto('/customer/products');

  await page.getByRole('spinbutton').first().fill('10');
  await page.getByRole('spinbutton').nth(1).fill('100');

  await page.getByRole('button', { name: 'Apply' }).click();

  await expect(page.getByRole('heading', { name: /all products/i })).toBeVisible();
});

test('category filter can be changed', async ({ page }) => {
  await page.goto('/customer/products');

  const categoryDropdown = page.getByRole('combobox').first();

  await categoryDropdown.selectOption({ index: 1 });

  await expect(categoryDropdown).not.toHaveValue('');
});

test('sort order can be changed', async ({ page }) => {
  await page.goto('/customer/products');

  const sortDropdown = page.getByRole('combobox').nth(1);

  await sortDropdown.selectOption({ index: 1 });

  await expect(sortDropdown).not.toHaveValue('');
});

test('add to cart button can be clicked', async ({ page }) => {
  await page.goto('/customer/products');

  await page.getByRole('button', { name: /add to cart/i }).first().click();

  await expect(page.getByRole('button', { name: /add to cart/i }).first()).toBeVisible();
});

test('login dropdown contains customer and seller login links', async ({ page }) => {
  await page.goto('/');

  await page.getByRole('button', { name: /login/i }).click();

  await expect(page.getByRole('link', { name: /log in as customer/i })).toBeVisible();
  await expect(page.getByRole('link', { name: /log in as seller/i })).toBeVisible();
});

test('seller login page loads', async ({ page }) => {
  await page.goto('/seller/login');

  await expect(page.getByRole('heading', { name: /seller login/i })).toBeVisible();
});