namespace TaskPool
{
	public abstract class SyncTask : TaskBase
	{
		public abstract void Main();


		public void Run()
		{
			try
			{
				Main();
			}
			catch (Exception ex)
			{
				SetException(ex);
			}
		}
	}
}
