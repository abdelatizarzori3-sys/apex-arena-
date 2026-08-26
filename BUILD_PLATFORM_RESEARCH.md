# مقارنة منصات بناء Apex Arena

## النتيجة

البديل الأكثر ملاءمة للمستودع الحالي هو GitHub Actions مع GameCI، لأن المشروع موجود أصلًا على GitHub وسير البناء الحالي يمكن استبداله دون نقل الملفات إلى خدمة جديدة. هذا المسار يبني مشروع Unity فعليًا داخل حاوية Unity ويخرج APK كـ artifact.

## قيود مشتركة

أي منصة تبني مشروع Unity حقيقيًا تحتاج ترخيص Unity صالحًا على عامل البناء. تغيير مزود CI لا يلغي شرط الترخيص. لا يجب وضع ملف الترخيص أو كلمة المرور أو keystore داخل المستودع.

## الخيارات

| المنصة | الملاءمة | القيد الأساسي |
|---|---|---|
| GitHub Actions + GameCI | الأفضل للمشروع الحالي | يحتاج UNITY_LICENSE أو بيانات ترخيص صالحة |
| Codemagic | يدعم Unity وAndroid | توثيقه يطلب ترخيص Unity Plus أو Pro للبناء السحابي، إضافة إلى إعداد حساب جديد |
| Bitrise | يدعم Unity وAndroid | يحتاج تثبيت Unity وترخيصًا في بيئة Bitrise وإعدادًا أطول |

## مصادر التحقق

- GameCI Getting Started: https://game.ci/docs/github/getting-started/
- Codemagic Unity apps: https://docs.codemagic.io/yaml-quick-start/building-a-unity-app/
- Bitrise Unity: https://docs.bitrise.io/en/bitrise-ci/getting-started/unity-on-bitrise

## القرار

سنستخدم GitHub Actions + GameCI كبديل أول، مع تشغيل يدوي فقط، وتثبيت Unity 2022.3.20f1، واستعمال `game-ci/unity-builder`, ثم رفع مجلد APK كـ artifact. إذا لم تتوفر رخصة Unity الصالحة، لن ينتج أي بديل APK حقيقيًا، ويجب توثيق ذلك بدل ادعاء النجاح.

## نتيجة التشغيل الفعلي

تشغيل GitHub Actions رقم `32978547183` نجح في إعداد المهمة وCheckout وCache، ثم فشل في خطوة `Build Android APK` قبل إنشاء APK. السجل الرسمي كان:

`No valid license activation strategy could be determined. Make sure to provide UNITY_EMAIL, UNITY_PASSWORD, and either a UNITY_SERIAL or UNITY_LICENSE.`

إذن البديل يعمل، لكن أسرار ترخيص Unity ليست متاحة في بيئة Actions أو لا تحتوي على استراتيجية تفعيل صالحة. وفق GameCI يجب توفير `UNITY_LICENSE` أو `UNITY_SERIAL` مع `UNITY_EMAIL` و`UNITY_PASSWORD`، أو استخدام Unity Licensing Server. لا توجد طريقة آمنة لاستخراج هذه القيم من GitHub أو اختلاقها.

مرجع التفعيل: https://game.ci/docs/github/activation
