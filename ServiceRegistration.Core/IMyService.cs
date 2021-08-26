using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceRegistration.Core
{
	public interface IMyService : IScopedDependency
	{
		string GetSomeData();
	}
}
