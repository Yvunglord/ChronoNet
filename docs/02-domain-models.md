# Доменные модели системы

Версия документа: 1.0  
Последнее обновление: 9 февраля 2026 г.  

## Обзор доменной модели

Система построена вокруг четырёх ключевых доменных моделей, каждая из которых имеет уникальный жизненный цикл и паттерн взаимодействия:

1. FunctionLibrary — Immutable Aggregate (неизменяемый агрегат)
2. StructConfiguration — Aggregate (агрегат)
3. DataFlow — Entity (сущность)
4. TemporalGraph — Value Object (объект-значение)

Ключевой принцип: каждая модель имеет единую ответственность и чётко определённый жизненный цикл. Смешение паттернов (например, использование репозитория для FunctionLibrary) нарушает архитектурные границы.

## 1. FunctionLibrary — Immutable Aggregate

Ответственность:
Глобальный справочник всех доступных функций сети:
- Процессы (ProcessType) — функции обработки данных
- Транспорты (TransportType) — функции передачи данных
- Хранилища (StorageType) — функции хранения данных

Архитектурные особенности:

Иммутабельность:
Все операции возвращают новый экземпляр вместо мутации существующего объекта. Например, метод AddProcesses() создаёт и возвращает новый экземпляр FunctionLibrary с добавленными процессами.

Паттерн взаимодействия — Провайдер:
Используется интерфейс IFunctionLibraryProvider вместо репозитория, потому что:
- Библиотека представляет глобальное состояние системы (один экземпляр на приложение)
- Репозиторий подразумевает коллекцию с операциями GetAll(), Delete(id), что не имеет смысла для библиотеки функций

Глобальное состояние:
Реализация провайдера регистрируется как Singleton в DI-контейнере, чтобы все компоненты системы работали с одной и той же библиотекой.

Инварианты:

Инвариант 1: Все коллекции только для чтения
    public IReadOnlyList<ProcessType> Processes { get; }
    public IReadOnlyList<TransportType> Transports { get; }
    public IReadOnlyList<StorageType> Storages { get; }

Инвариант 2: Конструктор гарантирует валидность
    public FunctionLibrary(
        IEnumerable<ProcessType> processes,
        IEnumerable<TransportType> transports,
        IEnumerable<StorageType> storages)
    {
        Processes = processes?.ToList().AsReadOnly() 
            ?? throw new ArgumentNullException(nameof(processes));
        // Аналогично для остальных коллекций
    }

Инвариант 3: Операции возвращают новый экземпляр
    public FunctionLibrary AddProcesses(IEnumerable<ProcessType> newProcesses)
    {
        // Логика добавления...
        return new FunctionLibrary(merged, Transports, Storages); // НОВЫЙ экземпляр
    }

Поведение агрегата (бизнес-операции):

AddProcesses() — добавление новых процессов без дубликатов по идентификатору
RemoveProcesses() — удаление процессов по идентификаторам
UpdateProcesses() — обновление существующих процессов и добавление новых
GetProcessById() — поиск процесса по идентификатору

Пример использования в сценарии:

    public class ManageFunctionLibraryUseCase : IManageFunctionLibraryUseCase
    {
        private readonly IFunctionLibraryProvider _provider;
        
        public void AddVideoProcessing()
        {
            // 1. Получаем текущую версию
            var current = _provider.GetCurrent();
            
            // 2. Создаём НОВУЮ версию через поведение агрегата
            var updated = current.AddProcesses(new[]
            {
                new ProcessType("video_decode", 0.1, "video_raw", "video_yuv", 1.0),
                new ProcessType("video_encode", 0.2, "video_yuv", "video_h264", 1.0)
            });
            
            // 3. Сохраняем новую версию через провайдер
            _provider.Update(updated);
        }
    }

Важно: прямая мутация через сеттеры (library.Processes.Add(...)) запрещена — это нарушает инварианты и иммутабельность.

## 2. StructConfiguration — Aggregate

Ответственность:
Полная конфигурация структуры сети на интервал постоянства структуры (TimeInterval):
- Конфигурации всех узлов (NodeConfiguration)
- Конфигурации всех связей (LinkConfiguration)
- Временной интервал актуальности (Interval)

Архитектурные особенности:

Агрегат с корнем:
StructConfiguration является корнем агрегата — единая транзакционная граница для всех изменений конфигурации.

Иммутабельность:
Изменения происходят через методы With...(), которые создают новый экземпляр агрегата с обновлённым состоянием.

Паттерн взаимодействия — Репозиторий:
Используется интерфейс IStructConfigurationRepository для управления коллекцией конфигураций с поиском по TimeInterval.

Хранилище:
Внутри репозитория используется Dictionary<TimeInterval, StructConfiguration> для O(1) поиска по интервалу и естественной гарантии уникальности.

Инварианты:

Инвариант 1: Интервал должен иметь положительную длительность
    if (interval.Start >= interval.End)
        throw new ArgumentException(
            $"Interval must have positive duration. Got [{interval.Start}, {interval.End}]",
            nameof(interval));

Инвариант 2: Нет дубликатов узлов по идентификатору
    var nodeIds = nodes.Select(n => n.NodeId).ToList();
    if (nodeIds.Count != nodeIds.Distinct().Count())
        throw new ArgumentException("Duplicate node IDs detected", nameof(nodes));

Инвариант 3: Нет дубликатов связей (игнорируя направление)
    var linkPairs = links.Select(l => 
        new { Min = Math.Min(l.NodeA, l.NodeB), Max = Math.Max(l.NodeA, l.NodeB) })
        .ToList();
    if (linkPairs.Count != linkPairs.DistinctBy(p => (p.Min, p.Max)).Count())
        throw new ArgumentException("Duplicate links detected", nameof(links));

Состав агрегата:

Корень агрегата: StructConfiguration
- TimeInterval Interval — временной интервал актуальности
- IReadOnlyCollection<NodeConfiguration> Nodes — конфигурации узлов
- IReadOnlyCollection<LinkConfiguration> Links — конфигурации связей

Узел (NodeConfiguration):
- string NodeId — уникальный идентификатор узла
- IReadOnlyCollection<string> EnabledProcesses — доступные процессы
- IReadOnlyCollection<string> Inputs — входные типы данных
- IReadOnlyCollection<string> Outputs — выходные типы данных
- IReadOnlyDictionary<string, double> StorageCapacities — ёмкости хранилищ
- IReadOnlyCollection<string> ActiveProcesses — активные процессы (подмножество EnabledProcesses)

Связь (LinkConfiguration):
- string NodeA — первый узел связи
- string NodeB — второй узел связи
- IReadOnlyCollection<string> EnabledTransports — доступные транспорты
- IReadOnlyCollection<string> ActiveTransports — активные транспорты (подмножество EnabledTransports)

Поведение агрегата:

WithUpdatedNode() — создаёт новый агрегат с обновлённым узлом
WithUpdatedLink() — создаёт новый агрегат с обновлённой связью
WithAddedLink() — создаёт новый агрегат с добавленной связью

Пример использования в сценарии синтеза:

    private StructConfiguration SynthesizeNodeConfiguration(
        NodeConfiguration baseNode,
        ResourceRequirements requirements,
        FunctionLibrary functionLibrary,
        TimeInterval interval)
    {
        // 1. Собираем активные процессы на основе требований
        var activeProcesses = new List<string>();
        if (requirements.NodeProcessRequirements
            .TryGetValue(baseNode.NodeId, out var intervalReqs) &&
            intervalReqs.TryGetValue(interval, out var requiredTransformations))
        {
            foreach (var transformation in requiredTransformations)
            {
                var parts = transformation.Split("->");
                if (parts.Length == 2)
                {
                    var matchingProcess = functionLibrary.Processes.FirstOrDefault(p =>
                        p.InputFlowType == parts[0] && 
                        p.OutputFlowType == parts[1] &&
                        baseNode.EnabledProcesses.Contains(p.Id));
                    
                    if (matchingProcess != null)
                        activeProcesses.Add(matchingProcess.Id);
                }
            }
        }
        
        // 2. Создаём НОВУЮ конфигурацию узла (иммутабельность!)
        return new NodeConfiguration(
            baseNode.NodeId,
            baseNode.EnabledProcesses,
            baseNode.Inputs,
            baseNode.Outputs,
            baseNode.StorageCapacities,
            activeProcesses); // Активные процессы передаются в конструктор
    }

Важно: все изменения конфигурации происходят через создание новых экземпляров, а не мутацию существующих объектов.

## 3. DataFlow — Entity

Ответственность:
Представление потока данных, который необходимо обработать в сети:
- Уникальный идентификатор и исходный тип данных (Id)
- Объём данных (Volume)
- Последовательность трансформаций (Transformations)

Архитектурные особенности:

Сущность с идентификатором:
Строковый Id служит идентификатором для уникальности в коллекции потоков.

Мутабельность:
Хотя предпочтительна иммутабельность, для простых сущностей допустимо создание новых экземпляров при изменении.

Паттерн взаимодействия — Репозиторий:
Используется интерфейс IDataFlowRepository для CRUD-операций по строковому идентификатору.

Хранилище:
Внутри репозитория используется Dictionary<string, DataFlow> для O(1) поиска по идентификатору.

Инварианты:

Инвариант 1: Идентификатор не может быть пустым
    if (string.IsNullOrWhiteSpace(id))
        throw new ArgumentException("ID cannot be null or empty", nameof(id));

Инвариант 2: Объём должен быть положительным
    if (volume <= 0)
        throw new ArgumentException($"Volume must be positive. Got {volume}", nameof(volume));

Инвариант 3: Трансформации должны образовывать цепочку
    for (int i = 0; i < transforms.Count - 1; i++)
    {
        if (transforms[i].OutputType != transforms[i + 1].InputType)
            throw new ArgumentException(
                $"Transformation chain broken at index {i}: " +
                $"{transforms[i].OutputType} != {transforms[i + 1].InputType}",
                nameof(transformations));
    }

Семантика трансформаций:

Поток данных начинается с исходного типа (указанного в Id) и последовательно проходит через все трансформации. Ключевое правило: выходной тип трансформации N должен совпадать с входным типом трансформации N+1 (цепочка трансформаций).

Пример потока "video_stream":
- Исходный тип: video_4k
- Трансформация 1: video_4k → video_1080p (декодирование)
- Трансформация 2: video_1080p → video_h264 (кодирование)
- Трансформация 3: video_h264 → metadata (анализ)

Пример использования:

    // Создание потока данных
    var videoFlow = new DataFlow(
        id: "video_stream",
        volume: 100.0, // 100 ГБ
        transformations: new[]
        {
            new FlowTransformation("video_4k", "video_1080p"),
            new FlowTransformation("video_1080p", "video_h264"),
            new FlowTransformation("video_h264", "metadata")
        });

    // Добавление в репозиторий
    _flowRepository.Add(videoFlow);

    // Получение по ID
    var retrieved = _flowRepository.GetById("video_stream");

## 4. TemporalGraph — Value Object

Ответственность:
Снимок топологии сети в интервал постоянства структуры:
- Уникальный индекс в последовательности (Index)
- Временной интервал активности (Interval)
- Коллекция активных связей (Links)
- Все узлы сети (AllNetworkNodes — включая изолированные!)

Архитектурные особенности:

Value Object:
Сравнение по значению через реализацию Equals() и GetHashCode(). Идентичность определяется содержимым, а не ссылкой на объект.

Производная структура:
Строится "на лету" из сырых данных (LinkParsingDto). Не имеет собственного жизненного цикла и не сохраняется напрямую в репозиторий.

Паттерн взаимодействия — Фабрика + Сессия:
- Построение через интерфейс ITemporalGraphFactory
- Управление через интерфейс IGraphSessionManager (хранение в рамках сессии)

Иммутабельность:
Все свойства только для чтения — гарантия неизменности снимка топологии после создания.

Критически важное отличие: AllNetworkNodes vs ActiveNodes

AllNetworkNodes — все узлы сети, включая изолированные (не имеющие связей). Передаётся при построении графа извне через фабрику.

ActiveNodes — только узлы, участвующие в связях (вычисляется как объединение NodeA и NodeB из всех связей).

Почему это важно: Такие методы, как построение матрицы смежности обязаны учитывать изолированные вершины графа. Алгоритм синтеза должен учитывать все узлы сети, включая изолированные (например, для размещения хранилищ). Поэтому AllNetworkNodes передаётся при построении графа извне, а не вычисляется из связей.

Реализация:

    public sealed class TemporalGraph : IEquatable<TemporalGraph>
    {
        public int Index { get; }
        public TimeInterval Interval { get; }
        public IReadOnlyList<Link> Links { get; }
        
        // Критически важно: передаётся извне при построении!
        public IReadOnlyList<string> AllNetworkNodes { get; }
        
        // Вычисляемое свойство (только из активных связей)
        public IReadOnlyList<string> ActiveNodes => Links
            .SelectMany(l => new[] { l.NodeA, l.NodeB })
            .Distinct()
            .OrderBy(n => n)
            .ToList()
            .AsReadOnly();
        
        public TemporalGraph(
            int index, 
            TimeInterval interval, 
            IEnumerable<Link> links,
            IEnumerable<string> allNetworkNodes) // Передаётся извне!
        {
            Index = index;
            Interval = interval ?? throw new ArgumentNullException(nameof(interval));
            Links = (links ?? throw new ArgumentNullException(nameof(links))).ToList().AsReadOnly();
            AllNetworkNodes = (allNetworkNodes ?? throw new ArgumentNullException(nameof(allNetworkNodes)))
                .ToList()
                .AsReadOnly();
        }
        
        // Value Object: сравнение по значению
        public bool Equals(TemporalGraph? other) =>
            other != null &&
            Index == other.Index &&
            Interval.Equals(other.Interval) &&
            Links.SequenceEqual(other.Links) &&
            AllNetworkNodes.SequenceEqual(other.AllNetworkNodes);
        
        public override int GetHashCode() => 
            HashCode.Combine(Index, Interval, Links.Count, AllNetworkNodes.Count);
    }

Жизненный цикл:

1. Исходные данные (LinkParsingDto) загружаются из JSON-файла
2. Фабрика (TemporalGraphFactory) строит коллекцию графов:
   - Извлекает все уникальные узлы через NodeExtractor
   - Строит временные интервалы через TemporalPartitioner
   - Создаёт графы для каждого интервала через GraphBuilder
3. Графы сохраняются в сессию (GraphSessionManager)
4. ViewModel получает графы из сессии для отображения
5. При загрузке нового файла старая сессия удаляется, создаётся новая

Важно: TemporalGraph не сохраняется в репозиторий! Он существует только в рамках сессии и перестраивается при загрузке нового файла.

## Взаимодействие моделей в сценарии синтеза

Последовательность взаимодействия моделей при синтезе конфигурации:

1. Входные данные:
   - TemporalGraph[] — топология сети на интервалы времени
   - DataFlow[] — потоки данных с требованиями к обработке
   - FunctionLibrary — доступные функции сети
   - StructConfiguration[] — базовые конфигурации узлов/связей

2. Анализ достижимости:
   - Доменный сервис IPathFindingDomainService выполняет алгоритм BFS на временных графах
   - Определяются все пути от источников к стокам с учётом временных переходов

3. Планирование маршрутов:
   - Сопоставление потоков с найденными путями
   - Определение необходимых трансформаций на узлах для каждого потока
   - Расчёт требований к транспортам на связях

4. Агрегация требований:
   - Сбор требований к процессам по узлам и интервалам
   - Сбор требований к транспортам по связям и интервалам
   - Расчёт требований к хранилищам по объёмам данных

5. Синтез конфигураций:
   - Подбор подходящих функций из FunctionLibrary для удовлетворения требований
   - Формирование активных наборов (ActiveProcesses, ActiveTransports)
   - Создание новых иммутабельных экземпляров StructConfiguration для каждого интервала

## Сравнительная характеристика моделей

FunctionLibrary:
- Тип: Immutable Aggregate
- Идентификатор: отсутствует (глобальное состояние)
- Жизненный цикл: приложение (создаётся при старте, живёт до завершения)
- Паттерн: Провайдер (IFunctionLibraryProvider)
- Хранилище: Singleton в памяти
- Изменения: все операции возвращают новый экземпляр

StructConfiguration:
- Тип: Aggregate
- Идентификатор: TimeInterval (интервал времени)
- Жизненный цикл: интервал времени (создаётся для каждого интервала)
- Паттерн: Репозиторий (IStructConfigurationRepository)
- Хранилище: Dictionary<TimeInterval, StructConfiguration>
- Изменения: методы With...() возвращают новый экземпляр

DataFlow:
- Тип: Entity
- Идентификатор: строковый Id
- Жизненный цикл: поток данных (создаётся пользователем, сохраняется до удаления)
- Паттерн: Репозиторий (IDataFlowRepository)
- Хранилище: Dictionary<string, DataFlow>
- Изменения: создаётся новый экземпляр при обновлении

TemporalGraph:
- Тип: Value Object
- Идентификатор: составной (Index + Interval)
- Жизненный цикл: сессия (создаётся при загрузке файла, удаляется при загрузке нового)
- Паттерн: Фабрика + Сессия
- Хранилище: List<TemporalGraph> внутри сессии
- Изменения: перестроение через фабрику при изменении исходных данных

## Рекомендации для разработчиков

При работе с FunctionLibrary:
- Всегда используйте методы агрегата (AddProcesses, RemoveTransports) для изменений
- Никогда не мутируйте коллекции напрямую (library.Processes.Add(...))
- После изменения всегда обновляйте состояние через провайдер: _provider.Update(updated)

При работе с StructConfiguration:
- Используйте методы With...() для создания обновлённых версий
- Никогда не мутируйте коллекции узлов/связей напрямую
- Сохраняйте изменения через репозиторий: _repo.Update(updated)

При работе с DataFlow:
- Создавайте новый экземпляр при изменении объёма или трансформаций
- Используйте репозиторий для сохранения изменений
- Проверяйте цепочку трансформаций при создании

При работе с TemporalGraph:
- Получайте графы только через сессию: _sessionManager.GetGraphs(sessionId)
- Никогда не пытайтесь сохранить граф в репозиторий конфигураций
- Помните, что графы существуют только в рамках текущей сессии

## Связанные документы

01-architecture-overview.md — Обзор архитектуры системы
03-layers-and-boundaries.md — Границы слоёв и правила взаимодействия
05-data-flow-synthesis.md — Алгоритм синтеза конфигураций