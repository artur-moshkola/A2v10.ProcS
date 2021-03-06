﻿// Copyright © 2020 Alex Kukhtin, Artur Moshkola. All rights reserved.

using System;
using System.Threading.Tasks;

namespace A2v10.ProcS.Infrastructure
{
	public interface IInstanceStorage
	{
		Task<IInstance> Load(Guid instanceId);
		Task Save(IInstance instance);
	}
}
