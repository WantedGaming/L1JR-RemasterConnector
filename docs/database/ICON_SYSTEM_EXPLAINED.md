# Lineage Icon System - Complete Explanation

**Date**: 2025-11-10
**Context**: l1r-database web application icon integration

---

## Overview

The Lineage item icon system uses a **numeric icon_id** that maps to image files stored in the client's `icon.pak` archive. Icons are **NOT** named by `item_id` - they use a separate icon identification system.

## How Icon System Works

### 1. Database Structure

Every item in the database has two key identifiers:

```sql
-- weapon table example
item_id   -- Unique item identifier (e.g., 3200015)
iconId    -- Icon file identifier (e.g., 245, 1823, 9456)
```

**Critical Understanding**: `item_id` ≠ `iconId`

- `item_id`: Unique identifier for the item in the game database
- `iconId`: Reference number for the icon image file
- Multiple items can share the same `iconId` (e.g., different enchantment levels)

### 2. Icon Storage in Client

Icons are stored in the Lineage client's `icon.pak` file:

```
LineageWarriorClient/
├── icon.idx       # Index file listing all icons
└── icon.pak       # Encrypted archive containing icon images
```

**Icon File Naming Convention**:
- Icons inside `icon.pak` are likely named sequentially or by icon_id
- Common naming patterns in Lineage clients:
  - `{iconId}.bmp` (e.g., `245.bmp`, `1823.bmp`)
  - `icon_{iconId}.png` (e.g., `icon_245.png`)
  - Or just numeric: `245`, `1823` without extension

### 3. Icon Extraction Process

To use icons in your web application, you must:

#### Step 1: Extract Icons from icon.pak

Use the L1R-PAK-Editor tool:

```bash
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"

# List all icons in icon.pak
dotnet run -- --test list "D:\L1R Project\LineageWarriorClient\icon.idx" "D:\L1R Project\LineageWarriorClient\icon.pak"

# Extract all icons
dotnet run -- --test extractall "D:\L1R Project\LineageWarriorClient\icon.idx" "D:\L1R Project\LineageWarriorClient\icon.pak" "extracted_icons"
```

#### Step 2: Convert Icons to Web Format

Icons extracted from the PAK file need processing:

1. **File Format**: May be `.bmp`, `.png`, or proprietary format
2. **Conversion**: Convert to PNG with transparency for web use
3. **Naming**: Rename to match `iconId` from database (e.g., `245.png`, `1823.png`)
4. **Size**: Lineage icons are typically 32×32px or 48×48px

#### Step 3: Organize Icons for Web Application

```
l1r-database/
└── public/
    └── images/
        └── icons/
            ├── 0.png          # Universal fallback icon
            ├── 245.png        # Icon for iconId 245
            ├── 1823.png       # Icon for iconId 1823
            ├── 9456.png       # Icon for iconId 9456
            └── ...
```

### 4. Correct Icon Loading in Web Application

**WRONG Approach** (what you were doing):
```typescript
// ❌ INCORRECT - Using item_id
const iconPath = `/images/icons/${itemId}.png`;  // item_id: 3200015
```

**CORRECT Approach**:
```typescript
// ✅ CORRECT - Using iconId from database
const iconPath = `/images/icons/${iconId}.png`;  // iconId: 245
```

## Implementation for l1r-database

### Database Query

Always fetch `iconId` (or `icon_id`) column:

```typescript
// In your API route (e.g., app/api/weapons/route.ts)
const [rows] = await pool.query(`
  SELECT
    item_id,
    iconId,           -- ← Essential for icon display
    item_name_id,
    desc_en,
    dmg_small,
    dmg_large
  FROM weapon
  WHERE 1=1
  ORDER BY item_id ASC
  LIMIT ? OFFSET ?
`, [limit, offset]);
```

### React Component

```typescript
// app/weapons/WeaponIcon.tsx
interface WeaponIconProps {
  iconId: number;    // ← Use iconId, not itemId
  alt: string;
}

export function WeaponIcon({ iconId, alt }: WeaponIconProps) {
  const iconPath = `/images/icons/${iconId}.png`;
  const fallbackPath = '/images/icons/0.png';  // Universal fallback

  return (
    <img
      src={iconPath}
      alt={alt}
      className="w-8 h-8 object-contain"
      onError={(e) => {
        // Fallback to default icon if specific icon not found
        (e.target as HTMLImageElement).src = fallbackPath;
      }}
    />
  );
}

// Usage in weapons page
<WeaponIcon iconId={weapon.iconId} alt={weapon.desc_en || 'Weapon'} />
```

## Icon Mapping Tables

While not standard in Lineage databases, you could create a mapping table for better organization:

```sql
-- Optional: Create icon metadata table
CREATE TABLE icon_metadata (
  icon_id INT PRIMARY KEY,
  file_name VARCHAR(255),
  original_name VARCHAR(255),
  width INT,
  height INT,
  format VARCHAR(10),
  extracted_date DATETIME
);
```

## Common Icon IDs in Lineage

Based on the codebase analysis, some known icon IDs:

- **Buff/Skill Icons**: 6679, 8532, 9943, 9944, 10031-10033, 11219-11221
- **Item Icons**: Typically range from 1 to ~15000
- **Letters**: 465 (unopened), 606, 616 (letter types)
- **Special Items**: 9659 (scheduled item)

## Extracting Icon ID from Database

To see actual icon IDs used in your database:

```sql
-- Get all unique icon IDs from weapons
SELECT DISTINCT iconId, COUNT(*) as count
FROM weapon
GROUP BY iconId
ORDER BY count DESC
LIMIT 50;

-- Find specific weapon's icon ID
SELECT item_id, desc_en, iconId
FROM weapon
WHERE item_id = 3200015;

-- Find all items sharing the same icon
SELECT item_id, desc_en, iconId
FROM weapon
WHERE iconId = 245;
```

## Icon Extraction Workflow

### Complete Step-by-Step Process

1. **Locate Client Files**:
   ```
   D:\L1R Project\LineageWarriorClient\icon.idx
   D:\L1R Project\LineageWarriorClient\icon.pak
   ```

2. **Extract All Icons**:
   ```bash
   cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"
   dotnet run -- --test extractall "../LineageWarriorClient/icon.idx" "../LineageWarriorClient/icon.pak" "extracted_icons"
   ```

3. **Process Icons**:
   - Check file format (likely BMP or PNG)
   - Convert to PNG if needed using ImageMagick or similar
   - Verify icon IDs match filenames

4. **Rename Icons to Match Database**:
   ```bash
   # If icons are extracted as icon_245.bmp, rename to 245.png
   # Script example (PowerShell):
   Get-ChildItem "extracted_icons" -Filter "icon_*.bmp" | ForEach-Object {
     $newName = $_.Name -replace 'icon_(\d+)\.bmp', '$1.png'
     # Convert BMP to PNG here if needed
     Rename-Item $_.FullName -NewName $newName
   }
   ```

5. **Copy to Web App**:
   ```bash
   # Copy processed icons to web application
   Copy-Item "extracted_icons/*.png" "D:\L1R Project\l1r-database\public\images\icons\"
   ```

6. **Create Fallback Icon**:
   - Create or designate `0.png` as universal fallback
   - Should be a generic "missing item" icon

## Testing Icon Display

### Quick Database Check

Query to verify icon IDs are valid:

```sql
-- Check if iconId is populated
SELECT
  item_id,
  desc_en,
  iconId,
  CASE
    WHEN iconId IS NULL THEN 'Missing Icon ID'
    WHEN iconId = 0 THEN 'Default Icon'
    ELSE 'Has Icon ID'
  END as icon_status
FROM weapon
LIMIT 20;
```

### Web Application Test

1. **Test with known icon**: Pick a weapon with confirmed icon in database
2. **Test fallback**: Use non-existent iconId (e.g., 99999)
3. **Test null handling**: Ensure code handles NULL iconId gracefully

## Troubleshooting

### Issue: Icons not displaying

**Possible causes:**
1. Icons not extracted from icon.pak yet
2. Icon files named incorrectly (using item_id instead of iconId)
3. IconId field not queried from database
4. File path incorrect in web app
5. Icons in wrong format (BMP instead of PNG)

**Solution checklist:**
- [ ] Extract icons from icon.pak using L1R-PAK-Editor
- [ ] Verify extracted icon filenames match iconId values in database
- [ ] Ensure iconId column is selected in database queries
- [ ] Check file paths in React components use iconId
- [ ] Convert BMP files to PNG if necessary
- [ ] Implement onError fallback handler

### Issue: Some icons missing

**Possible causes:**
1. Icon extraction incomplete
2. Some iconIds reference icons not in icon.pak
3. Database has incorrect iconId values

**Solution:**
- Extract all icons from icon.pak
- Create fallback icon (0.png) for missing icons
- Implement graceful degradation (show default icon on error)

## Related Files in L1J-Remastered

### Server Code References

- `l1j/server/server/templates/L1Item.java` - `getIconId()` method
- `l1j/server/common/bin/item/CommonItemInfo.java` - Icon ID storage
- `l1j/server/server/serverpackets/*` - Icon ID used in packets

### Database Tables with Icon IDs

- `weapon` - `iconId` column
- `armor` - `iconId` column
- `etcitem` - `iconId` column
- `skills_passive` - `on_icon_id` column
- `spellbook` - uses icon system via item references

## Best Practices

1. **Always use iconId from database**, never item_id
2. **Implement fallback icons** for missing/broken icon files
3. **Cache icon files** in browser using proper headers
4. **Lazy load icons** for large item lists (performance)
5. **Validate iconId** before constructing file paths
6. **Document icon extraction process** for future updates
7. **Version control icon set** separately from code (large binary files)

## Next Steps for l1r-database

1. Extract all icons from client's icon.pak file
2. Process and rename icons to match database iconId values
3. Place icons in `public/images/icons/` directory
4. Update all item display components to use `iconId` field
5. Implement fallback icon system with `0.png`
6. Test with various items (weapons, armor, etcitem)
7. Document icon extraction process for other developers

## References

- **PAK Format Documentation**: `docs/pak-editor/ARCHITECTURE.md`
- **PAK Editor Tool**: `L1R-PAK-Editor/L1RPakEditor/`
- **Icon Usage in Server**: `L1JR-Server/src/l1j/server/server/templates/L1Item.java`
- **Database Schema**: `L1JR-Server/src/l1j_remastered.sql`

---

**Key Takeaway**: In Lineage, `item_id` identifies the item, but `iconId` identifies the icon image. Always use `iconId` to load item icons, never `item_id`.
