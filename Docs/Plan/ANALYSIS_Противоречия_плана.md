# Детальный анализ плана разработки Match-3 игры

## Введение

Данный документ представляет собой комплексный анализ плана разработки Match-3 игры для платформы Android на движке Unity 2022.3.62f3. Анализ охватывает все ключевые аспекты плана: цели и задачи, временные рамки, распределение ресурсов, зависимости между элементами и потенциальные риски реализации.

---

## 0. Фундаментальные ограничения проекта

### 0.1. Сторонние плагины — НЕДОПУСТИМО

**Определение:**
> **Сторонние плагины** — любые внешние библиотеки, SDK, фреймворки или инструменты, не входящие в стандартную поставку Unity 2022.3.62f3 и не являющиеся частью официального Unity Engine API.

**Полный запрет распространяется на:**
- Firebase (все продукты: Analytics, Crashlytics, Remote Config, Auth, Firestore)
- Unity Analytics, Unity Remote Config, Unity Cloud Build
- PlayFab, Photon, Mirror, Bolt (networking)
- DOTween, LeanTween, iTween (анимации)
- PlayMaker, Behavior Designer (AI/behavior)
- AdMob, Unity Ads, ironSource (монетизация)
- Amplitude, Mixpanel, Adjust (аналитика)
- Easy Save, Odin Serializer (сериализация)
- Любые Asset Store ассеты, требующие integration

**Разрешённые технологии:**
- Unity Engine API (MonoBehaviour, ScriptableObject, Coroutines)
- C# .NET Standard 2.1 (базовые библиотеки)
- Unity PlayerPrefs (сохранение данных)
- JSON сериализация через JsonUtility или System.Text.Json

**Последствия нарушения:**
- Отказ от merge/pull request
- Удаление кода при code review
- Пересмотр архитектуры решения

### 0.2. Аналитика — НЕ ТРЕБУЕТСЯ

**Нет внешней аналитики**
**Нет локальной аналитики**

Отсутствует необходимость в сборе метрик пользователей:
- Нет требований к отслеживанию Retention (D1, D7, D30)
- Нет требований к session length метрикам
- Нет требований к A/B тестированию
- Нет требований к сбору данных о churn points

Все системы аналитики удалены из плана разработки.

### 0.3. Серверная инфраструктура — НЕ ТРЕБУЕТСЯ

**Все системы работают локально:**
- Сохранение прогресса: PlayerPrefs + JSON
- Аналитика: локальные метрики с логированием
- Балансировка: встроенные таблицы и AI-симулятор
- Социальные функции: не поддерживаются (Friend lists, clans, PvP)

---

## 1. Противоречия в количественных показателях уровней

### 1.1. Несоответствие целевого количества уровней — ИСПРАВЛЕНО

**Расположение противоречия:**
- [`09_План_разработки.md:99`](Docs/Plan/09_План_разработки.md:99) — Roadmap указывает "Global Launch: ✅ 100 уровней"
- [`05_Прогрессия_и_сложность.md:11`](Docs/Plan/05_Прогрессия_и_сложность.md:11) — "На релиз: 60-100 уровней (рекомендация: 80)"
- [`09_План_разработки.md:47`](Docs/Plan/09_План_разработки.md:47) — Alpha deliverables: "30+ уровней"
- [`09_План_разработки.md:59`](Docs/Plan/09_План_разработки.md:59) — Beta: "60-80 уровней с кривой сложности"

**Анализ:**
План содержит три различных целевых показателя количества уровней на релиз: 100 уровней (в roadmap), 80 уровней (рекомендация в документе о прогрессии) и 60-100 уровней (вариативный диапазон). Это создаёт неопределённость в планировании ресурсов и оценке объёма работы.

**РЕШЕНИЕ — УНИФИКАЦИЯ КОЛИЧЕСТВА УРОВНЕЙ:**

**Global Launch: 80 уровней**

| Этап | Количество уровней | Структура |
|------|-------------------|-----------|
| Pre-production | 3-5 уровней | Тестовые уровни |
| Alpha | 30+ уровней | Полное покрытие механик |
| Beta | 70-80 уровней | Финальная кривая сложности |
| Global Launch | 80 уровней | 4 эпизода по 20 уровней |

**Структура эпизодов:**
```
Эпизод 1: Уровни 1-20 (Easy → Medium)
Эпизод 2: Уровни 21-40 (Medium)
Эпизод 3: Уровни 41-60 (Medium → Hard)
Эпизод 4: Уровни 61-80 (Hard + Expert)
```

### 1.2. Система управления уровнями — ScriptableObject

**Принятое решение:** Полностью ScriptableObject-базированная система.

```csharp
// Основной ScriptableObject для уровня
[CreateAssetMenu(fileName = "Level_XXX", menuName = "Match3/Level")]
public class LevelData : ScriptableObject
{
    [Header("Basic Info")]
    public int levelNumber;
    public string levelID;
    public LevelType levelType;
    
    [Header("Grid Configuration")]
    public int width = 8;
    public int height = 8;
    public GridShape shape = GridShape.Rectangle;
    public bool[,] customCells;
    
    [Header("Objectives")]
    public Objective[] objectives;
    public int movesAllowed;
    public int[] starThresholds = new int[3];
    
    [Header("Elements & Obstacles")]
    public ElementType[] allowedElements;
    public ElementWeights elementWeights;
    public ObstacleSpawnData[] obstacles;
    
    [Header("Metadata")]
    public bool isHardLevel;
    public bool isTutorialLevel;
    public Sprite previewImage;
}

// Эпизод - контейнер для уровней
[CreateAssetMenu(fileName = "Episode_XXX", menuName = "Match3/Episode")]
public class EpisodeData : ScriptableObject
{
    public int episodeNumber;
    public string episodeName;
    public Sprite backgroundImage;
    public List<LevelData> levels = new List<LevelData>();
}
```

**LevelManager — централизованное управление:**

```csharp
public class LevelManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private List<EpisodeData> episodes = new List<EpisodeData>();
    
    // Автоматическая генерация списка уровней при старте
    private void Awake()
    {
        GenerateLevelIndex();
        ValidateAllLevels();
    }
    
    // Загрузка уровня по индексу
    public LevelData GetLevelByIndex(int globalIndex)
    {
        if (globalIndex < 0 || globalIndex >= levelIndex.Count)
        {
            Debug.LogError($"Level index {globalIndex} out of range");
            return null;
        }
        return levelIndex[globalIndex];
    }
    
    // Загрузка уровня по ID
    public LevelData GetLevelByID(string levelID)
    {
        if (levelDictionary.TryGetValue(levelID, out LevelData level))
            return level;
        return null;
    }
    
    // Валидация данных уровня при загрузке
    private void ValidateLevel(LevelData level)
    {
        Debug.Assert(level.width >= 4 && level.width <= 12, "Invalid width");
        Debug.Assert(level.height >= 4 && level.height <= 12, "Invalid height");
        Debug.Assert(level.movesAllowed > 0, "Moves must be positive");
        Debug.Assert(level.starThresholds.Length == 3, "Must have 3 star thresholds");
        ValidateObstaclesAgainstGrid(level);
    }
}
```

**Кастомный инспектор для Unity Editor:**

```csharp
#if UNITY_EDITOR
[CustomEditor(typeof(LevelData))]
public class LevelDataEditor : UnityEditor.Editor
{
    public override void OnInspectorGUI()
    {
        LevelData level = (LevelData)target;
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Objectives: {level.objectives.Length}");
        EditorGUILayout.LabelField($"Obstacles: {level.obstacles.Length}");
        
        if (GUILayout.Button("Validate"))
            ValidateLevelData(level);
    }
}
#endif
```

**Особенности системы:**
- Все данные уровней хранятся в ассетах проекта
- Добавление новых уровней требует пересборки билда
- Централизованное управление через LevelManager
- Валидация данных при загрузке уровня

---

## 2. Противоречия в технических требованиях

### 2.1. Сторонние плагины — УДАЛЕНЫ

**Было:**
- [`10_Технические_требования.md:44-48`](Docs/Plan/10_Технические_требования.md:44-48) — "Без сторонних плагинов" + Firebase
- [`09_План_разработки.md:85`](Docs/Plan/09_План_разработки.md:85) — "Интеграция аналитики (Unity Analytics / Firebase)"

**Стало:**
Firebase и Unity Analytics полностью удалены из требований.

**Локальная аналитика (без внешних сервисов):**

```csharp
public class LocalAnalytics
{
    // Метрики сохраняютсялокально
    public void TrackLevelStart(int levelNumber) { }
    public void TrackLevelComplete(int levelNumber, int stars, float duration) { }
    public void TrackLevelFailed(int levelNumber, int attempts) { }
    public void TrackBoosterUsed(string boosterType) { }
    public void TrackHintUsed() { }
    
    // Экспорт данных (для разработчика)
    public string ExportMetrics() => JsonUtility.ToJson(this);
}
```

### 2.2. Конфликт между производительностью и сложностью уровней

**Расположение противоречия:**
- [`10_Технические_требования.md:40`](Docs/Plan/10_Технические_требования.md:40) — "FPS target: 60 FPS на средних устройствах"
- [`05_Прогрессия_и_сложность.md:93`](Docs/Plan/05_Прогрессия_и_сложность.md:93) — "Уровни 61-80: 6-7 типов элементов, 40-60% плотность препятствий"

**Рекомендация:**
Добавить раздел по оптимизации BFS и этап профилирования производительности.

---

## 3. Сводная таблица решений

| № | Проблема | Решение | Статус |
|---|----------|---------|--------|
| 1 | Количество уровней | 80 уровней, ScriptableObject | ✅ Готово |
| 2 | Сторонние плагины | Полный запрет | ✅ Готово |
| 3 | Firebase/Analytics | Удалены, локальная замена | ✅ Готово |
| 4 | Win Rate формула | Требует пересмотра | ⏳ В работе |
| 5 | Произходительность | Добавить профилирование | ⏳ В работе |
| 6 | Alpha перегруженность | Разделить этап | ⏳ В работе |

---

## Заключение

**Фундаментальные ограничения:**
1. ❌ Сторонние плагины — НЕДОПУСТИМО
2. ❌ IAP/Реклама — НЕ ПЛАНИРУЕТСЯ
3. ❌ Серверная инфраструктура — НЕ ТРЕБУЕТСЯ

**Архитектура:**
- 80 уровней на Global Launch
- ScriptableObject для всех данных уровней
- LevelManager для централизованного управления
- Локальная аналитика вместо Firebase
- PlayerPrefs + JSON для сохранения прогресса
