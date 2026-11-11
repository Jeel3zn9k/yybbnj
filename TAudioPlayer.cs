using System;
using System.IO;
using System.Windows.Forms;
using DirectShowLib;

namespace Tablo
{
	// Token: 0x0200001A RID: 26
	internal class TAudioPlayer : Control
	{
		// Token: 0x14000005 RID: 5
		// (add) Token: 0x0600010F RID: 271 RVA: 0x000027D4 File Offset: 0x000009D4
		// (remove) Token: 0x06000110 RID: 272 RVA: 0x000027ED File Offset: 0x000009ED
		public event EventHandler OnAudioPlayerStop;

		// Token: 0x06000111 RID: 273 RVA: 0x000104C4 File Offset: 0x0000E6C4
		protected virtual void OnAudioPlayerStopEvent(object sender, EventArgs e)
		{
			EventHandler onAudioPlayerStop = this.OnAudioPlayerStop;
			if (onAudioPlayerStop != null)
			{
				onAudioPlayerStop(this, e);
			}
		}

		// Token: 0x06000112 RID: 274 RVA: 0x00002806 File Offset: 0x00000A06
		public bool GetAudioPrepared(string fileName)
		{
			return this._audioPrepared && this._fileName == fileName;
		}

		// Token: 0x06000113 RID: 275 RVA: 0x00002821 File Offset: 0x00000A21
		public TAudioPlayer()
		{
			this._audioPrepared = false;
		}

		// Token: 0x06000114 RID: 276 RVA: 0x000104E4 File Offset: 0x0000E6E4
		private void HandleGraphEvent()
		{
			if (this.mediaEventEx == null)
			{
				return;
			}
			EventCode eventCode;
			IntPtr lParam;
			IntPtr lParam2;
			while (this.mediaEventEx.GetEvent(out eventCode, out lParam, out lParam2, 0) == 0)
			{
				int num = this.mediaEventEx.FreeEventParams(eventCode, lParam, lParam2);
				if (eventCode == EventCode.Complete)
				{
					this.mediaControl.Stop();
					DsLong pCurrent = new DsLong(0L);
					num = this.mediaSeeking.SetPositions(pCurrent, AMSeekingSeekingFlags.AbsolutePositioning, null, AMSeekingSeekingFlags.NoPositioning);
					if (num < 0)
					{
						num = this.mediaControl.Stop();
						num = this.mediaControl.Run();
					}
					// БЛОК 7 (аудит), Б13: БЫЛО this.mediaEventEx = null — после этого CleanUp()
					// не мог снять notify-окно, и DirectShow продолжал слать WM_GRAPHNOTIFY в окно,
					// которое к моменту закрытия формы могло быть уничтожено.
					// Чистка 2026-11 (CS0414): стоявший здесь флаг _graphDone = true никогда не читался —
					// удалён; суть правки Б13 (не обнулять mediaEventEx) сохранена.
					this.OnAudioPlayerStopEvent(this, new EventArgs());
					return;
				}
			}
		}

		// Token: 0x06000115 RID: 277 RVA: 0x0001057C File Offset: 0x0000E77C
		protected override void WndProc(ref Message m)
		{
			int msg = m.Msg;
			if (msg == 1037)
			{
				this.HandleGraphEvent();
			}
			base.WndProc(ref m);
		}

		// Token: 0x06000116 RID: 278 RVA: 0x000105A8 File Offset: 0x0000E7A8
		public int PrepareSound(string fileName)
		{
			// БЛОК 7 (аудит), Б12: БЫЛО — при любом исключении метод возвращал result == 0 (ER_OK),
			// т.е. сообщал «успех». Вызывающий код (frmMain.Initial) проверял «!= 0», поэтому
			// плеер оставался «живым», но нерабочим: свисток/сирена молчали ВЕСЬ матч, и в логе
			// не было ни одной строчки об этом.
			// СТАЛО: каждый отказ имеет свой код и пишется в crash_log.txt.
			if (string.IsNullOrEmpty(fileName) || !File.Exists(fileName))
			{
				this._audioPrepared = false;
				TabloPatch.CrashLog.Append("AUDIO", "файл звука НЕ НАЙДЕН: " + ((fileName == null) ? "(не задан)" : fileName));
				return ER_FileNotFound;
			}
			try
			{
				this._audioPrepared = false;
				this._fileName = fileName;
				this.CleanUp();
				if (this.graphBuilder == null)
				{
					this.graphBuilder = (IGraphBuilder)new FilterGraph();
				}
				int hr = this.graphBuilder.RenderFile(this._fileName, null);
				DsError.ThrowExceptionForHR(hr);
				this.mediaControl = (IMediaControl)this.graphBuilder;
				this.mediaEventEx = (IMediaEventEx)this.graphBuilder;
				this.mediaSeeking = (IMediaSeeking)this.graphBuilder;
				this.mediaPosition = (IMediaPosition)this.graphBuilder;
				this.basicAudio = (this.graphBuilder as IBasicAudio);
				if (this.basicAudio == null)
				{
					// БЛОК 7 (аудит), Б12: БЫЛО — basicAudio.put_Volume(...) без проверки на null
					// (NullReferenceException, который тем же catch превращался в «успех»).
					TabloPatch.CrashLog.Append("AUDIO", "в графе нет IBasicAudio (нет аудио-рендерера?): " + fileName);
					this.CleanUp();
					return ER_NoAudio;
				}
				this.basicAudio.put_Volume(-10000);
				hr = this.mediaEventEx.SetNotifyWindow(base.Handle, 1037, IntPtr.Zero);
				DsError.ThrowExceptionForHR(hr);
				this._audioPrepared = true;
				return ER_OK;
			}
			catch (Exception ex)
			{
				this._audioPrepared = false;
				try { this.CleanUp(); } catch { }
				TabloPatch.CrashLog.Append("AUDIO", "PrepareSound(«" + fileName + "») не удался: " + ex.GetType().Name + ": " + ex.Message);
				return ER_RenderFailed;
			}
		}

		// Token: 0x06000117 RID: 279 RVA: 0x000106B8 File Offset: 0x0000E8B8
		public int SoundPlay()
		{
			// БЛОК 7 (аудит), Б12/Б13: mediaPosition/basicAudio использовались без проверки —
			// при частично подготовленном графе это был NullReferenceException.
			if (this.graphBuilder == null || this.mediaControl == null || !this._audioPrepared)
			{
				return ER_NotReady;
			}
			try
			{
				this.mediaControl.Stop();
				if (this.mediaPosition != null) { this.mediaPosition.put_CurrentPosition(0.0); }
				if (this.basicAudio != null) { this.basicAudio.put_Volume(0); }   // Чистка 2026-11 (CS0649): было put_Volume(this.currentVolume); поле никогда не присваивалось и всегда равнялось 0 (0 дБ = полная громкость) — поведение 1:1
				int hr = this.mediaControl.Run();
				if (hr < 0)
				{
					TabloPatch.CrashLog.Append("AUDIO", "SoundPlay: mediaControl.Run вернул 0x" + hr.ToString("X8") + " для «" + this._fileName + "»");
					return ER_RenderFailed;
				}
				return ER_OK;
			}
			catch (Exception ex)
			{
				TabloPatch.CrashLog.Append("AUDIO", "SoundPlay(«" + this._fileName + "»): " + ex.GetType().Name + ": " + ex.Message);
				return ER_RenderFailed;
			}
		}

		// О-14 (аудит 2026-11): удалён мёртвый SoundPlay(string fileName) — ни одного вызова
		// во всём проекте (единственный внешний вызов — безаргументный SoundPlay() в frmMain).
		// Метод также глушил звук (put_Volume(-10000)) и дублировал PrepareSound без защит БЛОКА 7.

		// Token: 0x06000119 RID: 281 RVA: 0x000107E0 File Offset: 0x0000E9E0
		public void CleanUp()
		{
			// БЛОК 7 (аудит), Б13:
			//  1) УБРАН GC.Collect() — полная блокирующая сборка мусора на UI-потоке.
			//     CleanUp вызывается из PrepareSound, а PrepareSound — 4 раза подряд при
			//     каждом сохранении настроек и при каждом нажатии «Прослушать»:
			//     интерфейс замерзал на 0.4-2.4 с (4 x 100-600 мс).
			//  2) Notify-окно снимается ДО обнуления mediaEventEx.
			//  3) COM-интерфейсы освобождаются явно, а не «когда GC решит».
			try { if (this.mediaControl != null) { this.mediaControl.Stop(); } } catch { }
			try
			{
				if (this.mediaEventEx != null)
				{
					this.mediaEventEx.SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
				}
			}
			catch { }
			ReleaseCom<IMediaControl>(ref this.mediaControl);
			ReleaseCom<IMediaPosition>(ref this.mediaPosition);
			ReleaseCom<IMediaEventEx>(ref this.mediaEventEx);
			ReleaseCom<IBasicAudio>(ref this.basicAudio);
			ReleaseCom<IMediaSeeking>(ref this.mediaSeeking);
			ReleaseCom<IGraphBuilder>(ref this.graphBuilder);
			this._audioPrepared = false;
		}

		private static void ReleaseCom<T>(ref T itf) where T : class
		{
			if (itf != null)
			{
				try { System.Runtime.InteropServices.Marshal.ReleaseComObject(itf); } catch { }
				itf = null;
			}
		}

		// БЛОК 7 (аудит), Б13: TAudioPlayer наследует Control (PrepareSound обращается к
		// base.Handle -> создаётся настоящее окно Windows), но объект никуда не добавлялся
		// и никогда не освобождался: экземпляры просто обнуляли (frmMain.Initial), а окно
		// и COM-граф оставались жить. Теперь Dispose корректен.
		protected override void Dispose(bool disposing)
		{
			try { this.CleanUp(); } catch { }
			base.Dispose(disposing);
		}

		// Token: 0x040000F1 RID: 241
		public const int ER_OK = 0;
		// БЛОК 7 (аудит), Б12: новые коды — раньше ЛЮБОЙ сбой возвращал 0 («успех»),
		// и вызывающий код считал плеер рабочим: звук молча отсутствовал весь матч.
		public const int ER_RenderFailed = 10;      // RenderFile/ThrowExceptionForHR не удался
		public const int ER_NoAudio = 11;           // в графе нет IBasicAudio (нет аудио-рендерера)
		public const int ER_NotReady = 12;          // попытка играть не подготовленный звук

		// БЛОК 7 (аудит): признак «звук действительно готов к воспроизведению».
		public bool IsPrepared
		{
			get { return this._audioPrepared; }
		}

		// Token: 0x040000F2 RID: 242
		public const int ER_FileNotFound = 1;

		// Token: 0x040000F3 RID: 243
		public const int ER_AudioNotPrepared = 2;

		// Token: 0x040000F4 RID: 244
		public const int ER_AudioIsBusy = 3;

		// Token: 0x040000F5 RID: 245
		private const int WMGraphNotify = 1037;

		// Token: 0x040000F6 RID: 246
		private const int VolumeFull = 0;

		// Token: 0x040000F7 RID: 247
		private const int VolumeSilence = -10000;

		// Token: 0x040000F9 RID: 249
		private IGraphBuilder graphBuilder;

		// Token: 0x040000FA RID: 250
		private IMediaControl mediaControl;

		// Token: 0x040000FB RID: 251
		private IMediaEventEx mediaEventEx;

		// Token: 0x040000FC RID: 252
		private IBasicAudio basicAudio;

		// Token: 0x040000FD RID: 253
		private IMediaSeeking mediaSeeking;

		// Token: 0x040000FE RID: 254
		private IMediaPosition mediaPosition;

		// Token: 0x04000101 RID: 257

		// Token: 0x04000102 RID: 258
		private string _fileName;

		// Token: 0x04000103 RID: 259
		private bool _audioPrepared;
	}
}
