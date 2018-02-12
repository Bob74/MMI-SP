```C#
/*
	
	Notification avec icône Contact_Blocked
	
*/

	DateTime SHVDNDate = new DateTime(2017, 12, 19);
	Dictionary<string, Version> prerequisites = new Dictionary<string, Version>
	{
		{"SHVDN-Extender.dll", new Version("1.0.0.0")},
		{"iFruitAddon2.dll", new Version("2.0.1.0")},
		{"NativeUI.dll", new Version("1.7.0.0")},
	}

	foreach (string key in prerequisites.Keys)
	{
		string fileName = AppDomain.CurrentDomain.BaseDirectory + "\\" + file;
		
		if (File.Exists(fileName))
		{
			FileInfo info = new FileInfo(fileName);
			FileVersionInfo fileInfo = FileVersionInfo.GetVersionInfo(fileName);
			
			int versionCheck = new Version(fileInfo.FileVersion).CompareTo(prerequisites[key].Value),
			if (versionCheck < 0)
			{
				UI.Notify(key + "is outdated! Download and install the latest version of this file.")
			}
		}
		else
		{
			UI.Notify(key + "is missing! Download and install this file before starting the game.")
		}
	}

	string SHVDNFileName = AppDomain.CurrentDomain.BaseDirectory + "\\..\\ScriptHookVDotNet2.dll";
	if (File.Exists(SHVDNFileName))
	{
		FileInfo info = new FileInfo(SHVDNFileName);
		DateTime date = info.LastWriteTime();
		if (!(date.Year == SHVDNDate.Year && date.Month == SHVDNDate.Month && date.Day == SHVDNDate.Day))
		{
			UI.Notify("ScriptHookVDotNet2 is outdated! Download and install the latest version.")
		}
	}
```

