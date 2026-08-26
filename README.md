# Apex Arena: Future Forge

## لعبة Battle Royale ديناميكية بمحرك Unity

**Apex Arena: Future Forge** هي لعبة Battle Royale تتميز بنظام "التقنية المحسّنة" الذي يغير قواعد اللعبة أثناء المباراة الواحدة.

---

## 🎮 الميزات الرئيسية

- **البيئة الديناميكية:** 4 مناطق (صناعية، عسكرية، غابة، خطرة) تتغير كل 3 دقائق
- **نظام التقنيات:** 5 تقنيات متطورة (دروع تكيفية، نانوبوتات، قفزة كمية، Overload، هولوغرام)
- **القتال المحسّن:** 6 أسلحة مع 100+ تعديل برمجي
- **التحالفات الديناميكية:** تحالف/خيانة/هدنة في أي لحظة
- **الذكاء الاصطناعي:** 3 أنواع أعداء يتعلمون من أسلوب اللاعب
- **الاقتصاد الديناميكي:** 4 موارد + Battle Pass موسمي

---

## 🛠️ المتطلبات

- **Unity:** 2022.3.20f1 LTS
- **Netcode:** Unity Netcode for GameObjects 1.6.0
- **URP:** Universal Render Pipeline 14.0.9
- **Input System:** Unity Input System 1.7.0

---

## 📁 هيكل المشروع

```
ApexArena-UnityProject/
├── Assets/
│   └── _Project/
│       ├── Scripts/
│       │   ├── Core/
│       │   │   └── GameManager.cs
│       │   ├── Gameplay/
│       │   │   ├── PlayerController.cs
│       │   │   ├── DynamicEnvironment.cs
│       │   │   ├── Zone.cs
│       │   │   ├── TechSystem.cs
│       │   │   ├── WeaponSystem.cs
│       │   │   ├── AllianceSystem.cs
│       │   │   ├── ResourceManager.cs
│       │   │   └── ResourceNode.cs
│       │   ├── AI/
│       │   │   └── AIController.cs
│       │   ├── UI/
│       │   │   └── UIManager.cs
│       │   ├── Network/
│       │   │   └── NetworkManager.cs
│       │   └── Audio/
│       │       └── AudioManager.cs
│       ├── Prefabs/
│       ├── Scenes/
│       ├── ScriptableObjects/
│       │   ├── Weapons/
│       │   ├── Techs/
│       │   └── Zones/
│       └── Resources/
├── Packages/
│   └── manifest.json
└── ProjectSettings/
```

---

## 🚀 البدء

### 1. استنساخ المستودع
```bash
git clone https://github.com/yourusername/apex-arena-future-forge.git
cd apex-arena-future-forge
```

### 2. فتح في Unity
- افتح Unity Hub
- اختر "Open"
- حدد مجلد `ApexArena-UnityProject`

### 3. تثبيت الحزم
- افتح Window → Package Manager
- تحقق من تثبيت الحزم المطلوبة

### 4. تشغيل المشروع
- افتح مشهد `Assets/_Project/Scenes/MainMenu`
- اضغط Play

---

## 🏗️ الأنظمة الرئيسية

### GameManager (Core)
- إدارة حالة المباراة (Lobby → Active → Ended)
- تتبع اللاعبين
- تفعيل التغييرات البيئية

### PlayerController (Gameplay)
- حركة (مشي، ركض، قفز)
- قتال (إطلاق، إعادة تحميل)
- تفاعل (جمع، تفعيل تقنية)
- صحة + دروع

### DynamicEnvironment (Gameplay)
- 4 مناطق ديناميكية
- تغيير التضاريس
- توسع المنطقة الخطرة
- إشعاع

### TechSystem (Gameplay)
- فتح تقنيات
- تفعيل/إلغاء
- Cooldowns
- تأثيرات

### WeaponSystem (Gameplay)
- 6 أنواع أسلحة
- نظام ذخيرة
- Overheat
- تعديلات

### AllianceSystem (Gameplay)
- عرض/قبول/رفض
- خيانة
- إعلان عداء/هدنة
- السمعة

### AIController (AI)
- 3 أنواع (Cyborg, Drone, Beast)
- Behavior States (Patrol, Chase, Attack, Retreat)
- NavMesh
- تعلم

### NetworkManager (Network)
- Host/Client
- Netcode for GameObjects
- RPCs
- مزامنة الحالة

### AudioManager (Audio)
- موسيقى تكيفية
- SFX
- Ambient
- 3D Spatial

---

## 📚 الوثائق

للاطلاع على الوثائق التفصيلية، بما في ذلك طريقة إعداد الرخصة وبناء APK دون فتح محرر Unity، راجع [دليل رخصة Unity والبناء الآلي](UNITY_LICENSE_SETUP.md).

للاطلاع على الوثائق التفصيلية:
- [Game Design Document](../docs/Apex_Arena_GDD_v1.0.md)
- [Art Bible](../docs/Apex_Arena_Art_Bible_v1.0.md)
- [Database Schema](../docs/Apex_Arena_Database_Schema_v1.0.md)
- [QA Test Plan](../docs/Apex_Arena_QA_Test_Plan_v1.0.md)

---

## 🤝 المساهمة

1. Fork المستودع
2. أنشئ فرعاً جديداً (`git checkout -b feature/amazing-feature`)
3. Commit التغييرات (`git commit -m 'Add amazing feature'`)
4. Push إلى الفرع (`git push origin feature/amazing-feature`)
5. افتح Pull Request

---

## 📄 الترخيص

هذا المشروع مرخص بموجب MIT License.

---

**Apex Arena Studios**  
**2026**
