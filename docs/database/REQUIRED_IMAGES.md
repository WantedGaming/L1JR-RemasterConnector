# Required Image Files for l1r-database

This document lists all image files that need to be provided for the l1r-database web application to display correctly.

## 📋 Status: Updated 2025-11-10

## 🖼️ Hero Background Image

### Universal Background
- **Path**: `D:\L1R Project\l1r-database\public\images\background.jpg`
- **Purpose**: Background image used for all hero sections across the application
- **Used On**: Homepage, Weapons page, and all future category pages
- **Recommended Specs**:
  - Format: JPG/JPEG (or PNG)
  - Dimensions: 1920x600px or larger
  - File size: <500KB (optimize for web)
  - Theme: Dark fantasy, Lineage-themed artwork
  - Should work well with dark overlays (various opacity levels)
  - Design should be versatile enough to work on different page types

## 🎯 Category Icons (Dropdown Navigation)

All category icons are displayed in the navigation dropdown menu.

### Required Icons:
1. **Weapon Icon**
   - Path: `D:\L1R Project\l1r-database\public\images\categories\weapon.png`
   - Size: 24x24px (displayed size)
   - Format: PNG with transparency

2. **Armor Icon**
   - Path: `D:\L1R Project\l1r-database\public\images\categories\armor.png`
   - Size: 24x24px (displayed size)
   - Format: PNG with transparency

3. **Items Icon**
   - Path: `D:\L1R Project\l1r-database\public\images\categories\items.png`
   - Size: 24x24px (displayed size)
   - Format: PNG with transparency

4. **Monsters Icon**
   - Path: `D:\L1R Project\l1r-database\public\images\categories\monsters.png`
   - Size: 24x24px (displayed size)
   - Format: PNG with transparency

5. **Maps Icon**
   - Path: `D:\L1R Project\l1r-database\public\images\categories\maps.png`
   - Size: 24x24px (displayed size)
   - Format: PNG with transparency

6. **Skills Icon**
   - Path: `D:\L1R Project\l1r-database\public\images\categories\skills.png`
   - Size: 24x24px (displayed size)
   - Format: PNG with transparency

## ⚔️ Item Icons (Weapons, Armor, Items, etc.)

### Icon Directory
- **Base Path**: `D:\L1R Project\l1r-database\public\images\icons\`
- **Naming Convention**: `{iconId}.png` (e.g., `34.png`, `144.png`, `322.png`)
- **Source**: Must be extracted from `icon.pak` using L1R-PAK-Editor

### Extraction Process

```bash
# Navigate to PAK Editor
cd "D:\L1R Project\L1R-PAK-Editor\L1RPakEditor"

# Extract all icons from icon.pak
dotnet run -- --test extractall "D:\L1R Project\LineageWarriorClient\icon.idx" "D:\L1R Project\LineageWarriorClient\icon.pak" "extracted_icons"

# Copy extracted icons to web app
Copy-Item "extracted_icons\*.png" "D:\L1R Project\l1r-database\public\images\icons\" -Force
```

### Icon Specs
- **Format**: PNG with transparency
- **Recommended Size**: 48x48px or 64x64px
- **File Size**: Aim for <10KB per icon
- **Background**: Transparent
- **Color Depth**: PNG-24 with alpha channel

### Fallback System
The application has a 3-tier fallback system:
1. **Tier 1**: Tries to load `/images/icons/{iconId}.png`
2. **Tier 2**: Falls back to `/images/icons/0.png` (optional placeholder)
3. **Tier 3**: Shows first letter of item name in colored circle

### Optional Placeholder
- **Path**: `D:\L1R Project\l1r-database\public\images\icons\0.png`
- **Purpose**: Generic fallback for missing icons
- **Size**: 48x48px or larger
- **Design**: Question mark, empty chest, or similar
- **Theme**: Grayscale or muted colors

## 📁 Directory Structure

```
D:\L1R Project\l1r-database\public\images\
├── background.jpg               # Universal hero background (all pages)
├── categories\
│   ├── weapon.png               # Weapon category icon (24x24px)
│   ├── armor.png                # Armor category icon (24x24px)
│   ├── items.png                # Items category icon (24x24px)
│   ├── monsters.png             # Monsters category icon (24x24px)
│   ├── maps.png                 # Maps category icon (24x24px)
│   └── skills.png               # Skills category icon (24x24px)
└── icons\
    ├── 0.png                    # Optional: Generic placeholder
    ├── 34.png                   # Weapon icon (item_id=1 → iconId=34)
    ├── 144.png                  # Weapon icon (item_id=2 → iconId=144)
    ├── 322.png                  # Weapon icon (item_id=3 → iconId=322)
    └── ...                      # All other icons from icon.pak
```

## ✅ Quick Setup Checklist

### Hero Background
- [ ] Find or create universal background image (1920x600px)
- [ ] Save as `public/images/background.jpg`

### Category Icons
- [ ] Create or extract 6 category icons (24x24px each)
- [ ] Create directory: `public/images/categories/`
- [ ] Save all icons to categories directory

### Item Icons
- [ ] Ensure L1R-PAK-Editor is built and working
- [ ] Run extraction command to extract all icons from icon.pak
- [ ] Create directory: `public/images/icons/`
- [ ] Copy all extracted PNG files to icons directory
- [ ] (Optional) Create placeholder icon `0.png`

### Verification
- [ ] Start dev server: `npm run dev`
- [ ] Visit http://localhost:3000
- [ ] Check homepage hero background displays
- [ ] Check dropdown navigation shows all 6 category icons
- [ ] Visit http://localhost:3000/weapons
- [ ] Check weapons page hero background displays
- [ ] Check weapon icons display correctly in table

## 🎨 Design Recommendations

### Hero Background
- Use high-quality Lineage official artwork or similar fantasy art
- Choose a versatile image that works well across different page types
- Ensure image works well with dark overlays of varying opacity
- Optimize for web (compress without losing quality)
- One image reduces bandwidth and simplifies asset management

### Category Icons
- Use consistent style across all icons
- Consider extracting from Lineage client UI
- Ensure good contrast on both light and dark backgrounds
- Test at 24x24px size for clarity

### Item Icons
- Icons are extracted directly from Lineage client
- Maintain original dimensions and quality
- No modification needed after extraction
- System handles missing icons automatically

## 🔗 Related Documentation

- **Icon Placement Guide**: `D:\L1R Project\l1r-database\public\images\ICON_PLACEMENT_GUIDE.md`
- **PAK Editor Architecture**: `D:\L1R Project\docs\pak-editor\ARCHITECTURE.md`
- **Database README**: `D:\L1R Project\docs\database\README.md`

## 📝 Notes

- All paths use Windows format (backslashes)
- Next.js automatically serves files from `public/` directory
- Images are referenced in code without `/public` prefix
- Example: `/images/icons/34.png` maps to `public/images/icons/34.png`
- Optimize all images before adding to reduce load times
- Use tools like TinyPNG or ImageOptim for compression

---

**Last Updated**: 2025-11-10
**Application Version**: l1r-database v1.0.0
**Next.js Version**: 14.2.33
