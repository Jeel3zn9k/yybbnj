using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace Tablo
{
	// Token: 0x0200001E RID: 30
	internal static class Program
	{
		// БЛОК 4, F8-06: подсказка об ошибке интерфейса показывается ОДИН раз за
		// сеанс — повторные ошибки пишутся только в лог, чтобы модальное окно не
		// блокировало табло во время матча.
		private static bool _uiHintShown = false;

		// Token: 0x06000159 RID: 345 RVA: 0x00015890 File Offset: 0x00013A90
		[STAThread]
		private static void Main(string[] args)
		{
			// ===== БЛОК 4, F1-11: контроль одиночного запуска через ИМЕНОВАННЫЙ MUTEX =====
			// GetProcessesByName давал ложные срабатывания на одноимённые процессы и не
			// защищал от двойного запуска при медленном старте. WaitOne(10 с) позволяет
			// новому экземпляру дождаться завершения старого — это используется
			// перезапуском «от администратора» (F8-08), чтобы два окна не пересекались.
			bool createdNew = true;
			Mutex mtx = null;
			try
			{
				mtx = new Mutex(true, "Tablo_HockeyScoreboard_SingleInstance", out createdNew);
			}
			catch
			{
				createdNew = true;   // не удалось создать mutex — запуск не блокируем
			}
			if (!createdNew)
			{
				bool acquired = false;
				try
				{
					if (mtx != null)
					{
						acquired = mtx.WaitOne(TimeSpan.FromSeconds(10.0), false);
					}
				}
				catch (AbandonedMutexException)
				{
					acquired = true;   // прежний экземпляр завершился — mutex наш
				}
				catch
				{
					acquired = false;
				}
				if (!acquired)
				{
					MessageBox.Show("Ошибка. Не могу открыть новую сессию программы, потому что один экземпляр уже работает. Закройте действующую программу, запустите заново программу.", "Табло", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
					return;
				}
			}
			try
			{
				if (args.Length != 0)
				{
					for (int i = 0; i < args.Length; i++)
					{
						string[] kv = args[i].Split(new char[]
						{
							'='
						});
						if (kv[0].ToLower() == "/u" && kv.Length > 1)
						{
							// ===== БЛОК 4, F1-10: валидация GUID перед запуском msiexec =====
							// Раньше ЛЮБОЙ текст из аргумента /u=... подставлялся в команду
							// msiexec /x — теперь принимается только корректный GUID продукта.
							string str = kv[1].Trim(new char[] { '{', '}', ' ' });
							try
							{
								Guid parsed = new Guid(str);
								Process.Start(Environment.GetFolderPath(Environment.SpecialFolder.System) + "\\msiexec.exe", "/x " + parsed.ToString("B"));
							}
							catch
							{
								MessageBox.Show("Некорректный идентификатор продукта для удаления: " + kv[1], "Табло", MessageBoxButtons.OK, MessageBoxIcon.Warning);
							}
						}
					}
				}
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
				Application.ThreadException += delegate(object s, ThreadExceptionEventArgs e)
				{
					// БЛОК 4, F8-06: единый crash_log (ротация 2 МБ x 3, запасной путь
					// в %TEMP% при отсутствии прав) + одна видимая подсказка за сеанс.
					TabloPatch.CrashLog.Append("UI", e.Exception.ToString());
					if (!_uiHintShown)
					{
						_uiHintShown = true;
						try
						{
							MessageBox.Show("Произошла внутренняя ошибка интерфейса.\r\n\r\nПодробности записаны в crash_log.txt в папке программы\r\n(если папка не доступна на запись — в %TEMP%\\Tablo_crash_log.txt).\r\n\r\nПрограмма продолжает работать. Если ошибка повторяется — пришлите crash_log.txt разработчику.", "Табло — внутренняя ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
						}
						catch
						{
						}
					}
				};
				AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e)
				{
					TabloPatch.CrashLog.Append("FATAL", e.ExceptionObject.ToString());
				};
				Application.Run(new frmMain());
			}
			finally
			{
				// БЛОК 4, F1-11: освобождаем mutex при выходе
				if (mtx != null)
				{
					try
					{
						mtx.ReleaseMutex();
					}
					catch
					{
					}
					mtx.Close();
				}
			}
		}
	}
}
