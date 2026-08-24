# APEX ARENA: FUTURE FORGE
## AI System Design Document - نظام الذكاء الاصطناعي
### الإصدار: 1.0 | 2026-08-24

---

## 1. نظرة عامة (Overview)

### 1.1 الهدف
نظام ذكاء اصطناعي ديناميكي يتكيف مع:
- أسلوب لعب اللاعب
- حالة البيئة
- مستوى المهارة
- التحالفات

### 1.2 المبادئ
- **التكيف:** العدو يتعلم من اللاعب
- **التوازن:** لا يغش، يستخدم نفس القواعد
- **التنوع:** سلوكيات مختلفة لكل عدو
- **الأداء:** < 5ms لكل قرار

---

## 2. بنية النظام (System Architecture)

### 2.1 المحرك
- **محرك AI:** Unity ML-Agents + Behavior Trees
- **التعلم:** Reinforcement Learning (PPO)
- **التنبؤ:** Neural Networks (LSTM)
- **القرار:** 60 مرة/ثانية

### 2.2 المكونات

```
┌─────────────────────────────────────────┐
│              AI Director                │
│         (مدير المباراة)               │
├──────────┬──────────┬──────────┬────────┤
│  Enemy   │  Drone   │  Beast   │  NPC   │
│   AI     │   AI     │   AI     │  AI    │
├──────────┼──────────┼──────────┼────────┤
│Behavior  │Patrol    │Instinct  │Dialog  │
│Tree      │Network   │Network   │System  │
├──────────┼──────────┼──────────┼────────┤
│Learning  │Swarm     │Pack      │Trade   │
│Module    │Intel     │Behavior  │Logic   │
└──────────┴──────────┴──────────┴────────┘
```

---

## 3. أنواع الأعداء (Enemy Archetypes)

### 3.1 سايبورغ-7 (Cyborg-7)

**الفئة:** Heavy Assault
**الصعوبة:** متوسط-عالٍ
**السلوك:**

```python
class Cyborg7AI:
    def __init__(self):
        self.state = 'patrol'
        self.health = 200
        self.armor = 50
        self.detection_range = 80
        self.attack_range = 40
        self.aggression = 0.7  # 0-1

    def update(self, delta_time):
        # 1. استشعار البيئة
        threats = self.scan_for_threats()

        # 2. اتخاذ القرار
        if threats:
            if self.health < 50:
                self.state = 'retreat'
            elif self.aggression > 0.8:
                self.state = 'charge'
            else:
                self.state = 'attack'
        else:
            self.state = 'patrol'

        # 3. التعلم
        self.learn_from_combat()

    def learn_from_combat(self):
        # تعديل العدوانية بناءً على النتائج
        if self.last_fight == 'won':
            self.aggression = min(1.0, self.aggression + 0.05)
        else:
            self.aggression = max(0.3, self.aggression - 0.1)
```

**القدرات:**
| القدرة | التكلفة | التأثير |
|--------|---------|---------|
| **Heavy Fire** | 20 طاقة | ضرر مضاعف |
| **Shield Wall** | 30 طاقة | درع +100 |
| **Charge** | 15 طاقة | اندفاع + صدمة |
| **Overclock** | 50 طاقة | سرعة ×2 لمدة 5s |

**التكيف:**
- إذا كان اللاعب يستخدم سلاح طويل المدى → يقترب بسرعة
- إذا كان اللاعب يستخدم تقنيات دفاعية → ينتظر التبريد
- إذا خسر 3 مرات متتالية → يصبح أكثر حذراً

---

### 3.2 الطائرات المسيرة (Drones)

**الفئة:** Scout / Support
**الصعوبة:** منخفض-متوسط
**السلوك:**

```python
class DroneAI:
    def __init__(self):
        self.squad = []  # مجموعة الطائرات
        self.role = random.choice(['scout', 'attacker', 'healer'])
        self.formation = 'swarm'

    def update(self):
        if self.role == 'scout':
            self.patrol_and_report()
        elif self.role == 'attacker':
            self.swarm_attack()
        elif self.role == 'healer':
            self.support_allies()

    def swarm_attack(self):
        # هجوم متزامن من زوايا مختلفة
        angles = self.calculate_optimal_angles()
        for drone, angle in zip(self.squad, angles):
            drone.attack_from_angle(angle)
```

**القدرات:**
| القدرة | التكلفة | التأثير |
|--------|---------|---------|
| **Scan** | 5 طاقة | كشف موقع لاعب |
| **Sting** | 10 طاقة | ضرر خفيف + تتبع |
| **Swarm** | 30 طاقة | هجوم متزامن |
| **Jam** | 20 طاقة | تعطيل تقنية |

---

### 3.3 الوحوش المشعة (Irradiated Beasts)

**الفئة:** Berserker
**الصعوبة:** متوسط
**السلوك:**

```python
class BeastAI:
    def __init__(self):
        self.radiation_level = random.randint(50, 100)
        self.pack = []  # القطيع
        self.hunger = 0.5  # 0-1

    def update(self):
        # الوحوش لا تستخدم طاقة، بل إشعاع
        if self.hunger > 0.8:
            self.state = 'hunt'
        elif self.radiation_level > 80:
            self.state = 'rage'  # هجوم عشوائي
        else:
            self.state = 'wander'

    def pack_behavior(self):
        # القطيع يهاجم معاً
        if len(self.pack) >= 3:
            alpha = self.pack[0]
            alpha.howl()  # إشارة للهجوم
            for beast in self.pack:
                beast.chase_target()
```

**القدرات:**
| القدرة | التكلفة | التأثير |
|--------|---------|---------|
| **Claw** | 0 | ضرر مادي |
| **Radiation Burst** | 20 إشعاع | ضرر منطقي |
| **Howl** | 0 | استدعاء قطيع |
| **Mutate** | 50 إشعاع | تحول + قوة ×2 |

---

## 4. نظام التعلم (Learning System)

### 4.1 تعلم اللاعب

```python
class PlayerModel:
    def __init__(self, player_id):
        self.player_id = player_id
        self.weapon_preference = {}  # تفضيل الأسلحة
        self.tech_usage = {}  # استخدام التقنيات
        self.movement_pattern = []  # أنماط الحركة
        self.alliance_behavior = []  # سلوك التحالف
        self.aggression_score = 0.5

    def analyze_match(self, match_data):
        # تحليل 100 مباراة سابقة
        self.weapon_preference = self.extract_weapons(match_data)
        self.tech_usage = self.extract_techs(match_data)
        self.movement_pattern = self.extract_movement(match_data)

    def predict_next_action(self, current_state):
        # التنبؤ بالحركة التالية
        if self.movement_pattern[-3:] == ['left', 'left', 'left']:
            return 'probably_right'  # اللاعب يتناوب
        elif self.tech_usage.get('adaptive', 0) > 0.8:
            return 'will_use_defense'
```

### 4.2 تكيف الصعوبة

| مؤشر | القيمة | التعديل |
|------|--------|---------|
| **معدل الفوز** | > 70% | +20% صعوبة |
| **معدل الفوز** | 40-60% | لا تغيير |
| **معدل الفوز** | < 30% | -20% صعوبة |
| **سرعة القتل** | < 30 ثانية | +10% صعوبة |
| **استخدام التقنيات** | > 5/مباراة | +15% صعوبة |

---

## 5. مدير المباراة (AI Director)

### 5.1 الوظائف

```python
class AIDirector:
    def __init__(self):
        self.match_phase = 'early'  # early, mid, late
        self.tension_level = 0.0    # 0-1
        self.player_models = {}

    def update(self, match_state):
        # 1. تحديد مرحلة المباراة
        self.match_phase = self.calculate_phase(match_state)

        # 2. حساب التوتر
        self.tension_level = self.calculate_tension(match_state)

        # 3. توليد الأحداث
        if self.tension_level < 0.3:
            self.spawn_ambient_threats()
        elif self.tension_level > 0.8:
            self.spawn_boss_encounter()

        # 4. تعديل البيئة
        if self.match_phase == 'late':
            self.shrink_safe_zone()

    def calculate_tension(self, state):
        factors = {
            'players_alive': state.alive_count / state.max_players,
            'recent_kills': state.kills_last_minute / 10,
            'alliance_stability': 1 - state.recent_betrayals,
            'resource_scarcity': 1 - (state.total_resources / state.max_resources)
        }
        return sum(factors.values()) / len(factors)
```

### 5.2 الأحداث الديناميكية

| الحدث | الشرط | التأثير |
|-------|-------|---------|
| **Ambush** | توتر < 0.2 | كمين مفاجئ |
| **Resource Drop** | موارد < 30% | إنزال إمدادات |
| **Boss Spawn** | توتر > 0.8 | وحش ضخم |
| **Zone Shift** | مرحلة متأخرة | تغيير بيئي |
| **Tech Surge** | استخدام تقنيات منخفض | تقنيات مجانية |

---

## 6. الأداء (Performance)

### 6.1 الميزانية

| المكون | الوقت/إطار | الحد |
|--------|-----------|------|
| **قرار عدو واحد** | 2ms | 5ms |
| **مدير المباراة** | 1ms | 3ms |
| **تعلم** | 0.5ms | 2ms |
| **التنبؤ** | 0.3ms | 1ms |
| **الإجمالي** | 3.8ms | 10ms |

### 6.2 التحسين

- **Pooling:** إعادة استخدام الأعداء
- **LOD AI:** ذكاء مبسط للأعداء البعيدين
- **Batching:** معالجة متزامنة للمجموعات
- **Caching:** تخزين نتائج التنبؤ

---

*العدو يتعلم، يتكيف، ويتحدى.*

**فريق الذكاء الاصطناعي - Apex Arena Studios**  
**2026**
