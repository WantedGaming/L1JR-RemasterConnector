# L1R Database Website - Implementation Guide

**Project**: Laravel-based CRUD application and public website for L1R (Lineage Remastered) game server
**Framework**: Laravel 11 with PHP 8.2+
**Database**: MySQL (XAMPP local testing)
**Theme**: Modern dark/black theme
**Created**: 2025-11-08
**Last Updated**: 2025-11-08

---

## Table of Contents

1. [Project Overview](#project-overview)
2. [Technology Stack](#technology-stack)
3. [Database Architecture](#database-architecture)
4. [Application Architecture](#application-architecture)
5. [Authentication Strategy](#authentication-strategy)
6. [Security Implementation](#security-implementation)
7. [Implementation Roadmap](#implementation-roadmap)
8. [Development Guidelines](#development-guidelines)

---

## Project Overview

### Purpose

Create a modern web interface for the L1J-WantedServer game that provides:

- **Public Website**: Server statistics, character rankings, item database (no login required)
- **Admin Panel**: Full CRUD operations for game management (requires authentication)

### Key Requirements

- Use **existing game database tables only** (no new SQL files or migrations on game DB)
- **Dark theme** throughout the entire website
- **No FontAwesome or emoji icons** - use game sprite icons from client assets
- **XAMPP** for local development with future public deployment capability
- Connect to existing `l1jdb` database (L1J game database)

---

## Technology Stack

### Backend

- **Laravel**: 11.x (latest stable)
- **PHP**: 8.2 or higher
- **Database**: MySQL 8.0+ via XAMPP

### Frontend

- **CSS Framework**: Tailwind CSS 3.4+ (dark theme optimized)
- **JavaScript**: Alpine.js 3.x (lightweight reactivity)
- **Build Tool**: Vite 5.x (Laravel default)
- **Icons**: Custom game sprite system (extracted from .pak files)

### Additional Packages

**Composer Dependencies**:
```json
{
  "spatie/laravel-permission": "^6.0",
  "spatie/laravel-query-builder": "^5.0",
  "spatie/laravel-activitylog": "^4.0",
  "laravel/sanctum": "^4.0"
}
```

**NPM Dependencies**:
```json
{
  "@tailwindcss/forms": "^0.5",
  "@tailwindcss/typography": "^0.5",
  "alpinejs": "^3.0"
}
```

---

## Database Architecture

### Database Summary

The L1J game database (`l1jdb`) contains **342 core tables** organized into the following categories:

| Category | Table Count | Key Tables |
|----------|-------------|------------|
| User/Account | 11 | `accounts`, `ban_account`, `ban_ip` |
| Character | 33 | `characters`, `character_items`, `character_quests` |
| Clan/Guild | 11 | `clan_data`, `clan_warehouse` |
| Items | 24 | `etcitem`, `weapon`, `armor`, `item_common` |
| NPCs/World | 48 | `npc`, `spawnlist`, `droplist` |
| Quests | 14 | `hunting_quest`, `beginner_quest` |
| Dungeons | 6 | `dungeon`, `dungeon_timer` |
| Crafting | 6 | `craft_info`, `craft_npcs` |
| Logs | 12 | `log_chat`, `log_enchant`, `log_shop` |
| Web/Shop | 20 | `app_nshop`, `app_customer`, `shop` |

### Multi-Database Strategy

The application uses **two database connections** for security:

**1. Laravel Application Database** (`mysql`)
- Connection: Default Laravel connection
- Database: `laravel_l1r` (separate from game DB)
- Purpose: Sessions, cache, Laravel-specific tables
- Tables: `sessions`, `cache`, `activity_log`, etc.

**2. Game Database - Admin Access** (`mysql_game`)
- Database: `l1jdb` (existing L1J database)
- Credentials: Full read-write access
- Purpose: Admin panel CRUD operations
- User: `root` or `l1j_admin`

**3. Game Database - Public Read-Only** (`mysql_game_public`)
- Database: `l1jdb` (same database)
- Credentials: SELECT-only MySQL user
- Purpose: Public website queries (rankings, lookups)
- User: `l1j_readonly` (to be created)

### Database Connection Configuration

**File**: `config/database.php`

```php
'connections' => [
    // Laravel app database (default)
    'mysql' => [
        'driver' => 'mysql',
        'host' => env('DB_HOST', '127.0.0.1'),
        'database' => env('DB_DATABASE', 'laravel_l1r'),
        'username' => env('DB_USERNAME', 'root'),
        'password' => env('DB_PASSWORD', ''),
        'charset' => 'utf8mb4',
        'collation' => 'utf8mb4_unicode_ci',
    ],

    // Game database - Admin access
    'mysql_game' => [
        'driver' => 'mysql',
        'host' => env('GAME_DB_HOST', '127.0.0.1'),
        'database' => env('GAME_DB_DATABASE', 'l1jdb'),
        'username' => env('GAME_DB_USERNAME', 'root'),
        'password' => env('GAME_DB_PASSWORD', ''),
        'charset' => 'utf8mb4',
        'strict' => false, // L1J may have non-standard schema
    ],

    // Game database - Public read-only
    'mysql_game_public' => [
        'driver' => 'mysql',
        'host' => env('GAME_DB_PUBLIC_HOST', '127.0.0.1'),
        'database' => env('GAME_DB_PUBLIC_DATABASE', 'l1jdb'),
        'username' => env('GAME_DB_PUBLIC_USERNAME', 'l1j_readonly'),
        'password' => env('GAME_DB_PUBLIC_PASSWORD', ''),
        'charset' => 'utf8mb4',
        'strict' => false,
        'options' => [
            PDO::MYSQL_ATTR_INIT_COMMAND => "SET SESSION TRANSACTION READ ONLY"
        ],
    ],
];
```

### Key Game Tables Reference

**Priority Tables for Phase 1**:

| Table Name | Primary Key | Purpose | Connection |
|------------|-------------|---------|------------|
| `accounts` | `login` (VARCHAR) | User accounts | Admin |
| `characters` | `objid` (INT) | Player characters | Both |
| `character_items` | `item_id` (INT) | Character inventory | Admin |
| `clan_data` | `clan_id` (INT) | Clan/guild information | Both |
| `etcitem` | `item_id` (INT) | Item definitions | Both |
| `weapon` | `item_id` (INT) | Weapon definitions | Both |
| `armor` | `item_id` (INT) | Armor definitions | Both |
| `npc` | `npcid` (INT) | NPC templates | Both |
| `spawnlist` | `id` (INT) | NPC spawn locations | Admin |
| `droplist` | `id` (INT) | NPC drop tables | Admin |

---

## Application Architecture

### Directory Structure

```
L1RDatabase/
├── app/
│   ├── Console/Commands/
│   │   └── ValidateGameSchema.php        # Schema validation command
│   ├── Http/
│   │   ├── Controllers/
│   │   │   ├── Admin/                    # Admin CRUD controllers
│   │   │   │   ├── DashboardController.php
│   │   │   │   ├── CharacterController.php
│   │   │   │   ├── ItemController.php
│   │   │   │   └── NpcController.php
│   │   │   ├── Auth/
│   │   │   │   └── GameLoginController.php
│   │   │   └── Public/                   # Public website controllers
│   │   │       ├── HomeController.php
│   │   │       ├── RankingsController.php
│   │   │       └── ServerStatusController.php
│   │   ├── Middleware/
│   │   │   ├── GameAdminAuth.php         # Admin access control
│   │   │   └── ReadOnlyGameDB.php        # Prevent game DB writes
│   │   └── Requests/
│   │       └── Admin/                    # Form validation
│   ├── Models/
│   │   ├── Game/                         # Game DB models (admin)
│   │   │   ├── GameModel.php             # Base model
│   │   │   ├── Account.php
│   │   │   ├── Character.php
│   │   │   ├── CharacterItem.php
│   │   │   └── Clan.php
│   │   ├── GamePublic/                   # Read-only models
│   │   │   ├── GamePublicModel.php
│   │   │   └── Character.php
│   │   └── User.php                      # Laravel users (optional)
│   ├── Repositories/                     # Query abstraction layer
│   │   ├── CharacterRepository.php
│   │   ├── RankingRepository.php
│   │   └── ItemRepository.php
│   ├── Services/
│   │   ├── CharacterService.php
│   │   ├── RankingService.php
│   │   └── TranslationService.php        # Handles desc-e.tbl
│   └── View/Components/
│       ├── AdminLayout.php
│       ├── PublicLayout.php
│       └── GameIcon.php                  # Sprite icon component
├── config/
│   ├── database.php                      # Multi-DB configuration
│   ├── auth.php                          # Custom auth guard
│   └── game.php                          # Game-specific settings
├── database/
│   ├── migrations/                       # Laravel app only
│   └── schemas/                          # Game DB documentation
│       ├── characters.md
│       └── accounts.md
├── public/
│   ├── assets/
│   │   ├── sprites/                      # Extracted game icons
│   │   ├── css/
│   │   └── js/
│   └── index.php
├── resources/
│   ├── css/
│   │   └── app.css                       # Tailwind dark theme
│   ├── js/
│   │   └── app.js                        # Alpine.js
│   └── views/
│       ├── layouts/
│       │   ├── public.blade.php          # Public layout
│       │   └── admin.blade.php           # Admin layout
│       ├── public/
│       │   ├── home.blade.php
│       │   ├── rankings.blade.php
│       │   └── server-status.blade.php
│       └── admin/
│           ├── dashboard.blade.php
│           ├── characters/
│           └── items/
├── routes/
│   └── web.php                           # Public + Admin routes
├── .env
├── composer.json
└── package.json
```

### Model Architecture

**Base Game Model** (`app/Models/Game/GameModel.php`):
```php
abstract class GameModel extends Model
{
    public $timestamps = false;           // L1J doesn't use created_at/updated_at
    protected $connection = 'mysql_game'; // Use game database
    protected $guarded = [];              // Careful with mass assignment
    protected $dateFormat = 'Y-m-d H:i:s';
}
```

**Read-Only Public Model** (`app/Models/GamePublic/GamePublicModel.php`):
```php
abstract class GamePublicModel extends Model
{
    public $timestamps = false;
    protected $connection = 'mysql_game_public';
    protected $guarded = [];

    // Prevent writes
    public function save(array $options = [])
    {
        throw new \Exception('Cannot save using read-only model');
    }
}
```

### Repository Pattern

**Purpose**: Centralize complex queries and provide caching layer

**Example** (`app/Repositories/RankingRepository.php`):
```php
class RankingRepository
{
    public function getTopPlayersByLevel($limit = 100)
    {
        return Cache::remember('rankings:top_players', 300, function () use ($limit) {
            return DB::connection('mysql_game_public')
                ->table('characters')
                ->select('char_name', 'level', 'Exp', 'ClanName')
                ->where('AccessLevel', 0) // Exclude GMs
                ->orderByDesc('level')
                ->orderByDesc('Exp')
                ->limit($limit)
                ->get();
        });
    }
}
```

---

## Authentication Strategy

### Custom Game Account Authentication

**Challenge**: L1J stores accounts in the `accounts` table with non-bcrypt passwords.

**Solution**: Custom authentication guard using existing accounts table.

**Configuration** (`config/auth.php`):
```php
'guards' => [
    'web' => [
        'driver' => 'session',
        'provider' => 'game_accounts',
    ],
],

'providers' => [
    'game_accounts' => [
        'driver' => 'eloquent',
        'model' => App\Models\Game\Account::class,
    ],
],
```

**Account Model** (`app/Models/Game/Account.php`):
```php
class Account extends Authenticatable
{
    protected $connection = 'mysql_game';
    protected $table = 'accounts';
    protected $primaryKey = 'login'; // Username is primary key
    public $incrementing = false;
    protected $keyType = 'string';

    public function getAuthPassword()
    {
        return $this->password; // Return raw password (L1J handles hashing)
    }

    public function isAdmin()
    {
        return $this->AccessLevel >= config('game.admin_level', 100);
    }
}
```

### Admin Access Control

**Middleware** (`app/Http/Middleware/GameAdminAuth.php`):
```php
public function handle($request, Closure $next)
{
    if (!auth()->check()) {
        return redirect('/login');
    }

    if (!auth()->user()->isAdmin()) {
        abort(403, 'Unauthorized - Admin access required');
    }

    return $next($request);
}
```

**Route Protection**:
```php
Route::prefix('admin')->middleware(['auth', 'game.admin'])->group(function () {
    Route::get('/', [DashboardController::class, 'index']);
    Route::resource('characters', CharacterController::class);
    // ...
});
```

---

## Security Implementation

### 1. Database Security

**Read-Only Enforcement**:
- MySQL user `l1j_readonly` with SELECT-only grants
- PDO connection option: `SET SESSION TRANSACTION READ ONLY`
- Model-level guards in `GamePublicModel`
- Middleware to block write operations on public routes

**SQL Injection Prevention**:
- Always use Eloquent or query builder with parameter binding
- Never use raw queries with user input
- Validate all input with Laravel's validation system

### 2. Authentication Security

**Password Handling**:
- **Do not modify** game account passwords
- Use separate Laravel authentication for website-only features
- Never expose game passwords in web responses

**Session Security** (`config/session.php`):
```php
'lifetime' => 120,
'expire_on_close' => true,
'secure' => env('SESSION_SECURE_COOKIE', true),
'http_only' => true,
'same_site' => 'lax',
```

### 3. Admin Panel Security

**Multi-Layer Protection**:
1. Session authentication (user logged in)
2. Admin level check (`AccessLevel >= 100`)
3. Activity logging (all admin actions tracked)
4. Rate limiting (prevent brute force)

**Activity Logging**:
```php
activity()
    ->performedOn($character)
    ->causedBy(auth()->user())
    ->withProperties(['old_level' => 50, 'new_level' => 99])
    ->log('Character level modified');
```

### 4. Input Validation

**Form Requests**:
```php
// app/Http/Requests/Admin/UpdateCharacterRequest.php
public function rules()
{
    return [
        'char_name' => 'required|string|max:16|alpha_dash',
        'level' => 'required|integer|min:1|max:99',
        'hp' => 'required|integer|min:1',
    ];
}
```

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1)

**Goals**: Set up Laravel project and database connections

**Tasks**:
- [ ] Install Laravel 11 in `L1RDatabase` directory
- [ ] Configure `.env` with multiple database connections
- [ ] Create MySQL read-only user (`l1j_readonly`)
- [ ] Test database connections with Tinker
- [ ] Document game database schema in `database/schemas/`
- [ ] Create base `GameModel` and `GamePublicModel` classes

**Deliverables**:
- Working Laravel installation
- Multi-database configuration
- Schema documentation

---

### Phase 2: Authentication & Base Layout (Week 1-2)

**Goals**: Implement authentication and dark theme layout

**Tasks**:
- [ ] Install Tailwind CSS with dark theme configuration
- [ ] Create base layouts (public, admin)
- [ ] Implement custom game account authentication
- [ ] Create login page with dark theme
- [ ] Add admin authentication middleware
- [ ] Set up Alpine.js for interactivity

**Deliverables**:
- Login system using game accounts
- Dark-themed public and admin layouts
- Authentication middleware

---

### Phase 3: Public Website (Week 2-3)

**Goals**: Build public-facing pages

**Tasks**:
- [ ] Create homepage with server statistics
- [ ] Implement server status page (online players, uptime)
- [ ] Build rankings system (by level, PVP, wealth)
- [ ] Create item database browser
- [ ] Add character lookup feature
- [ ] Extract and implement game sprite icon system

**Deliverables**:
- Functional public website
- Rankings and statistics
- Game sprite icons working

---

### Phase 4: Admin Panel - Phase 1 (Week 3-4)

**Goals**: Core admin CRUD functionality

**Tasks**:
- [ ] Create admin dashboard with overview stats
- [ ] Implement character management (list, view, edit)
- [ ] Build item management interface
- [ ] Add NPC management (view spawns/drops)
- [ ] Implement activity logging
- [ ] Add search and filtering

**Deliverables**:
- Working admin panel
- Character and item CRUD
- Activity audit trail

---

### Phase 5: Admin Panel - Phase 2 (Week 4-5)

**Goals**: Advanced admin features

**Tasks**:
- [ ] Server statistics and monitoring
- [ ] Economy analytics
- [ ] Log viewer (chat, commands, enchantments)
- [ ] Export/import tools
- [ ] Configuration viewer (rates, settings)

**Deliverables**:
- Complete admin panel
- Analytics and reporting
- Log viewing capabilities

---

### Phase 6: Optimization & Testing (Week 5-6)

**Goals**: Performance optimization and security hardening

**Tasks**:
- [ ] Implement Redis caching strategy
- [ ] Optimize database queries (eager loading, indexing)
- [ ] Add rate limiting to all routes
- [ ] Security audit (SQL injection, XSS, CSRF)
- [ ] Load testing with realistic traffic
- [ ] Write feature tests for critical paths

**Deliverables**:
- Optimized performance
- Security hardening complete
- Test coverage for core features

---

## Development Guidelines

### Code Style

- Follow **PSR-12** coding standards
- Use **Laravel Pint** for automatic formatting: `./vendor/bin/pint`
- Use **PHPDoc** comments for all public methods
- Use **type hints** for all method parameters and return types

### Git Workflow

```bash
# Branch naming
feature/character-management
fix/ranking-query-performance
docs/implementation-guide

# Commit messages
feat: Add character CRUD operations
fix: Resolve N+1 query in rankings
docs: Update implementation roadmap
```

### Testing Strategy

**Unit Tests**: Repository and service classes
```bash
php artisan test --filter=CharacterRepositoryTest
```

**Feature Tests**: Controllers and routes
```bash
php artisan test --filter=RankingControllerTest
```

**Manual Testing Checklist**:
- [ ] Admin login with game account
- [ ] Public rankings load correctly
- [ ] Character search finds results
- [ ] Admin character edit saves changes
- [ ] Activity log records admin actions
- [ ] Read-only connection prevents writes

### Performance Guidelines

**Query Optimization**:
- Always use eager loading: `Character::with('clan')->get()`
- Paginate large result sets: `->paginate(100)`
- Add query timeouts: `->timeout(5)`
- Use database indexes for filter columns

**Caching Strategy**:
| Data Type | TTL | Cache Key Pattern |
|-----------|-----|-------------------|
| Rankings | 5 min | `rankings:top_players` |
| Server status | 1 min | `server:status` |
| Item definitions | 24 hours | `items:definition:{id}` |
| Character profile | 10 min | `character:profile:{name}` |

### Security Checklist

**Before Deploying**:
- [ ] All user inputs validated
- [ ] CSRF protection enabled on forms
- [ ] SQL injection tests passed
- [ ] XSS protection verified
- [ ] Rate limiting configured
- [ ] HTTPS enforced in production
- [ ] `.env` file not committed to git
- [ ] Database credentials rotated
- [ ] Admin actions logged
- [ ] Error messages don't expose sensitive data

---

## Environment Setup

### XAMPP Configuration

**Requirements**:
- PHP 8.2 or higher
- MySQL 8.0 or higher
- Composer installed
- Node.js 18+ and npm

**Database Setup**:
```sql
-- Create Laravel app database
CREATE DATABASE laravel_l1r CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Create read-only user for public access
CREATE USER 'l1j_readonly'@'localhost' IDENTIFIED BY 'secure_password';
GRANT SELECT ON l1jdb.* TO 'l1j_readonly'@'localhost';
FLUSH PRIVILEGES;
```

**.env Configuration**:
```env
APP_NAME="L1R Database"
APP_ENV=local
APP_DEBUG=true
APP_URL=http://localhost:8000

# Laravel Database
DB_CONNECTION=mysql
DB_HOST=127.0.0.1
DB_PORT=3306
DB_DATABASE=laravel_l1r
DB_USERNAME=root
DB_PASSWORD=

# Game Database - Admin
GAME_DB_HOST=127.0.0.1
GAME_DB_DATABASE=l1jdb
GAME_DB_USERNAME=root
GAME_DB_PASSWORD=

# Game Database - Public
GAME_DB_PUBLIC_HOST=127.0.0.1
GAME_DB_PUBLIC_DATABASE=l1jdb
GAME_DB_PUBLIC_USERNAME=l1j_readonly
GAME_DB_PUBLIC_PASSWORD=secure_password

# Cache (use Redis in production)
CACHE_DRIVER=file
SESSION_DRIVER=file

# Game Settings
GAME_ADMIN_LEVEL=100
```

**Installation Steps**:
```bash
cd "D:\L1R Project\L1RDatabase"

# Install PHP dependencies
composer install

# Install Node dependencies
npm install

# Generate application key
php artisan key:generate

# Run Laravel development server
php artisan serve

# In another terminal, run Vite
npm run dev
```

---

## Notes

- This implementation guide will be updated as development progresses
- See `changelog.md` for version history and updates
- All game database modifications must be done through the game server
- This web application is **read-only** for public users, **admin-controlled** for management
- Sprite icons will be extracted from game client `.pak` files and stored in `public/assets/sprites/`

---

**Document Version**: 1.0
**Last Updated**: 2025-11-08
**Next Review**: After Phase 1 completion
