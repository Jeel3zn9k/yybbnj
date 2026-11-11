using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Tablo.Properties
{
	// Token: 0x02000017 RID: 23
	[DebuggerNonUserCode]
	[CompilerGenerated]
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "2.0.0.0")]
	internal class Resources
	{
		// Token: 0x0600010B RID: 267 RVA: 0x000026EB File Offset: 0x000008EB
		internal Resources()
		{
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x0600010C RID: 268 RVA: 0x00010488 File Offset: 0x0000E688
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (Resources.resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("Tablo.Properties.Resources", typeof(Resources).Assembly);
					Resources.resourceMan = resourceManager;
				}
				return Resources.resourceMan;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x0600010D RID: 269 RVA: 0x000027C5 File Offset: 0x000009C5
		// (set) Token: 0x0600010E RID: 270 RVA: 0x000027CC File Offset: 0x000009CC
		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return Resources.resourceCulture;
			}
			set
			{
				Resources.resourceCulture = value;
			}
		}

		// Token: 0x040000E7 RID: 231
		private static ResourceManager resourceMan;

		// Token: 0x040000E8 RID: 232
		private static CultureInfo resourceCulture;
	}
}
