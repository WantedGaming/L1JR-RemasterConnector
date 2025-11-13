# L1R Database Website - Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [Unreleased]

### Planned
- Custom game account authentication
- Admin dashboard
- Character management CRUD
- Item management CRUD
- Activity logging system
- Armor Database Phase 3: Set bonuses, enchant system, comparison tool
- NPCs and Maps database expansion
- Monster drop table integration
- Monster spawn location system

---

## [1.3.2] - 2025-11-09

### Added
- **Maps Database - Complete Implementation**
  - **Search and Filtering System:**
    - Search by map name (locationname or desc_kr) or map ID
    - Map Type filter with 7 categories:
      - Dungeons (dungeon = 1)
      - Safe Zones (penalty = 0, not dungeon)
      - PvP Zones (penalty = 1)
      - Underwater (underwater = 1)
      - Beginner Zones (beginZone = 1)
      - Red Knight Zones (redKnightZone = 1)
      - War Zones (interWarZone = 1)

  - **Enhanced Display:**
    - Compact 5-column table: Map ID, Location Name, Coordinates, Dimensions, Properties
    - Coordinate range display: (startX, startY) to (endX, endY)
    - Calculated dimensions badge (width × height)
    - Comprehensive property badge system with color coding:
      - Dungeon (purple) - dungeon maps
      - Underwater (blue) - underwater areas
      - PvP (red) - penalty zones
      - Safe (green) - non-penalty zones
      - Beginner (cyan) - beginner zones
      - Red Knight (dark red) - red knight training zones
      - War Zone (orange) - inter-war zones
      - No Teleport (gray) - teleport disabled
      - No Escape (gray) - escape disabled

  - **Smart Location Display:**
    - Fallback hierarchy: locationname → desc_kr → "Map {id}"
    - Handles maps with missing English names

### Changed
- **Maps Controller Improvements:**
  - Added pagination (50 maps per page)
  - Implemented comprehensive search across name fields and ID
  - Added map type filtering with switch logic
  - Select only relevant fields (16 fields instead of all 36)
  - Added Request parameter for filter handling

### Technical Details
- Database Table: `mapids` (1,021 total maps)
- Result Set: Varies by filter (all maps, dungeons only, safe zones, etc.)
- Smart Coordinate Display: Combined 4 coordinate fields into compact range display
- Property Badges: Dynamic rendering based on boolean flags
- Files Modified:
  - `app/Http/Controllers/DatabaseController.php` - maps() method complete rewrite
  - `resources/views/database/maps.blade.php` - added search/filter form, property badges

---

## [1.3.1] - 2025-11-09

### Added
- **NPCs Database - Enhanced Implementation**
  - **Improved Filtering System:**
    - NPC Type dropdown filter with all interactive NPC types
    - Dynamic population of NPC types from database
    - Search by NPC name (desc_en field)
    - Excluded non-interactive system objects (doors, effects, field objects, furniture, boards)
    - Only shows NPCs with actual names (non-empty desc_en)

  - **Enhanced Display:**
    - Added sprite icon column (48x48px with fallback system)
    - Added Level column for NPC level display
    - Fixed search to use correct field (desc_en instead of name)
    - Color code stripping for clean name display
    - 5-column table: Icon, NPC ID, Name, Type, Level
    - NPC type badges with gold color scheme
    - Hover effects on table rows

### Changed
- **NPCs Controller Improvements:**
  - Updated query to select only relevant fields (npcid, desc_en, impl, spriteId, lvl)
  - Added exclusion list for non-interactive NPC implementations
  - Implemented color code stripping in transform function
  - Changed ordering to alphabetical by desc_en

### Technical Details
- Database Query: Filters `impl NOT LIKE '%Monster%'` AND `impl NOT IN (excluded list)`
- Excluded implementations: L1Door, L1FieldObject, L1Effect, L1Furniture, L1Board, L1AuctionBoard
- Result set: ~1,500+ interactive NPCs (merchants, teleporters, quest givers, guards, etc.)
- Files Modified:
  - `app/Http/Controllers/DatabaseController.php` - npcs() method rewrite
  - `resources/views/database/npcs.blade.php` - table enhancement

---

## [1.3.0] - 2025-11-09

### Added
- **Monsters Database - Complete Implementation**
  - **Monsters List View - Card Layout:**
    - Card-based grid layout (responsive: 1-4 columns based on screen size)
    - Monster sprite icons with fallback system
    - Boss/Raid badges with accent borders for boss monsters
    - Aggressive warning badges (⚠️ AGGRO) for aggressive monsters
    - Card displays: Name, Level, HP, EXP, AC, Weakness attribute
    - Visual distinction for boss monsters (gold/red borders)
    - Hover effects on cards with accent border transition
    - Clickable cards navigate to monster detail page

  - **Advanced Filtering System:**
    - **Level Range Filter** - 11 ranges following official Lineage conventions:
      - 1-9 (Beginner), 10-19 (Novice), 20-29 (Intermediate)
      - 30-39 (Advanced), 40-49 (Expert), 50-59 (Master)
      - 60-69 (Elite), 70-79 (Epic), 80-89 (Legendary)
      - 90-99 (End Game), 100+ (Transcendent)
    - **Monster Type Filter** - 5 types:
      - Boss (is_bossmonster = true)
      - Raid Boss (boss + HP > 50,000)
      - Undead (undead != NONE)
      - Aggressive (is_agro = true)
      - Ranged (ranged > 0)
    - **Family Filter** - Dynamic dropdown populated from database
    - **Weakness Attribute Filter** - 5 options with emoji icons:
      - None, 🌿 Earth, 🔥 Fire, 💧 Water, ⚡ Wind
    - Name search functionality
    - Filter persistence across pagination
    - Active filter indicator

  - **Monsters Detail View:**
    - **Header Section:**
      - Large monster sprite (128×128px)
      - Monster name with boss/raid badges
      - Status badges: BOSS/RAID BOSS, AGGRESSIVE, Undead type, RANGED
      - Quick stats: Level, HP, EXP, AC (large display)
      - Boss monsters get accent border highlighting

    - **Basic Information Card:**
      - Monster ID
      - Family classification
      - Alignment with color coding and labels:
        - Chaotic (red), Neutral (gray), Lawful (blue)
        - Ranges: Very Chaotic, Chaotic, Slightly Chaotic, Neutral, Slightly Lawful, Lawful, Very Lawful
      - Size indicator (Large monsters)

    - **Combat Statistics Card:**
      - MP (mana points)
      - Magic Resistance (%)
      - Damage Reduction (%)
      - Attack Range
      - Movement Speed
      - Attack Speed
      - All stats conditionally displayed

    - **Attributes Section:**
      - 5 core attributes in grid layout: STR, DEX, CON, INT, WIS
      - Color-coded attribute cards
      - Large, readable numbers

    - **Resistances & Weaknesses:**
      - Weakness attribute with emoji icons and colors
      - Special attack types (Poison, Paralysis, Silence)
      - Clear visual distinction for weaknesses

    - **Drop Table Section:** Placeholder for future implementation
    - **Spawn Locations Section:** Placeholder for future implementation

### Changed
- **DatabaseController Updates:**
  - Rewrote `monsters()` method with comprehensive field selection
  - Changed filter from `impl LIKE '%Monster%'` to exact `impl = 'L1Monster'` per Lineage expert guidance
  - Added validation: `lvl > 0 AND hp > 0` to filter out test/debug entries
  - Implemented advanced filtering logic (level ranges, monster types, family, weakness)
  - Added dynamic family dropdown population
  - Changed perPage from 50 to 24 (optimized for card grid: 3×4=12, 4×6=24)
  - Sort order: Level ascending, then name ascending
  - Strip color codes from monster names
  - Added `monsterDetail($id)` method for detail view

- **Monsters List Page:**
  - Converted from table layout to card-based grid
  - Responsive grid: 1 column (mobile), 2 (tablet), 3 (desktop), 4 (xl screens)
  - Cards show essential combat information at a glance
  - Boss monsters visually distinguished with borders and badges
  - Smart pagination with page number navigation (matches other databases)
  - 24 items per page for clean grid layout

- **Routes:**
  - Added `GET /database/monsters/{id}` route for monster detail page
  - Route name: `database.monsters.detail`

### Technical Details
- **Database Fields Used:**
  - List view (16 fields): npcid, desc_en, spriteId, lvl, hp, mp, ac, mr, exp, alignment, is_bossmonster, is_agro, weakAttr, family, undead, ranged
  - Detail view: All npc table fields (67+ columns)
  - Verified against actual MySQL npc table structure

- **Filtering Logic:**
  - Level range: Switch statement with whereBetween() for 11 ranges
  - Monster type: Switch with specific conditions (boss, raid, undead, aggressive, ranged)
  - Family: Direct where clause on family field
  - Weakness: Direct where clause on weakAttr enum field
  - All filters work independently and in combination

- **Visual Design:**
  - Boss cards: border-game-accent (orange accent border)
  - Normal cards: border-game-border
  - RAID badge: Red background (bg-red-500)
  - BOSS badge: Accent background (bg-game-accent)
  - AGGRO badge: Red background with warning emoji
  - Weakness colors: Amber (Earth), Red (Fire), Blue (Water), Cyan (Wind)

- **Monster Statistics:**
  - Total monsters: 2,447 (filtered from 5,127 total NPCs)
  - Filter: `impl = 'L1Monster' AND lvl > 0 AND hp > 0`
  - Dynamic family list generated from distinct values
  - Alignment ranges from -500 (very chaotic) to +500 (very lawful)

- **Alignment Display Logic:**
  - < -500: Very Chaotic (red)
  - -500 to -100: Chaotic (red)
  - -100 to -10: Slightly Chaotic (red)
  - -10 to 10: Neutral (gray)
  - 10 to 100: Slightly Lawful (blue)
  - 100 to 500: Lawful (blue)
  - > 500: Very Lawful (blue)

- **Conditional Rendering:**
  - All detail sections use isset() checks
  - Badges only show when conditions met
  - Weakness section hidden if NONE
  - Special attack section hidden if NONE
  - Size indicator only for large monsters
  - Stats only shown if values exist

### Fixed
- **Database Query Validation:**
  - Confirmed `impl = 'L1Monster'` is the correct and only impl type for monsters
  - Added hp > 0 filter to exclude placeholder/test entries
  - Removed non-existent `name_id` field from queries

- **Field Name Corrections:**
  - Used `desc_en` not `name` for monster names
  - Used `intel` not `int` for intelligence attribute
  - Verified all 67 fields in npc table structure

### Notes
- **Monsters Database Complete** - Fully functional with card-based list and comprehensive detail views
- Design follows official NCsoft Lineage Powerbook conventions per expert guidance
- Card layout provides better visual hierarchy than table format
- Boss monsters receive special visual treatment as recommended
- Alignment system matches official Lineage standards
- 11 level ranges match official player progression tiers
- Ready for future enhancements: Drop table integration, Spawn location system
- Family filter dynamically populated from database (ensures all families available)
- Weakness attribute system ready for combat strategy guides
- Monster type filters enable quick filtering for specific encounter types

---

## [1.2.1] - 2025-11-09

### Added
- **Armor List Page Enhancements:**
  - Added "Safe" column displaying SafeEnchant value
  - SafeEnchant shown in accent color for visual emphasis
  - Column positioned after AC column

### Changed
- **Weapon List Page Updates:**
  - Replaced "Hit" column with "Safe" (SafeEnchant field)
  - Changed damage color from red to white for both Damage (S) and Damage (L) columns
  - SafeEnchant displayed in accent color matching armor page
  - Improved column readability with white damage values

- **Pagination:**
  - Armor list: Updated empty state colspan from 5 to 6 (added Safe column)
  - Weapon list: Maintained colspan at 7 (Hit replaced with Safe)

### Technical Details
- SafeEnchant field already fetched in DatabaseController for both armor() and weapons() methods
- No database query changes required
- Applied consistent accent color (`text-game-accent`) to Safe column across both pages

### Notes
- List pages now display safe enchant limit for all equipment
- Visual consistency maintained between armor and weapons list views
- White damage values provide better contrast and readability

---

## [1.2.0] - 2025-11-08

### Added
- **Items Database - Complete Implementation**
  - **Items List View:**
    - Icon column displaying item sprites from `iconId` field
    - Grade-based color coding (NORMAL, ADVANC, RARE, HERO, LEGEND, MYTH, ONLY)
    - Comprehensive columns: Icon, Name (grade-colored), Grade badge, Type badge, Level, Stackable
    - Clickable rows navigate to detail page
    - Advanced filtering system:
      - Item Type filter (17 types: Potion, Scroll, Food, Quest Item, Material, Spell Book, Treasure Box, Arrow, Wand, Gem, Pet Item, Event, Firecracker, Totem, Light, Sting, Other)
      - Item Grade filter (Normal, Advanced, Rare, Hero, Legend, Myth, Unique)
      - Level range filters (Min/Max level)
      - Name search
    - Pagination maintained across filter changes (50 items per page)
    - Sort by level then name
    - Matches armor/weapon database layout and styling

  - **Items Detail View:**
    - **Simplified Layout per user requirements:**
      - Removed Item Stats, Attribute, Resistances, Combat, Special, and Magic cards
      - Focused on essential information only
    - **2-Column Layout:** Image & Info card (matching armor/weapon structure)
    - **Full-Width Classes Section:** Shows all 10 character class restrictions (conditional display)
    - **Full-Width Properties Section:** 8 boolean properties with green checkmarks (✓) and red X marks (✗)
      - Cannot Delete, Cannot Sell, Tradeable (inverted), Blessed
      - Retrievable, Special Retrieval, Save At Once, Stackable
      - Properties grid: 2×4 responsive layout
      - Green checkmarks for active properties, red X marks for inactive
    - **Dropped By Section:** Placeholder for future drop information

  - **Field Normalization System:**
    - **Type field:** Converts database format to readable format
      - `QUEST_ITEM` → `Quest Item`
      - `SPELL_LONG` → `Spell Long`
      - Pattern: `ucwords(str_replace('_', ' ', strtolower($field)))`
    - **Use Type field:** Same normalization as Type field
    - **Material field:** Removes Korean text and normalizes
      - Regex: `preg_replace('/\([^)]*\)/', '', $material)` removes parentheses content
      - Example: `MITHRIL(미스릴)` → `Mithril`
      - Applied normalization after removing Korean text

### Changed
- **DatabaseController Updates:**
  - Enhanced `items()` method with comprehensive field selection
  - Added item_type, grade, and level range filtering
  - Implemented `itemDetail($id)` method with all etcitem fields
  - Strip color codes from item names (same regex pattern as armor/weapons)
  - Resolve `Magic_name` field using DescResolver helper (if present)

- **Items List Page:**
  - Filter interface matches armor/weapon page design
  - "Search & Filters" header section
  - Descriptive placeholders instead of labels
  - 4-column filter grid (Item Type, Grade, Min Level, Max Level)
  - "Apply Filters" and "Reset" buttons with active filter indicator
  - Full-width search bar with magnifying glass icon
  - 17 item type options in dropdown

- **Items Detail Page:**
  - Info section includes: Grade, Type (normalized), Use Type (normalized), Material (cleaned), Weight, Level Required (with range), Stackable, Blessed
  - All fields use proper isset() checks before display
  - Material field conditionally hidden if NONE(-) or not set
  - Level range display format: "Lv. X" or "Lv. X - Y" for min/max ranges
  - Stackable field converts enum('true','false') to Yes/No display

- **Color Code Stripping:**
  - Applied same `/\\\\a[A-Za-z]/` regex to item names
  - Processed server-side in both list and detail views
  - Clean display without Lineage color prefixes

### Technical Details
- **Database Fields Used:**
  - List view: item_id, desc_en, iconId, itemGrade, item_type, use_type, material, weight, min_lvl, merge, bless
  - Detail view: All etcitem table fields (150+ columns)
  - Verified all field names against actual MySQL table structure (DESCRIBE etcitem)

- **Query Optimization:**
  - Specific field selection for list queries
  - Indexed queries on item_type, grade, level
  - Pagination with preserved filter state

- **Icon System:**
  - Icons loaded from `public/images/icons/{iconId}.png`
  - Fallback to `0.png` (universal default icon)
  - 32×32px icons in list view, 96×96px in detail view

- **Conditional Rendering:**
  - All sections use `isset()` checks before accessing properties
  - Classes section only displays if item has class restrictions
  - Material field hidden if value is NONE(-) or not set
  - Level requirement hidden if min_lvl is 0 or not set
  - Blessed status hidden if bless <= 1
  - Safe property access pattern prevents undefined property errors

- **Item Statistics:**
  - Total items: 4,554 in etcitem table
  - 17 distinct item types (POTION, SCROLL, FOOD, QUEST_ITEM, MATERIAL, SPELL_BOOK, TREASURE_BOX, ARROW, WAND, GEM, PET_ITEM, EVENT, FIRE_CRACKER, TOTEM, LIGHT, STING, OTHER)
  - Same 7 grade system as armor/weapons

- **Properties Logic:**
  - Special handling for `merge` field (enum to boolean conversion)
  - Inverted logic for "Tradeable" (trade=0 means tradeable)
  - Blessed check: value > 1 (1=normal, 2=blessed, 3=cursed)
  - All other properties: 1=true/active, 0=false/inactive

### Fixed
- **Field Name Normalization:**
  - Prevented display of SCREAMING_SNAKE_CASE values
  - Converted all type/use_type values to Title Case
  - Removed Korean translations from material field

- **Material Field Cleanup:**
  - Regex pattern removes all content within parentheses
  - Handles multiple parentheses groups
  - Trims whitespace after removal
  - Normalizes remaining text to Title Case

### Routes
- **New Routes:**
  - `GET /database/items` - Items list with filters
  - `GET /database/items/{id}` - Item detail page

### Notes
- **Items Database Complete** - Fully functional with list and detail views
- Layout and styling perfectly match armor/weapon databases for consistency
- Simplified detail page focuses on essential information per user requirements
- Green/red checkmark system for properties matches armor/weapon patterns
- Field normalization ensures clean, readable display
- All 17 item types supported from etcitem table
- Properties section uses same 2×4 grid as armor/weapon detail pages
- Ready to extend to Monsters and NPCs databases next
- Field names verified against actual database to prevent errors

---

## [1.1.0] - 2025-11-08

### Added
- **Weapons Database - Complete Implementation**
  - **Weapons List View:**
    - Icon column displaying weapon sprites from `iconId` field
    - Grade-based color coding (NORMAL, ADVANC, RARE, HERO, LEGEND, MYTH, ONLY)
    - Comprehensive columns: Icon, Name (grade-colored), Grade badge, Type badge, Damage (Small/Large), Hit modifier, Level
    - Clickable rows navigate to detail page
    - Advanced filtering system:
      - Weapon Type filter (11 types: Sword, Dagger, Two-Handed Sword, Bow, Spear, Axe/Mace, Staff, Gauntlet, Claw, Dual-Wield, Two-Handed Staff)
      - Item Grade filter (Normal, Advanced, Rare, Hero, Legend, Myth, Unique)
      - Level range filters (Min/Max level)
      - Name search
    - Pagination maintained across filter changes (50 items per page)
    - Sort by level then name
    - Matches armor database layout and styling

  - **Weapons Detail View:**
    - **2-Column Layout:** Image & Info card (matching armor page structure)
    - **Full-Width Classes Section:** Shows all 10 character class restrictions
    - **Full-Width Properties Section:** 8 boolean properties with green checkmarks
    - **3-Column Main Stats Row:**
      - Damage card (DMG Small/Large, Hit/DMG modifiers, attack speed, range)
      - Attribute card (STR/DEX/INT bonuses, HP/MP bonuses, AC bonus)
      - Resistances card (elemental and status resistances)
    - **3-Column Secondary Stats Row:**
      - Combat card (critical rates, PVP damage, special combat bonuses)
      - Special card (EXP bonus, potion effects, regen rates, movement speed)
      - Magic card (centered magic effect name with light cyan color)
    - **Other Stats:** Damage reduction and PVP statistics sections
    - **Dropped By:** Placeholder section for future drop information

  - **Magic Effect Name Resolution System:**
    - Created `DescResolver.php` helper class
    - Loads and parses `desc-e.tbl` file for text lookups
    - Resolves magic name references (e.g., `$27120` → "Earth Jail")
    - Handles multiple effects separated by colons (e.g., `$19131:$18657`)
    - Removes parentheses from magic effect names for clean display
    - Copied `desc-e.tbl` file to Laravel storage (841KB)
    - Registered helper in composer autoload system

  - **Magic Card Special Design:**
    - Centered layout (both horizontal and vertical)
    - Light cyan/teal color (`text-cyan-400`)
    - Smaller, semibold text (`text-base font-semibold`)
    - Minimum height of 120px for consistent sizing
    - Different visual treatment from other stat cards

### Changed
- **DatabaseController Updates:**
  - Enhanced `weapons()` method with comprehensive field selection
  - Added type, grade, and level range filtering
  - Implemented `weaponDetail($id)` method with magic name resolution
  - Strip color codes from weapon names (same regex pattern as armor)
  - Resolve `Magic_name` field using DescResolver helper

- **Weapons List Page:**
  - Filter interface matches armor page design
  - "Search & Filters" header section
  - Descriptive placeholders instead of labels
  - 4-column filter grid (Type, Grade, Min Level, Max Level)
  - "Apply Filters" and "Reset" buttons with active filter indicator
  - Full-width search bar with magnifying glass icon

- **Color Code Stripping:**
  - Applied same `/\\\\a[A-Za-z]/` regex to weapon names
  - Processed server-side in both list and detail views
  - Clean display without Lineage color prefixes

### Technical Details
- **Database Fields Used:**
  - List view: item_id, desc_en, iconId, itemGrade, type, material, weight, dmg_small, dmg_large, hitmodifier, min_lvl, safenchant, bless
  - Detail view: All weapon table fields (104+ columns)
  - Verified all field names against actual MySQL table structure

- **Query Optimization:**
  - Specific field selection for list queries
  - Indexed queries on type, grade, level
  - Pagination with preserved filter state

- **Icon System:**
  - Icons loaded from `public/images/icons/{iconId}.png`
  - Fallback to `0.png` (universal default icon)
  - 32×32px icons in list view, 96×96px in detail view

- **Conditional Rendering:**
  - All sections use `isset()` checks before accessing properties
  - Sections only display if data exists
  - Safe property access pattern prevents undefined property errors

- **Material Translation:**
  - Same 12 material types as armor (IRON, MITHRIL, SILVER, ORIHARUKON, etc.)

- **Magic Name Resolution:**
  - DescResolver helper class uses static caching for performance
  - File loaded once per request and stored in memory
  - Supports both single references (`$27120`) and multiple (`$19131:$18657`)
  - Line-based lookup: ID maps to line number (ID + 1) in desc-e.tbl

### Fixed
- **Database Field Name Corrections:**
  - Corrected `skillName` → `Magic_name`
  - Removed non-existent fields: `retrieveEnchant`, `two_hand`, `ranged`, `MainId`
  - Corrected `magicDmgmodifier` → `magicdmgmodifier`
  - Corrected `defense_earth` → `addEr`
  - Added proper replacements: `haste_item`, `canbedmg`
  - All field names verified against actual weapon table structure using MySQL DESCRIBE

- **Undefined Property Errors:**
  - Added comprehensive `isset()` checks throughout detail page
  - Implemented safe property access pattern for all conditionals
  - Properties section uses null-safe access with fallback values

### Routes
- **New Routes:**
  - `GET /database/weapons` - Weapons list with filters
  - `GET /database/weapons/{id}` - Weapon detail page

### Notes
- **Weapons Database Complete** - Fully functional with list and detail views
- Layout and styling perfectly match armor database for consistency
- Magic name resolution uses authentic NCsoft Lineage implementation pattern
- Text files (`.tbl`) used for localization, referenced by IDs throughout database
- All 18 weapon types supported (SWORD, DAGGER, TOHAND_SWORD, BOW, SPEAR, BLUNT, STAFF, GAUNTLET, CLAW, EDORYU, TOHAND_STAFF)
- Combat/Special/Magic sections displayed in unified 3-column row
- Ready to extend same template to Items database
- Field names verified against actual database to prevent errors

---

## [1.0.0] - 2025-11-08

### Added
- **Armor Database - Phase 3: Polish & Refinements**
  - **Dropped By Section:**
    - Added placeholder section for mob drop information
    - Prepared structure for displaying monsters that drop this item
    - Future implementation will show drop rates and chances

  - **Universal Default Icon System:**
    - Implemented `0.png` as universal fallback icon across entire website
    - Applied to armor list view, detail view, set pieces, and class icons
    - Consistent fallback behavior for missing item sprites

### Changed
- **Armor Detail Page - Layout Unification:**
  - **Converted all stat cards to consistent label-value format:**
    - Defense section: Removed duplicate Safe Enchant, converted to label-value pairs
    - Attribute section: Combined base stats and resources, label-value format
    - Resistances section: Merged elemental and status resistances into single card
    - Combat Bonuses section: Converted from grid boxes to label-value list
    - PVP Statistics section: Converted from grid boxes to label-value list
    - Special Effects section: Converted from grid boxes to label-value list
  - **Three-column row layout for main stats:**
    - Defense, Attribute, and Resistances cards now in equal-width row
    - Responsive grid: `grid-cols-1 md:grid-cols-3`
    - All cards use same spacing and border styling
  - **Section title changes:**
    - "Basic Information" → "Info"
    - "Attribute Bonuses" → "Attribute"
    - Combined "Elemental Resistances" + "Status Resistances" → "Resistances"
  - **Info section cleanup:**
    - Removed Item ID display
    - Added color-coded Safe Enchant (green for positive, red for negative)
    - Removed duplicate Safe Enchant from Defense card
  - **Added missing database fields:**
    - Info: max_use_time, MainId, MainId2, MainId3, polyDescId
    - Resistances: regist_skill, regist_spirit, regist_dragon
    - Combat Bonuses: hitup_all, hitup_magic
    - PVP Statistics: PVPMagicDamageReductionEgnor, abnormalStatusDamageReduction

- **Armor List Page - Filter Interface Redesign:**
  - **Modern filter section with header:**
    - Added "Search & Filters" title bar with dark background
    - Removed all field labels (Search by Name, Equipment Type, etc.)
    - Replaced labels with descriptive placeholders
    - Changed form spacing from `space-y-6` to `space-y-4`
  - **Enhanced search bar:**
    - Full-width search input with magnifying glass icon
    - Larger padding (py-3) for better touch targets
    - Updated placeholder: "Search armor by name..."
  - **Improved filter grid:**
    - Cleaner 4-column layout for Type, Grade, Min Level, Max Level
    - Rounded corners changed from `md` to `lg`
    - Better visual hierarchy with input grouping
  - **Better action buttons:**
    - "Search" → "Apply Filters"
    - "Clear Filters" → "Reset"
    - Added separator border above buttons
    - Added "Active filters applied" indicator when filters are active
    - Better spacing and hover effects with shadows

- **Color Code Stripping System:**
  - **DatabaseController updates:**
    - Armor list: Strip Lineage color codes (\\aF, \\aH, etc.) from all item names
    - Armor detail: Strip color codes from main item name
    - Set pieces: Strip color codes from all set piece names
  - **Regex pattern:** `/\\\\a[A-Za-z]/` removes all in-game color prefixes
  - **Applied to:**
    - armor.blade.php list view (line 69-72)
    - armor-detail.blade.php main item (line 93)
    - armor-detail.blade.php set pieces (line 117-120)

### Technical Details
- **Layout Changes:**
  - Unified all conditional cards to use `space-y-2 text-sm` with `flex justify-between py-2 border-b`
  - Consistent styling: label on left (text-game-text-muted), value on right (font-bold text-game-text)
  - All cards have matching padding (p-6) and border styling
  - Three-column row uses `grid grid-cols-1 md:grid-cols-3 gap-6`

- **Icon System:**
  - Default icon path: `public/images/icons/0.png`
  - Applied via `onerror` attribute on all `<img>` tags
  - Covers: armor list icons, detail page main icon, class restriction icons, set piece icons

- **Color Code Regex:**
  - Pattern matches Lineage color codes: backslash + 'a' + any letter
  - Examples removed: `\aF` (white), `\aH` (green), `\aD` (blue), etc.
  - Processed server-side before rendering to blade templates

### Notes
- **Phase 3 Complete** - Armor database now has unified, polished interface
- Consistent label-value layout across all stat cards
- Cleaner filter interface without label clutter
- Item names display cleanly without color codes
- Universal fallback icon system implemented
- Ready to extend same template to other database categories (Weapons, Items, Monsters, NPCs)
- Dropped By section prepared for future implementation with droplist table integration

---

## [0.9.0] - 2025-11-08

### Added
- **Armor Database - Phase 2: Complete Combat & Resistance System**
  - **Enhanced Detail Page Visual Design:**
    - Modern gradient header with glowing icon effect (blur-xl on accent background)
    - Quick Stats Summary card (4-stat grid: AC, Level, MR, Safe Enchant)
    - 3-column responsive layout (left sidebar + 2-column main content area)
    - SVG icons for all section headers
    - Color-coded card borders matching stat types
    - Hover effects on all stat cards (border-game-accent transition)
    - Improved spacing and organization with consistent padding/gaps

  - **Section 6 - Combat Bonuses:**
    - Melee combat: Hit Rate, Damage Rate
    - Ranged combat: Bow Hit Rate, Bow Damage Rate
    - Critical strikes: Short Critical, Long Critical, Magic Critical
    - Special bonuses: Haste, Carry Bonus
    - Advanced stats: Dragon Damage (addDg), Elf Resistance (addEr), Spirit Resistance (addMe)
    - 4-column responsive grid layout
    - Green color coding for offensive stats

  - **Section 7 - Elemental Resistances:**
    - Fire Resistance 🔥 (red border)
    - Water Resistance 💧 (blue border)
    - Wind Resistance 🌪️ (cyan border)
    - Earth Resistance 🪨 (amber border)
    - All Elements Resistance (purple border)
    - Emoji icons for quick visual identification
    - Color-coded borders matching element types
    - 4-column responsive grid

  - **Section 8 - Status Resistances:**
    - Petrification Resistance 🗿
    - Sleep Resistance 😴
    - Freeze Resistance 🧊
    - Blindness Resistance 🙈
    - Fear Resistance 😱
    - Stun Resistance 💫
    - Poison Immunity 💀
    - 4-column responsive grid
    - Blue color scheme for defensive stats

  - **Section 9 - PVP Statistics:**
    - Special red-themed gradient card (from-red-900 to-game-secondary)
    - Red accent border (border-2 border-red-500)
    - PVP Damage (flat bonus)
    - PVP Damage % (percentage increase)
    - PVP Damage Reduction (flat reduction)
    - PVP Damage Reduction % (percentage reduction)
    - PVP Magic Damage Reduction
    - PVP Magic Damage Reduction %
    - PVP Evasion Rate
    - Color-coded: Red for offensive stats, green for defensive stats
    - 3-column responsive grid

  - **Section 10 - Special Effects:**
    - Experience Bonus (%) - gold color
    - Potion Effectiveness (%) - green color
    - Attack Speed Delay Reduction - purple color
    - Movement Speed Bonus - cyan color
    - HP Regeneration Rate - red color
    - MP Regeneration Rate - blue color
    - 3-column responsive grid
    - Color-coded borders for each effect type

  - **Conditional Rendering System:**
    - All 10 sections use @if directives to check for data
    - Over 40 individual field checks across all sections
    - Sections only display when armor has relevant stats
    - Clean, minimal display for items with few bonuses
    - Comprehensive display for high-tier items with many bonuses

### Changed
- **Armor Detail Page Complete Redesign:**
  - Reorganized from basic 5-section layout to modern 10-section layout
  - Moved item icon from standard display to large gradient header with glow effect
  - Added quick stats summary for at-a-glance information
  - Implemented 3-column layout (sidebar: basic info & requirements, main: all stats)
  - Enhanced section headers with inline SVG icons
  - Improved visual hierarchy with consistent card styling
  - Better responsive design with mobile-first grid layouts

- **DatabaseController Minor Update:**
  - Updated armorDetail() method comment to emphasize comprehensive field fetching
  - No query changes needed (already fetches all fields via ->first())

### Technical Details
- **Database Fields Used:**
  - Combat: hit_rate, dmg_rate, bow_hit_rate, bow_dmg_rate, shortCritical, longCritical, magicCritical, haste_item, carryBonus, addDg, addEr, addMe
  - Elemental: defense_fire, defense_water, defense_wind, defense_earth, attr_all
  - Status: ToleranceStone, ToleranceSleep, ToleranceFreeze, ToleranceBlind, ToleranceFear, ToleranceStun, immu (poison immunity)
  - PVP: PVPDamage, PVPDamagePercent, PVPDamageReduction, PVPDamageReductionPercent, PVPMagicDamageReduction, PVPMagicDamageReductionPercent, PVPEvasionRate
  - Special: expBonus, potionPercent, attackSpeedDelayRate, addSp, hpRegen, mpRegen

- **Visual Enhancements:**
  - Gradient backgrounds: `bg-gradient-to-r from-game-secondary to-game-primary`
  - Icon glow effect: `absolute inset-0 bg-game-accent opacity-20 blur-xl`
  - Quick stats grid: 2×2 responsive grid (4 cards)
  - Section layouts: 2-4 column responsive grids depending on stat density
  - Color palette:
    - Red (#ef4444) - HP, fire, PVP damage
    - Blue (#3b82f6) - MP, water, status resistance
    - Green (#10b981) - Physical stats, potion effects
    - Gold (#fbbf24) - Experience, special bonuses
    - Cyan (#06b6d4) - Wind, speed
    - Amber (#f59e0b) - Earth
    - Purple (#a855f7) - Magic, all elements

- **Responsive Breakpoints:**
  - Mobile: 1-2 column grids
  - Medium (md): 3-4 column grids
  - Large (lg): Full 3-column layout with sidebar

- **Blade Directives Used:**
  - 10 major @if blocks (one per section)
  - 40+ nested @if checks for individual fields
  - Grid layouts with responsive classes
  - Inline SVG icons for performance

### Notes
- **Phase 2 Complete** - Armor detail page now displays 100+ armor statistics
- 10-section comprehensive view (5 base + 5 new Phase 2 sections)
- Modern, clean, easy-to-scan design
- Conditional rendering ensures only relevant data is shown
- Color-coded for quick stat identification
- Responsive design works on all screen sizes
- Phase 3 planned features: Set bonuses, enchant preview, comparison tool, drop locations

---

## [0.8.0] - 2025-11-08

### Added
- **Armor Database - Phase 1 MVP** (Based on NCsoft Lineage expert recommendations)
  - **Enhanced List View:**
    - Icon column displaying item sprites from `iconId` field
    - Grade-based color coding (NORMAL=white, ADVANCED=green, RARE=blue, HERO=purple, LEGEND=orange, MYTH=red, ONLY=gold)
    - Comprehensive columns: Icon, Name (grade-colored), Grade badge, Type, AC, Level
    - Clickable rows navigate to detail page
    - Item names colored by grade using official Lineage color standards
    - Advanced filtering system:
      - Equipment Type filter (Helmet, Armor, Shield, Gloves, Boots, etc.)
      - Item Grade filter (Normal, Advanced, Rare, Hero, Legend, Myth, Unique)
      - Level range filters (Min/Max level)
      - Name search
    - Pagination maintained across filter changes (50 items per page)
    - Sort by level then name

  - **Armor Detail View (5 Sections):**
    - **Section 1 - Item Header:**
      - Large 96×96px item icon
      - Item name with grade-based color coding
      - Grade badge
      - Equipment type (Helmet, Armor, Shield, etc.)
      - Item notes/description if available
    - **Section 2 - Basic Information:**
      - Item ID (for reference)
      - Material type (Cloth, Leather, Mithril, Oriharukon, etc.)
      - Weight (with kg conversion)
      - Tradeable status (Bound/Tradeable)
      - Deletable status
      - Warehouse storage capability
      - Blessed type (Normal/Blessed/Cursed)
    - **Section 3 - Requirements:**
      - Level requirement (min-max range)
      - Class restrictions with visual indicators (✓/✗)
      - All 10 character classes: Royal, Knight, Mage, Elf, Dark Elf, Dragon Knight, Illusionist, Warrior, Fencer, Lancer
      - Green checkmark for usable classes, red X for restricted
    - **Section 4 - Defense Statistics:**
      - Armor Class (AC) - primary defense value
      - AC Bonus (sub) - additional AC
      - Magic Defense (MR)
      - Safe Enchant level
      - Damage Reduction (physical)
      - Magic Damage Reduction
      - Card-based layout for each stat
    - **Section 5 - Attribute Bonuses** (only shown if item has bonuses):
      - Base Attributes (STR, CON, DEX, INT, WIS, CHA)
      - Resource Bonuses (HP, MP, HP Regen, MP Regen, SP/Stamina)
      - Grid layout with color-coded values

  - **Grade Color System:**
    - Added 7 new Tailwind colors for item grades:
      - `grade-normal`: #ffffff (white)
      - `grade-advanced`: #00ff00 (green)
      - `grade-rare`: #4169e1 (blue)
      - `grade-hero`: #9d4dff (purple)
      - `grade-legend`: #ff8c00 (orange)
      - `grade-myth`: #ff0000 (red)
      - `grade-only`: #ffd700 (gold)
    - Colors match official Lineage item grade standards

  - **Routes:**
    - `GET /database/armor` - List view with filters
    - `GET /database/armor/{id}` - Detail view for specific armor

### Changed
- **DatabaseController Updates:**
  - Enhanced `armor()` method with multiple filter parameters
  - Added field selection for optimized queries
  - Implemented type, grade, and level range filtering
  - Renamed variable from `$items` to `$armor` for clarity
  - Added `armorDetail($id)` method for detail page
  - Sort order changed to level-first, then name

- **Armor List View Improvements:**
  - Changed from basic table to enhanced table with icons
  - Added grade badges with color-coded backgrounds
  - Removed Material and Weight columns (moved to detail view)
  - Added clickable rows with cursor-pointer
  - Enhanced search form with 5 filter options (was 2)
  - Improved responsive layout (3-column grid for filters)

### Technical Details
- **Database Fields Used:**
  - List view: item_id, desc_en, iconId, itemGrade, type, material, ac, min_lvl, weight, safenchant
  - Detail view: All armor table fields (100+ columns)
- **Query Optimization:**
  - Specific field selection instead of SELECT *
  - Indexed queries on type, grade, level
  - Pagination with preserved filter state
- **Icon System:**
  - Icons loaded from `public/images/icons/{iconId}.png`
  - Fallback to `default-armor.png` if icon missing
  - 32×32px icons in list view, 96×96px in detail view
- **Conditional Rendering:**
  - Sections only display if data exists (e.g., no attribute section if no bonuses)
  - Optional fields hidden when zero/null
- **Material Translation:**
  - 12 material types mapped to readable names
  - Numeric codes (0-11) converted to Cloth, Leather, Mithril, Oriharukon, etc.

### Notes
- **Phase 1 MVP Complete** - Basic but functional armor database
- Designed by NCsoft Lineage expert with 30 years experience
- Follows official Lineage Powerbook presentation style
- Phase 2 will add: Combat bonuses, elemental resistances, PVP stats, special effects (40+ additional fields)
- Phase 3 will add: Set bonuses, enchant bonuses, comparison tool, drop information
- Icons must be extracted from Lineage client .pak files
- Detail page supports 100+ armor table fields, conditionally displaying only relevant data

---

## [0.7.0] - 2025-11-08

### Added
- **Game Icon Integration**
  - Replaced all placeholder icons with actual Lineage game sprites
  - Server statistics now display authentic game icons:
    - Online Players: icon #654
    - Total Accounts: icon #660
    - Total Clans: icon #661
    - Highest Level: icon #650
  - Database category cards now display authentic game icons:
    - Armor: icon #29
    - Weapons: icon #38
    - Items: icon #237
    - Monsters: icon #259
    - NPCs: icon #1486
    - Maps: icon #465

### Changed
- **Icon System Complete**
  - All 10 icon placeholders replaced with real game sprites
  - Icons stored in `public/images/icons/` directory
  - Using PNG format with transparency
  - Server stats icons: 48×48px
  - Category icons: 128×128px

### Removed
- Emoji placeholders (🛡️, ⚔️, 💎, 👹, 👤, 🗺️)
- Letter placeholders (P, A, C, L)
- Colored background boxes for placeholders

### Technical Details
- All icons use `object-contain` for proportional scaling
- Images loaded via Laravel's `asset()` helper
- Maintains responsive design across all screen sizes
- Icons enhance visual consistency with Lineage game aesthetic

### Notes
- Icons extracted from Lineage client sprite sheets
- ICONS_TODO.md can now be archived or removed
- Homepage now fully represents authentic Lineage visual style

---

## [0.6.0] - 2025-11-08

### Changed
- **Database Category Cards Layout Redesign**
  - Reorganized card structure with new vertical layout:
    - Title positioned at top (text-2xl font-bold, centered)
    - Icon displayed in middle with generous padding (py-8)
    - Total count shown at bottom (text-4xl font-bold)
  - Removed description text from cards for cleaner appearance
  - All category counts now display live data from database:
    - Armor: fetched from `armor` table
    - Weapons: fetched from `weapon` table
    - Items: fetched from `etcitem` table
    - Monsters: filtered from `npc` table (`impl LIKE '%Monster%'`)
    - NPCs: filtered from `npc` table (`impl NOT LIKE '%Monster%'`)
    - Maps: fetched from `mapids` table
  - Counts formatted with `number_format()` for readability (e.g., "1,234")

- **HomeController Updates**
  - Added `$categoryCounts` array with live database queries
  - All counts fetched using read-only `mysql_game_public` connection
  - Optimized queries for category count retrieval

### Technical Details
- Card layout uses flexbox centering for all elements
- Icon section maintains 128x128px placeholder size
- Count section includes descriptive label ("Total Items", "Total Monsters", etc.)
- Hover effects preserved on group interaction
- Maintained color scheme consistency (accent color for counts)

### Notes
- Cards now more data-focused with prominent count display
- Live database counts update on each page load
- Icon placeholders remain temporary (see ICONS_TODO.md for replacement guide)

---

## [0.5.0] - 2025-11-08

### Added
- **Server Status Banner**
  - Added animated green pulse indicator for online status
  - Real-time timestamp showing last update time
  - Gradient background with accent border highlighting

- **Game Icon Placeholders**
  - Added icon placeholder system for all UI elements
  - Server statistics now display colored icon placeholders (P, A, C, L)
  - Database category cards feature large emoji placeholders (🛡️, ⚔️, 💎, 👹, 👤, 🗺️)
  - All placeholders marked with HTML comments for easy replacement
  - Created `ICONS_TODO.md` documentation for icon integration

### Changed
- **Server Statistics Labels Renamed**
  - "Total Characters" → "Online Players" (more accurate for current players)
  - "Player Characters" → "Total Accounts" (better reflects account count)
  - "Total Clans" - unchanged
  - "Highest Level" - unchanged

- **Database Category Cards Redesign**
  - Removed "Database Categories" section heading
  - Removed "Database" suffix from all card titles:
    - "Armor Database" → "Armor"
    - "Weapon Database" → "Weapons"
    - "Item Database" → "Items"
    - "Monster Database" → "Monsters"
    - "NPC Database" → "NPCs"
    - "Map Database" → "Maps"
  - Added 192px (h-48) image section at top of each card
  - Cards now use overflow-hidden for clean image display
  - Larger 128x128px icon placeholders for better visibility

- **Icon System**
  - Replaced SVG icons with game-style placeholder system
  - Server stats use 48x48px icon placeholders
  - Category cards use 128x128px icon placeholders
  - All icons use colored background boxes with opacity

### Technical Details
- Server status timestamp uses Laravel's `now()` helper
- Icon placeholders use Tailwind's bg-opacity utility classes
- Cards maintain hover effects with border-accent transition
- Image sections use flexbox centering for icons
- Documentation includes 3 implementation options (IMG tags, background images, sprite sheets)

### Documentation
- Created `ICONS_TODO.md` with complete icon integration guide
- Documented all icon locations with line numbers
- Included extraction instructions for client .pak files
- Provided multiple implementation examples (IMG, CSS background, sprite sheets)
- Listed icon requirements (size, format, source)

### Notes
- All icon placeholders temporary - replace with actual Lineage sprites
- Icons should be extracted from `LineageWarriorClient/*.pak` files
- Recommended format: PNG with transparency, 64x64px (stats) and 128x128px (categories)
- Consider sprite sheet implementation for better performance

---

## [0.4.0] - 2025-11-08

### Added
- **Hero Section Redesign**
  - Implemented 900px height hero section with background image
  - Added dark gradient overlay for better text readability
  - Increased title size to text-7xl with drop shadow effects
  - Centered content with flexbox layout

- **Database Category System**
  - Created `/database` main page with 6 category cards
  - Created `DatabaseController.php` with methods for all categories
  - Implemented database category pages:
    - **Armor Database** - Browse all armor with AC stats, material filtering
    - **Weapons Database** - Explore all weapons with damage stats
    - **Items Database** - View consumables and quest items with stackable filter
    - **Monsters Database** - Complete bestiary with HP, EXP, level filtering
    - **NPCs Database** - Find merchants and quest givers
    - **Maps Database** - View all maps with coordinates and dimensions
  - All category pages include search functionality (except maps)
  - Pagination support (50 items per page for items, 50 for monsters/NPCs)
  - Responsive card-based category selection on main database page

- **Database Views**
  - Created `resources/views/database/index.blade.php` - Category selection hub
  - Created `resources/views/database/armor.blade.php`
  - Created `resources/views/database/weapons.blade.php`
  - Created `resources/views/database/items.blade.php`
  - Created `resources/views/database/monsters.blade.php`
  - Created `resources/views/database/npcs.blade.php`
  - Created `resources/views/database/maps.blade.php`

### Changed
- **Homepage Redesign**
  - Reduced top players display from 10 to 5 players
  - Removed "View Rankings" and "Item Database" buttons from hero section
  - Replaced character-focused quick links with database category cards (6 categories)
  - Updated hero background image to character lineup image
  - Simplified hero content to just title and subtitle

- **Navigation Overhaul**
  - Removed "Rankings", "Item Database", and "Characters" from navigation
  - Added single "Database" navigation link
  - Updated both desktop and mobile navigation menus
  - Simplified navigation from 4 links to 2 (Home, Database)

- **Controller Updates**
  - Updated `HomeController.php` to fetch only 5 top players instead of 10

### Removed
- **Deprecated Features**
  - Removed character rankings system (/rankings route and controller)
  - Removed character lookup system (/characters route and controller)
  - Removed standalone items browser (/items route)
  - Removed `RankingsController.php`
  - Removed `CharactersController.php`
  - Removed `ItemsController.php`
  - Removed character-related navigation links
  - Removed character-related views (rankings.blade.php, characters/index.blade.php, characters/show.blade.php)
  - Removed items views (items/index.blade.php, items/show.blade.php)

### Routes
- **New Routes:**
  - `GET /database` - Database category hub
  - `GET /database/armor` - Armor database
  - `GET /database/weapons` - Weapons database
  - `GET /database/items` - Items database
  - `GET /database/monsters` - Monsters database
  - `GET /database/npcs` - NPCs database
  - `GET /database/maps` - Maps database

- **Removed Routes:**
  - `GET /rankings`
  - `GET /items`
  - `GET /items/{id}`
  - `GET /characters`
  - `GET /characters/{name}`

### Technical Details
- Database queries use `mysql_game_public` read-only connection
- Material filtering available on armor and weapons
- Level range filtering on monsters
- Stackable filtering on items
- All tables display relevant stats (AC for armor, damage for weapons, HP/EXP for monsters)
- Maps page calculates dimensions automatically from coordinate data

### Notes
- **User-Requested Redesign** - Complete homepage and navigation restructure
- Hero background image should be saved to `public/images/hero-background.jpg`
- Focus shifted from character-centric to database-centric application
- All database categories accessible from single hub page
- Simplified user experience with fewer navigation options

---

## [0.3.0] - 2025-11-08

### Added
- **Frontend Framework Setup**
  - Installed Tailwind CSS 3.4.0 with PostCSS
  - Installed Alpine.js 3.x for reactive components
  - Configured Vite 7.2.2 as build tool
  - Created `resources/css/app.css` with Tailwind directives
  - Created `resources/js/app.js` with Alpine.js initialization
  - Fixed IPv6 binding issue in Vite (forced IPv4 on 127.0.0.1:5175)

- **Custom Color Scheme**
  - Implemented custom game-themed color palette in `tailwind.config.js`:
    - `game-text`: #ffffff (white text)
    - `game-bg`: #141414 (main background)
    - `game-primary`: #080808 (darkest background)
    - `game-secondary`: #191919 (secondary surfaces)
    - `game-accent`: #ff883e (orange accent)
    - `game-accent-hover`: #ff9d5c (lighter orange)
  - Added utility colors (game-border, game-text-muted, game-gold, game-green, game-red, game-blue)
  - Configured Inter font for sans-serif, Fira Code for monospace

- **Layout System**
  - Created `resources/views/layouts/app.blade.php` - Base layout with Vite integration
  - Created `resources/views/layouts/public.blade.php` - Public layout with navigation
    - Responsive navigation with mobile menu (Alpine.js powered)
    - Active link highlighting
    - Desktop and mobile menu support
    - Footer section
  - All layouts use custom color scheme with dark theme

- **Homepage (/)**
  - Created `app/Http/Controllers/HomeController.php`
  - Created `resources/views/home.blade.php`
  - Features:
    - Hero section with server branding
    - Server statistics cards (Total Characters, Player Characters, Total Clans, Highest Level)
    - Top 10 players table with live data
    - Quick links to Rankings, Items, and Characters pages
    - All data pulled live from game database via read-only connection

- **Rankings System (/rankings)**
  - Created `app/Http/Controllers/RankingsController.php`
  - Created `resources/views/rankings.blade.php`
  - Features:
    - 5 ranking categories with tabs:
      - Level Rankings (default)
      - PVP Kills (TotalKillCount)
      - PK Count (PKcount)
      - Wealth (Adena)
      - Online Time (hours)
    - Paginated results (50 per page)
    - Rank badges (gold/silver/bronze for top 3)
    - Character name, class, level, clan display
    - Responsive table design

- **Item Database (/items)**
  - Created `app/Models/GamePublic/Item.php`
  - Created `app/Http/Controllers/ItemsController.php`
  - Created `resources/views/items/index.blade.php`
  - Created `resources/views/items/show.blade.php`
  - Features:
    - Combined view of etcitem, armor, and weapon tables
    - Search by item name
    - Filter by category (Weapons, Armor, Items)
    - Filter by material (23 material types)
    - Paginated results (50 per page)
    - Item detail page with full stats
    - Material name translation (Bronze, Mithril, Oriharukon, etc.)
    - Weapon stats (DMG small/large, hit/dmg modifiers)
    - Armor stats (AC, HP/MP bonus, magic defense)

- **Character Lookup (/characters)**
  - Created `app/Http/Controllers/CharactersController.php`
  - Created `resources/views/characters/index.blade.php`
  - Created `resources/views/characters/show.blade.php`
  - Features:
    - Character search by name
    - Filter by class (10 character classes)
    - Paginated results (20 per page)
    - Character profile page with:
      - Basic stats (STR, DEX, CON, INT, WIS, CHA)
      - Combat stats (HP, MP, AC)
      - PVP statistics (Total Kills, PK Count, Deaths, K/D Ratio)
      - Additional info (Experience, Alignment, Online Time)
      - Inventory item count
      - Quest count
      - Clan information

- **Routing**
  - Updated `routes/web.php` with all public routes:
    - `/` - Homepage
    - `/rankings` - Character rankings with type parameter
    - `/items` - Item database index
    - `/items/{id}` - Item detail
    - `/characters` - Character search
    - `/characters/{name}` - Character profile

### Changed
- Updated navigation links in public layout to include all new pages
- Updated homepage quick links to point to actual functional pages
- Fixed color scheme from planning version to user-requested colors
- Downgraded from Tailwind CSS v4 to v3.4 for compatibility

### Fixed
- **Tailwind CSS v4 Incompatibility**
  - Uninstalled Tailwind v4 which caused PostCSS errors
  - Installed Tailwind CSS 3.4.0
  - Updated app.css syntax to v3 format (@tailwind directives)
  - Removed @tailwindcss/vite plugin (v4 specific)

- **IPv6 Binding Issue**
  - Vite was binding to [::1]:5175 which browsers couldn't load properly
  - Updated `vite.config.js` to force IPv4 (127.0.0.1)
  - CSS and JavaScript now loading correctly

- **Database Name Correction**
  - Corrected database name from `l1jdb` to `l1j_remastered`
  - Updated MySQL grants for l1j_readonly user

### Testing
- ✓ Homepage displays live server statistics (33 characters, 12 players, 21 GMs)
- ✓ Top 10 players table showing real character data
- ✓ All 5 ranking types working with pagination
- ✓ Item database showing combined items from all 3 tables
- ✓ Item search and filtering functional
- ✓ Character search working with name and class filters
- ✓ Character profiles displaying full stats and PVP data
- ✓ Tailwind CSS styling applied across all pages
- ✓ Responsive design working on mobile and desktop
- ✓ Alpine.js mobile menu toggle working
- ✓ All navigation links functional
- ✓ Custom orange accent color scheme (#ff883e) applied

### Notes
- **Phase 2: Public Website Core** is now complete
- All main public pages implemented (Home, Rankings, Items, Characters)
- Read-only database access working securely for all public pages
- Next phase: Authentication system and admin panel
- Vite dev server running on http://127.0.0.1:5175
- Laravel server running on http://127.0.0.1:8000

---

## [0.2.0] - 2025-11-08

### Added
- **Laravel Installation**
  - Installed Laravel 12.37.0 (latest stable version)
  - Installed Node.js dependencies (npm packages)
  - Generated application key
  - Verified PHP 8.2.12, Composer 2.8.12, Node 22.21.0 installed

- **Database Configuration**
  - Created `laravel_l1r` database for Laravel application
  - Created `l1j_readonly` MySQL user with SELECT-only permissions on `l1j_remastered` database
  - Configured multi-database connections in `config/database.php`:
    - `mysql` - Laravel app database (laravel_l1r)
    - `mysql_game` - Game database admin access (l1j_remastered)
    - `mysql_game_public` - Game database read-only access (l1j_remastered)
  - Updated `.env` with all database connection details
  - Created `config/game.php` for game-specific settings

- **Database Migrations**
  - Ran Laravel migrations on `laravel_l1r` database
  - Created tables: users, cache, jobs, migrations
  - All migrations completed successfully

- **Base Model Classes**
  - Created `app/Models/Game/GameModel.php` - Base model for admin (read-write) access
    - Uses `mysql_game` connection
    - Disables timestamps
    - Configures L1J datetime format
  - Created `app/Models/GamePublic/GamePublicModel.php` - Base model for public (read-only) access
    - Uses `mysql_game_public` connection
    - Prevents save(), delete(), update() operations
    - Three-layer security (DB user, connection, application)
  - Created `app/Models/Game/Character.php` - Admin character model
    - Full CRUD capabilities
    - Relationships (account, clan, items)
    - Query scopes (online, ofClass, playersOnly, orderByLevel)
    - Helper methods (isGM, isOnline, getClassNameAttribute)
  - Created `app/Models/GamePublic/Character.php` - Public character model
    - Read-only access
    - Query scopes for rankings and lookups
    - Helper methods for display

- **Database Setup Scripts**
  - Created `database/setup_databases.sql` for automated database/user setup

### Changed
- Updated `.env` database configuration from SQLite to MySQL
- Set `APP_NAME` to "L1R Database"
- Set `APP_URL` to http://localhost:8000

### Testing
- ✓ Verified all three database connections working
- ✓ Successfully queried game database (found 33 characters, 12 players)
- ✓ Tested admin Character model (query successful)
- ✓ Tested public Character model with scopes (playersOnly working)
- ✓ Confirmed read-only protection in GamePublicModel

### Notes
- **Phase 1: Foundation** is now complete
- Game database name is `l1j_remastered` (not `l1jdb`)
- Found 33 total characters in database (12 players, 21 GMs)
- Ready to begin Phase 2: Authentication & Base Layout

---

## [0.1.0] - 2025-11-08

### Added
- **Project Initialization**
  - Created project directory: `L1RDatabase`
  - Created `implementation.md` with comprehensive architecture documentation
  - Created `changelog.md` for version tracking

- **Documentation**
  - Database architecture analysis (342 game tables identified and categorized)
  - Laravel application architecture design
  - Multi-database connection strategy (Laravel DB + Game DB + Game DB Read-Only)
  - Security implementation plan
  - Authentication strategy using game accounts
  - 6-phase implementation roadmap

- **Technical Specifications**
  - Technology stack defined: Laravel 11, PHP 8.2+, Tailwind CSS 3.4+, Alpine.js
  - Directory structure planned
  - Model architecture designed (GameModel, GamePublicModel base classes)
  - Repository pattern planned for query abstraction
  - Security layers documented

### Notes
- This version contains planning and documentation only
- No code implementation yet
- Database schema fully analyzed (see implementation.md for details)
- Ready to begin Phase 1: Foundation (Laravel installation)

---

## Version History

| Version | Date | Description |
|---------|------|-------------|
| 1.0.0 | 2025-11-08 | Armor Database Phase 3: Polish & refinements - unified layout, filter redesign, color code stripping, default icons |
| 0.9.0 | 2025-11-08 | Armor Database Phase 2: Complete combat & resistance system (10 sections), modern visual redesign |
| 0.8.0 | 2025-11-08 | Armor Database Phase 1 MVP: Enhanced list view with filters, comprehensive detail view (5 sections) |
| 0.7.0 | 2025-11-08 | Game Icon Integration: All 10 placeholder icons replaced with authentic Lineage sprites |
| 0.6.0 | 2025-11-08 | Card Layout Redesign: Title-icon-count structure with live database counts |
| 0.5.0 | 2025-11-08 | UI Polish: Server status banner, renamed stats, icon placeholders, category cards redesign |
| 0.4.0 | 2025-11-08 | Major Redesign: Database-centric homepage, 6 category database system, removed character features |
| 0.3.0 | 2025-11-08 | Phase 2 Complete: Public website core with all main pages (Home, Rankings, Items, Characters) |
| 0.2.0 | 2025-11-08 | Phase 1 Complete: Laravel installation, database setup, base models |
| 0.1.0 | 2025-11-08 | Initial project setup and documentation |

---

## Upcoming Milestones

### v0.2.0 - Foundation (Target: Week 1)
- Laravel 11 installation
- Database connections configured
- Base models created
- Authentication system implemented

### v0.3.0 - Public Website (Target: Week 2-3)
- Homepage with server stats
- Rankings system
- Item database browser
- Character lookup
- Sprite icon system

### v0.4.0 - Admin Panel Phase 1 (Target: Week 3-4)
- Admin dashboard
- Character management
- Item management
- Activity logging

### v0.5.0 - Admin Panel Phase 2 (Target: Week 4-5)
- Server statistics
- Log viewer
- Analytics
- Export tools

### v1.0.0 - Production Ready (Target: Week 6)
- Performance optimization
- Security hardening
- Complete testing
- Production deployment guide

---

## Development Notes

### Change Categories

Changes are grouped into the following categories:

- **Added**: New features
- **Changed**: Changes to existing functionality
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features
- **Fixed**: Bug fixes
- **Security**: Security improvements

### Version Numbering

- **Major version** (X.0.0): Breaking changes or major feature releases
- **Minor version** (0.X.0): New features, backwards compatible
- **Patch version** (0.0.X): Bug fixes and minor improvements

---

**Last Updated**: 2025-11-08
