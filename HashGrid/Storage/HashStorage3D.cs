﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gist.HashGridSystem.Storage {
    
    public class HashStorage3D<T> : System.IDisposable, IEnumerable<T> where T : class {
        System.Func<T, Vector3> _GetPosition;
        List<T>[] _grid;
        List<T> _points;
        List<Vector3> _positions;
        Hash3D _hash;

        public HashStorage3D (System.Func<T, Vector3> GetPosition, float cellSize, int nx, int ny, int nz) {
            this._GetPosition = GetPosition;
            this._points = new List<T> ();
            this._positions = new List<Vector3> ();
            Rebuild (cellSize, nx, ny, nz);
        }

        public Hash3D GridInfo { get { return _hash; } }

        public void Add(T point) {
            _points.Add (point);
            AddOnGrid(point, _GetPosition(point));
        }
        public void Remove(T point) {
            RemoveOnGrid(point, _GetPosition(point));
            _points.Remove (point);
        }
        public T Find(System.Predicate<T> Predicate) {
            return _points.Find (Predicate);
        }
        public IEnumerable<S> Neighbors<S>(Vector3 center, float distance) where S : class, T {
            var r2 = distance * distance;
            foreach (var id in _hash.CellIds(center, distance)) {
                var cell = _grid [id];
                foreach (var p in cell) {
                    var s = p as S;
                    if (s == null)
                        continue;

                    var d2 = (_GetPosition (s) - center).sqrMagnitude;
                    if (d2 < r2)
                        yield return s;
                }
            }
        }
        public void Rebuild(float cellSize, int nx, int ny, int nz) {
            _hash = new Hash3D(cellSize, nx, ny, nz);
            var totalCells = nx * ny * nz;
            if (_grid == null || _grid.Length != totalCells) {
                _grid = new List<T>[totalCells];
                for (var i = 0; i < _grid.Length; i++)
                    _grid [i] = new List<T> ();
            }
            Update ();
        }
        public void Update() {
            var limit = _grid.Length;
            for (var i = 0; i < limit; i++)
                _grid [i].Clear ();

            limit = _points.Count;
            _positions.Clear ();
            for (var i = 0; i < limit; i++)
                _positions.Add(_GetPosition (_points [i]));
            Parallel.For (0, limit, (i) =>
                AddOnGrid (_points [i], _positions [i])
            );

        }
        public int[,,] Stat() {
            var counter = new int[_hash.nx, _hash.ny, _hash.nz];
            for (var z = 0; z < _hash.nz; z++)
                for (var y = 0; y < _hash.ny; y++)
                    for (var x = 0; x < _hash.nx; x++)
                        counter [x, y, z] = _grid [_hash.CellId (x, y, z)].Count;
            return counter;
        }
        public int Stat(Vector3 pos) {
            var id = _hash.CellId (pos);
            var cell = _grid [id];
            return cell.Count;
        }

        void AddOnGrid (T point, Vector3 pos) {
            var id = _hash.CellId (pos);
            var cell = _grid [id];
            cell.Add(point);
        }
        void RemoveOnGrid (T point, Vector3 pos) {
            var id = _hash.CellId (pos);
            var cell = _grid [id];
            cell.Remove (point);
        }

        #region IDisposable implementation
        public void Dispose () {}
        #endregion

        #region IEnumerable implementation
        public IEnumerator<T> GetEnumerator () {
            return _points.GetEnumerator ();
        }
        #endregion

        #region IEnumerable implementation
        IEnumerator IEnumerable.GetEnumerator () {
            return this.GetEnumerator ();
        }
        #endregion

    }
}