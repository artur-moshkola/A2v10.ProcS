﻿// Copyright © 2020 Alex Kukhtin. All rights reserved.

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using A2v10.ProcS.Interfaces;

namespace A2v10.ProcS
{
	public class Repository : IRepository
	{
		private readonly IWorkflowStorage _workflowStorage;
		private readonly IInstanceStorage _instanceStorage;

		private Dictionary<Guid, IInstance> _instanceCache = new Dictionary<Guid, IInstance>();

		public Repository(IWorkflowStorage workflowStorage, IInstanceStorage instanceStorage)
		{
			_workflowStorage = workflowStorage ?? throw new ArgumentNullException(nameof(workflowStorage));
			_instanceStorage = instanceStorage ?? throw new ArgumentNullException(nameof(instanceStorage));
		}

		async public Task<IInstance> Get(Guid id)
		{
			if (_instanceCache.TryGetValue(id, out IInstance instance))
				return instance;
			instance = await _instanceStorage.Load(id);
			_instanceCache.Add(id, instance);
			return instance;
		}

		public Task Save(IInstance instance)
		{
			return _instanceStorage.Save(instance);
		}

		public Task<IWorkflowDefinition> WorkflowFromStorage(IIdentity identity)
		{
			return _workflowStorage.WorkflowFromStorage(identity);
		}
	}
}