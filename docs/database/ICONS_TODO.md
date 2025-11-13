# Game Icons Integration Guide

This document tracks the locations where placeholder icons/emojis need to be replaced with actual Lineage game sprites.

## Icon Locations

### Homepage (`resources/views/home.blade.php`)

#### Server Statistics Icons
Replace letter placeholders with game sprites:

1. **Online Players** (Line ~45)
   - Current: `<span class="text-game-blue text-2xl font-bold">P</span>`
   - Replace with: Game sprite for player/character icon
   - Suggested path: `public/images/icons/player.png`

2. **Total Accounts** (Line ~61)
   - Current: `<span class="text-game-green text-2xl font-bold">A</span>`
   - Replace with: Game sprite for account/user icon
   - Suggested path: `public/images/icons/account.png`

3. **Total Clans** (Line ~77)
   - Current: `<span class="text-game-gold text-2xl font-bold">C</span>`
   - Replace with: Game sprite for clan/pledge icon
   - Suggested path: `public/images/icons/clan.png`

4. **Highest Level** (Line ~93)
   - Current: `<span class="text-game-accent text-2xl font-bold">L</span>`
   - Replace with: Game sprite for level/rank icon
   - Suggested path: `public/images/icons/level.png`

#### Database Category Icons
Replace emoji placeholders with game sprites:

1. **Armor** (Line ~159)
   - Current: `<span class="text-game-blue text-6xl font-bold">🛡️</span>`
   - Replace with: Game sprite for armor/shield
   - Suggested path: `public/images/icons/armor.png`

2. **Weapons** (Line ~172)
   - Current: `<span class="text-game-red text-6xl font-bold">⚔️</span>`
   - Replace with: Game sprite for weapon/sword
   - Suggested path: `public/images/icons/weapon.png`

3. **Items** (Line ~185)
   - Current: `<span class="text-game-green text-6xl font-bold">💎</span>`
   - Replace with: Game sprite for items/potion
   - Suggested path: `public/images/icons/item.png`

4. **Monsters** (Line ~198)
   - Current: `<span class="text-game-accent text-6xl font-bold">👹</span>`
   - Replace with: Game sprite for monster
   - Suggested path: `public/images/icons/monster.png`

5. **NPCs** (Line ~211)
   - Current: `<span class="text-game-gold text-6xl font-bold">👤</span>`
   - Replace with: Game sprite for NPC/merchant
   - Suggested path: `public/images/icons/npc.png`

6. **Maps** (Line ~224)
   - Current: `<span class="text-game-blue text-6xl font-bold">🗺️</span>`
   - Replace with: Game sprite for map/scroll
   - Suggested path: `public/images/icons/map.png`

## Implementation Instructions

### Option 1: Using IMG Tags
Replace the placeholder divs with image tags:

```blade
<!-- Example for Armor icon -->
<img src="{{ asset('images/icons/armor.png') }}" alt="Armor" class="w-32 h-32 object-contain">
```

### Option 2: Using Background Images
Keep the div structure and use CSS background:

```blade
<!-- Example for Armor icon -->
<div class="w-32 h-32 bg-contain bg-center bg-no-repeat"
     style="background-image: url('{{ asset('images/icons/armor.png') }}');">
</div>
```

### Option 3: Using Sprite Sheets (Recommended for Performance)
If using a sprite sheet:

1. Place sprite sheet at `public/images/sprites.png`
2. Define CSS classes in `resources/css/app.css`:

```css
.sprite {
    background-image: url('/images/sprites.png');
    background-repeat: no-repeat;
    display: inline-block;
}

.sprite-armor { background-position: 0 0; width: 32px; height: 32px; }
.sprite-weapon { background-position: -32px 0; width: 32px; height: 32px; }
/* etc. */
```

3. Use in Blade templates:

```blade
<div class="sprite sprite-armor scale-[4]"></div>
```

## Icon Requirements

- **Format**: PNG with transparency (recommended) or WebP
- **Size**:
  - Server stats icons: 64x64px minimum
  - Category icons: 128x128px minimum
- **Style**: Should match Lineage game aesthetic
- **Source**: Extract from client `.pak` files or use official sprites

## Extraction from Client

Icons can be extracted from:
- `LineageWarriorClient/UI.pak` - UI elements
- `LineageWarriorClient/Item.pak` - Item sprites
- `LineageWarriorClient/NPC.pak` - NPC/Monster sprites

Use a `.pak` file extractor tool to access the sprites.

## Notes

- All placeholders have been marked with HTML comments: `<!-- Game icon placeholder - replace with actual [type] sprite -->`
- Icon sizes can be adjusted using Tailwind classes: `w-{size} h-{size}`
- Colors can be overlaid using Tailwind filters if needed
- Consider using lazy loading for performance: `loading="lazy"`
