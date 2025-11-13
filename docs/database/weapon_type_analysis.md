# Weapon Type Analysis Report

**Generated:** 2025-11-10
**Database:** l1j_remastered
**Table:** weapon

## Executive Summary

Analysis of the `weapon` table reveals 16 distinct weapon types with a total of 528 weapons across all categories. The weapon type values stored in the database are **all uppercase strings** representing different weapon categories.

## Weapon Type Distribution

| Weapon Type | Count | Percentage |
|-------------|-------|------------|
| SWORD | 91 | 17.2% |
| BOW | 53 | 10.0% |
| STAFF | 51 | 9.7% |
| TOHAND_SWORD | 48 | 9.1% |
| DAGGER | 43 | 8.1% |
| BLUNT | 43 | 8.1% |
| EDORYU | 37 | 7.0% |
| SPEAR | 32 | 6.1% |
| CLAW | 32 | 6.1% |
| SINGLE_SPEAR | 20 | 3.8% |
| KEYRINGK | 20 | 3.8% |
| CHAINSWORD | 18 | 3.4% |
| TOHAND_BLUNT | 15 | 2.8% |
| SINGLE_BOW | 10 | 1.9% |
| TOHAND_STAFF | 8 | 1.5% |
| GAUNTLET | 7 | 1.3% |
| **TOTAL** | **528** | **100%** |

## Weapon Type Values (Alphabetically)

```sql
BLUNT
BOW
CHAINSWORD
CLAW
DAGGER
EDORYU
GAUNTLET
KEYRINGK
SINGLE_BOW
SINGLE_SPEAR
SPEAR
STAFF
SWORD
TOHAND_BLUNT
TOHAND_STAFF
TOHAND_SWORD
```

## Key Findings

1. **Format:** All weapon type values are **UPPERCASE strings** (not integers or enums)
2. **Naming Convention:** Uses underscore for multi-word types (e.g., `TOHAND_SWORD`, `SINGLE_BOW`)
3. **Most Common:** SWORD category has the most entries (91 weapons, 17.2%)
4. **Least Common:** GAUNTLET category has the fewest entries (7 weapons, 1.3%)
5. **Two-Handed Weapons:** Prefix pattern `TOHAND_` for two-handed variants
6. **Single-Hand Weapons:** Prefix pattern `SINGLE_` for single-handed variants

## Database Schema Notes

### Primary Columns
- `item_id` (INT, PRIMARY KEY, AUTO_INCREMENT) - Unique weapon identifier
- `type` (ENUM) - Weapon type category (16 values listed above)
- `item_name_id` (INT) - Reference ID for item name in translation system
- `desc_kr` (VARCHAR) - Korean description
- `desc_en` (VARCHAR) - English description

### Key Observations
- The `type` column is an ENUM type containing exactly these 16 values
- No NULL values found in the type column
- All weapons are properly categorized

## Frontend Implementation Recommendations

### Current Filter Values Need Update

The frontend weapon type filter should use these exact values:

```typescript
const WEAPON_TYPES = [
  { value: 'SWORD', label: 'Sword' },
  { value: 'DAGGER', label: 'Dagger' },
  { value: 'TOHAND_SWORD', label: 'Two-Hand Sword' },
  { value: 'BOW', label: 'Bow' },
  { value: 'SPEAR', label: 'Spear' },
  { value: 'BLUNT', label: 'Blunt' },
  { value: 'STAFF', label: 'Staff' },
  { value: 'GAUNTLET', label: 'Gauntlet' },
  { value: 'CLAW', label: 'Claw' },
  { value: 'EDORYU', label: 'Edoryu' },
  { value: 'SINGLE_BOW', label: 'Single Bow' },
  { value: 'SINGLE_SPEAR', label: 'Single Spear' },
  { value: 'TOHAND_BLUNT', label: 'Two-Hand Blunt' },
  { value: 'TOHAND_STAFF', label: 'Two-Hand Staff' },
  { value: 'KEYRINGK', label: 'Keyring (K)' },
  { value: 'CHAINSWORD', label: 'Chain Sword' },
] as const;
```

### SQL Query Examples

**Filter by weapon type:**
```sql
SELECT item_id, desc_en, type, dmg_small, dmg_large
FROM weapon
WHERE type = 'SWORD'
ORDER BY item_id;
```

**Count weapons by type:**
```sql
SELECT type, COUNT(*) as count
FROM weapon
GROUP BY type
ORDER BY count DESC;
```

**Get all two-handed weapons:**
```sql
SELECT item_id, desc_en, type
FROM weapon
WHERE type LIKE 'TOHAND_%'
ORDER BY type, item_id;
```

## Performance Considerations

1. **Indexing:** The `type` column should have an index for efficient filtering
2. **Query Optimization:** When filtering by type, use exact match (`=`) not LIKE
3. **Frontend Dropdown:** Sort by popularity (count) or alphabetically based on UX needs

## Related Documentation

- **Schema Documentation:** `docs/database/schema/weapon_table.md`
- **API Endpoint:** `/api/db/tables/weapon` supports `?type=SWORD` filter parameter
- **Frontend Component:** `app/admin/weapons/page.tsx` (weapon management UI)

## SQL Query Used

```sql
-- Get distinct weapon types
SELECT DISTINCT type FROM weapon ORDER BY type;

-- Count weapons by type
SELECT type, COUNT(*) as count
FROM weapon
GROUP BY type
ORDER BY type;
```

## Change Log

| Date | Author | Changes |
|------|--------|---------|
| 2025-11-10 | SQL Expert | Initial analysis and documentation |

---

**Note:** This analysis is based on the current database state. Always verify against the live database before making schema changes or assumptions in code.
