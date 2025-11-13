# Fixing Weapon Icons in l1r-database - Quick Guide

**Issue**: WeaponIcon component incorrectly uses `itemId` instead of `iconId`
**Date**: 2025-11-10

---

## The Problem

Your current implementation in `app/weapons/page.tsx`:

```typescript
// ❌ WRONG - This assumes icons are named by item_id
<img
  src={`/images/icons/${itemId}.png`}
  alt={weapon.desc_en}
  className="w-8 h-8 object-contain"
  onError={(e) => {
    (e.target as HTMLImageElement).src = '/images/icons/0.png';
  }}
/>
```

**Why this is wrong:**
- Lineage items have TWO separate identifiers:
  - `item_id`: Unique identifier for the item (e.g., 3200015)
  - `iconId`: Reference to the icon image file (e.g., 245)
- Icon files in `icon.pak` are named by `iconId`, NOT by `item_id`
- Example: Item ID 3200015 might use icon ID 245
- Looking for `/images/icons/3200015.png` when the file is actually `/images/icons/245.png`

## The Solution

### Step 1: Update Database Query

Your API route needs to fetch the `iconId` column:

**File**: `app/api/weapons/route.ts` (or wherever your weapons query is)

```typescript
// ✅ CORRECT - Include iconId in SELECT
const [rows] = await pool.query(`
  SELECT
    item_id,
    iconId,              -- ← ADD THIS FIELD
    item_name_id,
    desc_kr,
    desc_en,
    desc_id,
    itemGrade,
    type,
    material,
    weight,
    dmg_small,
    dmg_large,
    hitmodifier,
    dmgmodifier,
    -- ... other fields
  FROM weapon
  WHERE 1=1
  ${weaponTypeFilter}
  ${gradeFilter}
  ${searchFilter}
  ORDER BY item_id ASC
  LIMIT ? OFFSET ?
`, queryParams);
```

### Step 2: Update TypeScript Interface

**File**: `app/weapons/page.tsx` (or your types file)

```typescript
interface Weapon {
  item_id: number;
  iconId: number;        // ← ADD THIS FIELD
  item_name_id: number;
  desc_kr: string | null;
  desc_en: string | null;
  itemGrade: string;
  type: string;
  dmg_small: number;
  dmg_large: number;
  // ... other fields
}
```

### Step 3: Create Proper WeaponIcon Component

**File**: `app/weapons/WeaponIcon.tsx` (create new file)

```typescript
'use client';

interface WeaponIconProps {
  iconId: number;        // ← Use iconId, not itemId
  alt: string;
  size?: 'sm' | 'md' | 'lg';
}

export function WeaponIcon({ iconId, alt, size = 'sm' }: WeaponIconProps) {
  const sizeClasses = {
    sm: 'w-8 h-8',
    md: 'w-12 h-12',
    lg: 'w-16 h-16',
  };

  const iconPath = `/images/icons/${iconId}.png`;
  const fallbackPath = '/images/icons/0.png';

  return (
    <img
      src={iconPath}
      alt={alt}
      className={`${sizeClasses[size]} object-contain`}
      onError={(e) => {
        const target = e.target as HTMLImageElement;
        if (target.src !== fallbackPath) {
          target.src = fallbackPath;
        }
      }}
    />
  );
}
```

### Step 4: Update Usage in Weapons Page

**File**: `app/weapons/page.tsx`

```typescript
import { WeaponIcon } from './WeaponIcon';

// In your table rendering code:
<tbody>
  {weapons.map((weapon) => (
    <tr key={weapon.item_id}>
      <td>
        {/* ✅ CORRECT - Pass iconId, not item_id */}
        <WeaponIcon
          iconId={weapon.iconId}
          alt={weapon.desc_en || 'Weapon icon'}
          size="sm"
        />
      </td>
      <td>{weapon.item_id}</td>
      <td>{weapon.desc_en}</td>
      {/* ... other cells */}
    </tr>
  ))}
</tbody>
```

## Step 5: Extract Icons from Client

Before icons will display, you need to extract them from the Lineage client:

### Extract Icons from icon.pak

```bash
# Navigate to PAK editor
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"

# List all icons first (to verify)
dotnet run -- --test list "D:\L1R Project\LineageWarriorClient\icon.idx" "D:\L1R Project\LineageWarriorClient\icon.pak"

# Extract all icons to temporary directory
dotnet run -- --test extractall "D:\L1R Project\LineageWarriorClient\icon.idx" "D:\L1R Project\LineageWarriorClient\icon.pak" "extracted_icons"
```

### Process and Copy Icons

```powershell
# Navigate to extracted icons directory
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor\extracted_icons"

# Check what format the icons are (likely .bmp or .png)
Get-ChildItem | Select-Object -First 10

# If icons need conversion from BMP to PNG, use ImageMagick or similar
# For now, assume they're in a compatible format

# Copy icons to web application public directory
$destination = "D:\L1R Project\l1r-database\public\images\icons"
if (-not (Test-Path $destination)) {
    New-Item -ItemType Directory -Path $destination -Force
}

# Copy all icon files
Copy-Item "*.png" $destination -Force
# OR if they're BMP files:
# Copy-Item "*.bmp" $destination -Force
```

### Create Fallback Icon

```powershell
# Create a simple fallback icon at public/images/icons/0.png
# You can use a generic "missing item" icon or copy an existing icon
# For testing, copy any icon as the fallback:
cd "D:\L1R Project\l1r-database\public\images\icons"
Copy-Item "245.png" "0.png" -Force  # Use first available icon as fallback
```

## Verification Steps

### 1. Verify Database Has iconId

```sql
-- Connect to l1j_remastered database
-- Check first 10 weapons and their icon IDs
SELECT item_id, desc_en, iconId
FROM weapon
LIMIT 10;

-- Expected output:
-- item_id | desc_en           | iconId
-- --------|-------------------|--------
-- 8       | Dagger            | 245
-- 9       | Silver Dagger     | 246
-- ...
```

### 2. Verify Icons Extracted

```powershell
# Check that icons exist in web app
cd "D:\L1R Project\l1r-database\public\images\icons"
Get-ChildItem | Select-Object -First 10

# Expected output:
# 0.png
# 1.png
# 2.png
# ...
# 245.png
# 246.png
# ...
```

### 3. Test in Browser

1. Start the web application:
   ```bash
   cd "D:\L1R Project\l1r-database"
   npm run dev
   ```

2. Navigate to weapons page: `http://localhost:3000/weapons`

3. Check browser DevTools:
   - Look for 404 errors on icon images
   - Should load `/images/icons/245.png`, NOT `/images/icons/3200015.png`

## Common Issues and Fixes

### Issue 1: Icons still showing as 404

**Cause**: Icons not extracted from icon.pak yet

**Fix**:
```bash
# Extract icons from client (see Step 5 above)
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"
dotnet run -- --test extractall "../LineageWarriorClient/icon.idx" "../LineageWarriorClient/icon.pak" "extracted_icons"
```

### Issue 2: TypeScript error "Property 'iconId' does not exist"

**Cause**: Database query doesn't select iconId column

**Fix**: Add `iconId` to your SELECT statement in API route

### Issue 3: All icons showing fallback (0.png)

**Cause 1**: IconId column is NULL in database
```sql
-- Check for NULL icon IDs
SELECT item_id, desc_en, iconId
FROM weapon
WHERE iconId IS NULL
LIMIT 10;
```

**Cause 2**: Icon files not in correct directory
```powershell
# Verify icon files exist
Test-Path "D:\L1R Project\l1r-database\public\images\icons\245.png"
```

### Issue 4: Icons are the wrong format (BMP instead of PNG)

**Fix**: Convert icons using ImageMagick
```powershell
# Install ImageMagick first, then:
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor\extracted_icons"
Get-ChildItem "*.bmp" | ForEach-Object {
    $pngName = $_.Name -replace '\.bmp$', '.png'
    magick $_.Name $pngName
    Remove-Item $_.Name
}
```

## Complete Example

Here's a complete working example with all pieces together:

### API Route (`app/api/weapons/route.ts`)

```typescript
import { pool } from '@/lib/db';

export async function GET(request: Request) {
  const { searchParams } = new URL(request.url);
  const limit = parseInt(searchParams.get('limit') || '50');
  const offset = parseInt(searchParams.get('offset') || '0');

  const [rows] = await pool.query(`
    SELECT
      item_id,
      iconId,          -- ← Essential for icons
      desc_en,
      itemGrade,
      dmg_small,
      dmg_large
    FROM weapon
    ORDER BY item_id ASC
    LIMIT ? OFFSET ?
  `, [limit, offset]);

  return Response.json({ weapons: rows });
}
```

### Weapons Page (`app/weapons/page.tsx`)

```typescript
import { WeaponIcon } from './WeaponIcon';

interface Weapon {
  item_id: number;
  iconId: number;    // ← Required field
  desc_en: string | null;
  itemGrade: string;
  dmg_small: number;
  dmg_large: number;
}

export default async function WeaponsPage() {
  const res = await fetch('http://localhost:3000/api/weapons');
  const { weapons }: { weapons: Weapon[] } = await res.json();

  return (
    <table>
      <thead>
        <tr>
          <th>Icon</th>
          <th>ID</th>
          <th>Name</th>
          <th>Damage</th>
        </tr>
      </thead>
      <tbody>
        {weapons.map((weapon) => (
          <tr key={weapon.item_id}>
            <td>
              <WeaponIcon
                iconId={weapon.iconId}
                alt={weapon.desc_en || 'Weapon'}
              />
            </td>
            <td>{weapon.item_id}</td>
            <td>{weapon.desc_en}</td>
            <td>{weapon.dmg_small} / {weapon.dmg_large}</td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}
```

## Summary Checklist

Before icons will work, complete these steps:

- [ ] Extract icons from client's `icon.pak` file using L1R-PAK-Editor
- [ ] Copy extracted icon files to `public/images/icons/`
- [ ] Create fallback icon at `public/images/icons/0.png`
- [ ] Update database query to SELECT `iconId` column
- [ ] Update TypeScript interface to include `iconId: number`
- [ ] Update icon rendering to use `iconId` instead of `itemId`
- [ ] Test in browser and verify icons load correctly

---

**Remember**: The fundamental rule is **`item_id` identifies the item, `iconId` identifies the icon**. Always use `iconId` for loading icon images.
