﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gist.Scoped {

    public class ScopedObject<T> : Scoped<T> where T : Object {

        public ScopedObject(T data) : base(data) {  }

        public override void Disposer(T data) {
            Release(Data);
        }

        public static implicit operator ScopedObject<T>(T data) {
            return new ScopedObject<T>(data);
        }
        public static implicit operator T(ScopedObject<T> scoped) {
            return scoped.Data;
        }

        public static void Release(Object obj) {
            if (Application.isPlaying)
                Object.Destroy(obj);
            else
                Object.DestroyImmediate(obj);
        }
    }

    public class ScopedPlug<T> : Scoped<T> {
        protected System.Action<T> disposer;

        public ScopedPlug(T data, System.Action<T> disposer) : base(data) {
            this.disposer = disposer;
        }

        public override void Disposer(T data) {
            disposer(data);
        }
    }

    public abstract class Scoped<T> : System.IDisposable {
        public T Data { get; protected set; }
        public bool Disposed { get; protected set; }

        public Scoped(T data) {
            this.Data = data;
            this.Disposed = false;
        }

        public void Dispose() {
            if (!Disposed) {
                Disposed = true;
                Disposer(Data);
            }
        }

        public abstract void Disposer(T data);
    }
}
