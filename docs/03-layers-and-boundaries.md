# Границы слоев и правила взаимодействия

Версия документа: 1.0  
Последнее обновление: 9 февраля 2026 г.  

## Введение

Чистая архитектура основана на чётком разделении системы на слои с контролируемыми границами. Каждый слой имеет свою ответственность и ограниченный набор зависимостей. Это обеспечивает:

- Независимость бизнес-логики от фреймворков и инфраструктуры
- Лёгкость тестирования (особенно доменного слоя)
- Возможность замены инфраструктурных компонентов без изменения бизнес-логики
- Чёткое понимание ответственности каждого компонента

Ключевой принцип: все зависимости направлены ВНУТРЬ к домену. Никакой внешний слой не должен влиять на внутренние слои.

## Слои архитектуры

Система разделена на четыре слоя, расположенных концентрически:

### 1. Domain (Домен) — внутренний слой

Ответственность:
- Чистая бизнес-логика системы
- Доменные модели (агрегаты, сущности, объекты-значения)
- Доменные сервисы (алгоритмы, не принадлежащие конкретным агрегатам)
- Инварианты и правила предметной области

Содержимое:
- Агрегаты: FunctionLibrary, StructConfiguration
- Сущности: DataFlow
- Value Objects: TemporalGraph, Link, TimeInterval, FlowTransformation
- Доменные сервисы: ISynthesisDomainService, IPathFindingDomainService, IGraphAnalysisDomainService
- Вспомогательные классы: матрицы, результаты анализа, пути достижимости

Зависимости:
- Не зависит ни от каких других слоёв
- Не содержит ссылок на внешние библиотеки (кроме стандартной библиотеки .NET)
- Не знает о существовании репозиториев, провайдеров, файловой системы, баз данных

Примеры компонентов:
- FunctionLibrary.cs — неизменяемый агрегат библиотеки функций
- StructConfiguration.cs — агрегат конфигурации сети
- TemporalGraph.cs — объект-значение временного графа
- SynthesisDomainService.cs — реализация алгоритма синтеза (в инфраструктуре, но реализует доменный интерфейс)

Важные правила:
- Все классы в домене должны быть чистыми (без атрибутов [Serializable], [Table] и т.д.)
- Конструкторы должны валидировать входные данные и защищать инварианты
- Методы должны быть сосредоточены на бизнес-логике, а не на технических деталях

### 2. Application (Приложение) — средний слой

Ответственность:
- Координация операций между доменом и инфраструктурой
- Сценарии использования (Use Cases) — конкретные операции системы
- Порты (интерфейсы) для абстракции инфраструктурных компонентов
- DTO для передачи данных между слоями
- Валидация входных данных перед передачей в домен

Содержимое:
- Сценарии использования: SynthesizeConfigurationUseCase, CheckReachabilityUseCase, LoadTemporalGraphsUseCase
- Интерфейсы портов: IFunctionLibraryProvider, IStructConfigurationRepository, IGraphSessionManager, IFileStoragePort
- DTO: StructConfigurationRequestDto, ReachabilityRequest, AnalysisResult, ReachabilityPathDto
- Интерфейсы доменных сервисов (дублируют доменные, но находятся в приложении для удобства)

Зависимости:
- Зависит от доменного слоя (использует доменные модели и интерфейсы)
- Не зависит от инфраструктурного слоя (только через интерфейсы-порты)
- Не зависит от представления

Примеры компонентов:
- SynthesizeConfigurationUseCase.cs — сценарий синтеза конфигурации
- IFunctionLibraryProvider.cs — порт для доступа к библиотеке функций
- StructConfigurationRequestDto.cs — DTO для запроса синтеза
- IPathFindingDomainService.cs — интерфейс доменного сервиса (дублирует доменный, но в приложении)

Важные правила:
- Сценарии использования не должны содержать бизнес-логику — только координацию
- Все зависимости от инфраструктуры должны быть абстрагированы через интерфейсы (порты)
- Сценарии должны работать с доменными объектами, а не с инфраструктурными деталями

### 3. Infrastructure (Инфраструктура) — внешний слой

Ответственность:
- Реализация портов, определённых в слое приложения
- Работа с внешними системами: файловой системой, базами данных, внешними API
- Адаптация внешних форматов данных к внутренним моделям
- Технические детали реализации (парсинг XML, сериализация и т.д.)

Содержимое:
- Реализации репозиториев: InMemoryStructConfigurationRepository, InMemoryDataFlowRepository
- Реализации провайдеров: InMemoryFunctionLibraryProvider
- Реализации фабрик: TemporalGraphFactory
- Адаптеры: XmlFileStorageAdapter, JsonDataSourceAdapter, XmlFunctionLibraryFileAdapter
- Реализации доменных сервисов: SynthesisDomainService, PathFindingDomainService, GraphAnalysisDomainService
- Вспомогательные классы для работы с инфраструктурой

Зависимости:
- Зависит от слоя приложения (реализует интерфейсы-порты)
- Зависит от доменного слоя (использует доменные модели)
- Может зависеть от внешних библиотек (System.Xml, System.Text.Json и т.д.)

Примеры компонентов:
- InMemoryStructConfigurationRepository.cs — реализация репозитория конфигураций
- XmlFileStorageAdapter.cs — адаптер для работы с файловой системой
- SynthesisDomainService.cs — реализация алгоритма синтеза (бизнес-логика без зависимостей)
- TemporalGraphFactory.cs — фабрика для построения временных графов

Важные правила:
- Инфраструктурные компоненты не должны содержать бизнес-логику
- Все детали реализации должны быть скрыты за интерфейсами-портами
- Инфраструктурные классы могут использовать внешние библиотеки, но не должны "утекать" их в другие слои

### 4. Presentation (Представление) — внешний слой

Ответственность:
- Отображение данных пользователю
- Обработка пользовательского ввода
- Маршрутизация команд к сценариям использования
- Управление состоянием пользовательского интерфейса

Содержимое:
- ViewModels: MainViewModel, StructConfigurationViewModel, DataFlowViewModel
- Views: MainWindow, вкладки интерфейса (описаны в XAML)
- Команды: RelayCommand, команды вьюмоделей
- Сервисы представления: IDialogService, визуализация графов

Зависимости:
- Зависит от слоя приложения (использует сценарии использования и порты)
- Не зависит от доменного слоя напрямую
- Не зависит от инфраструктурного слоя напрямую

Примеры компонентов:
- MainViewModel.cs — главная вьюмодель приложения
- StructConfigurationViewModel.cs — вьюмодель для работы с конфигурациями
- RelayCommand.cs — реализация ICommand для привязки команд
- DialogService.cs — сервис для отображения диалоговых окон

Важные правила:
- Вьюмодели не должны содержать бизнес-логику — только координацию вызовов сценариев
- Вьюмодели должны работать только с интерфейсами из слоя приложения
- Никакой прямой работы с репозиториями или инфраструктурными компонентами

## Правила взаимодействия между слоями

### Направление зависимостей

Все зависимости должны быть направлены ВНУТРЬ к домену:

- Presentation зависит от Application
- Application зависит от Domain
- Infrastructure зависит от Application и Domain
- Domain не зависит ни от каких других слоёв

Нарушение этого правила приводит к:
- Утечке инфраструктурных деталей в бизнес-логику
- Сложности при тестировании (требуются моки для внешних систем)
- Невозможности замены инфраструктурных компонентов

### Поток данных

Типичный поток данных при выполнении операции:

1. Пользователь выполняет действие в интерфейсе (клик, ввод данных)
2. Presentation (ViewModel) получает команду и собирает входные данные
3. ViewModel вызывает соответствующий сценарий использования из слоя Application
4. Сценарий использования:
   - Валидирует входные данные
   - Получает необходимые данные через порты (репозитории, провайдеры)
   - Вызывает доменные сервисы или методы агрегатов для выполнения бизнес-логики
   - Сохраняет результаты через порты
   - Возвращает результат (обычно в виде DTO)
5. ViewModel получает результат и обновляет состояние интерфейса
6. Представление (View) отображает обновлённые данные

### Передача данных между слоями

Между слоями данные передаются через:

- Доменные объекты (при передаче внутри домена и из домена в приложение)
- DTO (при передаче из приложения в представление или из инфраструктуры в приложение)
- Примитивные типы и коллекции (для простых параметров)

Запрещено:
- Передавать инфраструктурные объекты (например, XDocument, Stream) в слой приложения или домен
- Возвращать доменные объекты напрямую в представление (используйте DTO)
- Передавать объекты представления (например, элементы управления) в другие слои

## Порты и адаптеры

### Концепция

Порт — это интерфейс, определённый в слое приложения, который абстрагирует некоторую инфраструктурную функциональность.

Адаптер — это реализация порта, расположенная в инфраструктурном слое.

Этот паттерн позволяет:
- Изолировать бизнес-логику от деталей реализации инфраструктуры
- Легко заменять реализации (например, с файловой системы на базу данных)
- Тестировать сценарии использования с моками портов

### Примеры портов и адаптеров в системе

Порт: IFunctionLibraryProvider
- Расположение: Application/Interfaces/Providers/
- Назначение: Абстракция доступа к глобальному состоянию библиотеки функций
- Методы: GetCurrent(), Update(FunctionLibrary)
- Адаптер: InMemoryFunctionLibraryProvider (Infrastructure/Persistence/Providers/)

Порт: IStructConfigurationRepository
- Расположение: Application/Interfaces/Repositories/
- Назначение: Абстракция управления коллекцией конфигураций структуры
- Методы: Add(), Update(), Delete(), GetByInterval(), GetAll()
- Адаптер: InMemoryStructConfigurationRepository (Infrastructure/Persistence/Repositories/)

Порт: IGraphSessionManager
- Расположение: Application/Interfaces/Session/
- Назначение: Абстракция управления сессией временных графов
- Методы: CreateSession(), GetGraphs(), UpdateLinkDirection(), CloseSession()
- Адаптер: GraphSessionManager (Infrastructure/Session/)

Порт: IFileStoragePort
- Расположение: Application/Interfaces/Ports/
- Назначение: Абстракция работы с файловой системой
- Методы: SaveXml(), LoadXml(), FileExists()
- Адаптер: XmlFileStorageAdapter (Infrastructure/Adapters/FileStorage/)

Порт: ITemporalDataSourcePort
- Расположение: Application/Interfaces/Ports/
- Назначение: Абстракция загрузки исходных данных о связях
- Методы: LoadRawLinks()
- Адаптер: JsonTemporalDataSourceAdapter (Infrastructure/Adapters/TemporalData/)

### Как добавить новый порт

1. Определите интерфейс порта в слое Application (Application/Interfaces/[Категория]/)
2. Определите методы порта, которые нужны для сценариев использования
3. Реализуйте адаптер в слое Infrastructure (Infrastructure/[Категория]/)
4. Зарегистрируйте адаптер в контейнере зависимостей (ServiceCollectionExtensions.cs)
5. Внедрите порт в сценарии использования через конструктор

Пример добавления нового порта для логирования:

    // 1. Определение порта в Application/Interfaces/Ports/ILoggingPort.cs
    public interface ILoggingPort
    {
        void LogInformation(string message);
        void LogError(string message, Exception exception);
    }

    // 2. Реализация адаптера в Infrastructure/Adapters/Logging/ConsoleLoggingAdapter.cs
    public class ConsoleLoggingAdapter : ILoggingPort
    {
        public void LogInformation(string message)
        {
            Console.WriteLine($"[INFO] {message}");
        }
        
        public void LogError(string message, Exception exception)
        {
            Console.WriteLine($"[ERROR] {message}: {exception.Message}");
        }
    }

    // 3. Регистрация в ServiceCollectionExtensions.cs
    services.AddScoped<ILoggingPort, ConsoleLoggingAdapter>();

    // 4. Использование в сценарии
    public class SynthesizeConfigurationUseCase
    {
        private readonly ILoggingPort _logger;
        
        public SynthesizeConfigurationUseCase(ILoggingPort logger)
        {
            _logger = logger;
        }
        
        public void Execute(...)
        {
            _logger.LogInformation("Starting synthesis...");
            // ... логика синтеза ...
            _logger.LogInformation("Synthesis completed successfully");
        }
    }

## Примеры нарушений границ и как их избежать

### Нарушение 1: Прямая зависимость домена от инфраструктуры

Плохо:
    // Domain/Aggregates/FunctionLibrary.cs
    using System.Xml.Linq; // ← Зависимость от инфраструктуры в домене!
    
    public class FunctionLibrary
    {
        public XDocument ExportToXml() // ← Утечка инфраструктуры в домен
        {
            // ... логика экспорта ...
        }
    }

Хорошо:
    // Domain/Aggregates/FunctionLibrary.cs — чистый домен без зависимостей
    public class FunctionLibrary
    {
        // Только бизнес-логика и методы изменения состояния
        public FunctionLibrary AddProcesses(IEnumerable<ProcessType> processes) { ... }
    }
    
    // Application/Interfaces/Ports/IFunctionLibraryFilePort.cs — порт в приложении
    public interface IFunctionLibraryFilePort
    {
        void Save(FunctionLibrary library, string path);
    }
    
    // Infrastructure/Adapters/FileStorage/XmlFunctionLibraryFileAdapter.cs — адаптер в инфраструктуре
    public class XmlFunctionLibraryFileAdapter : IFunctionLibraryFilePort
    {
        public void Save(FunctionLibrary library, string path)
        {
            // Работа с XDocument здесь допустима
            var doc = new XDocument(...);
            doc.Save(path);
        }
    }

### Нарушение 2: Бизнес-логика в представлении

Плохо:
    // Presentation/ViewModels/MainViewModel.cs
    public void SynthesizeConfiguration()
    {
        // ... пропущена валидация ...
        
        // Прямая работа с репозиторием (нарушение границ!)
        var configs = _configRepository.GetAll();
        
        // Бизнес-логика в вьюмодели (нарушение!)
        foreach (var config in configs)
        {
            // ... сложная логика синтеза ...
        }
        
        // Прямая работа с файловой системой (нарушение!)
        File.WriteAllText("result.xml", xmlContent);
    }

Хорошо:
    // Presentation/ViewModels/MainViewModel.cs
    public void SynthesizeConfiguration()
    {
        try
        {
            // Только координация и обработка ошибок
            var request = new StructConfigurationRequestDto { ... };
            var configs = _synthesizeUseCase.Execute(request, graphs, flows);
            
            _dialogService.ShowInfo($"Синтез завершён: {configs.Count} конфигураций");
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Ошибка синтеза: {ex.Message}");
        }
    }
    
    // Application/UseCases/Configuration/SynthesizeConfigurationUseCase.cs
    public class SynthesizeConfigurationUseCase : ISynthesizeConfigurationUseCase
    {
        // Вся координация и бизнес-логика здесь
        public IReadOnlyList<StructConfiguration> Execute(...) { ... }
    }

### Нарушение 3: Возврат доменных объектов в представление

Плохо:
    // Application/UseCases/Analysis/AnalyzeGraphStructureUseCase.cs
    public TemporalGraph Execute(TemporalGraph graph) // ← Возврат доменного объекта
    {
        return _analyzer.Analyze(graph); // ← Доменный объект уходит в представление
    }

Хорошо:
    // Application/UseCases/Analysis/AnalyzeGraphStructureUseCase.cs
    public AnalysisResult Execute(TemporalGraph graph) // ← Возврат DTO
    {
        var domainResult = _analyzer.Analyze(graph);
        return MapToDto(domainResult); // Преобразование в DTO
    }
    
    private AnalysisResult MapToDto(GraphAnalysisResult domainResult)
    {
        return new AnalysisResult
        {
            VertexCount = domainResult.VertexCount,
            EdgeCount = domainResult.EdgeCount,
            // ... остальные поля ...
        };
    }

### Нарушение 4: Мутация доменных объектов извне

Плохо:
    // Presentation/ViewModels/StructConfigurationViewModel.cs
    public void UpdateNodeProcess()
    {
        // Прямая мутация доменного объекта (нарушение инвариантов!)
        var node = _currentConfiguration.Nodes.First();
        node.Processes.Add("new_process"); // ← Обход валидации!
    }

Хорошо:
    // Presentation/ViewModels/StructConfigurationViewModel.cs
    public void UpdateNodeProcess()
    {
        // Создание нового объекта через метод агрегата
        var updatedNode = _currentNode.WithActiveProcesses(
            _currentNode.ActiveProcesses.Append("new_process"));
        
        // Обновление конфигурации через сценарий
        _editUseCase.EditNode(_currentInterval, _currentNode.NodeId, 
            node => node.WithActiveProcesses(updatedNode.ActiveProcesses));
    }

## Рекомендации для разработчиков

### При создании нового компонента

1. Определите, к какому слою относится компонент:
   - Бизнес-логика без зависимостей от внешних систем? → Domain
   - Координация операций между слоями? → Application
   - Работа с внешними системами (файлы, БД)? → Infrastructure
   - Отображение данных пользователю? → Presentation

2. Поместите компонент в соответствующую папку:
   - Domain/Aggregates/, Domain/Entities/, Domain/Services/
   - Application/UseCases/, Application/Interfaces/, Application/Dtos/
   - Infrastructure/Persistence/, Infrastructure/Adapters/, Infrastructure/DomainServices/
   - Presentation/ViewModels/, Presentation/Views/, Presentation/Services/

3. Проверьте зависимости:
   - Компонент домена не должен зависеть от других слоёв
   - Компонент приложения может зависеть только от домена
   - Компонент инфраструктуры может зависеть от приложения и домена
   - Компонент представления может зависеть только от приложения

### При добавлении новой функциональности

1. Начните с домена (если требуется новая бизнес-логика):
   - Добавьте методы в существующие агрегаты или создайте новые
   - Создайте доменные сервисы для сложных алгоритмов
   - Защитите инварианты в конструкторах и методах

2. Создайте сценарий использования в слое приложения:
   - Определите интерфейс сценария (например, ISynthesizeConfigurationUseCase)
   - Реализуйте сценарий, координирующий операции
   - Используйте порты для доступа к данным и инфраструктуре

3. Реализуйте необходимые порты и адаптеры:
   - Определите интерфейсы портов в слое приложения
   - Реализуйте адаптеры в слое инфраструктуры
   - Зарегистрируйте адаптеры в контейнере зависимостей

4. Интегрируйте в представление:
   - Добавьте команды и свойства в вьюмодель
   - Вызовите сценарий использования из вьюмодели
   - Обработайте результаты и ошибки

### При тестировании

1. Тестируйте доменные объекты без моков:
   - Создавайте объекты напрямую
   - Проверяйте инварианты и поведение
   - Не требуется внедрение зависимостей

2. Тестируйте сценарии использования с моками портов:
   - Создавайте моки для репозиториев, провайдеров и других портов
   - Проверяйте взаимодействие сценария с портами
   - Проверяйте результаты выполнения

3. Тестируйте инфраструктурные адаптеры интеграционно:
   - Тестируйте с реальными внешними системами (файлами, БД)
   - Проверяйте корректность преобразования данных
   - Проверяйте обработку ошибок

## Связанные документы

01-architecture-overview.md — Обзор архитектуры системы
02-domain-models.md — Детальное описание доменных моделей
04-use-cases.md — Все сценарии использования системы
05-data-flow-synthesis.md — Алгоритм синтеза конфигураций