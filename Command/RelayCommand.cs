using System;
using System.Windows.Input;

namespace TemplateEngine_v3.Command
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute; // Действие, выполняемое при выполнении команды.
        private readonly Predicate<object> _canExecute; // Условие для проверки возможности выполнения команды.

        /// <summary>
        /// Инициализирует новый экземпляр класса <see cref="RelayCommand"/>.
        /// </summary>
        /// <param name="execute">Действие, выполняемое при выполнении команды.</param>
        /// <param name="canExecute">Функция для проверки возможности выполнения команды (опционально).</param>
        /// <exception cref="ArgumentNullException">Бросается, если параметр <paramref name="execute"/> равен null.</exception>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {   
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null) throw new ArgumentNullException(nameof(execute));
            _execute = _ => execute();
            _canExecute = canExecute != null ? new Predicate<object>(_ => canExecute()) : null;
        }

        /// <summary>
        /// Проверяет, может ли команда быть выполнена.
        /// </summary>
        /// <param name="parameter">Параметры, переданные команде.</param>
        /// <returns>Возвращает <c>true</c>, если команда может быть выполнена; иначе <c>false</c>.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true; // Если _canExecute == null, возвращаем true
        }

        /// <summary>
        /// Выполняет команду.
        /// </summary>
        /// <param name="parameter">Параметры, переданные команде.</param>
        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        /// <summary>
        /// Происходит, когда изменяется возможность выполнения команды.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value; // Подписка на событие изменения возможности выполнения
            remove => CommandManager.RequerySuggested -= value; // Отписка от события изменения возможности выполнения
        }

        /// <summary>
        /// Уведомляет WPF о необходимости повторной проверки возможности выполнения команды.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested(); // Уведомить WPF о необходимости повторной проверки CanExecute
        }
    }
}
