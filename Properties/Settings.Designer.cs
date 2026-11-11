using System;
using System.CodeDom.Compiler;
using System.Configuration;
using System.Runtime.CompilerServices;

namespace Tablo.Properties
{
	// Token: 0x0200001C RID: 28
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "9.0.0.0")]
	[CompilerGenerated]
	internal sealed partial class Settings : ApplicationSettingsBase
	{
		// Token: 0x1700000C RID: 12
		// (get) Token: 0x0600011F RID: 287 RVA: 0x00002846 File Offset: 0x00000A46
		public static Settings Default
		{
			get
			{
				return Settings.defaultInstance;
			}
		}

		// Token: 0x04000106 RID: 262
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());
	}
}
