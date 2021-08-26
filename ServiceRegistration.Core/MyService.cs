using System;

namespace ServiceRegistration.Core
{
	public class MyService : IMyService
	{
		public string GetSomeData()
		{
			return "This was injected";
		}
	}
}
