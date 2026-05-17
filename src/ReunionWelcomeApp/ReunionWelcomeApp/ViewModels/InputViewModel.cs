using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ReunionWelcomeApp.Models;
using ReunionWelcomeApp.Services;

namespace ReunionWelcomeApp.ViewModels;

public class InputViewModel : INotifyPropertyChanged
{
    private string _inputName = string.Empty;
    private bool _isInputActive;
    private string _statusMessage = "Presione ENTER para comenzar";
    private int _attendeeCount;
    private bool _isDemoMode;
    private System.Timers.Timer? _demoTimer;
    private readonly Random _random = new();
    private readonly string[] _mockNames = {
        "Juan Pérez", "María García", "Carlos López", "Ana Martínez", "Pedro Rodríguez",
        "Laura Sánchez", "Miguel González", "Sofia Hernández", "David Pérez", "Isabel Torres",
        "Fernando Ruiz", "Carmen Díaz", "Antonio Moreno", "Elena Jiménez", "José Fernández",
        "Lucía González", "Manuel López", "Patricia Álvarez", "Francisco García", "Angela Romero"
    };

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<string>? NameSubmitted;

    public string InputName
    {
        get => _inputName;
        set
        {
            _inputName = value;
            OnPropertyChanged();
            if (_isInputActive && !string.IsNullOrEmpty(value))
                AudioService.PlayTypingFeedback();
        }
    }

    public bool IsInputActive
    {
        get => _isInputActive;
        set { _isInputActive = value; OnPropertyChanged(); }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { _statusMessage = value; OnPropertyChanged(); }
    }

    public int AttendeeCount
    {
        get => _attendeeCount;
        set { _attendeeCount = value; OnPropertyChanged(); }
    }

    public bool IsDemoMode
    {
        get => _isDemoMode;
        set
        {
            _isDemoMode = value;
            OnPropertyChanged();
            if (_isDemoMode)
                StartDemoMode();
            else
                StopDemoMode();
        }
    }

    public ICommand EnterPressedCommand { get; }
    public ICommand EscapePressedCommand { get; }
    public ICommand ToggleDemoCommand { get; }

    public InputViewModel()
    {
        EnterPressedCommand = new RelayCommand(_ => OnEnterPressed());
        EscapePressedCommand = new RelayCommand(_ => OnEscapePressed());
        ToggleDemoCommand = new RelayCommand(_ => IsDemoMode = !IsDemoMode);

        var config = ConfigService.Load();
        AudioService.SetVolume(config.Sounds.Volume);
    }

    public void OnEnterPressed()
    {
        if (!IsInputActive)
        {
            IsInputActive = true;
            AudioService.PlayStartInteraction();
            StatusMessage = "Ingrese su nombre y presione ENTER";
        }
        else if (!string.IsNullOrWhiteSpace(InputName) && InputName.Length <= 100)
        {
            var name = InputName.Trim();
            NameSubmitted?.Invoke(name);
            AudioService.PlaySubmissionConfirm();
            AttendeeCount++;
            InputName = string.Empty;
            StatusMessage = $"¡Bienvenido! {name}";
            LoggingService.Log($"Attendee registered: {name}");

            System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
            {
                if (IsInputActive)
                    StatusMessage = "Ingrese otro nombre y presione ENTER";
            });
        }
    }

    public void OnEscapePressed()
    {
        InputName = string.Empty;
        IsInputActive = false;
        StatusMessage = "Presione ENTER para comenzar";
    }

    private void StartDemoMode()
    {
        var config = ConfigService.GetConfig();
        _demoTimer = new System.Timers.Timer(config.Demo.IntervalMs);
        _demoTimer.Elapsed += (s, e) =>
        {
            var name = _mockNames[_random.Next(_mockNames.Length)] + " " + _random.Next(100);
            NameSubmitted?.Invoke(name);
            AttendeeCount++;
        };
        _demoTimer.Start();
        StatusMessage = "MODO DEMO ACTIVADO - Presione F3 para salir";
    }

    private void StopDemoMode()
    {
        _demoTimer?.Stop();
        _demoTimer?.Dispose();
        _demoTimer = null;
        StatusMessage = "Presione ENTER para comenzar";
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    public event EventHandler? CanExecuteChanged;

    public RelayCommand(Action<object?> execute) => _execute = execute;
    public bool CanExecute(object? parameter) => true;
    public void Execute(object? parameter) => _execute(parameter);
}