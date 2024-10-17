namespace TaskPool
{
	internal class TaskHolder
	{
		public readonly TaskBase Task;
		public bool IsBooked { get; private set; }
		/// <summary>
		/// Tells to taskPool that the task should be removed from the taskPool
		/// </summary>
		public bool IsGoingToStop { get; private set; }

		public TaskHolder(TaskBase task)
		{
			Task = task;
		}


		public void Stop()
		{
			IsGoingToStop = true;
		}


		public TaskHolder Book()
		{
			IsBooked = true;
			return this;
		}


		public void Release()
		{
			IsBooked = false;
		}
	}
}
