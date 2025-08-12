# ForEach Decorator

## Описание
`ForEach<T>` - это абстрактная нода-декоратор для Behavior Tree, которая позволяет пройтись по списку элементов и выполнить дочернюю ноду для каждого элемента.

## Основные возможности
- **Генерики**: Работает с любым типом данных через `List<T>`
- **Абстрактность**: Требует реализации метода `GetList()` для получения списка
- **Гибкость**: Позволяет настраивать поведение до и после обработки каждого элемента
- **Автоматическое управление**: Сама управляет итерацией и состоянием выполнения

## Как использовать

### 1. Создание собственной реализации
```csharp
public class ForEachEnemy : ForEach<Enemy>
{
    private List<Enemy> enemyList;

    public ForEachEnemy(Node child, List<Enemy> enemies) : base(child)
    {
        this.enemyList = enemies;
    }

    protected override List<Enemy> GetList()
    {
        return enemyList;
    }

    protected override void OnElementStart(Enemy enemy, int index)
    {
        // Действия перед обработкой каждого врага
        enemy.SetTarget(Player.Instance);
    }

    protected override void OnElementComplete(Enemy enemy, int index, NodeState result)
    {
        // Действия после обработки каждого врага
        Debug.Log($"Enemy {enemy.name} processed with result: {result}");
    }
}
```

### 2. Использование в дереве поведения
```csharp
// Создаем список врагов
List<Enemy> enemies = GetEnemiesInRange();

// Создаем ForEach декоратор с дочерней нодой
var forEachNode = new ForEachEnemy(
    new Sequence(
        new MoveToTarget(),
        new Attack()
    ),
    enemies
);

// Добавляем в дерево
tree.AddNode(forEachNode);
```

## Ключевые методы для переопределения

### `GetList()`
```csharp
protected abstract List<T> GetList();
```
**Обязательный метод** - возвращает список элементов для итерации.

### `OnElementStart(T element, int index)`
```csharp
protected virtual void OnElementStart(T element, int index)
```
**Опциональный метод** - вызывается перед выполнением дочерней ноды для каждого элемента.

### `OnElementComplete(T element, int index, NodeState result)`
```csharp
protected virtual void OnElementComplete(T element, int index, NodeState result)
```
**Опциональный метод** - вызывается после завершения выполнения дочерней ноды для каждого элемента.

### `Clone(Node node)`
```csharp
protected virtual Node Clone(Node node)
```
**Опциональный метод** - создает копию дочерней ноды для следующей итерации.

## Логика работы

1. **Инициализация**: При первом вызове `Tick()` получает список через `GetList()`
2. **Итерация**: Последовательно обрабатывает каждый элемент списка
3. **Выполнение**: Для каждого элемента выполняет дочернюю ноду
4. **Переход**: После завершения дочерней ноды переходит к следующему элементу
5. **Завершение**: Возвращает `Success` когда все элементы обработаны

## Состояния выполнения

- **Running**: Обрабатывает элементы списка
- **Success**: Все элементы успешно обработаны
- **Failure**: Ошибка инициализации или список пуст

## Примеры использования

### Обработка списка позиций
```csharp
public class ForEachPosition : ForEach<Vector3>
{
    private Transform agent;
    private List<Vector3> waypoints;

    public ForEachPosition(Node child, List<Vector3> waypoints, Transform agent) 
        : base(child)
    {
        this.waypoints = waypoints;
        this.agent = agent;
    }

    protected override List<Vector3> GetList() => waypoints;

    protected override void OnElementStart(Vector3 position, int index)
    {
        agent.position = position;
    }
}
```

### Обработка списка задач
```csharp
public class ForEachTask : ForEach<ITask>
{
    private List<ITask> taskList;

    public ForEachTask(Node child, List<ITask> tasks) : base(child)
    {
        this.taskList = tasks;
    }

    protected override List<ITask> GetList() => taskList;

    protected override void OnElementStart(ITask task, int index)
    {
        task.Initialize();
    }
}
```

## Примечания

- Дочерняя нода клонируется для каждой итерации
- Список должен быть предоставлен до начала выполнения
- Декоратор автоматически управляет состоянием выполнения
- Поддерживает прерывание и возобновление выполнения 