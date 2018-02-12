```C#

/*
	
	Notification avec icône Contact_Blocked
	
*/

Version dotNetVersion = new Version("4.7.0.0");
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
		
		int versionCheck = new Version(fileInfo.FileVersion).CompareTo(prerequisites[key].Value);
		if (versionCheck < 0)
		{
			UI.Notify(key + "is outdated! Download and install the latest version of this file.");
		}
	}
	else
	{
		UI.Notify(key + "is missing! Download and install this file before starting the game.");
	}
}

string SHVDNFileName = AppDomain.CurrentDomain.BaseDirectory + "\\..\\ScriptHookVDotNet2.dll";
if (File.Exists(SHVDNFileName))
{
	FileInfo info = new FileInfo(SHVDNFileName);
	DateTime date = info.LastWriteTime();
	if (!(date.Year == SHVDNDate.Year && date.Month == SHVDNDate.Month && date.Day == SHVDNDate.Day))
	{
		UI.Notify("ScriptHookVDotNet2 is outdated! Download and install the latest version.");
	}
}

if (!CheckVisualCVersion())
{
	UI.Notify("Your Microsoft Visual C++ is outdated! Download and install version 2015 or later.");
}

if (GetNETFrameworkVersions().CompareTo(doNetVersion) < 0)
{
	UI.Notify("Your Microsoft .NET Framework is outdated! Download and install version 4.7 or later.");
}


///////////////////////////////////////////////////////////////////////////////////////////////////////////////


public static bool CheckVisualCVersion()
{
	List<string> visual2017 = new List<string> {
		"Installer\\Dependencies\\,,x86,14.0,bundle\\Dependents\\{404c9c27-8377-4fd1-b607-7ca635db4e49}",
		"Installer\\Dependencies\\,,amd64,14.0,bundle\\Dependents\\{6c6356fe-cbfa-4944-9bed-a9e99f45cb7a}"
	};
	List<string> visual2015 = new List<string> {
		"SOFTWARE\\Classes\\Installer\\Dependencies\\{e2803110-78b3-4664-a479-3611a381656a}",
		"SOFTWARE\\Classes\\Installer\\Dependencies\\{d992c12e-cab2-426f-bde3-fb8c53950b0d}"
	};
	
	if (Registry.ClassesRoot.OpenSubKey(visual2017[0], false) != null && Registry.ClassesRoot.OpenSubKey(visual2017[1], false) != null)
		return true;
	
	if (Registry.LocalMachine.OpenSubKey(visual2015[0], false) != null && Registry.LocalMachine.OpenSubKey(visual2015[1], false) != null)
		return true;
	
	return false;
}


public static Version GetNETFrameworkVersions()
{
	using (RegistryKey ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey("SOFTWARE\\Microsoft\\NET Framework Setup\\NDP\\v4\\Full\\"))
	{
		if (ndpKey != null && ndpKey.GetValue("Release") != null)
		{
			int releaseKey = (int)ndpKey.GetValue("Release");
			
			if (releaseKey >= 461308)
			{
				return new Version("4.7.1.0");
			}
			else if (releaseKey >= 460798)
			{
				return new Version("4.7.0.0");
			}
			else
			{
				return new Version("4.0.0.0");
			}
		}
		else
		{
			return new Version("0.0.0.0");
		}
	}
}
```

