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
  private readonly string[] _mockNames = {"Alejandro Silva", "Sofía Mendoza", "Mateo Fernández", "Valentina Rojas", "Santiago Castillo",
  "Camila Herrera", "Diego Morales", "Isabella Vargas", "Sebastián Castro", "Valeria Medina",
  "Matías Ortiz", "Lucía Guzmán", "Joaquín Torres", "Martina Ruiz", "Benjamín Romero",
  "Emilia Molina", "Lucas Díaz", "Catalina Vega", "Gabriel Fuentes", "Rafaela Soto",
  "Martín Muñoz", "Fernanda Ramos", "Tomás Navarro", "Julieta Ibáñez", "Agustín Guerrero",
  "Antonella Castro", "Nicolás Silva", "Elena Cruz", "Damián Pardo", "Victoria Fuentes",
  "Bruno Mendoza", "Paulina Acuña", "Ignacio Delgado", "Renata Salinas", "Samuel Figueroa",
  "Constanza Bustos", "Daniel Tapia", "Mia Sepúlveda", "Vicente Corvalán", "Amanda Orellana",
  "Maximiliano Jara", "Isidora Valenzuela", "Felipe Rivas", "Maite Araya", "Juan Pablo Gallardo",
  "Antonia Godoy", "Gonzalo Carvajal", "Florencia Palma", "Francisco Quezada", "Camila Lagos",
  "Manuel Bravo", "Josefa Briceño", "Eduardo Espinoza", "Trinidad Alarcón", "Rodrigo Aguilera",
  "Pía Barraza", "Cristián Bustamante", "Carla Cárdenas", "Javier Cifuentes", "Daniela Concha",
  "Luis Contreras", "Javiera Cortés", "Antonio Dávila", "Javiera Donoso", "Carlos Echeverría",
  "Constanza Elizondo", "Esteban Escobar", "Fernanda Espina", "Mauricio Farías", "Andrea Ferrada",
  "Mauricio Flores", "Francisca Fuentes", "Ricardo Galdames", "Belén Gatica", "Álvaro Gómez",
  "Paula Henríquez", "Patricio Hernández", "Josefina Hurtado", "Alberto Lagos", "Javiera Leiva",
  "Víctor Lira", "Nicole López", "Fernando Lorca", "Belén Loyola", "Claudio Macaya",
  "Camila Manríquez", "Carlos Martínez", "Paulina Maturana", "Carlos Medel", "Valentina Meléndez",
  "Sergio Miranda", "Catalina Molina", "René Mondaca", "Daniela Montero", "Andrés Morales",
  "Francisca Moraga", "Enrique Moreno", "Javiera Muñoz", "Roberto Navarro", "Sofía Navarrete",
  "Pedro Norambuena", "Valentina Noriega", "Roberto Ogalde", "Constanza Olivares", "Claudio Orellana",
  "Isidora Osorio", "Jorge Pacheco", "Fernanda Palma", "Rodrigo Parra", "Camila Parada",
  "Gustavo Peñailillo", "Josefa Peralta", "Héctor Pérez", "Gabriela Pizarro", "Mauricio Plaza",
  "Catalina Poblete", "Felipe Ponce", "Valentina Portilla", "Ernesto Prado", "Daniela Pulgar",
  "Esteban Quezada", "Francisca Quintanilla", "Jorge Ramírez", "Fernanda Ramos", "Javier Retamal",
  "Antonia Riquelme", "Alejandro Rivas", "Javiera Rivera", "Carlos Robles", "Paula Rodríguez",
  "Fernando Rojas", "Martina Romero", "Cristián Rosas", "Camila Rubilar", "Eduardo Ruiz",
  "Isidora Saavedra", "Gonzalo Salamanca", "Florencia Salas", "Diego Salinas", "Valentina San Martín",
  "Ignacio Sánchez", "Antonia Sandoval", "Daniel Santibáñez", "Josefa Saravia", "Nicolás Sepúlveda",
  "Javiera Silva", "Vicente Solís", "Catalina Soto", "Alberto Sotomayor", "Francisca Suárez",
  "Matías Tapia", "Valentina Toledo", "Martín Toro", "Fernanda Torres", "Cristián Troncoso",
  "Josefa Uribe", "Andrés Urzúa", "Isidora Valdés", "Sergio Valdivia", "Paula Valencia",
  "Francisco Valenzuela", "Camila Vargas", "Carlos Vásquez", "Javiera Vega", "Mauricio Velásquez",
  "Martina Venegas", "Alejandro Vera", "Valentina Vergara", "Gustavo Vidal", "Francisca Villalobos",
  "Patricio Villarroel", "Josefa Villegas", "Héctor Viveros", "Catalina Yáñez", "Sergio Yévenes",
  "Antonia Zamorano", "Diego Zapata", "Javiera Zúñiga", "Felipe Aburto", "Valentina Acevedo",
  "Mauricio Acosta", "Josefa Agüero", "Carlos Alarcón", "Catalina Aldunate", "Rodrigo Alfaro",
  "Francisca Allendes", "Sergio Almeida", "Martina Alvarado", "Jorge Álvarez", "Camila Amaro",
  "Matías Amigo", "Valentina Ampuero", "Patricio Andrade", "Isidora Angulo", "Héctor Aravena",
  "Javiera Arancibia", "Alejandro Araos", "Catalina Araya", "Gustavo Arce", "Francisca Arellano"
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