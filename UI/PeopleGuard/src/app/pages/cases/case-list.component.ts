import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { Subject, debounceTime, takeUntil } from 'rxjs';
import { 
  CasesService, 
  CaseListItem, 
  CaseFilterParams, 
  CASE_TYPES, 
  CASE_STATUSES 
} from '../../core/services/cases.service';
import { TranslatePipe } from '../../core/pipes/translate.pipe';

interface SortState {
  column: string;
  direction: 'asc' | 'desc';
}

@Component({
  selector: 'pg-case-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './case-list.component.html',
  styleUrls: ['./case-list.component.scss']
})
export class CaseListComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private casesService = inject(CasesService);
  private destroy$ = new Subject<void>();
  private filterChange$ = new Subject<void>();

  // Data
  cases = signal<CaseListItem[]>([]);
  factories = signal<string[]>([]);
  
  // Pagination
  currentPage = signal(1);
  pageSize = signal(20);
  totalItems = signal(0);
  totalPages = computed(() => Math.ceil(this.totalItems() / this.pageSize()));
  
  // Filters
  filters = signal<CaseFilterParams>({});
  
  // UI State
  loading = signal(false);
  showFilters = signal(true);
  sort = signal<SortState>({ column: 'createdAt', direction: 'desc' });

  // Constants
  readonly caseTypes = CASE_TYPES;
  readonly caseStatuses = CASE_STATUSES;
  readonly pageSizeOptions = [10, 20, 50, 100];

  // Filter form values (bound to inputs)
  filterEmployee = '';
  filterFactory = '';
  filterType: number | undefined;
  filterStatus: number | undefined;
  filterFromDate = '';
  filterToDate = '';

  ngOnInit() {
    this.loadFactories();
    this.initFromQueryParams();
    this.setupFilterDebounce();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private initFromQueryParams() {
    this.route.queryParams.pipe(takeUntil(this.destroy$)).subscribe(params => {
      // Restore filters from URL
      this.filterEmployee = params['employeeId'] || '';
      this.filterFactory = params['factory'] || '';
      this.filterType = params['type'] ? parseInt(params['type']) : undefined;
      this.filterStatus = params['status'] ? parseInt(params['status']) : undefined;
      this.filterFromDate = params['from'] || '';
      this.filterToDate = params['to'] || '';
      this.currentPage.set(params['page'] ? parseInt(params['page']) : 1);
      this.pageSize.set(params['size'] ? parseInt(params['size']) : 20);
      
      if (params['sortBy']) {
        this.sort.set({
          column: params['sortBy'],
          direction: params['sortDesc'] === 'true' ? 'desc' : 'asc'
        });
      }

      this.loadCases();
    });
  }

  private setupFilterDebounce() {
    this.filterChange$.pipe(
      debounceTime(300),
      takeUntil(this.destroy$)
    ).subscribe(() => {
      this.currentPage.set(1); // Reset to first page on filter change
      this.syncUrlAndLoad();
    });
  }

  loadFactories() {
    this.casesService.getFactories().subscribe({
      next: (factories) => this.factories.set(factories),
      error: () => console.error('Failed to load factories')
    });
  }

  loadCases() {
    this.loading.set(true);

    const params: CaseFilterParams = {
      employeeId: this.filterEmployee || undefined,
      factory: this.filterFactory || undefined,
      type: this.filterType,
      status: this.filterStatus,
      from: this.filterFromDate || undefined,
      to: this.filterToDate || undefined,
      page: this.currentPage(),
      size: this.pageSize(),
      sortBy: this.sort().column,
      sortDesc: this.sort().direction === 'desc'
    };

    this.casesService.getCases(params).subscribe({
      next: (response) => {
        this.cases.set(response.data);
        this.totalItems.set(response.total);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Failed to load cases', err);
        this.loading.set(false);
      }
    });
  }

  onFilterChange() {
    this.filterChange$.next();
  }

  syncUrlAndLoad() {
    const queryParams: any = {};
    
    if (this.filterEmployee) queryParams.employeeId = this.filterEmployee;
    if (this.filterFactory) queryParams.factory = this.filterFactory;
    if (this.filterType !== undefined) queryParams.type = this.filterType;
    if (this.filterStatus !== undefined) queryParams.status = this.filterStatus;
    if (this.filterFromDate) queryParams.from = this.filterFromDate;
    if (this.filterToDate) queryParams.to = this.filterToDate;
    if (this.currentPage() !== 1) queryParams.page = this.currentPage();
    if (this.pageSize() !== 20) queryParams.size = this.pageSize();
    if (this.sort().column !== 'createdAt') queryParams.sortBy = this.sort().column;
    if (this.sort().direction === 'asc') queryParams.sortDesc = 'false';
    else if (this.sort().column !== 'createdAt') queryParams.sortDesc = 'true';

    this.router.navigate([], {
      relativeTo: this.route,
      queryParams,
      queryParamsHandling: 'replace'
    });
  }

  clearFilters() {
    this.filterEmployee = '';
    this.filterFactory = '';
    this.filterType = undefined;
    this.filterStatus = undefined;
    this.filterFromDate = '';
    this.filterToDate = '';
    this.currentPage.set(1);
    this.syncUrlAndLoad();
  }

  toggleFilters() {
    this.showFilters.update(v => !v);
  }

  // Sorting
  onSort(column: string) {
    const current = this.sort();
    if (current.column === column) {
      // Toggle direction
      this.sort.set({
        column,
        direction: current.direction === 'asc' ? 'desc' : 'asc'
      });
    } else {
      this.sort.set({ column, direction: 'asc' });
    }
    this.syncUrlAndLoad();
  }

  getSortIcon(column: string): string {
    const current = this.sort();
    if (current.column !== column) return '↕️';
    return current.direction === 'asc' ? '↑' : '↓';
  }

  // Pagination
  goToPage(page: number) {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.syncUrlAndLoad();
  }

  onPageSizeChange(size: number) {
    this.pageSize.set(size);
    this.currentPage.set(1);
    this.syncUrlAndLoad();
  }

  getVisiblePages(): number[] {
    const total = this.totalPages();
    const current = this.currentPage();
    const pages: number[] = [];
    
    // Show max 5 pages
    let start = Math.max(1, current - 2);
    let end = Math.min(total, start + 4);
    
    if (end - start < 4) {
      start = Math.max(1, end - 4);
    }
    
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    
    return pages;
  }

  // Row actions
  onRowClick(caseItem: CaseListItem) {
    this.router.navigate(['/cases', caseItem.id]);
  }

  onCreateCase() {
    this.router.navigate(['/cases/new']);
  }

  // Helpers
  getStatusClass(status: number): string {
    return this.casesService.getStatusClass(status);
  }

  formatDate(date: string): string {
    if (!date) return '-';
    return new Date(date).toLocaleDateString('en-GB', {
      day: '2-digit',
      month: 'short',
      year: 'numeric'
    });
  }

  get hasActiveFilters(): boolean {
    return !!(
      this.filterEmployee || 
      this.filterFactory || 
      this.filterType !== undefined || 
      this.filterStatus !== undefined || 
      this.filterFromDate || 
      this.filterToDate
    );
  }
}
