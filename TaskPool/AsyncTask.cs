namespace TaskPool
{
	public abstract class AsyncTask : TaskBase
	{
		public abstract Task Main();


		public async Task Run()
		{
			try
			{
				await Main();
			}
			catch (Exception ex)
			{
				SetException(ex);
			}
		}
	}
}
