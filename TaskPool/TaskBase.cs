using System.ComponentModel;


namespace TaskPool
{
	public interface ITaskBaseProps
	{
		Exception? Exception { get; }
		string? ErrorMessage { get; }
	}


	public abstract class TaskBase : ITaskBaseProps, INotifyPropertyChanged
	{
		public Exception? Exception { get; private set; }
		public string? ErrorMessage => Exception?.Message;
		public event PropertyChangedEventHandler? PropertyChanged;


		internal void FireTaskStopped()
		{
			OnStopped();
		}


		protected void SetException(Exception exception)
		{
			Exception = exception;
			FirePropertyChanged(nameof(Exception));
			FirePropertyChanged(nameof(ErrorMessage));
		}


		protected void FirePropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new(propertyName));
		}


		public abstract bool CanBeExecuted();
		public virtual void OnStopped() { }
	}
}
