# APEX ARENA: FUTURE FORGE
## Database Schema - مخطط قاعدة البيانات
### الإصدار: 1.0 | 2026-08-24

---

## 1. نظرة عامة (Overview)

**محرك قاعدة البيانات:** PostgreSQL 15+
**نظام التخزين المؤقت:** Redis (للحالة المباشرة)
**التخزين السحابي:** AWS S3 (للأصول والسجلات)

**الأنظمة المدعومة:**
- بيانات اللاعبين (Player Data)
- حالة المباريات (Match State)
- البيئة الديناميكية (Dynamic Environment)
- التقنيات والتطور (Tech & Progression)
- التحالفات والعلاقات (Alliances)
- السجلات والتحليلات (Analytics)

---

## 2. جداول اللاعبين (Player Tables)

### 2.1 players - اللاعبون

```sql
CREATE TABLE players (
    player_id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username            VARCHAR(32) UNIQUE NOT NULL,
    display_name        VARCHAR(64),
    email               VARCHAR(255) UNIQUE NOT NULL,
    password_hash       VARCHAR(255) NOT NULL,
    region              VARCHAR(10) DEFAULT 'ME',
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_login          TIMESTAMP,
    account_status      VARCHAR(20) DEFAULT 'active', -- active, suspended, banned
    premium_status      VARCHAR(20) DEFAULT 'free',   -- free, premium, vip
    total_playtime      INTEGER DEFAULT 0,            -- بالدقائق
    reputation_score    INTEGER DEFAULT 50,           -- 0-100
    skill_rating        INTEGER DEFAULT 1000,         -- ELO rating
    season_rank         VARCHAR(20) DEFAULT 'unranked',
    avatar_url          VARCHAR(500),

    CONSTRAINT chk_username CHECK (username ~ '^[a-zA-Z0-9_]{3,32}$'),
    CONSTRAINT chk_reputation CHECK (reputation_score BETWEEN 0 AND 100)
);

CREATE INDEX idx_players_username ON players(username);
CREATE INDEX idx_players_region ON players(region);
CREATE INDEX idx_players_skill ON players(skill_rating);
```

### 2.2 player_stats - إحصائيات اللاعب

```sql
CREATE TABLE player_stats (
    stat_id             UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id           UUID REFERENCES players(player_id) ON DELETE CASCADE,
    total_matches       INTEGER DEFAULT 0,
    wins                INTEGER DEFAULT 0,
    losses              INTEGER DEFAULT 0,
    draws               INTEGER DEFAULT 0,
    kills               INTEGER DEFAULT 0,
    deaths              INTEGER DEFAULT 0,
    assists             INTEGER DEFAULT 0,
    damage_dealt        BIGINT DEFAULT 0,
    damage_taken        BIGINT DEFAULT 0,
    techs_unlocked      INTEGER DEFAULT 0,
    techs_activated     INTEGER DEFAULT 0,
    alliances_formed    INTEGER DEFAULT 0,
    alliances_betrayed  INTEGER DEFAULT 0,
    zones_captured      INTEGER DEFAULT 0,
    resources_gathered  BIGINT DEFAULT 0,
    longest_survival    INTEGER DEFAULT 0,            -- بالثواني
    favorite_zone       VARCHAR(50),
    favorite_tech       VARCHAR(50),
    updated_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

    UNIQUE(player_id)
);

CREATE INDEX idx_player_stats_player ON player_stats(player_id);
CREATE INDEX idx_player_stats_wins ON player_stats(wins DESC);
```

### 2.3 player_inventory - مخزون اللاعب

```sql
CREATE TABLE player_inventory (
    inventory_id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id           UUID REFERENCES players(player_id) ON DELETE CASCADE,
    item_type           VARCHAR(50) NOT NULL,         -- tech, cosmetic, resource, weapon
    item_id             VARCHAR(100) NOT NULL,
    quantity            INTEGER DEFAULT 1,
    acquired_at         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at          TIMESTAMP,                    -- للعناصر المؤقتة
    is_equipped         BOOLEAN DEFAULT FALSE,
    metadata            JSONB,                        -- بيانات إضافية مرنة

    UNIQUE(player_id, item_id, item_type)
);

CREATE INDEX idx_inventory_player ON player_inventory(player_id);
CREATE INDEX idx_inventory_type ON player_inventory(item_type);
```

---

## 3. جداول المباريات (Match Tables)

### 3.1 matches - المباريات

```sql
CREATE TABLE matches (
    match_id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    match_type          VARCHAR(30) NOT NULL,         -- solo, duo, squad, event
    match_status        VARCHAR(20) DEFAULT 'pending', -- pending, active, ended, cancelled
    map_seed            VARCHAR(100),                 -- بذور الخريطة الديناميكية
    max_players         INTEGER DEFAULT 100,
    current_players     INTEGER DEFAULT 0,
    start_time          TIMESTAMP,
    end_time            TIMESTAMP,
    duration_seconds    INTEGER,
    winner_id           UUID REFERENCES players(player_id),
    winning_alliance    UUID,                         -- للمباريات الجماعية
    environment_state   JSONB,                        -- حالة البيئة الديناميكية
    algorithm_version   VARCHAR(20) DEFAULT '2.4.1',
    server_region       VARCHAR(20),
    server_id           VARCHAR(100),

    CONSTRAINT chk_max_players CHECK (max_players BETWEEN 2 AND 200)
);

CREATE INDEX idx_matches_status ON matches(match_status);
CREATE INDEX idx_matches_start ON matches(start_time DESC);
CREATE INDEX idx_matches_winner ON matches(winner_id);
```

### 3.2 match_players - لاعبو المباراة

```sql
CREATE TABLE match_players (
    match_player_id     UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    match_id            UUID REFERENCES matches(match_id) ON DELETE CASCADE,
    player_id           UUID REFERENCES players(player_id),
    team_id             UUID,                         -- للمباريات الجماعية
    placement           INTEGER,                      -- المركز النهائي
    kills               INTEGER DEFAULT 0,
    damage_dealt        INTEGER DEFAULT 0,
    damage_taken        INTEGER DEFAULT 0,
    survival_time       INTEGER DEFAULT 0,            -- بالثواني
    techs_used          JSONB,                        -- قائمة التقنيات المستخدمة
    zones_visited       JSONB,                        -- المناطق التي زارها
    alliance_history    JSONB,                        -- تاريخ التحالفات
    resources_collected JSONB,                        -- الموارد المجمعة
    final_stats         JSONB,                        -- إحصائيات نهائية
    disconnected        BOOLEAN DEFAULT FALSE,
    disconnect_time     TIMESTAMP,

    UNIQUE(match_id, player_id)
);

CREATE INDEX idx_match_players_match ON match_players(match_id);
CREATE INDEX idx_match_players_player ON match_players(player_id);
CREATE INDEX idx_match_players_placement ON match_players(placement);
```

### 3.3 match_events - أحداث المباراة

```sql
CREATE TABLE match_events (
    event_id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    match_id            UUID REFERENCES matches(match_id) ON DELETE CASCADE,
    event_type          VARCHAR(50) NOT NULL,         -- kill, alliance, tech, zone_change, etc.
    event_time          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    player_id           UUID REFERENCES players(player_id),
    target_id           UUID REFERENCES players(player_id),
    zone_id             VARCHAR(50),
    tech_id             VARCHAR(50),
    event_data          JSONB,                        -- بيانات الحدث التفصيلية

    CONSTRAINT chk_event_type CHECK (event_type IN (
        'kill', 'death', 'alliance_formed', 'alliance_broken', 
        'tech_unlocked', 'tech_activated', 'zone_entered', 'zone_left',
        'resource_collected', 'resource_spent', 'match_start', 'match_end'
    ))
);

CREATE INDEX idx_match_events_match ON match_events(match_id);
CREATE INDEX idx_match_events_type ON match_events(event_type);
CREATE INDEX idx_match_events_time ON match_events(event_time);
```

---

## 4. جداول البيئة (Environment Tables)

### 4.1 zones - المناطق

```sql
CREATE TABLE zones (
    zone_id             VARCHAR(50) PRIMARY KEY,
    zone_name_ar        VARCHAR(100) NOT NULL,
    zone_name_en        VARCHAR(100) NOT NULL,
    zone_type           VARCHAR(30) NOT NULL,         -- industrial, military, forest, danger
    base_difficulty     INTEGER DEFAULT 1,            -- 1-10
    resource_richness   INTEGER DEFAULT 5,            -- 1-10
    tech_density        INTEGER DEFAULT 5,            -- 1-10
    max_players         INTEGER DEFAULT 25,
    terrain_complexity  INTEGER DEFAULT 5,            -- 1-10
    description         TEXT,
    base_layout         JSONB,                        -- تخطيط المنطقة الأساسي

    CONSTRAINT chk_difficulty CHECK (base_difficulty BETWEEN 1 AND 10)
);

INSERT INTO zones (zone_id, zone_name_ar, zone_name_en, zone_type, base_difficulty, resource_richness, tech_density, max_players, terrain_complexity, description) VALUES
('industrial', 'منطقة صناعية', 'Industrial Zone', 'industrial', 5, 9, 7, 30, 8, 'مصانع مهجورة مع تقنيات متقدمة'),
('military', 'ساحة عسكرية', 'Military Arena', 'military', 8, 6, 9, 25, 6, 'تحصينات عسكرية مع أسلحة متطورة'),
('forest', 'غابة مزدهرة', 'Forest Zone', 'forest', 3, 7, 4, 20, 9, 'غابة هجينة مع موارد حيوية'),
('danger', 'منطقة خطرة', 'Danger Zone', 'danger', 10, 3, 5, 15, 7, 'منطقة مشعة مع مكافآت ضخمة');
```

### 4.2 zone_mutations - تحولات المناطق

```sql
CREATE TABLE zone_mutations (
    mutation_id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    zone_id             VARCHAR(50) REFERENCES zones(zone_id),
    match_id            UUID REFERENCES matches(match_id) ON DELETE CASCADE,
    mutation_type       VARCHAR(50) NOT NULL,         -- terrain, weather, hazard, resource
    mutation_data       JSONB NOT NULL,               -- تفاصيل التحول
    start_time          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    end_time            TIMESTAMP,
    is_active           BOOLEAN DEFAULT TRUE,
    affected_players    INTEGER DEFAULT 0,

    CONSTRAINT chk_mutation_type CHECK (mutation_type IN (
        'terrain_shift', 'weather_change', 'hazard_spawn', 
        'resource_burst', 'tech_surge', 'radiation_spike'
    ))
);

CREATE INDEX idx_zone_mutations_zone ON zone_mutations(zone_id);
CREATE INDEX idx_zone_mutations_match ON zone_mutations(match_id);
CREATE INDEX idx_zone_mutations_active ON zone_mutations(is_active);
```

---

## 5. جداول التقنيات (Tech Tables)

### 5.1 technologies - التقنيات

```sql
CREATE TABLE technologies (
    tech_id             VARCHAR(50) PRIMARY KEY,
    tech_name_ar        VARCHAR(100) NOT NULL,
    tech_name_en        VARCHAR(100) NOT NULL,
    tech_type           VARCHAR(30) NOT NULL,         -- combat, defense, utility, movement
    tech_tier           INTEGER DEFAULT 1,            -- 1-5
    base_cost_energy    INTEGER DEFAULT 0,
    base_cost_data      INTEGER DEFAULT 0,
    base_cost_materials INTEGER DEFAULT 0,
    description         TEXT,
    effects             JSONB,                        -- التأثيرات والإحصائيات
    prerequisites       JSONB,                        -- المتطلبات المسبقة
    cooldown_seconds    INTEGER DEFAULT 0,
    duration_seconds    INTEGER DEFAULT 0,
    is_active           BOOLEAN DEFAULT TRUE,

    CONSTRAINT chk_tech_tier CHECK (tech_tier BETWEEN 1 AND 5)
);

INSERT INTO technologies (tech_id, tech_name_ar, tech_name_en, tech_type, tech_tier, base_cost_energy, base_cost_data, description, effects, cooldown_seconds, duration_seconds) VALUES
('adaptive', 'دروع تكيفية', 'Adaptive Armor', 'defense', 1, 0, 0, 'تتكيف مع نوع الضرر الوارد', '{"damage_reduction": 40, "adaptation_speed": "instant"}', 0, 0),
('nanobots', 'نانوبوتات إصلاح', 'Nanobot Repair', 'defense', 2, 450, 200, 'إصلاح ذاتي مستمر', '{"heal_per_second": 5, "max_heal": 50}', 0, 30),
('quantum', 'قفزة كمية', 'Quantum Leap', 'movement', 3, 900, 500, 'انتقال مكاني قصير المدى', '{"range": 50, "phasing": true}', 15, 0),
('overdrive', 'Overload أسلحة', 'Weapon Overdrive', 'combat', 3, 1200, 800, 'مضاعفة قوة الأسلحة', '{"damage_multiplier": 2.0, "self_damage": 10}', 30, 10),
('hologram', 'هولوغرام تكتيكي', 'Tactical Hologram', 'utility', 4, 1500, 1000, 'نسخة وهمية تشتت العدو', '{"duration": 8, "health": 50}', 45, 8);
```

### 5.2 player_techs - تقنيات اللاعب

```sql
CREATE TABLE player_techs (
    player_tech_id      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id           UUID REFERENCES players(player_id) ON DELETE CASCADE,
    tech_id             VARCHAR(50) REFERENCES technologies(tech_id),
    unlocked_at         TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    times_activated     INTEGER DEFAULT 0,
    total_duration      INTEGER DEFAULT 0,            -- بالثواني
    is_favorite         BOOLEAN DEFAULT FALSE,
    custom_config       JSONB,                        -- إعدادات مخصصة

    UNIQUE(player_id, tech_id)
);

CREATE INDEX idx_player_techs_player ON player_techs(player_id);
CREATE INDEX idx_player_techs_tech ON player_techs(tech_id);
```

---

## 6. جداول التحالفات (Alliance Tables)

### 6.1 alliances - التحالفات

```sql
CREATE TABLE alliances (
    alliance_id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    match_id            UUID REFERENCES matches(match_id) ON DELETE CASCADE,
    alliance_name       VARCHAR(100),
    created_at          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    dissolved_at        TIMESTAMP,
    is_active           BOOLEAN DEFAULT TRUE,
    max_members         INTEGER DEFAULT 4,
    current_members     INTEGER DEFAULT 0,
    combined_power      INTEGER DEFAULT 0,
    total_kills         INTEGER DEFAULT 0,

    CONSTRAINT chk_max_members CHECK (max_members BETWEEN 2 AND 10)
);

CREATE INDEX idx_alliances_match ON alliances(match_id);
CREATE INDEX idx_alliances_active ON alliances(is_active);
```

### 6.2 alliance_members - أعضاء التحالف

```sql
CREATE TABLE alliance_members (
    member_id           UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    alliance_id         UUID REFERENCES alliances(alliance_id) ON DELETE CASCADE,
    player_id           UUID REFERENCES players(player_id),
    joined_at           TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    left_at             TIMESTAMP,
    is_active           BOOLEAN DEFAULT TRUE,
    role                VARCHAR(20) DEFAULT 'member', -- leader, member, scout, defender
    contribution_score  INTEGER DEFAULT 0,
    betrayed            BOOLEAN DEFAULT FALSE,

    UNIQUE(alliance_id, player_id, is_active)
);

CREATE INDEX idx_alliance_members_alliance ON alliance_members(alliance_id);
CREATE INDEX idx_alliance_members_player ON alliance_members(player_id);
```

### 6.3 alliance_history - تاريخ التحالفات

```sql
CREATE TABLE alliance_history (
    history_id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id           UUID REFERENCES players(player_id),
    other_player_id     UUID REFERENCES players(player_id),
    match_id            UUID REFERENCES matches(match_id),
    relationship_type   VARCHAR(20) NOT NULL,         -- ally, enemy, neutral
    start_time          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    end_time            TIMESTAMP,
    betrayal            BOOLEAN DEFAULT FALSE,
    betrayal_reason     VARCHAR(255),

    CONSTRAINT chk_relationship CHECK (relationship_type IN ('ally', 'enemy', 'neutral'))
);

CREATE INDEX idx_alliance_history_player ON alliance_history(player_id);
CREATE INDEX idx_alliance_history_other ON alliance_history(other_player_id);
```

---

## 7. جداول الموارد (Resource Tables)

### 7.1 resource_types - أنواع الموارد

```sql
CREATE TABLE resource_types (
    resource_type       VARCHAR(50) PRIMARY KEY,
    resource_name_ar    VARCHAR(100) NOT NULL,
    resource_name_en    VARCHAR(100) NOT NULL,
    base_value          INTEGER DEFAULT 1,
    spawn_rate          DECIMAL(5,2) DEFAULT 1.0,     -- معدل الظهور
    max_stack           INTEGER DEFAULT 999,
    description         TEXT,
    icon_url            VARCHAR(500)
);

INSERT INTO resource_types (resource_type, resource_name_ar, resource_name_en, base_value, spawn_rate, max_stack, description) VALUES
('energy', 'طاقة', 'Energy', 1, 1.5, 9999, 'الطاقة الأساسية لتفعيل التقنيات'),
('data', 'بيانات', 'Data', 2, 1.0, 5000, 'بيانات للبحث التقني'),
('materials', 'مواد', 'Materials', 3, 0.8, 3000, 'مواد للبناء والإصلاح'),
('reputation', 'سمعة', 'Reputation', 10, 0.3, 100, 'نقاط سمعة للتحالفات');
```

### 7.2 resource_spawns - موارد المباريات

```sql
CREATE TABLE resource_spawns (
    spawn_id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    match_id            UUID REFERENCES matches(match_id) ON DELETE CASCADE,
    zone_id             VARCHAR(50) REFERENCES zones(zone_id),
    resource_type       VARCHAR(50) REFERENCES resource_types(resource_type),
    quantity            INTEGER DEFAULT 1,
    spawn_location      JSONB,                        -- {x, y, z}
    spawn_time          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    collected_by        UUID REFERENCES players(player_id),
    collected_at        TIMESTAMP,
    is_active           BOOLEAN DEFAULT TRUE
);

CREATE INDEX idx_resource_spawns_match ON resource_spawns(match_id);
CREATE INDEX idx_resource_spawns_zone ON resource_spawns(zone_id);
CREATE INDEX idx_resource_spawns_active ON resource_spawns(is_active);
```

---

## 8. جداول التحليلات (Analytics Tables)

### 8.1 player_sessions - جلسات اللاعب

```sql
CREATE TABLE player_sessions (
    session_id          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    player_id           UUID REFERENCES players(player_id),
    start_time          TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    end_time            TIMESTAMP,
    duration_seconds    INTEGER,
    matches_played      INTEGER DEFAULT 0,
    peak_skill_rating   INTEGER,
    resources_earned    JSONB,
    techs_unlocked      INTEGER DEFAULT 0,
    platform            VARCHAR(20),                  -- pc, ps5, xbox
    client_version      VARCHAR(20),
    ip_address          INET,
    country             VARCHAR(10)
);

CREATE INDEX idx_sessions_player ON player_sessions(player_id);
CREATE INDEX idx_sessions_start ON player_sessions(start_time DESC);
```

### 8.2 match_analytics - تحليلات المباريات

```sql
CREATE TABLE match_analytics (
    analytics_id        UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    match_id            UUID REFERENCES matches(match_id) ON DELETE CASCADE,
    total_kills         INTEGER DEFAULT 0,
    total_alliances     INTEGER DEFAULT 0,
    total_betrayals     INTEGER DEFAULT 0,
    total_techs_used    INTEGER DEFAULT 0,
    zone_transitions    INTEGER DEFAULT 0,
    avg_survival_time   INTEGER,
    most_active_zone    VARCHAR(50),
    most_used_tech      VARCHAR(50),
    balance_score       DECIMAL(3,2),               -- 0.00-1.00
    fun_score           DECIMAL(3,2),               -- استناداً إلى البيانات
    algorithm_performance JSONB,
    generated_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_analytics_match ON match_analytics(match_id);
CREATE INDEX idx_analytics_generated ON match_analytics(generated_at);
```

---

## 9. Redis Structures - هيا Redis

### 9.1 حالة اللاعب المباشرة

```
player:{player_id}:status -> HASH
  - current_match: match_id
  - current_zone: zone_id
  - hp: 100
  - energy: 850
  - position: {x, y, z}
  - last_update: timestamp

TTL: 300 seconds (5 دقائق بدون نشاط)
```

### 9.2 حالة المباراة المباشرة

```
match:{match_id}:state -> HASH
  - status: active
  - current_players: 47
  - alive_players: 23
  - current_zone: danger
  - algorithm_version: 2.4.1
  - next_mutation: timestamp

match:{match_id}:players -> SET
  - player_id_1
  - player_id_2
  - ...

match:{match_id}:leaderboard -> ZSET
  - player_id_1: 1500 (score)
  - player_id_2: 1200
  - ...

TTL: match_duration + 3600 (ساعة بعد الانتهاء)
```

### 9.3 التحالفات المباشرة

```
match:{match_id}:alliances -> HASH
  - alliance_id_1: "player1,player2,player3"
  - alliance_id_2: "player4,player5"

match:{match_id}:player:{player_id}:allies -> SET
  - player_id_2
  - player_id_3

match:{match_id}:player:{player_id}:enemies -> SET
  - player_id_4
```

### 9.4 الموارد المباشرة

```
match:{match_id}:resources -> GEO
  - {longitude, latitude, resource_id}

match:{match_id}:resource:{resource_id} -> HASH
  - type: energy
  - quantity: 50
  - active: true
```

---

## 10. الأمان والأداء (Security & Performance)

### 10.1 الأمان

```sql
-- تشفير البيانات الحساسة
ALTER TABLE players ALTER COLUMN email TYPE VARCHAR(255) ENCRYPTED;

-- صلاحيات محدودة للتطبيق
CREATE ROLE apex_app WITH LOGIN PASSWORD 'secure_password';
GRANT SELECT, INSERT, UPDATE ON ALL TABLES IN SCHEMA public TO apex_app;
REVOKE DELETE ON players, player_stats FROM apex_app;

-- تدقيق (Audit)
CREATE TABLE audit_log (
    audit_id            UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    table_name          VARCHAR(50),
    record_id           UUID,
    action              VARCHAR(20),                  -- INSERT, UPDATE, DELETE
    old_data            JSONB,
    new_data            JSONB,
    performed_by        UUID,
    performed_at        TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
```

### 10.2 الأداء

```sql
-- Partitioning للجداول الكبيرة
CREATE TABLE match_events_partitioned (
    LIKE match_events INCLUDING ALL
) PARTITION BY RANGE (event_time);

CREATE TABLE match_events_2026_08 PARTITION OF match_events_partitioned
    FOR VALUES FROM ('2026-08-01') TO ('2026-09-01');

-- Archive للبيانات القديمة
CREATE TABLE match_events_archive (LIKE match_events INCLUDING ALL);

-- Vacuum and Analyze schedule
-- يومياً في الساعة 3:00 AM
```

### 10.3 النسخ الاحتياطي

```
الاستراتيجية:
- Full backup: يومياً في 2:00 AM
- Incremental: كل 6 ساعات
- WAL archiving: مستمر
- Retention: 30 يوم
- Geo-redundancy: 3 مناطق
```

---

## 11. ER Diagram - المخطط العلائقي

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   players   │────▶│player_stats │     │player_techs │
└─────────────┘     └─────────────┘     └──────┬──────┘
       │                                        │
       │         ┌─────────────┐               │
       └────────▶│player_inventory│             │
                 └─────────────┘               │
                                               ▼
                                        ┌─────────────┐
                                        │technologies │
                                        └─────────────┘
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   matches   │────▶│match_players│     │match_events │
└──────┬──────┘     └─────────────┘     └─────────────┘
       │
       │         ┌─────────────┐     ┌─────────────┐
       └────────▶│  alliances  │────▶│alliance_members│
                 └─────────────┘     └─────────────┘
       │
       │         ┌─────────────┐     ┌─────────────┐
       └────────▶│zone_mutations│    │resource_spawns│
                 └──────┬──────┘     └─────────────┘
                        │
                        ▼
                 ┌─────────────┐
                 │    zones    │
                 └─────────────┘

┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│player_sessions│    │match_analytics│   │  audit_log  │
└─────────────┘     └─────────────┘     └─────────────┘
```

---

*هذا المخطط قابل للتطور مع نمو اللعبة.*

**فريق البنية التحتية - Apex Arena Studios**  
**2026**
