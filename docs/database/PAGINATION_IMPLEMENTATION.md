# Pagination Implementation Guide

This document describes the pagination component implementation for the L1R Database web application.

## Overview

The pagination system consists of a reusable `Pagination` component built with Ark UI primitives and Panda CSS styling, following the project's established design system and accessibility standards.

## Architecture

### Component Structure

```
components/ui/
├── Pagination.tsx          # Main pagination component
└── Pagination.md           # Component documentation
```

### Technology Stack

- **Ark UI 5.27.1**: Provides accessible pagination primitives
- **Panda CSS**: Styling with semantic design tokens
- **TypeScript**: Type-safe component props and interfaces
- **React 18**: Client-side component with hooks

## Component API

### Main Pagination Component

**File**: `D:\L1R Project\l1r-database\components\ui\Pagination.tsx`

```typescript
interface PaginationProps {
  currentPage: number      // Current active page (1-indexed)
  totalPages: number       // Total number of pages
  onPageChange: (page: number) => void  // Callback when page changes
  siblingCount?: number    // Page buttons on each side of current (default: 1)
  className?: string       // Additional CSS classes
}
```

**Features**:
- Previous/Next navigation buttons
- Page number buttons with smart display logic
- Ellipsis (...) for skipped page ranges
- Automatic disabled states at boundaries
- Full keyboard accessibility
- Hover, focus, and active states
- Auto-hide when totalPages <= 1

### Compact Pagination Component

**File**: `D:\L1R Project\l1r-database\components\ui\Pagination.tsx`

```typescript
interface CompactPaginationProps {
  currentPage: number
  totalPages: number
  onPageChange: (page: number) => void
  className?: string
}
```

**Features**:
- Minimal layout with Previous/Next only
- Page info display (e.g., "Page 1 of 10")
- Ideal for mobile/limited space scenarios

## Design System Integration

### Semantic Tokens Used

From `panda.config.ts`:

```typescript
// Accent colors
accent.default      // Primary accent (#F2782D)
accent.emphasized   // Hover state
accent.fg           // Text on accent backgrounds
accent.text         // Accent text color
accent.border       // Accent borders

// Background colors
bg.default          // Default backgrounds
bg.subtle           // Subtle backgrounds
bg.muted            // Muted backgrounds
bg.disabled         // Disabled state backgrounds

// Foreground/text colors
fg.default          // Default text
fg.muted            // Muted text
fg.disabled         // Disabled text

// Border colors
border.default      // Default borders
border.disabled     // Disabled borders
```

### Visual States

1. **Default State**:
   - Page buttons: `bg.default` with `border.default`
   - Text: `fg.default`

2. **Hover State**:
   - Transform: `translateY(-1px)`
   - Shadow: `0 4px 6px -1px rgba(0, 0, 0, 0.1)`
   - Background: `accent.emphasized`

3. **Active/Selected State**:
   - Background: `accent.default`
   - Text: `accent.fg`
   - Font weight: semibold

4. **Disabled State**:
   - Background: `bg.disabled`
   - Text: `fg.disabled`
   - Cursor: `not-allowed`
   - Opacity: 0.5

5. **Focus State**:
   - Outline: 2px solid `accent.border`
   - Outline offset: 2px

## Integration Example

### Weapons Page Implementation

**File**: `D:\L1R Project\l1r-database\app\weapons\page.tsx`

#### Before (Old Pagination)

```tsx
{/* Old pagination controls */}
<div className={flex({ justify: 'space-between', align: 'center' })}>
  <button onClick={handlePrevious} disabled={offset === 0}>
    Previous
  </button>
  <span>Page {currentPage} of {totalPages}</span>
  <button onClick={handleNext} disabled={!hasMore}>
    Next
  </button>
</div>
```

#### After (New Pagination Component)

```tsx
import { Pagination } from '@/components/ui/Pagination'

// Calculate pagination values
const currentPage = Math.floor(offset / limit) + 1
const totalPages = Math.ceil(total / limit)

// Handle page change
const handlePageChange = (page: number) => {
  setOffset((page - 1) * limit)
}

{/* New pagination controls */}
<div className={css({ mt: '6' })}>
  <Pagination
    currentPage={currentPage}
    totalPages={totalPages}
    onPageChange={handlePageChange}
    siblingCount={1}
  />
</div>
```

### State Management Pattern

```tsx
// 1. Maintain offset and limit in state
const [offset, setOffset] = useState(0)
const [limit, setLimit] = useState(25)
const [total, setTotal] = useState(0)

// 2. Calculate derived pagination values
const currentPage = Math.floor(offset / limit) + 1
const totalPages = Math.ceil(total / limit)

// 3. Convert page number back to offset
const handlePageChange = (page: number) => {
  setOffset((page - 1) * limit)
}

// 4. Use offset in API calls
const fetchData = async () => {
  const response = await fetch(`/api/data?limit=${limit}&offset=${offset}`)
  const data = await response.json()
  setTotal(data.total)
  // ...
}
```

## Page Display Logic

The component uses Ark UI's smart page display algorithm:

### With siblingCount={1} (default)

| Current Page | Display |
|--------------|---------|
| 1 of 10 | `[1] [2] ... [10]` |
| 2 of 10 | `[1] [2] [3] ... [10]` |
| 5 of 10 | `[1] ... [4] [5] [6] ... [10]` |
| 9 of 10 | `[1] ... [8] [9] [10]` |
| 10 of 10 | `[1] ... [9] [10]` |

### With siblingCount={2}

| Current Page | Display |
|--------------|---------|
| 1 of 20 | `[1] [2] [3] ... [20]` |
| 10 of 20 | `[1] ... [8] [9] [10] [11] [12] ... [20]` |
| 20 of 20 | `[1] ... [18] [19] [20]` |

## Accessibility Implementation

### Keyboard Navigation

- **Tab**: Navigate between interactive elements
- **Enter/Space**: Activate focused button/page
- **Arrow Left/Right**: Navigate between page numbers
- **Home**: Jump to first page
- **End**: Jump to last page

### ARIA Attributes

Ark UI automatically provides:
- `aria-label`: Descriptive labels for all buttons
- `aria-current="page"`: Marks current page
- `aria-disabled`: Marks disabled buttons
- `role="navigation"`: Semantic navigation role

### Screen Reader Support

- Announces current page number
- Announces total number of pages
- Announces when Previous/Next are disabled
- Provides context for page navigation

## Performance Optimizations

1. **GPU-Accelerated Animations**: Uses `transform` and `opacity` for smooth transitions
2. **Auto-Hide**: Returns `null` when not needed (totalPages <= 1)
3. **Memoization Ready**: Compatible with `useCallback` for expensive handlers
4. **Efficient Re-renders**: Only re-renders when props change

### Recommended Optimization

```tsx
const handlePageChange = useCallback((page: number) => {
  setOffset((page - 1) * limit)
}, [limit])
```

## Styling Customization

### Custom Styles

```tsx
<Pagination
  currentPage={1}
  totalPages={10}
  onPageChange={handlePageChange}
  className={css({
    gap: '4',
    '& button': {
      borderRadius: 'full',
      px: '6'
    }
  })}
/>
```

### Responsive Behavior

```tsx
// Desktop: Full pagination with page numbers
<div className={css({ display: { base: 'none', md: 'flex' } })}>
  <Pagination {...props} />
</div>

// Mobile: Compact pagination
<div className={css({ display: { base: 'flex', md: 'none' } })}>
  <CompactPagination {...props} />
</div>
```

## Testing Checklist

- [ ] Navigation works correctly (Previous/Next)
- [ ] Page numbers are clickable and functional
- [ ] Current page is visually distinct
- [ ] Disabled states work at boundaries
- [ ] Keyboard navigation works (Tab, Enter, Arrows, Home, End)
- [ ] Focus indicators are visible
- [ ] Hover effects work on all interactive elements
- [ ] Component hides when totalPages <= 1
- [ ] Screen reader announces page changes
- [ ] Works in all major browsers (Chrome, Firefox, Safari, Edge)
- [ ] Responsive on mobile devices
- [ ] Integrates correctly with API pagination

## Common Patterns

### Pattern 1: Basic Table Pagination

```tsx
function DataTable() {
  const [data, setData] = useState([])
  const [offset, setOffset] = useState(0)
  const [total, setTotal] = useState(0)
  const limit = 25

  const currentPage = Math.floor(offset / limit) + 1
  const totalPages = Math.ceil(total / limit)

  return (
    <>
      <Table data={data} />
      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        onPageChange={(page) => setOffset((page - 1) * limit)}
      />
    </>
  )
}
```

### Pattern 2: Pagination with Filters

```tsx
function FilterableTable() {
  const [offset, setOffset] = useState(0)
  const [filters, setFilters] = useState({})

  // Reset to page 1 when filters change
  useEffect(() => {
    setOffset(0)
  }, [filters])

  const handlePageChange = (page: number) => {
    setOffset((page - 1) * limit)
  }

  return (
    <>
      <Filters onChange={setFilters} />
      <Table data={data} />
      <Pagination
        currentPage={currentPage}
        totalPages={totalPages}
        onPageChange={handlePageChange}
      />
    </>
  )
}
```

### Pattern 3: URL-Based Pagination

```tsx
'use client'

import { useSearchParams, useRouter } from 'next/navigation'

function SearchResults() {
  const searchParams = useSearchParams()
  const router = useRouter()
  const currentPage = Number(searchParams.get('page')) || 1

  const handlePageChange = (page: number) => {
    const params = new URLSearchParams(searchParams)
    params.set('page', page.toString())
    router.push(`/search?${params.toString()}`)
  }

  return (
    <Pagination
      currentPage={currentPage}
      totalPages={totalPages}
      onPageChange={handlePageChange}
    />
  )
}
```

## File Locations

All files are in the `D:\L1R Project\l1r-database` directory:

- **Component**: `components/ui/Pagination.tsx`
- **Documentation**: `components/ui/Pagination.md`
- **Implementation**: `app/weapons/page.tsx` (lines 7, 168-175, 749-758)
- **Design Tokens**: `panda.config.ts` (lines 130-136 for accent tokens)
- **This Guide**: `docs/database/PAGINATION_IMPLEMENTATION.md`

## Dependencies

```json
{
  "@ark-ui/react": "^5.27.1",
  "react": "^18.3.1",
  "next": "^14.2.33"
}
```

## Browser Compatibility

- Chrome 90+
- Firefox 88+
- Safari 14+
- Edge 90+
- Mobile browsers (iOS Safari, Chrome Mobile)

## Future Enhancements

Potential improvements for future versions:

1. **Page Size Selector**: Integrate items-per-page dropdown
2. **Jump to Page**: Input field for direct page navigation
3. **Loading States**: Show spinner during page transitions
4. **URL Sync**: Built-in URL query parameter synchronization
5. **Animation Variants**: Configurable transition styles
6. **RTL Support**: Right-to-left language support
7. **Custom Icons**: Allow custom Previous/Next icons
8. **Compact Breakpoint**: Auto-switch to compact on mobile

## Related Documentation

- **Component API**: `components/ui/Pagination.md`
- **Design System**: `panda.config.ts`
- **Weapons Page**: `app/weapons/page.tsx`
- **Coding Standards**: `l1r-database/.cursorrules`
- **Database Setup**: `docs/database/README.md`

## Support

For issues or questions about the pagination implementation:

1. Check the component documentation: `components/ui/Pagination.md`
2. Review the weapons page integration example
3. Consult the Ark UI documentation: https://ark-ui.com/docs/components/pagination
4. Review Panda CSS patterns: https://panda-css.com/

## Version History

- **v1.0.0** (2025-11-11): Initial implementation
  - Full pagination component with Ark UI
  - Compact pagination variant
  - Integration with weapons page
  - Complete documentation
