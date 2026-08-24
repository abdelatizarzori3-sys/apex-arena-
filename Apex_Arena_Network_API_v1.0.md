# APEX ARENA: FUTURE FORGE
## Network API Specification - مواصفات API الشبكات
### الإصدار: 1.0 | 2026-08-24

---

## 1. نظرة عامة (Overview)

### 1.1 البروتوكول
- **Transport:** UDP (للعبة) + TCP (للتحكم)
- **Serialization:** Protocol Buffers
- **Encryption:** TLS 1.3
- **Rate Limit:** 60 tick/sec (server), 30 tick/sec (client)

### 1.2 البنية

```
Client → Load Balancer → Game Server → Redis → PostgreSQL
                ↓
         Matchmaking Service
                ↓
         Analytics Pipeline
```

---

## 2. مصادقة اللاعب (Player Authentication)

### 2.1 تسجيل الدخول

**Endpoint:** `POST /api/v1/auth/login`

**Request:**
```json
{
  "username": "Shadow_9",
  "password": "hashed_password",
  "platform": "pc",
  "client_version": "2.4.1"
}
```

**Response:**
```json
{
  "success": true,
  "token": "jwt_token_here",
  "refresh_token": "refresh_token_here",
  "player_id": "uuid",
  "expires_in": 3600
}
```

### 2.2 التحقق

**Endpoint:** `GET /api/v1/auth/verify`

**Headers:**
```
Authorization: Bearer jwt_token_here
```

**Response:**
```json
{
  "valid": true,
  "player_id": "uuid",
  "username": "Shadow_9"
}
```

---

## 3. إدارة المباريات (Match Management)

### 3.1 البحث عن مباراة

**Endpoint:** `POST /api/v1/match/find`

**Request:**
```json
{
  "player_id": "uuid",
  "match_type": "solo",
  "region": "me",
  "skill_rating": 2500
}
```

**Response:**
```json
{
  "match_id": "match_uuid",
  "status": "queued",
  "estimated_wait": 15,
  "queue_position": 3
}
```

### 3.2 حالة المباراة

**Endpoint:** `GET /api/v1/match/{match_id}/status`

**Response:**
```json
{
  "match_id": "match_uuid",
  "status": "active",
  "players": 47,
  "alive": 23,
  "phase": "mid",
  "zone": "military",
  "algorithm_version": "2.4.1"
}
```

### 3.3 انضمام للمباراة (WebSocket)

**Connection:** `wss://game.apexarena.com/match/{match_id}`

**Messages:**

**Client → Server:**
```protobuf
message PlayerInput {
  string player_id = 1;
  float pos_x = 2;
  float pos_y = 3;
  float pos_z = 4;
  float rot_x = 5;
  float rot_y = 6;
  int32 action = 7;  // 0=none, 1=fire, 2=jump, etc.
  bytes extra_data = 8;
}
```

**Server → Client:**
```protobuf
message GameState {
  int32 tick = 1;
  repeated PlayerState players = 2;
  repeated EntityState entities = 3;
  ZoneState zone = 4;
  repeated Event events = 5;
}

message PlayerState {
  string player_id = 1;
  float pos_x = 2;
  float pos_y = 3;
  float pos_z = 4;
  int32 health = 5;
  int32 energy = 6;
  repeated string active_techs = 7;
}
```

---

## 4. نظام التحالفات (Alliance System)

### 4.1 عرض تحالف

**Endpoint:** `POST /api/v1/alliance/offer`

**Request:**
```json
{
  "match_id": "match_uuid",
  "from_player": "uuid1",
  "to_player": "uuid2",
  "message": "Let's team up!"
}
```

**Response:**
```json
{
  "alliance_id": "alliance_uuid",
  "status": "pending",
  "expires_at": "2026-08-24T12:00:00Z"
}
```

### 4.2 الرد

**Endpoint:** `POST /api/v1/alliance/respond`

**Request:**
```json
{
  "alliance_id": "alliance_uuid",
  "response": "accept",  // accept, reject
  "player_id": "uuid2"
}
```

### 4.3 خيانة

**Endpoint:** `POST /api/v1/alliance/betray`

**Request:**
```json
{
  "alliance_id": "alliance_uuid",
  "player_id": "uuid1",
  "reason": "strategic"
}
```

**Response:**
```json
{
  "success": true,
  "reputation_change": -15,
  "alliance_dissolved": true
}
```

---

## 5. نظام التقنيات (Tech System)

### 5.1 فتح تقنية

**Endpoint:** `POST /api/v1/tech/unlock`

**Request:**
```json
{
  "player_id": "uuid",
  "match_id": "match_uuid",
  "tech_id": "nanobots",
  "cost": {
    "energy": 450,
    "data": 200
  }
}
```

**Response:**
```json
{
  "success": true,
  "tech_id": "nanobots",
  "unlocked_at": "2026-08-24T11:30:00Z",
  "remaining_energy": 400
}
```

### 5.2 تفعيل تقنية

**Endpoint:** `POST /api/v1/tech/activate`

**Request:**
```json
{
  "player_id": "uuid",
  "match_id": "match_uuid",
  "tech_id": "adaptive",
  "target": "self"
}
```

**Response:**
```json
{
  "success": true,
  "tech_id": "adaptive",
  "duration": 0,
  "cooldown": 0,
  "effects": ["damage_reduction_40"]
}
```

---

## 6. نظام الموارد (Resource System)

### 6.1 جمع موارد

**Endpoint:** `POST /api/v1/resource/collect`

**Request:**
```json
{
  "player_id": "uuid",
  "match_id": "match_uuid",
  "resource_id": "res_uuid",
  "amount": 50
}
```

**Response:**
```json
{
  "success": true,
  "resource_type": "energy",
  "amount_collected": 50,
  "player_total": 900
}
```

### 6.2 إنفاق موارد

**Endpoint:** `POST /api/v1/resource/spend`

**Request:**
```json
{
  "player_id": "uuid",
  "match_id": "match_uuid",
  "amount": {
    "energy": 100,
    "data": 50
  },
  "reason": "tech_unlock"
}
```

---

## 7. التحديثات التلقائية (Auto-Updates)

### 7.1 التحقق

**Endpoint:** `GET /api/v1/update/check`

**Request:**
```json
{
  "client_version": "2.4.1",
  "platform": "pc"
}
```

**Response:**
```json
{
  "update_available": true,
  "latest_version": "2.4.2",
  "patch_size": "15MB",
  "changelog": "Balance updates, bug fixes",
  "urgency": "normal",  // normal, recommended, critical
  "download_url": "https://cdn.apexarena.com/patches/2.4.2.zip"
}
```

### 7.2 التطبيق الساخن

**WebSocket:** `wss://game.apexarena.com/updates`

**Server → Client:**
```json
{
  "type": "hot_update",
  "algorithm_version": "2.4.2",
  "changes": {
    "weapon_balance": {
      "plasma_rifle": {"damage": -5}
    },
    "zone_mutation": {
      "industrial": {"resource_richness": +10}
    }
  },
  "effective_immediately": true
}
```

---

## 8. التحليلات (Analytics)

### 8.1 إرسال الأحداث

**Endpoint:** `POST /api/v1/analytics/event`

**Request:**
```json
{
  "match_id": "match_uuid",
  "player_id": "uuid",
  "event_type": "kill",
  "event_data": {
    "target_id": "uuid2",
    "weapon": "plasma_rifle",
    "distance": 45.5,
    "headshot": false
  },
  "timestamp": "2026-08-24T11:35:00Z"
}
```

### 8.2 الإحصائيات

**Endpoint:** `GET /api/v1/analytics/player/{player_id}`

**Response:**
```json
{
  "player_id": "uuid",
  "total_matches": 150,
  "win_rate": 0.23,
  "kd_ratio": 2.5,
  "avg_survival": 420,
  "favorite_weapon": "plasma_rifle",
  "favorite_zone": "military",
  "skill_rating": 2500,
  "rank": "Platinum"
}
```

---

## 9. الأمان (Security)

### 9.1 Rate Limiting

| Endpoint | الحد | الفترة |
|----------|------|--------|
| `/auth/*` | 5 | دقيقة |
| `/match/find` | 10 | دقيقة |
| `/resource/*` | 60 | دقيقة |
| `/tech/*` | 30 | دقيقة |
| `/alliance/*` | 20 | دقيقة |

### 9.2 التحقق

- **JWT:** صلاحية 1 ساعة
- **Refresh:** صلاحية 30 يوم
- **IP Binding:** اختياري
- **2FA:** مدعوم للحسابات المميزة

---

*API سريع، آمن، وقابل للتوسع.*

**فريق البنية التحتية - Apex Arena Studios**  
**2026**
