namespace TaskPool
{
	public interface ITaskPool
	{
		bool IsRunning { get; }
		void Add(AsyncTask task);
		void Add(SyncTask task);
		void Stop();
		void RemoveTask(TaskBase task);
	}


	public class TaskPool : ITaskPool
	{
		private readonly Lock _lock = new();
		private readonly List<TaskHolder> _tasks = [];
		private int _taskPointer = 0;
		private readonly Task[] _workers;
		public bool IsRunning { get; private set; } = true;


		public TaskPool()
		{
			int numberOfCores = Environment.ProcessorCount;
			int numberOfProcesses = Math.Max(1, numberOfCores - 1);
			_workers = new Task[numberOfProcesses];

			for (int i = 0; i < numberOfProcesses; i++)
			{
				_workers[i] = Task.Run(RunWorker);
			}
		}


		public void Add(AsyncTask task)
		{
			lock (_lock)
			{
				_tasks.Add(new(task));
			}
		}


		public void Add(SyncTask task)
		{
			lock (_lock)
			{
				_tasks.Add(new(task));
			}
		}


		public void Stop()
		{
			IsRunning = false;
		}


		private void Remove(TaskHolder holder)
		{
			lock (_lock)
			{
				_tasks.Remove(holder);
			}
		}


		private async Task RunWorker()
		{
			do
			{
				await TryExecuteAnyTask();
				await Task.Delay(1000);
			} while (IsRunning);
		}


		protected virtual void OnTaskError(TaskBase task)
		{
			// Do nothing
		}


		private async Task<bool> TryExecuteAnyTask()
		{
			TaskHolder? holder = BookATask();
			if (holder is null)
			{
				return false;
			}

			if (holder.Task is SyncTask syncTask)
			{
				syncTask.Run();
			}
			else if (holder.Task is AsyncTask asyncTask)
			{
				await asyncTask.Run();
			}
			else
			{
				throw new Exception("Unexpected type of task");
			}

			if (
				holder.Task.Exception is not null
			)
			{
				Remove(holder);
				OnTaskError(holder.Task);
			}

			if (holder.IsGoingToStop)
			{
				Remove(holder);
				holder.Task.FireTaskStopped();
			}

			holder.Release();
			return true;
		}


		private TaskHolder? BookATask()
		{
			lock (_lock)
			{
				if (_tasks.Count == 0)
				{
					return null;
				}

				int startPoint = _taskPointer;
				do
				{
					_taskPointer++;
					if (_taskPointer >= _tasks.Count)
					{
						_taskPointer = 0;
					}

					if (
						_tasks[_taskPointer].IsBooked == false
						&& _tasks[_taskPointer].Task.CanBeExecuted()
					)
					{
						return _tasks[_taskPointer].Book();
					}
				} while (_taskPointer != startPoint);
			}

			return null;
		}


		/// <summary>
		/// Removes the task from the pool if it is not executing.
		/// When it is executing, plans the removing for the future.
		/// 
		/// To be sure that task was removed override and listen to <see cref="TaskBase.OnStopped"/> hook.
		/// </summary>
		/// <param name="task"></param>
		public void RemoveTask(TaskBase task)
		{
			lock (_lock)
			{
				if (_tasks.Count == 0)
				{
					return;
				}

				var holder = _tasks.Find((t) => t.Task == task);
				if (holder is not null)
				{
					if (holder.IsBooked)
					{
						holder.Stop();
					}
					else
					{
						_tasks.Remove(holder);
						task.FireTaskStopped();
					}
				}
			}
		}
	}
}
