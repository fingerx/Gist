using nobnak.Gist.ObjectExt;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace nobnak.Gist.Resizable {
    public class ResizableRenderTexture : System.IDisposable {
        public const int DEFAULT_ANTIALIASING = 1;

        public event System.Action<RenderTexture> AfterCreateTexture;
        public event System.Action<RenderTexture> BeforeDestroyTexture;

		protected Validator validator = new Validator();
		protected RenderTexture tex;

		protected RenderTextureReadWrite readWrite;
		protected RenderTextureFormat format;
		protected TextureWrapMode wrapMode;
		protected FilterMode filterMode;
		protected int antiAliasing;
		protected Vector2Int size;
		protected int depth;

		public ResizableRenderTexture(int depth = 24, 
			RenderTextureFormat format = RenderTextureFormat.ARGB32,
            RenderTextureReadWrite readWrite = RenderTextureReadWrite.Default, 
			int antiAliasing = 0,
			FilterMode filterMode = FilterMode.Bilinear,
			TextureWrapMode wrapMode = TextureWrapMode.Clamp) {
            this.depth = depth;
            this.format = format;
            this.readWrite = readWrite;
            this.antiAliasing = ParseAntiAliasing (antiAliasing);
			this.filterMode = FilterMode;
			this.wrapMode = wrapMode;

			validator.Reset();
			validator.Validation += () => {
				CreateTexture(size.x, size.y);
			};
			validator.SetCheckers(() =>
				tex != null && tex.width == size.x && tex.height == size.y);
        }

		#region IDisposable implementation
		public void Dispose() {
			ReleaseTexture();
		}
		#endregion

		#region public
		public Vector2Int Size {
			get { return size; }
			set {
				if (size != value) {
					validator.Invalidate();
					size = value;
				}
			}
		}
		public RenderTexture Texture {
			get {
				validator.Validate();
				return tex;
			}
		}
        public FilterMode FilterMode {
			get { return filterMode; }
			set {
				if (filterMode != value) {
					validator.Invalidate();
					filterMode = value;
				}
			}
		}
        public TextureWrapMode WrapMode {
			get { return wrapMode; }
			set {
				if (wrapMode != value) {
					validator.Invalidate();
					wrapMode = value;
				}
			}
		}

        public bool Update () {
			return validator.Validate();
        }
        public void Clear(Color color, bool clearDepth = true, bool clearColor = true) {
            var active = RenderTexture.active;
            RenderTexture.active = tex;
            GL.Clear (clearDepth, clearColor, color);
            RenderTexture.active = active;
        }
		#endregion

		#region private
		protected void CreateTexture(int width, int height) {
            ReleaseTexture();

			if (width < 2 || height < 2) {
				Debug.LogFormat("Texture size too small : {0}x{1}", width, height);
				return;
			}

            tex = new RenderTexture (width, height, depth, format, readWrite);
            tex.filterMode = FilterMode;
            tex.wrapMode = WrapMode;
            tex.antiAliasing = antiAliasing;
			Debug.LogFormat("ResizableRenderTexture.Create size={0}x{1}", width, height);
            NotifyAfterCreateTexture ();
        }
        protected void NotifyAfterCreateTexture() {
            if (AfterCreateTexture != null)
                AfterCreateTexture (tex);
        }
        protected void NotifyBeforeDestroyTexture() {
            if (BeforeDestroyTexture != null)
                BeforeDestroyTexture (tex);
        }

        protected void ReleaseTexture() {
            NotifyBeforeDestroyTexture ();
            tex.Destroy();
            tex = null;
        }
		protected static int ParseAntiAliasing(int antiAliasing) {
			return (antiAliasing > 0 ? antiAliasing : QualitySettings.antiAliasing);
		}
		#endregion
	}
}
