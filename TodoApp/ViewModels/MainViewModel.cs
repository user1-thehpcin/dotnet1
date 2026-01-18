using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using TodoApp.Models;

namespace TodoApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    private string? _newTitle;
    private string? _newDescription;
    private TodoItem? _selectedTodo;
    private readonly string _dataFile = "todos.json";

    public ObservableCollection<TodoItem> Todos { get; } = new();

    public string? NewTitle
    {
        get => _newTitle;
        set
        {
            _newTitle = value;
            OnPropertyChanged();
        }
    }

    public string? NewDescription
    {
        get => _newDescription;
        set
        {
            _newDescription = value;
            OnPropertyChanged();
        }
    }

    public TodoItem? SelectedTodo
    {
        get => _selectedTodo;
        set
        {
            _selectedTodo = value;
            OnPropertyChanged();
            if (value != null)
            {
                NewTitle = value.Title;
                NewDescription = value.Description;
            }
        }
    }

    public ICommand AddCommand { get; }
    public ICommand RemoveCommand { get; }
    public ICommand UpdateCommand { get; }

    public MainViewModel()
    {
        AddCommand = new RelayCommand(AddTodo);
        RemoveCommand = new RelayCommand(RemoveTodo, () => SelectedTodo != null);
        UpdateCommand = new RelayCommand(UpdateTodo, () => SelectedTodo != null);
        LoadTodos();
    }

    private void AddTodo()
    {
        if (!string.IsNullOrWhiteSpace(NewTitle))
        {
            var todo = new TodoItem
            {
                Title = NewTitle,
                Description = NewDescription,
                IsCompleted = false
            };
            Todos.Add(todo);
            todo.PropertyChanged += Todo_PropertyChanged;
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            SaveTodos();
        }
    }

    private void RemoveTodo()
    {
        if (SelectedTodo != null)
        {
            Todos.Remove(SelectedTodo);
            SelectedTodo = null;
            SaveTodos();
        }
    }

    private void UpdateTodo()
    {
        if (SelectedTodo != null && !string.IsNullOrWhiteSpace(NewTitle))
        {
            SelectedTodo.Title = NewTitle;
            SelectedTodo.Description = NewDescription;
            OnPropertyChanged(nameof(Todos)); // To refresh UI
            NewTitle = string.Empty;
            NewDescription = string.Empty;
            SelectedTodo = null;
            SaveTodos();
        }
    }

    private void LoadTodos()
    {
        if (File.Exists(_dataFile))
        {
            var json = File.ReadAllText(_dataFile);
            var todos = JsonSerializer.Deserialize<List<TodoItem>>(json);
            if (todos != null)
            {
                foreach (var todo in todos)
                {
                    Todos.Add(todo);
                    todo.PropertyChanged += Todo_PropertyChanged;
                }
            }
        }
    }

    private void SaveTodos()
    {
        var json = JsonSerializer.Serialize(Todos.ToList());
        File.WriteAllText(_dataFile, json);
    }

    private void Todo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveTodos();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;

    public void Execute(object? parameter) => _execute();

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}