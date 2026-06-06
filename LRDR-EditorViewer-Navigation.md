# LRDR: Behavior Tree Viewer — навигация, фокус и кнопка Scene View

## Краткое описание

Добавлена настраиваемая навигация по viewport и клавиатурное кадрирование в окно редактора **Behavior Tree Viewer** (`BehaviorTreeEditorWindow`).

Новые возможности:

- Popup настроек (справа сверху): кнопка панорамирования, инверсия зума, скорость pan/zoom
- Сохранение настроек через **EditorPrefs** (явная кнопка Save)
- Сброс до дефолтов из кода
- **F** — сфокусироваться на самой глубокой Running-ноде (без Running-потомков), центрирование + zoom
- **Shift+F** — вписать в кадр всё дерево
- Кнопка **Behavior Tree** в Scene View toolbar (при выделении NPC с `BehaviorTreeRunner`)

Все изменения additive. `BehaviorTreeConfig` не изменён.

## Scene View: кнопка быстрого открытия

В **Scene View** добавлена контекстная кнопка для быстрого доступа к **Behavior Tree Viewer** без перехода в меню **Window**.

### Когда появляется

Overlay с кнопкой **виден только при релевантном выделении** (`ITransientOverlay`): Unity показывает toolbar, если на выбранном GameObject (или рядом в иерархии) найден `BehaviorTreeRunner`.

| Выделение | Кнопка в Scene View |
|-----------|---------------------|
| GO с `BehaviorTreeRunner` | видна |
| Дочерний объект (runner на родителе) | видна |
| Родитель / потомок с runner в иерархии | видна |
| Объект без runner | скрыта |

Поиск runner (общий helper `BehaviorTreeEditorSelection`):

1. `GetComponentInChildren<BehaviorTreeRunner>(true)` — на себе и у детей
2. `GetComponentInParent<BehaviorTreeRunner>(true)` — у родителей (удобно при выделении mesh дочернего объекта NPC)

`includeInactive: true` — runner учитывается даже на disabled-компонентах.

### Что делает клик

- Вызывает `BehaviorTreeEditorWindow.OpenOrFocus()`
- Открывает существующее окно viewer или создаёт одно новое (`GetWindow`, без дубликатов)
- Передаёт фокус на окно
- Дерево подхватывается из текущего **Selection** (та же логика, что и в самом viewer)

Кнопка открывает **окно viewer**, не popup настроек внутри него.

### Где искать в редакторе

- Toolbar **Scene View** — иконка с tooltip **Behavior Tree Viewer**
- Если после обновления кнопки нет: **Scene View → Overlays (⋮) → Behavior Tree** — включить overlay в layout (Unity запоминает layout пользователя)

### Техническая реализация

| Элемент | Файл / API |
|---------|------------|
| Кнопка | `OpenBehaviorTreeViewerButton` : `EditorToolbarButton` |
| Overlay | `BehaviorTreeViewerToolbarOverlay` : `ToolbarOverlay`, `ITransientOverlay` |
| ID кнопки | `BehaviorTree/SceneView/OpenViewer` |
| Обновление видимости | `Selection.selectionChanged` → `SceneView.RepaintAll()` |
| Иконка | `d_tree_icon_leaf` (fallback: `Texture2D.whiteTexture`) |
| Unity API | Overlays (2022.3+): `UnityEditor.Overlays`, `UnityEditor.Toolbars` |

Файлы: [`BehaviorTreeSceneViewToolbar.cs`](Runtime/Editor/BehaviorTreeSceneViewToolbar.cs), [`BehaviorTreeEditorSelection.cs`](Runtime/Editor/BehaviorTreeEditorSelection.cs).

## Popup настроек

Открывается кнопкой с иконкой popup в правом верхнем углу (рядом с меткой zoom).

| Настройка | По умолчанию | Описание |
|-----------|--------------|----------|
| Pan button | Mouse0 (ЛКМ) | Кнопка мыши для панорамирования canvas |
| Invert zoom | выкл. | Меняет направление зума колёсиком |
| Pan speed | 1.0 | Множитель delta при перетаскивании (0.1–3.0) |
| Zoom speed | 1.0 | Множитель для `BehaviorTreeConfig.zoomSensitivity` (0.1–3.0) |

### Кнопки

- **Save** — записывает текущие значения в EditorPrefs и показывает уведомление
- **Reset** — восстанавливает дефолты из кода (без автосохранения; для записи на диск нажмите Save)

Изменения в popup применяются сразу (live preview). На диск пишет только **Save**.

### Ключи EditorPrefs

| Ключ | Тип |
|------|-----|
| `BehaviorTree.EditorView.PanButton` | int (enum) |
| `BehaviorTree.EditorView.InvertZoom` | bool |
| `BehaviorTree.EditorView.PanSpeed` | float |
| `BehaviorTree.EditorView.ZoomSpeed` | float |

При открытии окна загружаются сохранённые prefs, если хотя бы один ключ существует; иначе — дефолты из кода.

## Горячие клавиши

| Сочетание | Действие |
|-----------|----------|
| **F** | Сфокусироваться на самой глубокой Running-ноде (leaf Running — без Running-детей): центр ноды + zoom |
| **Shift+F** | Вписать bounding box всех нод в viewport |

Граничные случаи:

- Дерево не загружено / пустой layout — no-op
- **F** без leaf Running-нод (только родители Running или нет Running) — no-op (без диалога)
- Zoom ограничен `BehaviorTreeConfig.minZoom` / `maxZoom`
- Отступ от краёв viewport: 40 px с каждой стороны

## Поведение

### Кнопка pan vs клик по ноде

- **Pan** использует выбранную кнопку мыши
- **Открытие скрипта ноды** всегда по **ЛКМ** (клик без drag), независимо от кнопки pan
- При pan на Mouse0 порог drag (4 px) отличает pan от клика — как раньше

### Первая загрузка дерева

Начальное центрирование по-прежнему использует текущий zoom (`defaultZoom` из config) и меняет только pan.

## Изменённые файлы

| Файл | Изменение |
|------|-----------|
| `Runtime/Editor/BehaviorTreeEditorViewSettings.cs` | **Новый** — модель настроек + EditorPrefs |
| `Runtime/Editor/BehaviorTreeEditorSelection.cs` | **Новый** — поиск `BehaviorTreeRunner` в Selection |
| `Runtime/Editor/BehaviorTreeSceneViewToolbar.cs` | **Новый** — Scene View overlay + кнопка |
| `Runtime/Editor/BehaviorTreeDebuggerWindow.cs` | UI настроек, input, фокус, `OpenOrFocus()` |
| `LRDR-EditorViewer-Navigation.md` | **Новый** — этот документ |

## План ручной проверки

### Scene View

- [ ] Выделить корень NPC с `BehaviorTreeRunner` — в Scene View toolbar появилась кнопка (tooltip **Behavior Tree Viewer**)
- [ ] Выделить дочерний mesh NPC (runner на родителе) — кнопка видна
- [ ] Выделить объект без runner (terrain, prop и т.д.) — кнопки нет
- [ ] Клик по кнопке Scene View — открывается / фокусируется одно окно viewer (без дубликатов)
- [ ] В Play Mode после клика — дерево выбранного NPC отображается в viewer
- [ ] **Window → Behavior Tree Viewer** по-прежнему работает
- [ ] Если кнопки нет — включить overlay через **Scene View → Overlays → Behavior Tree**

### Viewer и навигация

- [ ] Открыть **Window → Behavior Tree Viewer**, выбрать GameObject с `BehaviorTreeRunner` в Play Mode
- [ ] Открыть popup настроек; сменить pan на Mouse1 — pan ПКМ, ЛКМ по-прежнему открывает скрипты нод
- [ ] Сменить pan на MouseWheelClick — pan средней кнопкой
- [ ] Подкрутить слайдеры pan/zoom speed — проверить ускорение/замедление навигации
- [ ] Включить invert zoom — направление колёсика меняется
- [ ] Нажать **Save**, закрыть и открыть окно — настройки восстановились
- [ ] Нажать **Reset** — дефолты (Mouse0, invert off, speed 1.0)
- [ ] Нажать **F** при Running-нодах — фокус на самой глубокой leaf Running (центр + zoom)
- [ ] Нажать **Shift+F** — всё дерево вписывается в viewport
- [ ] Нажать **F** без leaf Running-нод — без ошибок, вид не меняется
- [ ] Первый выбор дерева — центрирование на default zoom как раньше
