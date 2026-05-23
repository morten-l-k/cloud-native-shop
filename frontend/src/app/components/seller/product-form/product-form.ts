import { Component, ChangeDetectorRef, Inject, OnInit, Optional } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { SellerService } from '../../../services/seller';
import { SellerProduct } from '../../../models/seller';

@Component({
  standalone: true,
  selector: 'app-product-form',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatProgressSpinnerModule,
  ],
  templateUrl: './product-form.html',
})
export class ProductFormDialog implements OnInit {
  form: FormGroup;
  saving = false;
  error = '';
  categories: { productCategoryName: string; productCategoryNameEnglish: string | null }[] = [];
  isEdit: boolean;

  constructor(
    private fb: FormBuilder,
    private cdr: ChangeDetectorRef,
    private dialogRef: MatDialogRef<ProductFormDialog>,
    private sellerService: SellerService,
    @Optional() @Inject(MAT_DIALOG_DATA) public product: SellerProduct | null
  ) {
    this.isEdit = !!product;
    this.form = this.fb.group({
      name:        [product?.productName ?? '',         Validators.required],
      category:    [product?.productCategoryName ?? '', Validators.required],
      description: [product?.productDescription ?? ''],
      price:       [product?.productPrice ?? null,      [Validators.required, Validators.min(0.01)]],
      stock:       [product?.productStock ?? 0,         [Validators.required, Validators.min(0)]],
    });
  }

  ngOnInit(): void {
    this.sellerService.getCategories().subscribe({
      next: (cats) => {
        this.categories = cats;
        this.cdr.detectChanges();
      },
      error: () => {}
    });
  }

  save(): void {
    if (this.form.invalid) return;
    this.saving = true;
    this.error = '';

    const request$ = this.isEdit
      ? this.sellerService.updateProduct(this.product!.productId, this.form.value)
      : this.sellerService.createProduct(this.form.value);

    request$.subscribe({
      next: (result) => this.dialogRef.close({ product: result, isEdit: this.isEdit }),
      error: () => {
        this.error = `Failed to ${this.isEdit ? 'update' : 'create'} product. Please try again.`;
        this.saving = false;
      }
    });
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
