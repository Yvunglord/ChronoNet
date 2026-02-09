# Сценарии использования системы

Версия документа: 1.0  
Последнее обновление: 9 февраля 2026 г.  

## Введение

Сценарии использования (Use Cases) представляют собой операции, которые система выполняет для достижения целей пользователей. В архитектуре Чистой архитектуры сценарии использования находятся в слое Application и отвечают за координацию взаимодействия между доменом и инфраструктурой.

Ключевые характеристики сценариев использования:
- Содержат только координацию операций, а не бизнес-логику
- Зависят от портов (интерфейсов) для доступа к данным и инфраструктуре
- Вызывают доменные сервисы для выполнения бизнес-логики
- Возвращают результаты в виде DTO для передачи в представление
- Обрабатывают ошибки и валидацию входных данных

В системе DynamicNetwork реализованы следующие основные сценарии использования:

1. Загрузка временных графов (LoadTemporalGraphsUseCase)
2. Управление библиотекой функций (ManageFunctionLibraryUseCase, ImportFunctionLibraryUseCase, ExportFunctionLibraryUseCase)
3. Управление потоками данных (ManageDataFlowsUseCase)
4. Управление структурными конфигурациями (ManageStructConfigurationUseCase, EditStructConfigurationUseCase)
5. Анализ структуры графа (AnalyzeGraphStructureUseCase)
6. Проверка достижимости (CheckReachabilityUseCase)
7. Синтез конфигурации (SynthesizeConfigurationUseCase)
8. Экспорт конфигурации (ExportConfigurationUseCase)

## Общий шаблон описания сценария

Каждый сценарий использования описывается по следующей структуре:

Название: Полное название сценария
Цель: Какую задачу решает сценарий
Предусловия: Что должно быть выполнено до запуска сценария
Основной поток: Последовательность шагов выполнения
Альтернативные потоки: Обработка ошибок и исключительных ситуаций
Постусловия: Состояние системы после выполнения
Зависимости: Порты и доменные сервисы, используемые сценарием
Пример кода: Упрощённая реализация сценария

## 1. Загрузка временных графов (LoadTemporalGraphsUseCase)

Название: Загрузка временных графов из файла
Актор: Аналитик сети
Цель: Загрузить топологию сети из JSON-файла и создать сессию для работы с графами
Предусловия: Пользователь выбрал файл в диалоговом окне
Основной поток:
  1. Получить путь к файлу от пользователя
  2. Вызвать сценарий LoadTemporalGraphsUseCase.Execute(path)
  3. Сценарий вызывает порт ITemporalDataSourcePort.LoadRawLinks(path) для загрузки сырых данных
  4. Сценарий вызывает фабрику ITemporalGraphFactory для построения графов
  5. Сценарий создаёт новую сессию через IGraphSessionManager.CreateSession()
  6. Сценарий возвращает коллекцию построенных графов
  7. ViewModel обновляет коллекцию графов и выбирает первый граф для отображения
  8. Система автоматически создаёт базовые конфигурации для всех интервалов
Альтернативные потоки:
  - Если файл не найден: выбросить исключение FileNotFoundException
  - Если файл имеет неверный формат: выбросить исключение с описанием ошибки
  - Если после загрузки нет графов: показать сообщение пользователю
Постусловия:
  - Создана новая сессия с загруженными графами
  - Старая сессия (если была) удалена
  - Базовые конфигурации созданы для всех интервалов времени
  - Первый граф выбран для отображения в интерфейсе
Зависимости:
  - Порт: ITemporalDataSourcePort (загрузка сырых данных)
  - Порт: ITemporalGraphFactory (построение графов)
  - Порт: IGraphSessionManager (управление сессией)
  - Порт: IStructConfigurationRepository (создание базовых конфигураций)
Пример кода:
    public class LoadTemporalGraphsUseCase : ILoadTemporalGraphsUseCase
    {
        private readonly ITemporalDataSourcePort _dataSource;
        private readonly ITemporalGraphFactory _graphFactory;
        private readonly IGraphSessionManager _sessionManager;
        private readonly IStructConfigurationRepository _configRepo;
        
        public IReadOnlyList<TemporalGraph> Execute(string sourcePath)
        {
            // Загрузка сырых данных
            var rawLinks = _dataSource.LoadRawLinks(sourcePath);
            
            // Построение графов
            var allNodes = _graphFactory.ExtractAllNodes(rawLinks);
            var graphs = _graphFactory.BuildGraphs(rawLinks, allNodes);
            
            // Создание сессии
            _sessionManager.CreateSession(sourcePath);
            
            // Создание базовых конфигураций
            foreach (var graph in graphs)
            {
                if (!_configRepo.Exists(graph.Interval))
                {
                    var config = CreateBaseConfiguration(graph);
                    _configRepo.Add(config);
                }
            }
            
            return graphs;
        }
    }

## 2. Управление библиотекой функций (ManageFunctionLibraryUseCase)

Название: Управление библиотекой функций (добавление, удаление, обновление)
Цель: Изменение состава доступных функций (процессов, транспортов, хранилищ)
Предусловия: Библиотека функций загружена или инициализирована
Основной поток (на примере добавления процесса):
  1. Пользователь заполняет форму добавления процесса
  2. ViewModel вызывает сценарий ManageFunctionLibraryUseCase.AddProcesses(processes)
  3. Сценарий получает текущую библиотеку через IFunctionLibraryProvider.GetCurrent()
  4. Сценарий вызывает метод агрегата library.AddProcesses(processes)
  5. Сценарий сохраняет обновлённую библиотеку через провайдер provider.Update(updatedLibrary)
  6. Сценарий возвращает результат операции
  7. ViewModel обновляет отображение библиотеки
Альтернативные потоки:
  - Если процесс с таким ID уже существует: вернуть ошибку
  - Если параметры процесса невалидны: выбросить исключение валидации
Постусловия:
  - Библиотека функций обновлена
  - Все компоненты системы используют новую версию библиотеки
Зависимости:
  - Порт: IFunctionLibraryProvider (доступ к библиотеке)
Пример кода:
    public class ManageFunctionLibraryUseCase : IManageFunctionLibraryUseCase
    {
        private readonly IFunctionLibraryProvider _provider;
        
        public void AddProcesses(IEnumerable<ProcessType> processes)
        {
            var current = _provider.GetCurrent();
            var updated = current.AddProcesses(processes);
            _provider.Update(updated);
        }
        
        public void RemoveProcesses(IEnumerable<string> processIds)
        {
            var current = _provider.GetCurrent();
            var updated = current.RemoveProcesses(processIds);
            _provider.Update(updated);
        }
        
        public void UpdateProcesses(IEnumerable<ProcessType> processes)
        {
            var current = _provider.GetCurrent();
            var updated = current.UpdateProcesses(processes);
            _provider.Update(updated);
        }
    }

## 3. Импорт библиотеки функций (ImportFunctionLibraryUseCase)

Название: Импорт библиотеки функций из XML-файла
Цель: Загрузить библиотеку функций из внешнего XML-файла
Предусловия: Пользователь выбрал XML-файл в диалоговом окне
Основной поток:
  1. Пользователь выбирает файл для импорта
  2. ViewModel вызывает сценарий ImportFunctionLibraryUseCase.Execute(filePath)
  3. Сценарий вызывает порт IFunctionLibraryFilePort.Load(filePath) для загрузки библиотеки
  4. Сценарий сохраняет загруженную библиотеку через провайдер provider.Update(library)
  5. Сценарий возвращает результат операции
  6. ViewModel обновляет отображение библиотеки
Альтернативные потоки:
  - Если файл не найден: выбросить исключение FileNotFoundException
  - Если файл имеет неверный формат: выбросить исключение с описанием ошибки
  - Если файл пустой: показать предупреждение пользователю
Постусловия:
  - Библиотека функций заменена содержимым из файла
  - Все предыдущие изменения библиотеки потеряны (если не было сохранения)
Зависимости:
  - Порт: IFunctionLibraryFilePort (загрузка из файла)
  - Порт: IFunctionLibraryProvider (сохранение библиотеки)
Пример кода:
    public class ImportFunctionLibraryUseCase : IImportFunctionLibraryUseCase
    {
        private readonly IFunctionLibraryFilePort _filePort;
        private readonly IFunctionLibraryProvider _provider;
        
        public void Execute(string filePath)
        {
            var library = _filePort.Load(filePath);
            _provider.Update(library);
        }
    }

## 4. Экспорт библиотеки функций (ExportFunctionLibraryUseCase)

Название: Экспорт библиотеки функций в XML-файл
Цель: Сохранить текущую библиотеку функций во внешний XML-файл
Предусловия: Библиотека функций содержит данные для экспорта
Основной поток:
  1. Пользователь выбирает путь для сохранения файла
  2. ViewModel вызывает сценарий ExportFunctionLibraryUseCase.Execute(filePath)
  3. Сценарий получает текущую библиотеку через провайдер provider.GetCurrent()
  4. Сценарий вызывает порт IFunctionLibraryFilePort.Save(library, filePath) для сохранения
  5. Сценарий возвращает результат операции
  6. ViewModel показывает сообщение об успешном экспорте
Альтернативные потоки:
  - Если путь недоступен для записи: выбросить исключение
  - Если библиотека пустая: показать предупреждение пользователю
Постусловия:
  - Библиотека функций сохранена в указанный файл
  - Состояние системы не изменилось
Зависимости:
  - Порт: IFunctionLibraryFilePort (сохранение в файл)
  - Порт: IFunctionLibraryProvider (получение библиотеки)
Пример кода:
    public class ExportFunctionLibraryUseCase : IExportFunctionLibraryUseCase
    {
        private readonly IFunctionLibraryFilePort _filePort;
        private readonly IFunctionLibraryProvider _provider;
        
        public void Execute(string filePath)
        {
            var library = _provider.GetCurrent();
            _filePort.Save(library, filePath);
        }
    }

## 5. Управление потоками данных (ManageDataFlowsUseCase)

Название: Управление потоками данных (добавление, удаление, обновление)
Цель: Изменение состава потоков данных в системе
Предусловия: Репозиторий потоков данных доступен
Основной поток (на примере добавления потока):
  1. Пользователь заполняет форму добавления потока данных
  2. ViewModel вызывает сценарий ManageDataFlowsUseCase.AddFlow(flow)
  3. Сценарий проверяет уникальность идентификатора потока
  4. Сценарий добавляет поток в репозиторий через порт IDataFlowRepository.Add(flow)
  5. Сценарий возвращает результат операции
  6. ViewModel обновляет отображение потоков данных
Альтернативные потоки:
  - Если поток с таким ID уже существует: вернуть ошибку
  - Если параметры потока невалидны (объём <= 0, разорванная цепочка трансформаций): выбросить исключение
Постусловия:
  - Поток данных добавлен в репозиторий
  - Поток доступен для использования в синтезе конфигурации
Зависимости:
  - Порт: IDataFlowRepository (управление коллекцией потоков)
Пример кода:
    public class ManageDataFlowsUseCase : IManageDataFlowsUseCase
    {
        private readonly IDataFlowRepository _repo;
        
        public bool AddFlow(DataFlow flow)
        {
            if (_repo.GetById(flow.Id) != null)
                return false;
            
            _repo.Add(flow);
            return true;
        }
        
        public void RemoveFlow(string flowId)
        {
            var flow = _repo.GetById(flowId);
            if (flow != null)
                _repo.Delete(flowId);
        }
        
        public void UpdateFlow(DataFlow flow)
        {
            _repo.Update(flow);
        }
    }

## 6. Управление структурными конфигурациями (ManageStructConfigurationUseCase)

Название: Управление структурными конфигурациями (добавление, удаление, получение)
Цель: Базовое управление коллекцией структурных конфигураций
Предусловия: Репозиторий конфигураций доступен
Основной поток (на примере добавления конфигурации):
  1. Система (или другой сценарий) создаёт новую конфигурацию
  2. Вызывается сценарий ManageStructConfigurationUseCase.Add(config)
  3. Сценарий добавляет конфигурацию в репозиторий через порт IStructConfigurationRepository.Add(config)
  4. Сценарий возвращает результат операции
Альтернативные потоки:
  - Если конфигурация для интервала уже существует: вернуть ошибку
Постусловия:
  - Конфигурация добавлена в репозиторий
  - Конфигурация доступна для редактирования и синтеза
Зависимости:
  - Порт: IStructConfigurationRepository (управление коллекцией конфигураций)
Пример кода:
    public class ManageStructConfigurationUseCase : IManageStructConfigurationUseCase
    {
        private readonly IStructConfigurationRepository _repo;
        
        public bool Add(StructConfiguration config)
        {
            return _repo.Add(config);
        }
        
        public void Delete(TimeInterval interval)
        {
            _repo.Delete(interval);
        }
        
        public StructConfiguration? GetByInterval(TimeInterval interval)
        {
            return _repo.GetByInterval(interval);
        }
        
        public IReadOnlyList<StructConfiguration> GetAll()
        {
            return _repo.GetAll();
        }
    }

## 7. Редактирование структурной конфигурации (EditStructConfigurationUseCase)

Название: Редактирование структурной конфигурации
Цель: Изменение параметров существующей конфигурации (узлов, связей)
Предусловия: Конфигурация для редактирования существует в репозитории
Основной поток:
  1. Пользователь изменяет параметры узла или связи в интерфейсе
  2. ViewModel вызывает сценарий EditStructConfigurationUseCase.Edit(interval, newConfig)
  3. Сценарий обновляет конфигурацию в репозитории через порт IStructConfigurationRepository.Update(newConfig)
  4. Сценарий возвращает обновлённую конфигурацию
  5. ViewModel обновляет отображение
Альтернативные потоки:
  - Если конфигурация для интервала не найдена: создать новую конфигурацию
Постусловия:
  - Конфигурация обновлена в репозитории
  - Изменения доступны для синтеза и анализа
Зависимости:
  - Порт: IStructConfigurationRepository (обновление конфигурации)
Пример кода:
    public class EditStructConfigurationUseCase : IEditStructConfigurationUseCase
    {
        private readonly IStructConfigurationRepository _repo;
        
        public StructConfiguration Edit(TimeInterval interval, StructConfiguration newConfig)
        {
            if (_repo.Exists(interval))
            {
                _repo.Update(newConfig);
            }
            else
            {
                _repo.Add(newConfig);
            }
            
            return _repo.GetByInterval(interval)!;
        }
    }

## 8. Анализ структуры графа (AnalyzeGraphStructureUseCase)

Название: Анализ структуры временного графа
Цель: Получение метрик и характеристик топологии сети
Предусловия: Временной граф выбран для анализа
Основной поток:
  1. Пользователь выбирает граф для анализа
  2. ViewModel вызывает сценарий AnalyzeGraphStructureUseCase.Execute(graph)
  3. Сценарий вызывает доменный сервис IGraphAnalysisDomainService.Analyze(graph)
  4. Сценарий преобразует результат анализа в DTO AnalysisResult
  5. Сценарий возвращает DTO для отображения в интерфейсе
Альтернативные потоки:
  - Если граф пустой: вернуть результат с нулевыми метриками
Постусловия:
  - Получены метрики графа (количество вершин, рёбер, связность и т.д.)
  - Метрики доступны для отображения в интерфейсе
Зависимости:
  - Доменный сервис: IGraphAnalysisDomainService (выполнение анализа)
Пример кода:
    public class AnalyzeGraphStructureUseCase : IAnalyzeGraphStructureUseCase
    {
        private readonly IGraphAnalysisDomainService _analyzer;
        
        public AnalysisResult Execute(TemporalGraph graph)
        {
            var domainResult = _analyzer.Analyze(graph);
            
            return new AnalysisResult
            {
                VertexCount = domainResult.VertexCount,
                EdgeCount = domainResult.EdgeCount,
                DirectedLinksCount = domainResult.DirectedLinksCount,
                UndirectedLinksCount = domainResult.UndirectedLinksCount,
                IsConnected = domainResult.IsConnected,
                HasCycles = domainResult.HasCycles,
                Density = domainResult.Density,
                Diameter = domainResult.Diameter,
                AdjacencyMatrix = MatrixVisualizationDto<int>.FromDomain(domainResult.AdjacencyMatrix),
                IncidenceMatrix = MatrixVisualizationDto<int>.FromDomain(domainResult.IncidenceMatrix),
                DegreeCentrality = domainResult.DegreeCentrality,
                BetweennessCentrality = domainResult.BetweennessCentrality,
                StronglyConnectedComponentsCount = domainResult.StronglyConnectedComponentsCount,
                Message = $"Анализ завершён: {domainResult.VertexCount} вершин, {domainResult.EdgeCount} рёбер"
            };
        }
    }

## 9. Проверка достижимости (CheckReachabilityUseCase)

Название: Проверка достижимости между узлами во временной сети
Цель: Определение всех возможных путей от источников к стокам с учётом временных интервалов
Предусловия: Загружены временные графы, определены источники и стоки
Основной поток:
  1. Пользователь заполняет параметры проверки (источник, стоки, интервал времени)
  2. ViewModel вызывает сценарий CheckReachabilityUseCase.Execute(graphs, request)
  3. Сценарий валидирует входные данные (существование узлов, пересечение интервалов)
  4. Сценарий получает конфигурации для интервалов через репозиторий
  5. Сценарий вызывает доменный сервис IPathFindingDomainService.FindAllPaths()
  6. Сценарий преобразует найденные пути в DTO ReachabilityPathDto
  7. Сценарий возвращает результат в виде ReachabilityResult
  8. ViewModel отображает результаты (пути, кратчайший путь, сообщение)
Альтернативные потоки:
  - Если источники или стоки не найдены в графах: вернуть ошибку
  - Если нет пересечения временных интервалов: вернуть сообщение "Нет графов в заданном интервале"
  - Если пути не найдены: вернуть результат с IsReachable = false
Постусловия:
  - Получены все возможные пути от источников к стокам
  - Результаты доступны для отображения и использования в синтезе
Зависимости:
  - Порт: IStructConfigurationRepository (получение конфигураций)
  - Доменный сервис: IPathFindingDomainService (поиск путей)
Пример кода:
    public class CheckReachabilityUseCase : ICheckReachabilityUseCase
    {
        private readonly IStructConfigurationRepository _configRepo;
        private readonly IPathFindingDomainService _pathFinder;
        
        public ReachabilityResult Execute(
            IReadOnlyList<TemporalGraph> graphs,
            ReachabilityRequest request)
        {
            // Валидация входных данных
            var validGraphs = graphs
                .Where(g => g.Interval.Overlaps(request.CustomInterval))
                .OrderBy(g => g.Interval.Start)
                .ToList();
            
            if (!validGraphs.Any())
                return new ReachabilityResult { Message = "Нет графов в заданном интервале времени", IsReachable = false };
            
            var allNodes = validGraphs.SelectMany(g => g.AllNetworkNodes).Distinct();
            if (!allNodes.Contains(request.SourceNode))
                return new ReachabilityResult { Message = $"Исходный узел '{request.SourceNode}' не найден", IsReachable = false };
            
            var missingTargets = request.TargetNodes.Where(t => !allNodes.Contains(t)).ToList();
            if (missingTargets.Any())
                return new ReachabilityResult { 
                    Message = $"Следующие целевые узлы не найдены: {string.Join(", ", missingTargets)}",
                    IsReachable = false 
                };
            
            // Получение конфигураций
            var configs = validGraphs
                .Select(g => _configRepo.GetByInterval(g.Interval))
                .Where(c => c != null)
                .ToList();
            
            // Поиск путей через доменный сервис
            var domainPaths = _pathFinder.FindAllPaths(
                validGraphs, configs, request.SourceNode, 
                request.TargetNodes.FirstOrDefault()!, request.CustomInterval);
            
            // Преобразование в DTO
            var dtoPaths = domainPaths.Select(p => new ReachabilityPathDto
            {
                Path = p.Nodes.ToList(),
                GraphIndices = p.GraphIndices.ToList(),
                Edges = p.Edges.Select(e => new EdgeInfoDto
                {
                    FromNode = e.FromNode,
                    ToNode = e.ToNode,
                    GraphIndex = e.GraphIndex
                }).ToList(),
                NodeCount = p.Nodes.Count,
                Interval = p.Interval
            }).ToList();
            
            return new ReachabilityResult
            {
                AllPaths = dtoPaths,
                IsReachable = dtoPaths.Any(),
                ShortestPathLength = dtoPaths.Any() ? dtoPaths.Min(p => p.NodeCount - 1) : null,
                Message = dtoPaths.Any() 
                    ? $"Найдено {dtoPaths.Count} путей. Кратчайший путь: {dtoPaths.Min(p => p.NodeCount - 1)} шагов"
                    : "Пути не найдены"
            };
        }
    }

## 10. Синтез конфигурации (SynthesizeConfigurationUseCase)

Название: Синтез конфигураций структуры сети
Цель: Автоматическое создание необходимых конфигураций для обработки потоков данных
Предусловия: Загружены временные графы, определены потоки данных, загружена библиотека функций, настроены входные/выходные узлы
Основной поток:
  1. Пользователь нажимает кнопку "Синтезировать конфигурацию"
  2. ViewModel собирает входные данные:
     - Получает все временные графы
     - Получает все потоки данных
     - Получает библиотеку функций
     - Собирает входные узлы (узлы с непустыми Inputs)
     - Собирает выходные узлы (узлы с непустыми Outputs)
     - Вычисляет временной интервал синтеза (мин. Start входов → макс. End выходов)
  3. ViewModel формирует запрос StructConfigurationRequestDto
  4. ViewModel вызывает сценарий SynthesizeConfigurationUseCase.Execute(request, graphs, flows)
  5. Сценарий получает библиотеку через провайдер
  6. Сценарий получает базовые конфигурации через репозиторий
  7. Сценарий вызывает проверку достижимости через сценарий CheckReachabilityUseCase
  8. Сценарий вызывает доменный сервис синтеза ISynthesisDomainService.SynthesizeAll()
  9. Сценарий очищает репозиторий от старых конфигураций
  10. Сценарий сохраняет синтезированные конфигурации в репозиторий
  11. Сценарий возвращает список синтезированных конфигураций
  12. ViewModel показывает сообщение об успешном синтезе
Альтернативные потоки:
  - Если нет входных узлов: вернуть ошибку "Не найдены входные узлы"
  - Если нет выходных узлов: вернуть ошибку "Не найдены выходные узлы"
  - Если нет потоков данных: вернуть ошибку "Необходимо определить хотя бы один поток данных"
  - Если пути достижимости не найдены: вернуть ошибку "Нет достижимых путей между источниками и стоками"
Постусловия:
  - Старые конфигурации удалены из репозитория
  - Новые синтезированные конфигурации сохранены в репозиторий
  - Конфигурации содержат минимально необходимые функции для обработки потоков
Зависимости:
  - Порт: IFunctionLibraryProvider (получение библиотеки функций)
  - Порт: IStructConfigurationRepository (получение и сохранение конфигураций)
  - Порт: IDataFlowRepository (получение потоков данных)
  - Сценарий: ICheckReachabilityUseCase (проверка достижимости)
  - Доменный сервис: ISynthesisDomainService (алгоритм синтеза)
Пример кода:
    public class SynthesizeConfigurationUseCase : ISynthesizeConfigurationUseCase
    {
        private readonly IFunctionLibraryProvider _libraryProvider;
        private readonly IStructConfigurationRepository _configRepo;
        private readonly IDataFlowRepository _flowRepo;
        private readonly ICheckReachabilityUseCase _reachabilityUseCase;
        private readonly ISynthesisDomainService _synthesisService;
        
        public IReadOnlyList<StructConfiguration> Execute(
            StructConfigurationRequestDto request,
            IReadOnlyList<TemporalGraph> graphs,
            IReadOnlyList<DataFlow> flows)
        {
            // Получение данных
            var library = _libraryProvider.GetCurrent();
            var baseConfigs = _configRepo.GetAll().ToList();
            
            // Проверка достижимости
            var reachabilityRequest = new ReachabilityRequest
            {
                SourceNode = request.NodeInputs.Keys.First().NodeId,
                TargetNodes = request.OutputNodes.Select(n => n.NodeId).ToList(),
                CustomInterval = request.CustomInterval
            };
            
            var reachabilityResult = _reachabilityUseCase.Execute(graphs, reachabilityRequest);
            if (!reachabilityResult.IsReachable)
                throw new InvalidOperationException("Нет достижимых путей между источниками и стоками");
            
            // Преобразование путей в доменные объекты
            var domainPaths = reachabilityResult.AllPaths
                .Select(dto => new ReachabilityPath
                {
                    Nodes = dto.Path.AsReadOnly(),
                    GraphIndices = dto.GraphIndices.AsReadOnly(),
                    Edges = dto.Edges.Select(e => new EdgeTraversal
                    {
                        FromNode = e.FromNode,
                        ToNode = e.ToNode,
                        GraphIndex = e.GraphIndex,
                        Link = new Link(e.FromNode, e.ToNode)
                    }).ToList().AsReadOnly(),
                    Interval = dto.Interval
                })
                .ToList();
            
            // Вызов доменного сервиса синтеза
            var synthesizedConfigs = _synthesisService.SynthesizeAll(
                request,
                graphs,
                flows,
                library,
                baseConfigs,
                domainPaths);
            
            // Сохранение результатов
            foreach (var config in _configRepo.GetAll().ToList())
                _configRepo.Delete(config.Interval);
            
            foreach (var config in synthesizedConfigs)
                _configRepo.Add(config);
            
            return synthesizedConfigs;
        }
    }

## 11. Экспорт конфигурации (ExportConfigurationUseCase)

Название: Экспорт синтезированных конфигураций в XML-файл
Цель: Сохранение результатов синтеза во внешний XML-файл для интеграции с системой управления сетью
Предусловия: Синтезированы конфигурации структуры, выбран путь для сохранения
Основной поток:
  1. Пользователь выбирает путь для сохранения файла
  2. ViewModel вызывает сценарий ExportConfigurationUseCase.Execute(outputPath)
  3. Сценарий получает все конфигурации через репозиторий
  4. Сценарий получает библиотеку функций через провайдер
  5. Сценарий получает все потоки данных через репозиторий
  6. Сценарий вызывает сервис экспорта IConfigurationExportService.Export()
  7. Сценарий вызывает порт IFileStoragePort.SaveXml() для сохранения документа
  8. Сценарий возвращает результат операции
  9. ViewModel показывает сообщение об успешном экспорте
Альтернативные потоки:
  - Если нет конфигураций для экспорта: вернуть ошибку "Нет конфигураций для экспорта"
  - Если путь недоступен для записи: выбросить исключение
Постусловия:
  - Конфигурации экспортированы в указанный файл
  - Состояние системы не изменилось
Зависимости:
  - Порт: IStructConfigurationRepository (получение конфигураций)
  - Порт: IFunctionLibraryProvider (получение библиотеки)
  - Порт: IDataFlowRepository (получение потоков)
  - Порт: IFileStoragePort (сохранение файла)
  - Сервис: IConfigurationExportService (формирование XML-документа)
Пример кода:
    public class ExportConfigurationUseCase : IExportConfigurationUseCase
    {
        private readonly IConfigurationExportService _exportService;
        private readonly IStructConfigurationRepository _configRepo;
        private readonly IDataFlowRepository _flowRepo;
        private readonly IFunctionLibraryProvider _libraryProvider;
        private readonly IFileStoragePort _fileStorage;
        
        public void Execute(string outputPath)
        {
            var configs = _configRepo.GetAll();
            if (!configs.Any())
                throw new InvalidOperationException("Нет конфигураций для экспорта");
            
            var library = _libraryProvider.GetCurrent();
            var flows = _flowRepo.GetAll();
            
            var document = _exportService.Export(configs, library, flows);
            _fileStorage.SaveXml(document, outputPath);
        }
    }

## Взаимодействие сценариев

Сценарии использования могут вызывать друг друга для выполнения сложных операций. Основные взаимодействия:

1. Синтез конфигурации использует проверку достижимости:
   - SynthesizeConfigurationUseCase вызывает CheckReachabilityUseCase для определения путей
   - Это позволяет разделить ответственность: синтез работает с результатами анализа, а не с алгоритмом поиска

2. Загрузка графов инициирует создание базовых конфигураций:
   - LoadTemporalGraphsUseCase вызывает внутренние операции для создания конфигураций
   - Это обеспечивает целостность данных при загрузке новых графов

3. Управление конфигурациями используется в синтезе:
   - SynthesizeConfigurationUseCase использует ManageStructConfigurationUseCase для очистки и сохранения
   - Это централизует логику управления коллекцией конфигураций

Важные правила взаимодействия:
- Сценарии могут вызывать другие сценарии только через интерфейсы (не напрямую)
- Сценарии не должны вызывать друг друга рекурсивно
- Сценарии не должны содержать общую бизнес-логику — только координацию
- Если два сценария используют один и тот же алгоритм, вынесите его в доменный сервис

## Рекомендации по созданию новых сценариев

При создании нового сценария использования следуйте этим правилам:

1. Определите чёткую цель сценария:
   - Сценарий должен решать одну конкретную задачу пользователя
   - Избегайте "божественных" сценариев, которые делают слишком много

2. Используйте интерфейсы для всех зависимостей:
   - Все порты и другие сценарии должны внедряться через конструктор
   - Это обеспечивает тестируемость и гибкость

3. Разделяйте ответственность:
   - Сценарий координирует, доменный сервис выполняет бизнес-логику
   - Если сценарий содержит сложную логику, вынесите её в доменный сервис

4. Валидируйте входные данные:
   - Проверяйте параметры перед передачей в домен
   - Возвращайте понятные сообщения об ошибках

5. Используйте DTO для возврата результатов:
   - Никогда не возвращайте доменные объекты напрямую в представление
   - Создавайте специализированные DTO для каждого сценария

6. Обрабатывайте ошибки:
   - Перехватывайте исключения и преобразуйте их в понятные сообщения
   - Логируйте критические ошибки (если реализовано логирование)

7. Тестируйте сценарии с моками:
   - Создавайте моки для всех портов
   - Проверяйте взаимодействие сценария с портами
   - Проверяйте результаты выполнения

Пример создания нового сценария "Копирование конфигурации":

    // 1. Определение интерфейса в Application/Interfaces/UseCases/
    public interface ICopyConfigurationUseCase
    {
        StructConfiguration Execute(TimeInterval sourceInterval, TimeInterval targetInterval);
    }
    
    // 2. Реализация в Application/UseCases/Configuration/
    public class CopyConfigurationUseCase : ICopyConfigurationUseCase
    {
        private readonly IStructConfigurationRepository _repo;
        
        public CopyConfigurationUseCase(IStructConfigurationRepository repo)
        {
            _repo = repo;
        }
        
        public StructConfiguration Execute(TimeInterval sourceInterval, TimeInterval targetInterval)
        {
            // Валидация
            if (!_repo.Exists(sourceInterval))
                throw new ArgumentException($"Конфигурация для интервала {sourceInterval} не найдена");
            
            if (_repo.Exists(targetInterval))
                throw new ArgumentException($"Конфигурация для интервала {targetInterval} уже существует");
            
            // Получение исходной конфигурации
            var sourceConfig = _repo.GetByInterval(sourceInterval)!;
            
            // Создание копии с новым интервалом
            var copiedConfig = new StructConfiguration(
                targetInterval,
                sourceConfig.Nodes,
                sourceConfig.Links);
            
            // Сохранение копии
            _repo.Add(copiedConfig);
            
            return copiedConfig;
        }
    }
    
    // 3. Регистрация в контейнере зависимостей
    services.AddScoped<ICopyConfigurationUseCase, CopyConfigurationUseCase>();
    
    // 4. Использование в ViewModel
    public void CopyConfigurationCommandExecute()
    {
        try
        {
            var newConfig = _copyUseCase.Execute(_currentInterval, _targetInterval);
            _dialogService.ShowInfo($"Конфигурация скопирована в интервал {_targetInterval}");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Ошибка копирования: {ex.Message}");
        }
    }

## Связанные документы

01-architecture-overview.md — Обзор архитектуры системы
02-domain-models.md — Детальное описание доменных моделей
03-layers-and-boundaries.md — Границы слоёв и правила взаимодействия
05-data-flow-synthesis.md — Алгоритм синтеза конфигураций