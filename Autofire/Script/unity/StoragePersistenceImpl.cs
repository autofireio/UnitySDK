using UnityEngine;
using AutofireClient.Iface;

namespace AutofireClient.Unity
{

	public class StoragePersistenceImpl : FileBatchPersistence
	{

		protected override string RootDirectory ()
		{
			return Application.persistentDataPath;
		}

	}

}
