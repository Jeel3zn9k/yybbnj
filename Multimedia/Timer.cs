using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;   // БЛОК 2, F8-01: проверка Control.IsDisposed/IsHandleCreated в callback

namespace Multimedia
{
	// Token: 0x02000004 RID: 4
	public sealed class Timer : IComponent, IDisposable
	{
		// Token: 0x06000001 RID: 1
		[DllImport("winmm.dll")]
		private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

		// Token: 0x06000002 RID: 2
		[DllImport("winmm.dll")]
		private static extern int timeSetEvent(int delay, int resolution, Timer.TimeProc proc, int user, int mode);

		// Token: 0x06000003 RID: 3
		[DllImport("winmm.dll")]
		private static extern int timeKillEvent(int id);

		// Token: 0x14000001 RID: 1
		// (add) Token: 0x06000004 RID: 4 RVA: 0x00002050 File Offset: 0x00000250
		// (remove) Token: 0x06000005 RID: 5 RVA: 0x00002069 File Offset: 0x00000269
		public event EventHandler Started;

		// Token: 0x14000002 RID: 2
		// (add) Token: 0x06000006 RID: 6 RVA: 0x00002082 File Offset: 0x00000282
		// (remove) Token: 0x06000007 RID: 7 RVA: 0x0000209B File Offset: 0x0000029B
		public event EventHandler Stopped;

		// Token: 0x14000003 RID: 3
		// (add) Token: 0x06000008 RID: 8 RVA: 0x000020B4 File Offset: 0x000002B4
		// (remove) Token: 0x06000009 RID: 9 RVA: 0x000020CD File Offset: 0x000002CD
		public event EventHandler Tick;

		// Token: 0x0600000A RID: 10 RVA: 0x000020E6 File Offset: 0x000002E6
		static Timer()
		{
			Timer.timeGetDevCaps(ref Timer.caps, Marshal.SizeOf(Timer.caps));
		}

		// Token: 0x0600000B RID: 11 RVA: 0x00002102 File Offset: 0x00000302
		public Timer(IContainer container)
		{
			container.Add(this);
			this.Initialize();
		}

		// Token: 0x0600000C RID: 12 RVA: 0x00002117 File Offset: 0x00000317
		public Timer()
		{
			this.Initialize();
		}

		// БЛОК 2, F8-01: ФИНАЛИЗАТОР УБРАН. Раньше ~Timer() вызывал timeKillEvent из потока
		// финализатора — по документации winmm освобождать мультимедиа-таймер из чужого
		// потока опасно (callback может выполниться уже после освобождения -> падение).
		// Штатное освобождение — Dispose(): он вызывается для всех таймеров в
		// Form1_FormClosed (см. Patch_KillTimer). На команды пульта не влияет.

		// Token: 0x0600000E RID: 14 RVA: 0x00002D7C File Offset: 0x00000F7C
		private void Initialize()
		{
			this.mode = TimerMode.Periodic;
			this.period = Timer.Capabilities.periodMin;
			this.resolution = 1;
			this.running = false;
			this.timeProcPeriodic = new Timer.TimeProc(this.TimerPeriodicEventCallback);
			this.timeProcOneShot = new Timer.TimeProc(this.TimerOneShotEventCallback);
			this.tickRaiser = new Timer.EventRaiser(this.OnTick);
		}

		// Token: 0x0600000F RID: 15 RVA: 0x00002DEC File Offset: 0x00000FEC
		public void Start()
		{
			// БЛОК 7 (аудит), Б34: Start/Stop/Dispose не были синхронизированы, timerID писали
			// из UI-потока и читали в Stop() из потока winmm-callback'а. Гонка «Stop убил новый
			// ID» давала состояние «таймер остановлен, а IsRunning == true» и потерянные тики.
			lock (this._syncRoot)
			{
			if (this.disposed)
			{
				throw new ObjectDisposedException("Timer");
			}
			if (this.IsRunning)
			{
				return;
			}
			if (this.Mode == TimerMode.Periodic)
			{
				this.timerID = Timer.timeSetEvent(this.Period, this.Resolution, this.timeProcPeriodic, 0, (int)this.Mode);
			}
			else
			{
				this.timerID = Timer.timeSetEvent(this.Period, this.Resolution, this.timeProcOneShot, 0, (int)this.Mode);
			}
			if (this.timerID == 0)
			{
				throw new TimerStartException("Unable to start multimedia Timer.");
			}
			this.running = true;
			}
			if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
			{
				this.SynchronizingObject.BeginInvoke(new Timer.EventRaiser(this.OnStarted), new object[]
				{
					EventArgs.Empty
				});
				return;
			}
			this.OnStarted(EventArgs.Empty);
		}

		// Token: 0x06000010 RID: 16 RVA: 0x00002ED0 File Offset: 0x000010D0
		public void Stop()
		{
			// БЛОК 7 (аудит), Б34: под блокировкой; disposed больше не бросает — Stop вызывается
			// из Dispose и при закрытии программы, исключение там только мешало.
			lock (this._syncRoot)
			{
			if (this.disposed)
			{
				return;
			}
			if (!this.running)
			{
				return;
			}
			Timer.timeKillEvent(this.timerID);
			this.timerID = 0;
			this.running = false;
			}
			if (this.SynchronizingObject != null && this.SynchronizingObject.InvokeRequired)
			{
				this.SynchronizingObject.BeginInvoke(new Timer.EventRaiser(this.OnStopped), new object[]
				{
					EventArgs.Empty
				});
				return;
			}
			this.OnStopped(EventArgs.Empty);
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002F58 File Offset: 0x00001158
		// БЛОК 2, F8-01/F2-08: можно ли доставить тик (таймер жив, форма не закрыта)?
		private bool Patch_CanDeliverTick()
		{
			if (this.disposed || !this.running)
			{
				return false;
			}
			Control c = this.synchronizingObject as Control;
			if (c != null && (c.IsDisposed || !c.IsHandleCreated))
			{
				return false;
			}
			return true;
		}

		private void TimerPeriodicEventCallback(int id, int msg, int user, int param1, int param2)
		{
			// БЛОК 2, F8-01/F2-08: было — BeginInvoke БЕЗ проверок: при закрытии программы
			// во время перерыва/тайм-аута callback попадал в уничтоженную форму
			// (ObjectDisposedException в winmm-потоке -> аварийное завершение). Также тик
			// больше не доставляется после Stop()/Dispose(). На пульт не влияет.
			if (!this.Patch_CanDeliverTick())
			{
				return;
			}
			if (this.synchronizingObject != null)
			{
				try
				{
					if (this.synchronizingObject.InvokeRequired)
					{
						this.synchronizingObject.BeginInvoke(this.tickRaiser, new object[]
						{
							EventArgs.Empty
						});
					}
					else
					{
						this.OnTick(EventArgs.Empty);
					}
				}
				catch (ObjectDisposedException)
				{
				}
				catch (InvalidOperationException)
				{
				}
				return;
			}
			this.OnTick(EventArgs.Empty);
		}

		// Token: 0x06000012 RID: 18 RVA: 0x00002F9C File Offset: 0x0000119C
		private void TimerOneShotEventCallback(int id, int msg, int user, int param1, int param2)
		{
			// БЛОК 2, F8-01: те же защиты, что и в периодическом callback;
			// Stop() обёрнут в try/catch (форма могла быть закрыта).
			if (this.disposed || !this.running)
			{
				return;
			}
			if (this.synchronizingObject != null)
			{
				Control c = this.synchronizingObject as Control;
				if (c == null || (!c.IsDisposed && c.IsHandleCreated))
				{
					try
					{
						if (this.synchronizingObject.InvokeRequired)
						{
							this.synchronizingObject.BeginInvoke(this.tickRaiser, new object[]
							{
								EventArgs.Empty
							});
						}
						else
						{
							this.OnTick(EventArgs.Empty);
						}
					}
					catch (ObjectDisposedException)
					{
					}
					catch (InvalidOperationException)
					{
					}
				}
				// БЛОК 7 (аудит), Б34: БЫЛО this.Stop() прямо из потока мультимедиа-таймера
				// (timeKillEvent из чужого потока — то самое, от чего отказались в финализаторе).
				// СТАЛО: остановка маршалируется в UI-поток.
				try
				{
					if (this.synchronizingObject.InvokeRequired)
					{
						this.synchronizingObject.BeginInvoke(new Timer.EventRaiser(this.Patch_StopSelf), new object[] { EventArgs.Empty });
					}
					else
					{
						this.Patch_StopSelf(EventArgs.Empty);
					}
				}
				catch
				{
				}
				return;
			}
			this.OnTick(EventArgs.Empty);
			try { this.Patch_StopSelf(EventArgs.Empty); } catch { }
		}

		// БЛОК 7 (аудит), Б34: остановка OneShot-таймера без timeKillEvent из потока winmm
		private void Patch_StopSelf(EventArgs e)
		{
			try { this.Stop(); } catch { }
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002FEC File Offset: 0x000011EC
		private void OnDisposed(EventArgs e)
		{
			EventHandler eventHandler = this.Disposed;
			if (eventHandler != null)
			{
				eventHandler(this, e);
			}
		}

		// Token: 0x06000014 RID: 20 RVA: 0x0000300C File Offset: 0x0000120C
		private void OnStarted(EventArgs e)
		{
			EventHandler started = this.Started;
			if (started != null)
			{
				started(this, e);
			}
		}

		// Token: 0x06000015 RID: 21 RVA: 0x0000302C File Offset: 0x0000122C
		private void OnStopped(EventArgs e)
		{
			EventHandler stopped = this.Stopped;
			if (stopped != null)
			{
				stopped(this, e);
			}
		}

		// Token: 0x06000016 RID: 22 RVA: 0x0000304C File Offset: 0x0000124C
		private void OnTick(EventArgs e)
		{
			EventHandler tick = this.Tick;
			if (tick != null)
			{
				tick(this, e);
			}
		}

		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000017 RID: 23 RVA: 0x00002125 File Offset: 0x00000325
		// (set) Token: 0x06000018 RID: 24 RVA: 0x00002142 File Offset: 0x00000342
		public ISynchronizeInvoke SynchronizingObject
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this.synchronizingObject;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				this.synchronizingObject = value;
			}
		}

		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000019 RID: 25 RVA: 0x00002160 File Offset: 0x00000360
		// (set) Token: 0x0600001A RID: 26 RVA: 0x0000306C File Offset: 0x0000126C
		// О-23 (аудит 2026-11): ВНИМАНИЕ — сеттер Period делает Stop()+Start(): бегущий таймер
		// уничтожается и создаётся заново с НОВЫМ timerId; тики между остановом и стартом теряются
		// безвозвратно. Не меняйте Period на запущенном таймере: сначала Stop(), затем Period = ...,
		// затем Start() — именно так сделано в заставке (Б34, однократное переключение периода).
		// Игровые часы от этого не зависят: с Б18 время берётся из Environment.TickCount (стенные
		// часы), потерянные тики лишь редко обновляют картинку, но не сдвигают время матча.
		public int Period
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this.period;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				if (value < Timer.Capabilities.periodMin || value > Timer.Capabilities.periodMax)
				{
					throw new ArgumentOutOfRangeException("Period", value, "Multimedia Timer period out of range.");
				}
				this.period = value;
				if (this.IsRunning)
				{
					this.Stop();
					this.Start();
				}
			}
		}

		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600001B RID: 27 RVA: 0x0000217F File Offset: 0x0000037F
		// (set) Token: 0x0600001C RID: 28 RVA: 0x000030DC File Offset: 0x000012DC
		public int Resolution
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this.resolution;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException("Resolution", value, "Multimedia timer resolution out of range.");
				}
				this.resolution = value;
				if (this.IsRunning)
				{
					this.Stop();
					this.Start();
				}
			}
		}

		// Token: 0x17000004 RID: 4
		// (get) Token: 0x0600001D RID: 29 RVA: 0x0000219E File Offset: 0x0000039E
		// (set) Token: 0x0600001E RID: 30 RVA: 0x000021BD File Offset: 0x000003BD
		public TimerMode Mode
		{
			get
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				return this.mode;
			}
			set
			{
				if (this.disposed)
				{
					throw new ObjectDisposedException("Timer");
				}
				this.mode = value;
				if (this.IsRunning)
				{
					this.Stop();
					this.Start();
				}
			}
		}

		// Token: 0x17000005 RID: 5
		// (get) Token: 0x0600001F RID: 31 RVA: 0x000021F1 File Offset: 0x000003F1
		public bool IsRunning
		{
			get
			{
				return this.running;
			}
		}

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x06000020 RID: 32 RVA: 0x000021F9 File Offset: 0x000003F9
		public static TimerCaps Capabilities
		{
			get
			{
				return Timer.caps;
			}
		}

		// Token: 0x14000004 RID: 4
		// (add) Token: 0x06000021 RID: 33 RVA: 0x00002200 File Offset: 0x00000400
		// (remove) Token: 0x06000022 RID: 34 RVA: 0x00002219 File Offset: 0x00000419
		public event EventHandler Disposed;

		// Token: 0x17000007 RID: 7
		// (get) Token: 0x06000023 RID: 35 RVA: 0x00002232 File Offset: 0x00000432
		// (set) Token: 0x06000024 RID: 36 RVA: 0x0000223A File Offset: 0x0000043A
		public ISite Site
		{
			get
			{
				return this.site;
			}
			set
			{
				this.site = value;
			}
		}

		// Token: 0x06000025 RID: 37 RVA: 0x00002243 File Offset: 0x00000443
		public void Dispose()
		{
			if (this.disposed)
			{
				return;
			}
			// БЛОК 2, F8-01: Stop() под try/catch — Dispose не должен бросать при закрытии
			try
			{
				if (this.IsRunning)
				{
					this.Stop();
				}
			}
			catch
			{
			}
			// БЛОК 7 (аудит), Б34: гарантированно глушим winmm-таймер, даже если IsRunning уже
			// false, а timerID ещё не ноль. Иначе системный таймер оставался в ОС навсегда
			// (финализатор убран намеренно — см. комментарий в начале файла).
			lock (this._syncRoot)
			{
				if (this.timerID != 0)
				{
					try { Timer.timeKillEvent(this.timerID); } catch { }
					this.timerID = 0;
				}
			}
			this.disposed = true;
			GC.SuppressFinalize(this);
			this.OnDisposed(EventArgs.Empty);
		}

		// Token: 0x04000006 RID: 6
		private const int TIMERR_NOERROR = 0;

		// Token: 0x04000007 RID: 7
		private readonly object _syncRoot = new object();   // БЛОК 7 (аудит), Б34
		private int timerID;

		// Token: 0x04000008 RID: 8
		private volatile TimerMode mode;

		// Token: 0x04000009 RID: 9
		private volatile int period;

		// Token: 0x0400000A RID: 10
		private volatile int resolution;

		// Token: 0x0400000B RID: 11
		private Timer.TimeProc timeProcPeriodic;

		// Token: 0x0400000C RID: 12
		private Timer.TimeProc timeProcOneShot;

		// Token: 0x0400000D RID: 13
		private Timer.EventRaiser tickRaiser;

		// Token: 0x0400000E RID: 14
		private volatile bool running;   // БЛОК 2, F8-01: видно из winmm-потока сразу

		// Token: 0x0400000F RID: 15
		private volatile bool disposed;

		// Token: 0x04000010 RID: 16
		private ISynchronizeInvoke synchronizingObject;

		// Token: 0x04000011 RID: 17
		private ISite site;

		// Token: 0x04000012 RID: 18
		private static TimerCaps caps;

		// Token: 0x02000005 RID: 5
		// (Invoke) Token: 0x06000027 RID: 39
		private delegate void TimeProc(int id, int msg, int user, int param1, int param2);

		// Token: 0x02000006 RID: 6
		// (Invoke) Token: 0x0600002B RID: 43
		private delegate void EventRaiser(EventArgs e);
	}
}
