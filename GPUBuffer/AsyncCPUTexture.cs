#pragma warning disable CS0067

using nobnak.Gist.ThreadSafe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace nobnak.Gist.GPUBuffer {
	public class AsyncCPUTexture<T> : System.IDisposable, ITextureData<T> where T:struct {
		public event System.Action<IList<T>, ListTextureData<T>> OnComplete;
		public event Action<ITextureData<T>> OnLoad;

		protected bool active = false;

		protected T[] data;
		protected ListTextureData<T> output;
		protected AsyncGPUReadbackRequest req;
		protected Vector2Int size;
		protected T defaultValue;

		public AsyncCPUTexture(T defaultValue = default(T)) {
			this.defaultValue = defaultValue;
		}

		#region interface
		public Texture Source { get; set; }
		public virtual void Update() {
			if (active) {
				if (req.hasError) {
					Debug.Log($"Failed to read back from GPU async");
					active = false;
				} else if (req.done) {
					Release();
					var nativeData = req.GetData<T>();
					System.Array.Resize(ref data, nativeData.Length);
					nativeData.CopyTo(data);
					output = GenerateCPUTexture(data, size);
					OnComplete?.Invoke(data, output);
				}
			} else {
				if (Source != null) {
					active = true;
					req = AsyncGPUReadback.Request(Source);
					size = new Vector2Int(Source.width, Source.height);
				}
			}
		}
		#endregion
		#region ITextureData
		public virtual Vector2Int Size => size;
		public Func<float, float, T> Interpolation { get; set; }
		public virtual T this[Vector2 uv] => (output != null ? output[uv] : defaultValue);
		public virtual T this[float nx, float ny] => (output != null ? output[nx, ny] : defaultValue);
		public virtual T this[int x, int y] => (output != null ? output[x, y] : defaultValue);
		#endregion
		#region IDisposable
		public virtual void Dispose() {
			Release();
		}
		#endregion
		#region private
		protected virtual ListTextureData<T> GenerateCPUTexture(IList<T> data, Vector2Int size) {
			var tex = new ListTextureData<T>(data, size);
			tex.Interpolation = Interpolation;
			return tex;
		}
		protected virtual void Release() {
			if (output != null) {
				output.Dispose();
				output = null;
			}
			active = false;
		}
		#endregion
	}
}
